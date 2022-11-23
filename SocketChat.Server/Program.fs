open SocketChat.Base.Server


[<EntryPoint>]
let main _ =
    let address = System.Net.IPAddress.Parse("127.0.0.1")
    let server = Server(address, 8080)
    server.Run()
    0
