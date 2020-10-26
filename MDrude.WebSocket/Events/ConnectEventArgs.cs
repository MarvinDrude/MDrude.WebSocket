using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Events {

    public class ConnectEventArgs : EventArgs {

        public WebSocketUser User { get; private set; }

        public ConnectEventArgs(WebSocketUser user) {

            User = user;

        }

    }

}
