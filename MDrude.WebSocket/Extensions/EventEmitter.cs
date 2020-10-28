using MDrude.WebSocket.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MDrude.WebSocket.Extensions {

    public class EventEmitter {

        private ConcurrentDictionary<string, EventEntry> Events { get; set; }

        public EventEmitter() {

            Events = new ConcurrentDictionary<string, EventEntry>();

        }

        public async Task<bool> Emit(string uid, string data) {

            EventEntry entry;

            if(Events.TryGetValue(uid, out entry)) {

                foreach(var func in entry.Listeners) {

                    bool err = false;

                    try {

                        DateTime start = DateTime.Now;
                        object ob = JsonConvert.DeserializeObject(data, func.Type);

                        TimeSpan took = DateTime.Now - start;

                        Logger.DebugWrite("INFO", $"JsonWebSocketServer Type {func.Type.Name} took {took.TotalMilliseconds}ms to parse in EventEmitter.");

                        if(ob != null) {

                            await func.Function(ob);

                        } else {

                            err = true;

                        }

                    } catch(Exception) {

                        err = true;
                           
                    }

                    if(err)
                        Logger.DebugWrite("INFO", $"Error parsing type to type of event listener.");

                }

                return true;

            }

            return false;

        }

        public bool On<T>(string uid, Func<object, Task> listener) {

            EventEntry entry;

            if(!Events.TryGetValue(uid, out entry)) { 

                entry = new EventEntry(uid);

            }

            entry.AddListener<T>(listener);

            return Events.TryAdd(uid, entry);

        }

        public bool Remove(string uid, Func<object, Task> listener) {

            EventEntry entry;

            if(Events.TryGetValue(uid, out entry)) {

                bool removed = entry.RemoveListener(listener);

                if (entry.Listeners.Count == 0) {
                    Remove(uid);
                }

                return removed;

            }

            return false;

        }

        public bool Remove(string uid) {

            EventEntry entry;
            return Events.TryRemove(uid, out entry);

        }

    }

}
