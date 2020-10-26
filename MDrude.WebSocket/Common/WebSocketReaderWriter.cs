using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Common {

    public static class WebSocketReaderWriter {

        public static async Task WriteNumber(Stream stream, dynamic number, bool littleEndian) {

            if (!(number is ushort ||
                number is ulong)) {

                throw new ArgumentException();

            }

            byte[] buffer = BitConverter.GetBytes(number);

            if (BitConverter.IsLittleEndian && !littleEndian) {

                Array.Reverse(buffer);

            }

            await stream.WriteAsync(buffer, 0, buffer.Length);

        }

        public static async Task<ushort> ReadUShort(Stream stream, bool littleEndian) {

            byte[] buffer = await Read(stream, 2);

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            return BitConverter.ToUInt16(buffer, 0);

        }

        public static async Task<ulong> ReadULong(Stream stream, bool littleEndian) {

            byte[] buffer = await Read(stream, 8);

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            return BitConverter.ToUInt64(buffer, 0);

        }

        public static async Task<long> ReadLong(Stream stream, bool littleEndian) {

            byte[] buffer = await Read(stream, 8);

            if (!littleEndian) {

                Array.Reverse(buffer);

            }

            return BitConverter.ToInt64(buffer, 0);

        }

        public static async Task<byte[]> Read(Stream stream, uint len) {

            byte[] buffer = new byte[len];
            int read = 0;

            read = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (read < len) {

                return null;

            }

            return buffer;

        }

    }

}
