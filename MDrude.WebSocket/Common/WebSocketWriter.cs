using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Common {

    public class WebSocketWriter {

        public WebSocketUser User { get; private set; }

        public Stream Stream { get; private set; }

        public Encoding Encoding { get; private set; }

        public WebSocketWriter(WebSocketUser user) {

            User = user;
            Stream = user.Stream;

        }

        public WebSocketWriter(Stream stream, Encoding encoding) {

            Stream = stream;
            Encoding = encoding;

        }

        public async Task WritePing() {

            await Write(WebSocketOpcode.PingFrame, new byte[] { 1, 2, 1, 2 });

        }

        public async Task WritePong(WebSocketFrame ping) {

            await Write(WebSocketOpcode.PongFrame, ping.Data);

        }

        public async Task WriteCustomPing() {

            await Write(WebSocketOpcode.BinaryFrame, new byte[] { 22, 23, 24, 25 });

        }

        public async Task WriteCustomPong() {

            await Write(WebSocketOpcode.BinaryFrame, new byte[] { 26, 27, 28, 29 });

        }

        public async Task WriteText(string text) {

            byte[] data;

            if(User != null) {

                data = User.Server.TextFrameEncoding.GetBytes(text);

            } else {

                data = Encoding.GetBytes(text);

            }

            await Write(WebSocketOpcode.TextFrame, data);

        }

        public async Task Write(WebSocketOpcode opcode, byte[] data) {

            using (MemoryStream ms = new MemoryStream()) {

                try {

                    byte bitFin = 0x80;
                    byte first = (byte)(bitFin | (byte)opcode);

                    byte[] firstData = new byte[] { first };
                    await ms.WriteAsync(firstData, 0, firstData.Length);

                    if (data.Length <= 125) {

                        byte[] secData = new byte[] { (byte)data.Length };
                        await ms.WriteAsync(secData, 0, secData.Length);

                    } else if (data.Length <= 65535) {

                        byte[] secData = new byte[] { 126 };
                        await ms.WriteAsync(secData, 0, secData.Length);

                        await WebSocketReaderWriter.WriteNumber(ms, (ushort)data.Length, false);

                    } else {

                        byte[] secData = new byte[] { 127 };
                        await ms.WriteAsync(secData, 0, secData.Length);

                        await WebSocketReaderWriter.WriteNumber(ms, (ulong)data.Length, false);

                    }

                    await ms.WriteAsync(data, 0, data.Length);

                    byte[] buffer = ms.ToArray();
                    await Stream.WriteAsync(buffer, 0, buffer.Length);

                } catch(Exception er) {



                }

            }

        }

    }

}
