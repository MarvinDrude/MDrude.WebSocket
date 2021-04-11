using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MDrude.WebSocket.Utils {

    public static class WebSocketUtils {

        public static bool IsClientConnected(WebSocketUser client) {

            try {

                return !(client.Socket.Poll(1, SelectMode.SelectRead) && client.Socket.Available == 0);

            } catch (Exception) {

                return false;

            }

        }

        public static IPAddress CheckAddress(string address) {

            IPAddress ip;

            if (!IPAddress.TryParse(address, out ip)) {

                ip = ResolveDNS(address);

            }

            return ip;

        }

        public static IPAddress ResolveDNS(string address) {

            var addresses = Dns.GetHostAddressesAsync(address).Result;

            if (addresses.Length == 0) {

                return null;

            }

            return addresses[0];

        }

    }

}
