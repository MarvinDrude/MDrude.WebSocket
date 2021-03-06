﻿using MDrude.WebSocket.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Extensions {

    public class EEntry {

        public Type Type { get; set; }

        public Func<object, Task> Function { get; set; }

    }

    public class EEntryServer {

        public Type Type { get; set; }

        public Func<object, WebSocketUser, Task> Function { get; set; }

    }

    public class EventEntry {

        public string UID { get; private set; }

        public List<EEntry> Listeners { get; private set; }

        public EventEntry(string uid) {

            UID = uid;
            Listeners = new List<EEntry>();

        }

        public void AddListener<T>(Func<object, Task> listener) {

            Listeners.Add(new EEntry() {
                Type = typeof(T),
                Function = listener
            });

        }

        public bool RemoveListener(Func<object, Task> listener) {

            bool found = false;

            for(int e = Listeners.Count - 1; e >= 0; e--) {

                EEntry curr = Listeners[e];

                if(curr.Function == listener) {
                    Listeners.RemoveAt(e);
                    found = true;
                }

            }

            return found;

        }

        public void Clear() {

            Listeners.Clear();

        }

    }

    public class EventEntryServer {

        public string UID { get; private set; }

        public List<EEntryServer> Listeners { get; private set; }

        public EventEntryServer(string uid) {

            UID = uid;
            Listeners = new List<EEntryServer>();

        }

        public void AddListener<T>(Func<object, WebSocketUser, Task> listener) {

            Listeners.Add(new EEntryServer() {
                Type = typeof(T),
                Function = listener
            });

        }

        public bool RemoveListener(Func<object, WebSocketUser, Task> listener) {

            bool found = false;

            for (int e = Listeners.Count - 1; e >= 0; e--) {

                EEntryServer curr = Listeners[e];

                if (curr.Function == listener) {
                    Listeners.RemoveAt(e);
                    found = true;
                }

            }

            return found;

        }

        public void Clear() {

            Listeners.Clear();

        }

    }

}
