using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Events {

    public class DisconnectEventArgs : EventArgs {

        public WebSocketUser User { get; private set; }

        public WebSocketDisconnection Reason { get; private set; }

        public DisconnectEventArgs(WebSocketUser user, WebSocketDisconnection reason) {

            User = user;
            Reason = reason;

        }

    }

}
