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

        public bool Disconnected { get; set; } = false;

        public RTT RTT { get; private set; } = new RTT();

        public Meta Meta { get; private set; } = new Meta();

        public void Disconnect() {

            Server.RemoveClient(this, WebSocketDisconnection.Disconnect);

        }

    }

    public class Meta {

        public string UserAgent { get; set; } = "None";

        public LastActions LastTime { get; set; } = new LastActions();

        public string Cookies { get; set; }

        public string SetCookies { get; set; }

        public string IP { get; set; }

    }

    public class LastActions {

        public DateTime Binary { get; set; } = DateTime.UtcNow;

        public DateTime Text { get; set; } = DateTime.UtcNow;

    }

    public class RTT {

        public bool Sending { get; set; } = false;

        public DateTime Sent { get; set; }

        public double Last { get; set; }

        public double Max { get; set; }

        public double Min { get; set; } = -1d;

    }

}
