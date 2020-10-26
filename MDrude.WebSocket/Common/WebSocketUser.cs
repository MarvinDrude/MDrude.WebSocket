using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Common {

    public class WebSocketUser {

        public string UID { get; set; }

        public Socket Socket { get; set; }

        public Stream Stream { get; set; }

        public Task ListenTask { get; set; }

        public CancellationTokenSource ListenToken { get; set; }

        public WebSocketServer Server { get; set; }

        public WebSocketWriter Writer { get; set; }

    }

}
