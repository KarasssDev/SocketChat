open SocketChat.Base.Server


[<EntryPoint>]
let main _ =
    #if DEBUG
    SocketChat.Base.Logging.ConfigureLogLevel SocketChat.Base.Logging.Debug
    #endif

    let address = System.Net.IPAddress.Parse("127.0.0.1")
    let server = Server(address, 8080)
    server.Run()
    0
