using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Events {

    public class PingEventArgs : EventArgs {

        public byte[] Payload { get; private set; }

        public PingEventArgs(byte[] payload) {

            Payload = payload;

        }

    }

}
