open SocketChat.CLIClient.CommandLineClient
open SocketChat.Base
open Terminal.Gui.Elmish


[<EntryPoint>]
let main argv =
    #if DEBUG
    Logging.ConfigureLogLevel Logging.Debug
    Logging.ConfigureWriter  (System.IO.File.CreateText "log.txt")
    #endif

    let client = CommandLineClient()
    client.Run()
    0
