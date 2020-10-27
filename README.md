# MDrude.WebSocket - lightweight WebSocket Server
This is a library in order to have a very lightweight C# WebSocket Server in .NET Core with great possibilities to build upon. It uses TLP and async to maximise performance and is based upon RFC 6455 which ensures compatibility. If you have any questions, bugs or suggestions to make the library even better feel free to let me know. Thanks in advance. It also supports secure connections via certificate.

## Supported Browsers
Due to implementation according to RFC 6455, the following browsers (and higher) should work. The library also allows you to use a certificate in order for secure connections to work.
* Internet Explorer 10 and hgiher
* Microsoft Edge
* Mozilla Firefox Version 4 and higher
* Opera Version 10.70 and higher
* All WebKit Browsers (Chrome, Safari, etc.)

## Getting Started
Very simple Example in which we listen for handshake of the user and then send him an example text and an example binary.
```C#
static void Main(string[] args) {

    WebSocketServer server = new WebSocketServer(27789, null);

    server.OnHandshake += async (e, args) => {

        Logger.Write("INFO", $"New handshake event {args.User.UID}");

        await args.User.Writer.WriteText("Hallo");

        await args.User.Writer.Write(WebSocketOpcode.BinaryFrame, new byte[] { 1, 2, 3, 4 });

    };

    server.OnMessage += async (e, args) => {

        Logger.Write("INFO", $"New message with code {args.Message.Opcode} and length {args.Message.Data.Length} {Encoding.UTF8.GetString(args.Message.Data)}");

    };

    server.Start();

    Console.ReadLine();

}
```

## Methods
### WebSocketServer.Broadcast
Can be used to send a message Binary/Text to all connected clients.
```C#
await server.Broadcast("Hello!");
await server.Broadcast(WebSocketOpcode.BinaryFrame, new byte[] { 1, 2, 3, 4 });
```

### WebSocketServer.Start
This starts the WebSocket Server with current settings and listens for new connections.

### WebSocketServer.Stop
This stops the WebSocket Server and disconnects all clients currently connected.

### WebSocketUser.Writer.Write
Send messages to the client. Example can be seen in "Getting Started"

## Events
### OnConnect
This is called even before the handshake update and should only be used if you know what you want to do with it. At this point you can't yet communicate with the user.
Argument contains the new User Object.
```C#
server.OnConnect += async (e, args) => {

    Logger.Write("INFO", $"New connect event {args.User.UID}");

};
```

### OnHandshake
This is called when all handshaking is done and the client is free to be communicated with.
Argument contains the User Object.
```C#
server.OnHandshake += async (e, args) => {

    Logger.Write("INFO", $"New handshake event {args.User.UID}");

};
```

### OnMessage
This is called when a new message arrived Text/Binary.
Arguments contain User and the Message/Frame
```C#
server.OnMessage += async (e, args) => {

    Logger.Write("INFO", $"New message with code {args.Message.Opcode} and length {args.Message.Data.Length} {Encoding.UTF8.GetString(args.Message.Data)}");

};
```

### OnDisconnect
This is called when a user disconnects.
Arguments contain User and Reason.
```C#
server.OnDisconnect += async (e, args) => {

    Logger.Write("INFO", $"New disconnect event {args.Reason}");

};
```

### OnPing
This is called when a ping arrived.
Argument contains the Payload.
```C#
server.OnPing += async (e, args) => {

    Logger.Write("INFO", $"New ping event {args.Payload.Length}");

};
```

### OnPong
This is called when a pong arrived.
Argument contains the Payload.
```C#
server.OnPong += async (e, args) => {

    Logger.Write("INFO", $"New pong event {args.Payload.Length}");

};
```


