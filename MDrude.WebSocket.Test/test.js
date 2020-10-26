
(async () => {

    const socket = new WebSocket("ws://127.0.0.1:27789");

    socket.onopen = () => {

        console.log("open");

    };

    socket.onmessage = (message) => {

        console.log(message);

        socket.send("Hier ist auch was");

    }

    socket.onerror = (err) => {

        console.log("error");

    }

    socket.onclose = (evt) => {

        console.log("close");

    }

})();
