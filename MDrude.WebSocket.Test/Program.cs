using MDrude.WebSocket.Common;
using MDrude.WebSocket.Extensions;
using MDrude.WebSocket.Utils;
using System;
using System.Text;

namespace MDrude.WebSocket.Test {

    class TestMessage {

        public string Name;
        public DateTime Time;
        public int Number;

    }

    class Program {

        static void Main(string[] args) {

            Logger.AddDefaultConsoleLogging();

            JsonWebSocketServer server = new JsonWebSocketServer(27789, null);

            server.Events.On<TestMessage>("testmessage", async (message, user) => {

                TestMessage mes = (TestMessage)message;

                Console.WriteLine(mes.Name + " " + mes.Number);

            });

            server.OnHandshake += async (sender, args) => {

                await args.User.Writer.Send("testmessage", new TestMessage() {
                    Name = "Marvin",
                    Time = DateTime.Now
                });

            };

            server.Start();

            JsonWebSocketClient client = new JsonWebSocketClient("127.0.0.1", 27789, false);

            client.OnHandshake += async (sender, args) => {

                await client.Writer.Send("testmessage", new TestMessage() {
                    Name = "Test",
                    Number = 222
                });

            };

            client.Connect();

            //WebSocketServer server = new WebSocketServer(27789, null);

            //server.OnConnect += (e, args) => {

            //    Logger.Write("INFO", $"New connect event {args.User.UID}");

            //};

            //server.OnPing += (e, args) => {

            //    Logger.Write("INFO", $"New ping event {args.Payload.Length}");

            //};

            //server.OnPong += (e, args) => {

            //    Logger.Write("INFO", $"New pong event {args.Payload.Length}");

            //};

            //server.OnHandshake += async (e, args) => {

            //    Logger.Write("INFO", $"New handshake event {args.User.UID}");

            //    await args.User.Writer.WriteText("Hallo");

            //    await args.User.Writer.Write(WebSocketOpcode.BinaryFrame, new byte[] { 1, 2, 3, 4 });

            //};

            //server.OnMessage += (e, args) => {

            //    Logger.Write("INFO", $"New message with code {args.Message.Opcode} and length {args.Message.Data.Length} {Encoding.UTF8.GetString(args.Message.Data)}");

            //};

            //server.OnDisconnect += (e, args) => {

            //    Logger.Write("INFO", $"New disconnect event {args.Reason}");

            //};

            //server.Start();

            Console.ReadLine();

        }

    }

}
