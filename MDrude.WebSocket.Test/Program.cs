using MDrude.WebSocket.Common;
using MDrude.WebSocket.Utils;
using System;
using System.Text;

namespace MDrude.WebSocket.Test {

    class Program {

        static void Main(string[] args) {

            Logger.AddDefaultConsoleLogging();

            WebSocketServer server = new WebSocketServer(27789, null);

            server.OnConnect += async (e, args) => {

                Logger.Write("INFO", $"New connect event {args.User.UID}");

            };

            server.OnPing += async (e, args) => {

                Logger.Write("INFO", $"New ping event {args.Payload.Length}");

            };

            server.OnPong += async (e, args) => {

                Logger.Write("INFO", $"New pong event {args.Payload.Length}");

            };

            server.OnHandshake += async (e, args) => {

                Logger.Write("INFO", $"New handshake event {args.User.UID}");

                await args.User.Writer.WriteText("Hallo");

                await args.User.Writer.Write(WebSocketOpcode.BinaryFrame, new byte[] { 1, 2, 3, 4 });

                await args.User.Writer.WritePing();

            };

            server.OnMessage += async (e, args) => {

                Logger.Write("INFO", $"New message with code {args.Message.Opcode} and length {args.Message.Data.Length} {Encoding.UTF8.GetString(args.Message.Data)}");

            };

            server.OnDisconnect += async (e, args) => {

                Logger.Write("INFO", $"New disconnect event {args.Reason}");

            };

            server.Start();

            Console.ReadLine();

        }

    }

}
