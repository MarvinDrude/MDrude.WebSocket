using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MDrude.WebSocket.Utils {

    public static class Logger {

        private static readonly object ConsoleWriteLock = new object();

        public delegate void LogEventHandler(DateTime time, string category, string message);

        public static event LogEventHandler OnLog;

        public static void Write(string category, string message) {
            OnLog?.Invoke(DateTime.Now, category, message);
        }

        public static void Write(string category, string message, Exception e) {
            Write(category, message + ": " + Regex.Replace((e.Message + ", " + e.StackTrace), @"\t|\n|\r", ""));
        }

        [Conditional("DEBUG")]
        public static void DebugWrite(string category, string message) {
            Write(category, message);
        }

        [Conditional("DEBUG")]
        public static void DebugWrite(string category, string message, Exception e) {
            Write(category, message, e);
        }

        public static void AddDefaultConsoleLogging() {

            OnLog += (time, cat, mes) => {

                lock (ConsoleWriteLock) {

                    string message = "[" + time.ToString() + "]";
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(message);

                    message = "[" + cat + "]";

                    if (cat == "SUCCESS") {
                        Console.ForegroundColor = ConsoleColor.Green;
                    } else if (cat == "FAILED") {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    } else if (cat == "INFO" || cat == "INIT") {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                    } else if (cat == "REGION") {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    Console.Write(message);

                    message = " " + mes;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(message);
                    Console.WriteLine();

                }

            };

        }

    }

}
