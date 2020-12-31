using MDrude.WebSocket.Events;
using MDrude.WebSocket.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Common {

    public class WebSocketClient {

        public event EventHandler<ConnectEventArgs> OnConnect;
        public event EventHandler<HandshakeEventArgs> OnHandshake;
        public event EventHandler<DisconnectEventArgs> OnDisconnect;

        public event EventHandler<PingEventArgs> OnPing;
        public event EventHandler<PongEventArgs> OnPong;

        public event EventHandler<MessageEventArgs> OnMessage;

        public bool IsReconnect { get; set; } = true;
        public int ReconnectInterval { get; set; } = 3000;

        public Socket Socket { get; private set; }
        public bool Running { get; private set; }
        public bool Secure { get; private set; }
        public string Host { get; private set; }

        public IPAddress Address { get; private set; }
        public ushort Port { get; private set; }

        private Task ListenTask { get; set; }
        private CancellationTokenSource ListenToken { get; set; }

        private Task ConnectTask { get; set; }
        private CancellationTokenSource ConnectToken { get; set; }

        private Stream Stream { get; set; }
        public WebSocketWriter Writer { get; private set; }

        private string Key { get; set; }

        public WebSocketClient(string address, ushort port, bool secure = false, string host = null) {

            Address = IPAddress.Parse(address);
            Port = port;

            Host = host;

            Secure = secure;
            Running = false;

            Key = RandomGen.CreateBase64Key();

        }

        public void Connect() {

            Running = true;

            ConnectToken = new CancellationTokenSource();
            ConnectTask = new Task(async () => {

                while (Running && !ConnectToken.IsCancellationRequested) {

                    try {

                        Logger.Write("INFO", $"Try connecting to {Address}:{Port}");

                        Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                        Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                        await Socket.ConnectAsync(new IPEndPoint(Address, Port));

                        Stream = await GetStream(Socket);
                        Writer = new WebSocketWriter(Stream, Encoding.UTF8);

                        await HttpUtils.WriteHeader(Stream, GetSendHeader());

                        string result = await HttpUtils.ReadHeader(Stream);
                        
                        if(result.ToLower().Contains("upgrade: websocket")) {

                            ListenToken = new CancellationTokenSource();
                            ListenTask = new Task(Listen, ListenToken.Token, TaskCreationOptions.LongRunning);

                            ListenTask.Start();

                        } else {

                            throw new Exception("Invalid Server response after handshake.");

                        }

                        ConnectToken.Cancel();
                        return;

                    } catch (Exception er) {

                        Logger.Write("INFO", $"Couldn't connect.");

                    }

                    await Task.Delay(ReconnectInterval);

                }

            }, ConnectToken.Token, TaskCreationOptions.LongRunning);

            ConnectTask.Start();

        }

        public void Disconnect() {

            Running = false;

            ConnectToken?.Cancel();
            ListenToken?.Cancel();

            try {

                Socket.Shutdown(SocketShutdown.Both);

            } catch(Exception er) {

            }

        }

        private void Remove(WebSocketDisconnection type) {

            Disconnect();

            Logger.DebugWrite("INFO", $"Disconnection Reason: {type}");
            OnDisconnect?.Invoke(this, new DisconnectEventArgs(null, type));

        }

        private async void Listen() {

            using (Stream ns = Stream) {

                WebSocketReader reader = new WebSocketReader();

                OnHandshake?.Invoke(this, new HandshakeEventArgs(null, null));

                while (Running && !ListenToken.IsCancellationRequested) {

                    WebSocketFrame frame = await reader.Read(ns, null);

                    if (frame == null || frame.Opcode == WebSocketOpcode.ConnectionCloseFrame) {

                        Remove(WebSocketDisconnection.Disconnect);
                        break;

                    }

                    switch (frame.Opcode) {

                        case WebSocketOpcode.PingFrame:

                            if (frame.Data.Length <= 125) {

                                await Writer.WritePong(frame);
                                OnPing?.Invoke(this, new PingEventArgs(frame.Data));

                            }

                            break;
                        case WebSocketOpcode.PongFrame:

                            OnPong?.Invoke(this, new PongEventArgs(frame.Data));

                            break;
                        case WebSocketOpcode.TextFrame:
                        case WebSocketOpcode.BinaryFrame:

                            OnMessage?.Invoke(this, new MessageEventArgs(null, frame));

                            break;

                    }

                }

            }

        }

        private string GetSendHeader() {

            return "GET / HTTP/1.1" + Environment.NewLine
                + $"Host: {Address}:{Port}" + Environment.NewLine
                + $"Connection: upgrade" + Environment.NewLine
                + $"Pragma: no-cache" + Environment.NewLine
                + $"User-Agent: Mozilla/5.0 (None) Chrome" + Environment.NewLine
                + $"Upgrade: websocket" + Environment.NewLine
                + $"Origin: websocket" + Environment.NewLine
                + $"Sec-WebSocket-Version: 13" + Environment.NewLine
                + $"Accept-Encoding: gzip, deflate, br" + Environment.NewLine
                + $"Accept-Language: en,en-US;q=0.9" + Environment.NewLine
                + $"Sec-WebSocket-Key: {Key}" + Environment.NewLine
                + $"Sec-WebSocket-Extensions: premessage-deflate; client_max_window_bits" + Environment.NewLine

                + Environment.NewLine;

        }

        private async Task<Stream> GetStream(Socket socket) {

            Stream stream = new NetworkStream(socket);

            if (!Secure) {

                return stream;

            }

            try {

                SslStream sslStream = new SslStream(stream, false);
                await sslStream.AuthenticateAsClientAsync(Host ?? Address.ToString());

                return sslStream;

            } catch (Exception e) {

                return stream;

            }

        }

    }

}
