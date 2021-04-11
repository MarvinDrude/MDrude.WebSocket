using MDrude.WebSocket.Common;
using MDrude.WebSocket.Events;
using MDrude.WebSocket.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Extensions {

    public class JsonWebSocketServer : WebSocketServer {

        public EventEmitterServer Events { get; private set; }

        public event EventHandler<MessageEventArgs> BeforeJsonMessage;

        public JsonWebSocketServer(string address, ushort port = 27789, X509Certificate2 cert = null)
            : base(address, port, cert) {

            Events = new EventEmitterServer();
            InitMessages();

        }

        public async Task Broadcast<T>(string id, T data, WebSocketUser user = null) {

            foreach (var keypair in Users) {

                if(user != null && keypair.Value.UID == user.UID) {
                    continue;
                }

                await keypair.Value.Writer.Send(id, data);

            }

        }

        public void InitMessages() {

            OnMessage += async (sender, args) => {

                WebSocketFrame message = args.Message;

                if(message.Opcode == WebSocketOpcode.TextFrame) {

                    string jsonStr = Encoding.UTF8.GetString(message.Data);
                    JsonMessage json = GetJsonMessage(jsonStr);

                    if(json == null || string.IsNullOrEmpty(json.UID) || json.UID.Trim().Length == 0) {
                        Logger.DebugWrite("INFO", $"JsonWebSocket received malformed TextFrame");
                        return;
                    }

                    BeforeJsonMessage?.Invoke(sender, new MessageEventArgs(args.User, message));

                    await Events.Emit(json.UID, json.Data, args.User);

                }

            };

        }

        private JsonMessage GetJsonMessage(string jsonStr) {

            try {

                JsonMessage json = JsonConvert.DeserializeObject<JsonMessage>(jsonStr);
                return json;

            } catch(Exception) {

                return null;

            }

        }

    }

}
