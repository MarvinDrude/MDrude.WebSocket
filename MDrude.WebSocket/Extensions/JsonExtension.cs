using MDrude.WebSocket.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Extensions {

    public static class JsonExtension {

        public static async Task Send<T>(this WebSocketWriter self, string uid, T ob) {

            string text = JsonConvert.SerializeObject(ob);

            JsonMessage message = new JsonMessage() {
                UID = uid,
                Data = text
            };

            await self.WriteText(JsonConvert.SerializeObject(message));

        }

    }

}
