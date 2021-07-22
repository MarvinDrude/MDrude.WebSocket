using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MDrude.WebSocket.Utils {

    public static class RandomGen {

        public static string RandomUIDPool { get; set; } = "abcdefghijklmnopqestuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string CreateBase64Key() {

            var src = new byte[16];
            RandomNumberGenerator.Fill(src);

            return Convert.ToBase64String(src);

        }

        public static string GenRandomUID<T>(IDictionary<string, T> dict, uint len) {

            string result = null;

            do {

                result = "";

                for (int e = (int)len - 1; e >= 0; e--) {

                    result += RandomUIDPool[RandomNumberGenerator.GetInt32(0, RandomUIDPool.Length)];
                    
                }

            } while (dict.ContainsKey(result));

            return result;

        }

    }
}
