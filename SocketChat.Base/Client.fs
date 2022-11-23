module SocketChat.Base.Client


open SocketChat.Base.Commands

open System.Net
open System.Net.Sockets

type Event =
    | MessageReceived of string
    | Disconnected


type Client (address, port) =


    member val private ServerAddress: IPAddress = address with get
    member val private ServerPort: int = port with get
    member val private Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) with get, set

    member val private Subscribers = System.Collections.Generic.List<Event -> Async<unit>>()

    member client.SendCommand (command: Command) =
        client.Socket.SendCommand command |> Async.Start

    member this.Subscribe(subscriber: Event -> Async<unit>) =
        this.Subscribers.Add subscriber

    member client.Run() =

        let rec updateLoop () =
            async {
                let! message = client.Socket.ReceiveMessage()
                for subscriber in client.Subscribers do
                    do! subscriber (MessageReceived message)
                return! updateLoop()
            }

        try
            client.Socket.Connect(client.ServerAddress, client.ServerPort)
            updateLoop() |> Async.Start
            true
        with
            | :? SocketException -> false




