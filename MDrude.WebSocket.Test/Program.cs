using MDrude.WebSocket.Common;
using MDrude.WebSocket.Utils;
using System;

namespace MDrude.WebSocket.Test {

    class Program {

        static void Main(string[] args) {

            Logger.AddDefaultConsoleLogging();

            WebSocketServer server = new WebSocketServer(27789, null);

            server.OnHandshake += (e, args) => {

                Logger.Write("INFO", $"New handshake event {args.User.UID}");

            };

            server.OnMessage += (e, args) => {

                Logger.Write("INFO", $"New message with code {args.Message.Opcode} and length {args.Message.Data.Length}");

            };

            server.Start();

            Console.ReadLine();

        }

    }

}
