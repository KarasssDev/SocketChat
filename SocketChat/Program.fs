module SocketChat.Main

open System.Net.Sockets
// For more information see https://aka.ms/fsharp-console-apps


[<EntryPoint>]
let main _ =
    let port = 80;
    let url = "www.google.com";
    let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    try
        // пытаемся подключиться используя URL-адрес и порт
        socket.ConnectAsync(url, port).Wait()
        printfn $"Подключение к {url} установлено"
        printfn  $"Адрес подключения {socket.RemoteEndPoint}"
        printfn $"Адрес приложения {socket.LocalEndPoint}"

    with
        | e ->  printfn $"Не удалось установить подключение к {url}"

    printfn "Hello from F#"
    0
