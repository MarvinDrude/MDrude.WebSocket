using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Utils {

    public class HttpUtils {

        public static async Task WriteHeader(Stream stream, string content) {

            byte[] buffer = Encoding.UTF8.GetBytes(content);
            await stream.WriteAsync(buffer, 0, buffer.Length);

        }

        public static async Task<string> ReadHeader(Stream stream) {

            int len = 32768;
            int read = 0;
            byte[] buffer = new byte[len];

            read = await stream.ReadAsync(buffer, 0, buffer.Length);

            string header = Encoding.UTF8.GetString(buffer);

            if (header.Contains("\r\n\r\n")) {

                return header;

            }

            return null;

        }

    }

}
