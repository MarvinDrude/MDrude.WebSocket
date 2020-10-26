using MDrude.WebSocket.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Common {

    public class WebSocketReader {

        public byte[] Buffer { get; private set; }

        public WebSocketReader() {

            Buffer = new byte[98304];

        }

        public async Task<WebSocketFrame> Read(Stream stream, WebSocketUser user) {

            byte first;

            try {

                byte[] firstData = new byte[1];
                await stream.ReadAsync(firstData, 0, firstData.Length);

                first = firstData[0];

            } catch (Exception e) {

                return null;

            }

            if (!WebSocketUtils.IsClientConnected(user)) {

                return null;

            }

            try {

                byte bitFinFlag = 0x80;
                byte opcodeFlag = 0x0F;

                bool bitFinSet = (first & bitFinFlag) == bitFinFlag;
                WebSocketOpcode opcode = (WebSocketOpcode)(first & opcodeFlag);

                byte bitMaskFlag = 0x80;

                byte[] secData = new byte[1];
                await stream.ReadAsync(secData, 0, secData.Length);

                byte second = secData[0];

                bool bitMaskSet = (second & bitMaskFlag) == bitMaskFlag;
                uint length = await ReadLength(stream, second);

                if (length != 0) {

                    byte[] decoded;

                    if (bitMaskSet) {

                        byte[] key = await WebSocketReaderWriter.Read(stream, 4);
                        byte[] encoded = await WebSocketReaderWriter.Read(stream, length);

                        decoded = new byte[length];

                        for (int i = 0; i < encoded.Length; i++) {

                            decoded[i] = (byte)(encoded[i] ^ key[i % 4]);

                        }

                    } else {

                        decoded = await WebSocketReaderWriter.Read(stream, length);

                    }

                    WebSocketFrame frame = new WebSocketFrame(opcode, decoded);

                    return frame;

                }

            }
            catch(Exception er) 
            {
                return null;
            }

            return null;

        }

        public async Task<uint> ReadLength(Stream stream, byte second) {

            byte dataLengthFlag = 0x7F;
            uint length = (uint)(second & dataLengthFlag);

            if (length == 126) {

                length = await WebSocketReaderWriter.ReadUShort(stream, false);

            } else if (length == 127) {

                length = (uint) await WebSocketReaderWriter.ReadULong(stream, false);

                //Max 500MB
                if (length < 0 || length > 536870912) {

                    return 0;

                }

            }

            return length;

        }

    }

}
