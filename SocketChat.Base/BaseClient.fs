module SocketChat.Base.BaseClient

open Microsoft.FSharp.Control
open SocketChat.Base.Commands

open System.Net
open System.Net.Sockets

type Events =
    | MessageReceived of string
    | ConnectionSuccess
    | ConnectionFailed
    | Disconnected

[<AbstractClass>]
type BaseClient (address, port) =


    member val private ServerAddress: IPAddress = address with get
    member val private ServerPort: int = port with get
    member val private Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) with get, set
    member val private CommandBuffer = System.Collections.Generic.Queue<Command>()

    abstract member EventHandler: Events -> Async<unit>

    member client.SendCommand (command: Command) =
        client.Socket.SendCommand command |> Async.Start

    member client.Run() =

        let rec updateLoop () =
            async {
                let! message = client.Socket.ReceiveMessage()
                do! client.EventHandler (MessageReceived message)
                return! updateLoop()
            }

        try
            client.Socket.Connect(client.ServerAddress, client.ServerPort)
            updateLoop() |> Async.Start
        with
            | :? SocketException ->
                client.EventHandler(ConnectionFailed) |> Async.RunSynchronously




