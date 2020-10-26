using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Events {

    public class MessageEventArgs : EventArgs {

        public WebSocketUser User { get; private set; }

        public WebSocketFrame Message { get; private set; }

        public MessageEventArgs(WebSocketUser user, WebSocketFrame message) {

            User = user;
            Message = message;

        }

    }

}
