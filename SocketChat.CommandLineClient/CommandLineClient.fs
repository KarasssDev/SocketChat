module SocketChat.Client.CommandLineClient

open SocketChat.Base
open SocketChat.Base.BaseClient

type CommandLineClient(address, port) =
    inherit BaseClient(address, port)

    override this.EventHandler(event) =
        let res =
            match event with
            | ConnectionFailed -> "Connection failed"
            | ConnectionSuccess -> "Connection success"
            | MessageReceived msg -> msg
            | Disconnected -> "Disconnected"
        printfn $"%s{res}"
        async { return () }

    member this.Run() =
        (this :> BaseClient).Run()
        printf "Enter your name: "
        let name = System.Console.ReadLine()
        Commands.Connect name |> (this :> BaseClient).SendCommand
        while true do
            let msg = System.Console.ReadLine()
            Commands.Send(name, msg) |> (this :> BaseClient).SendCommand
