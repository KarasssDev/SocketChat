open System
open SocketChat.Base.Server


let rec readAddress () =
    Console.Write "Enter the address of the server: "
    let s = Console.ReadLine ()
    try
        System.Net.IPAddress.Parse(s)
    with
    | :? FormatException ->
        printfn "Invalid address. Please try again."
        readAddress ()

let rec readPort () =
    Console.Write "Enter the port of the server: "
    let s = Console.ReadLine ()
    try
        int s
    with
    | :? FormatException ->
        printfn "Invalid port. Please try again."
        readPort ()

let rec initServer () =
    let address = readAddress ()
    let port = readPort ()
    try
        Server(address, port)
    with
    | :? System.Net.Sockets.SocketException ->
        printfn "Could not create server. Please try again."
        initServer ()


[<EntryPoint>]
let main _ =
    #if DEBUG
    SocketChat.Base.Logging.ConfigureLogLevel SocketChat.Base.Logging.Debug
    #endif

    let server = initServer ()
    try
        server.Run()
    with
    | :? System.Net.Sockets.SocketException -> printfn "Server stopped."
    0
