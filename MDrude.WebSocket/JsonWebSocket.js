
var MD = MD || { };

(async () => {

    const EventEmitter = class EventEmitter {

        constructor() {

            this._events = { };

        }

        on(event, listener) {

            var evt = null;

            if(!this._events.hasOwnProperty(event)) {
                this._events[event] = evt = [];
            } else {
                evt = this._events[event];
            }

            evt.push(listener);

        }

        async emit(event, ob) {

            if(!this._events.hasOwnProperty(event)) {
                return;
            }

            var arr = this._events[event];

            for(var e = 0; e < arr.length; e++) {

                await arr[e](ob);

            }

        }

        remove(event, listener) {

            if(!this._events.hasOwnProperty(event)) {
                return;
            }

            var arr = this._events[event];

            for(var e = arr.length - 1; e >= 0; e--) {

                if(arr[e] == listener) {
                    arr.splice(e, 1);
                }

            }

        }

    }

    const JsonWebSocket = class JsonWebSocket extends EventEmitter {

        constructor() {

            super();

            var args = arguments[0];

            this.address = args.address;
            this.reconnect = typeof args.reconnect === "undefined" ? true : args.reconnect;
            this.interval = 2000;
            this.connected = false;

        }

        connect() {

            if(this.connected) {
                return;
            }

            this.connected = false;

            this.initSocket();

        }

        send(uid, ob) {

            var ob = JSON.stringify(ob);
            var send = JSON.stringify({ "UID": uid, "Data": ob });

            this.socket.send(send);

        }

        initSocket() {

            this.socket = new WebSocket(this.address);

            this.socket.onopen = () => {

                this.connected = true;
                this.emit("connect", { });

            };

            this.socket.onmessage = (message) => {

                if(typeof message.data !== "string") {
                    return this.emit("binary", message.data);
                }

                try {

                    var data = JSON.parse(message.data);

                    if(typeof data.UID !== "string" || data.UID.length == 0) {
                        return;
                    }

                    var ob = JSON.parse(data.Data);

                    this.emit(data.UID, ob);

                } catch (e) {

                }

            };

            this.socket.onclose = () => {

                this.connected = false;
                this.emit("disconnect", { });

                if(this.reconnect) {

                    setTimeout(() => {

                        this.connect();

                    }, this.interval);

                }

            };

            this.socket.onerror = () => { };

        }

    }

    MD.EventEmitter = EventEmitter;
    MD.JsonWebSocket = JsonWebSocket;

})();

var t = new MD.JsonWebSocket({
    "address": "ws://127.0.0.1:27789"
});

t.on("testmessage", async (ob) => {
    console.log(ob);
    t.send("testmessage", { "Name": "Marvin", "Number": 22 });
});

t.connect();