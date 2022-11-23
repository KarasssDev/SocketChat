module SocketChat.Base.Server

open System.Threading.Tasks
open SocketChat.Base.Commands

open System.Net
open System.Net.Sockets


type Server (address, port) =

    member val private Address: IPAddress = address with get
    member val private Port: int = port with get
    member val private Clients = System.Collections.Generic.List<Socket>()

    member private server.SendToAll (message : string) =
        async {
            for client in server.Clients do
                do! client.SendString message
        }

    member private server.ExecuteCommand(socket, command) =
        printfn $"Executing command: {command}"
        match command with
        | Connect username ->
            server.Clients.Add(socket)
            server.SendToAll $"User {username} connected"
        | Disconnect username ->
            server.Clients.Remove(socket) |> ignore
            server.SendToAll $"User {username} disconnected"
        | Send (username, message) ->
            server.SendToAll $"{username}: {message}"

    member private server.ProcessClient (client : Socket) =
        let rec loop () =
            async {
                let! command = client.ReceiveCommand ()
                do! server.ExecuteCommand(client, command)
                return! loop()
            }
        loop()

    member server.Run(): unit =

        let endPoint = IPEndPoint(server.Address, server.Port)
        let socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)

        printfn $"Starting server on {endPoint.Address}:{endPoint.Port}"

        socket.Bind(endPoint)
        socket.Listen(10)

        while true do
            async {
                let! userSocket = socket.AcceptAsync() |> Async.AwaitTask
                do! server.ProcessClient userSocket
            } |> Async.Start



