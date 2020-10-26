using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Events {

    public class HandshakeEventArgs : EventArgs {

        public WebSocketUser User { get; private set; }

        public HandshakeEventArgs(WebSocketUser user) {

            User = user;

        }

    }

}