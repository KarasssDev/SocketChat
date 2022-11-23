open SocketChat.Client.CommandLineClient

[<EntryPoint>]
let main _ =
    let address = System.Net.IPAddress.Parse("127.0.0.1")
    let client = CommandLineClient(address, 8080)
    client.Run()
    0
