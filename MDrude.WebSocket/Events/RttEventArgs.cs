using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Events {

    public class RttEventArgs : EventArgs {

        public WebSocketUser User { get; private set; }

        public RttEventArgs(WebSocketUser user) {

            User = user;

        }

    }

}
