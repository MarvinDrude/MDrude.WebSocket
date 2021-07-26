using MDrude.WebSocket.Events;
using MDrude.WebSocket.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Common {

    public class WebSocketServer {

        public event EventHandler<ConnectEventArgs> OnConnect;
        public event EventHandler<HandshakeEventArgs> OnHandshake;
        public event EventHandler<DisconnectEventArgs> OnDisconnect;

        public event EventHandler<PingEventArgs> OnPing;
        public event EventHandler<PongEventArgs> OnPong;

        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<RttEventArgs> OnRTT;

        public ConcurrentDictionary<string, WebSocketUser> Users { get; private set; }
        public int Backlog { get; set; } = 500;
        public Encoding TextFrameEncoding { get; set; } = Encoding.UTF8;

        public Socket Socket { get; private set; }
        public bool Running { get; private set; }

        public IPAddress Address { get; private set; }
        public ushort Port { get; private set; }
        public X509Certificate2 CertificateSsl { get; private set; }

        private Task ListenTask { get; set; }
        private CancellationTokenSource ListenToken { get; set; }

        private CancellationTokenSource RttToken { get; set; }
        private Task RttTask { get; set; }
        public bool RttEnabled { get; set; }
        public int RttInterval { get; set; }

        public WebSocketServer(string address, ushort port = 27789, X509Certificate2 cert = null) {

            if (address == null)
                GetAddress();
            else
                Address = IPAddress.Parse(address);

            CertificateSsl = cert;
            Port = port;

            Running = false;
            Users = new ConcurrentDictionary<string, WebSocketUser>();

            RttEnabled = false;
            RttInterval = 10000;

            Logger.DebugWrite("INFO", $"New WebSocket Server Instance: {Address}:{port}, Cert: {(cert == null ? "NO" : "YES")}");

        }

        public bool Start() {

            if(Running) {
                Logger.DebugWrite("INFO", $"Tried started WebSocket Server but already running.");
                return false;
            }

            Logger.DebugWrite("INFO", $"WebSocket Server is now getting started");

            Users.Clear();

            Running = true;
            ListenToken = new CancellationTokenSource();

            ListenTask = new Task(Listen, ListenToken.Token, TaskCreationOptions.LongRunning);
            ListenTask.Start();

            if(RttEnabled) {

                RttToken = new CancellationTokenSource();
                RttTask = new Task(RttRoutine, RttToken.Token, TaskCreationOptions.LongRunning);

                RttTask.Start();

            }

            return true;

        }

        public void Stop() {

            if(!Running) {
                Logger.DebugWrite("INFO", $"Tried stopping WebSocket Server but already stopped.");
                return;
            }

            foreach(var key in Users.Keys) {

                WebSocketUser user;

                if(Users.TryGetValue(key, out user)) {

                    RemoveClient(user, WebSocketDisconnection.ServerShutdown);
                    user.ListenToken.Cancel();

                }

            }

            ListenToken.Cancel();
            RttToken?.Cancel();

            try {

                Socket.Shutdown(SocketShutdown.Both);

            } catch(Exception er) {

            }

            Running = false;

        }

        public async Task Broadcast(string data) {

            await Broadcast(WebSocketOpcode.TextFrame, TextFrameEncoding.GetBytes(data));

        }

        public async Task Broadcast(WebSocketOpcode code, byte[] data) {

            foreach(var keypair in Users) {

                await keypair.Value.Writer.Write(code, data);

            }

        }

        private async void Listen() {

            Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            Socket.Bind(new IPEndPoint(Address, Port));

            Socket.Listen(Backlog);
            Logger.DebugWrite("INFO", $"WebSocket Server is now listening to new connections.");

            while (Running && !ListenToken.IsCancellationRequested) {

                Socket socket = null;

                try {

                    socket = await Socket.AcceptAsync();

                } catch(Exception er) {

                    continue;

                }

                WebSocketUser user = new WebSocketUser() {
                    Socket = socket,
                    Stream = await GetStream(socket),
                    UID = RandomGen.GenRandomUID(Users, 128),
                    Server = this
                };

                if(user.Stream == null) {

                    try {

                        user.Socket.Shutdown(SocketShutdown.Both);

                    } catch (Exception) { }

                    continue;

                }

                while(!Users.TryAdd(user.UID, user)) {

                    user.UID = RandomGen.GenRandomUID(Users, 12);

                }

                user.Writer = new WebSocketWriter(user);

                Logger.DebugWrite("INFO", $"New Socket connected to the WebSocket Server. {(user.Socket.RemoteEndPoint as IPEndPoint).Address}, UID: {user.UID}");
                OnConnect?.Invoke(this, new ConnectEventArgs(user));

                CancellationTokenSource cancel = new CancellationTokenSource();

                Task task = new Task(async () => {

                    try {

                        await ListenClient(user);

                    } catch(Exception er) {

                        Logger.DebugWrite("FAILED", "Listen Error: ", er);

                        RemoveClient(user, WebSocketDisconnection.Disconnect);

                    }
                
                }, cancel.Token, TaskCreationOptions.LongRunning);

                task.Start();

                user.ListenTask = task;
                user.ListenToken = cancel;

            }

        }

        private async Task ListenClient(WebSocketUser user) {

            using (Stream ns = user.Stream) {

                WebSocketReader reader = new WebSocketReader();
                var res = await InterpretHeader(user, ns);

                if (!(res.Item1)) {

                    RemoveClient(user, WebSocketDisconnection.NoHeader);
                    return;

                }

                Logger.DebugWrite("INFO", $"Socket successfully handshaked the update. UID: {user.UID}");
                OnHandshake?.Invoke(this, new HandshakeEventArgs(user, res.Item2));

                while (Running && !user.ListenToken.IsCancellationRequested) {

                    WebSocketFrame frame = await reader.Read(ns, user);

                    if(frame == null || frame.Opcode == WebSocketOpcode.ConnectionCloseFrame) {

                        RemoveClient(user, WebSocketDisconnection.Disconnect);
                        break;

                    }

                    switch(frame.Opcode) {

                        case WebSocketOpcode.PingFrame:

                            if(frame.Data.Length <= 125) {

                                await user.Writer.WritePong(frame);
                                OnPing?.Invoke(this, new PingEventArgs(frame.Data));

                            }

                            break;
                        case WebSocketOpcode.PongFrame:

                            OnPong?.Invoke(this, new PongEventArgs(frame.Data));

                            break;

                        case WebSocketOpcode.BinaryFrame:

                            user.Meta.LastTime.Binary = DateTime.UtcNow;

                            if(RttEnabled && frame.Data.Length == 4 && frame.Data[0] == 26
                                && frame.Data[1] == 27 && frame.Data[2] == 28 && frame.Data[3] == 29) {

                                OnPongReceived(user);

                            }

                            OnMessage?.Invoke(this, new MessageEventArgs(user, frame));

                            break;

                        case WebSocketOpcode.TextFrame:

                            user.Meta.LastTime.Text = DateTime.UtcNow;

                            OnMessage?.Invoke(this, new MessageEventArgs(user, frame));

                            break;

                    }

                }

            }

        }

        public void RemoveClient(WebSocketUser user, WebSocketDisconnection reason) {

            try {

                if(user == null) {
                    return;
                }

                user.Disconnected = true;

                WebSocketUser outer;

                if(Users.ContainsKey(user.UID))
                    Users.TryRemove(user.UID, out outer);

                if(user.ListenToken != null)
                    user.ListenToken.Cancel();

                if(user.Socket != null)
                    user.Socket.Shutdown(SocketShutdown.Both);

            } catch(Exception) { }


            Logger.DebugWrite("INFO", $"Socket got removed. UID: {user.UID}, Reason: {reason}");
            OnDisconnect?.Invoke(this, new DisconnectEventArgs(user, reason));

        }

        private void OnPongReceived(WebSocketUser user) {

            if(user.RTT.Sending) {

                user.RTT.Sending = false;

                var span = DateTime.UtcNow - user.RTT.Sent;
                var ms = user.RTT.Last = span.TotalMilliseconds;

                if(ms < user.RTT.Min || user.RTT.Min == -1) {
                    user.RTT.Min = ms;
                }

                if(ms > user.RTT.Max) {
                    user.RTT.Max = ms;
                }

                OnRTT?.Invoke(this, new RttEventArgs(user));

                //Logger.DebugWrite("INFO", $"RTT Last {user.RTT.Last} / {user.RTT.Min} / {user.RTT.Max}");

            }
        
        }

        private async void RttRoutine() {

            while(Running && !RttToken.IsCancellationRequested) {

                try {

                    await Task.Delay(RttInterval, RttToken.Token);

                } catch(Exception) { }

                if(!RttToken.IsCancellationRequested) {

                    foreach(var keypair in Users) {

                        WebSocketUser user = keypair.Value;

                        if(user.RTT.Sending) {

                            user.RTT.Last = RttInterval;

                            if(user.RTT.Max < RttInterval) {

                                user.RTT.Max = RttInterval;
                                OnRTT?.Invoke(this, new RttEventArgs(user));

                            }

                        }

                        user.RTT.Sending = true;
                        user.RTT.Sent = DateTime.UtcNow;

                        await user.Writer.WriteCustomPing();

                    }

                }

            }

        }

        private async Task<(bool, string)> InterpretHeader(WebSocketUser user, Stream ns) {

            string header = await HttpUtils.ReadHeader(ns);
            Regex getRegex = new Regex(@"^GET(.*)HTTP\/1\.1", RegexOptions.IgnoreCase);
            Match getRegexMatch = getRegex.Match(header);

            if(getRegexMatch.Success) {

                string[] lines = header.Split('\n');

                foreach(string line in lines) {

                    if(line.ToLower().StartsWith("user-agent:")) {

                        int index = line.IndexOf(':') + 1;

                        if(index >= line.Length) {
                            break;
                        }

                        user.Meta.UserAgent = line.Substring(index, line.Length - index).Trim();

                        break;

                    }

                }

                await DoHandshake(ns, header);
                return (true, header);

            }

            return (false, header);

        }

        private async Task DoHandshake(Stream ns, string data) {

            string response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                + "Connection: Upgrade" + Environment.NewLine
                + "Upgrade: websocket" + Environment.NewLine
                + "Sec-Websocket-Accept: " + Convert.ToBase64String(
                    SHA1.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(
                            new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                        )
                    )
                ) + Environment.NewLine
                + Environment.NewLine;

            await HttpUtils.WriteHeader(ns, response);

        }

        private async Task<Stream> GetStream(Socket socket) {

            Stream stream = new NetworkStream(socket);

            if (CertificateSsl == null) {

                return stream;

            }

            try {

                SslStream sslStream = new SslStream(stream, false);
                await sslStream.AuthenticateAsServerAsync(CertificateSsl, false, SslProtocols.None, true);

                return sslStream;

            } catch (Exception e) {

                return null;

            }

        }

        private void GetAddress() {

            //var host = Dns.GetHostEntry(Dns.GetHostName());

            //foreach (IPAddress adr in host.AddressList) {
            //    if (adr.AddressFamily == AddressFamily.InterNetwork) {
            //        Address = adr;
            //        break;
            //    }
            //}

            //if (Address == null) {
            //    Address = host.AddressList[0];
            //}

            Address = IPAddress.Loopback;

        }

    }

}
