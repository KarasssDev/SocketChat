module SocketChat.Base.Server

open SocketChat.Base.Commands
open SocketChat.Base

open System.Net
open System.Net.Sockets


type Server (address, port) =

    member val private Address: IPAddress = address with get
    member val private Port: int = port with get
    member val private Clients = System.Collections.Generic.List<Socket>()

    member private server.SendToAll (message : string) (exclude: Socket option) =
        match exclude with
        | Some socket ->
            async {
                for client in server.Clients do
                    if client <> socket then
                        do! client.SendMessage message
            }
        | None ->
            async {
                for client in server.Clients do
                    do! client.SendMessage message
            }

    member private server.ExecuteCommand(socket, command) =
        Logging.debug $"Executing command: {command}"
        match command with
        | Connect username ->
            server.Clients.Add(socket)
            server.SendToAll $"User {username} connected" None
        | Disconnect username ->
            server.Clients.Remove(socket) |> ignore
            server.SendToAll $"User {username} disconnected" None
        | Send (username, message) ->
            server.SendToAll $"{username}: {message}" (Some socket)

    member private server.ProcessClient (client : Socket) =
        let rec loop () =
            async {
                try
                    let! command = client.ReceiveCommand ()
                    Logging.debug $"Received command: {command}"
                    do! server.ExecuteCommand(client, command)
                    return! loop()
                with
                | e ->  Logging.error $"Error: {e.Message}"
            }
        loop()

    member server.Run(): unit =

        let endPoint = IPEndPoint(server.Address, server.Port)
        let socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)

        Logging.info $"Starting server on {endPoint.Address}:{endPoint.Port}"

        socket.Bind(endPoint)
        socket.Listen(10)

        while true do
            let client = socket.AcceptAsync() |> Async.AwaitTask |> Async.RunSynchronously
            Logging.info $"Client connected: {client.RemoteEndPoint}"
            server.ProcessClient(client) |> Async.Start



