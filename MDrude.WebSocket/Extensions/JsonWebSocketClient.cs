﻿using MDrude.WebSocket.Common;
using MDrude.WebSocket.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Extensions {

    public class JsonWebSocketClient : WebSocketClient {

        public EventEmitter Events { get; private set; }

        public JsonWebSocketClient(string address, ushort port, bool secure = false, string host = null) 
            : base(address, port,secure, host) {

            Events = new EventEmitter();

            InitMessages();

        }

        public void InitMessages() {

            OnMessage += async (sender, args) => {

                WebSocketFrame message = args.Message;

                if (message.Opcode == WebSocketOpcode.TextFrame) {

                    string jsonStr = Encoding.UTF8.GetString(message.Data);
                    JsonMessage json = GetJsonMessage(jsonStr);

                    if (json == null || string.IsNullOrEmpty(json.UID) || json.UID.Trim().Length == 0) {
                        Logger.DebugWrite("INFO", $"JsonWebSocket received malformed TextFrame");
                        return;
                    }

                    await Events.Emit(json.UID, json.Data);

                }

            };

        }

        private JsonMessage GetJsonMessage(string jsonStr) {

            try {

                JsonMessage json = JsonConvert.DeserializeObject<JsonMessage>(jsonStr);
                return json;

            } catch (Exception) {

                return null;

            }

        }

    }

}
