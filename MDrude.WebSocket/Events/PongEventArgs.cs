using System;
using System.Collections.Generic;
using System.Text;

namespace MDrude.WebSocket.Events {

    public class PongEventArgs : EventArgs {

        public byte[] Payload { get; private set; }

        public PongEventArgs(byte[] payload) {

            Payload = payload;

        }

    }

}
