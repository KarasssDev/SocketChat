module SocketChat.CLIClient.CommandLineClient

open SocketChat.Base
open SocketChat.Base.Client

open System
type CommandLineClient() =

    member val private Username = "" with get, set
    member val private Client = Unchecked.defaultof<Client> with get, set
    member val private Name = "[SocketChat]" with get
    member val private MaxMessageLength = 100 with get

    member private this.Write message = Console.Write $"{this.Name} {message}"
    member private this.WriteLine message = Console.WriteLine $"{this.Name} {message}"
    member private this.GetAddress() =
        this.Write "Enter server address: "
        let addressS = Console.ReadLine()
        try
            Net.IPAddress.Parse(addressS)
        with
            | :? FormatException ->
                this.WriteLine "Incorrect address, try again"
                this.GetAddress()

    member private this.GetPort() =
        this.Write "Enter server port: "
        let portS = Console.ReadLine()
        try
            Int32.Parse portS
        with
            | :? FormatException
            | :? OverflowException ->
                this.WriteLine "Incorrect port, try again"
                this.GetPort()

    member private this.GetUsername() =
        this.Write "Enter username: "
        Console.ReadLine()

    member private this.Connect() =
        try
            let address = this.GetAddress()
            let port = this.GetPort()
            this.Client <- Client(address, port)
        with
            | :? Net.Sockets.SocketException ->
                this.WriteLine "Connection failed, try again"
                this.Connect()

    member private this.HandleClientEvent(event) =
        async {
            match event with
            | Event.MessageReceived message -> Console.WriteLine message
            | Event.Disconnected ->
                this.WriteLine "Disconnected"
                this.Connect()
        }

    member private this.Subscribe() =
        Console.CancelKeyPress.Add (fun _ -> this.Client.SendCommand <| Commands.Disconnect this.Username)
        this.Client.SendCommand <| Commands.Connect this.Username
        this.Client.Subscribe this.HandleClientEvent

    member private this.MessageLoop() =
        let message = Console.ReadLine()
        if message = "" then this.MessageLoop()
        else if message.Length > this.MaxMessageLength then
            this.WriteLine "Message is too long"
        else
            try
                this.Client.SendCommand <| Commands.Send(this.Username, message)
                this.MessageLoop()
            with
                | :? Net.Sockets.SocketException ->
                    this.WriteLine "Connection lost"
                    this.Connect()

    member this.ConnectionLoop() =
        this.Connect()
        let isBaseClientRan = this.Client.Run()
        if isBaseClientRan then
            this.Subscribe()
        else
            this.WriteLine "Connection failed, try again"
            this.ConnectionLoop()

    member this.Run() =
        this.Username <- this.GetUsername()
        this.ConnectionLoop()
        this.MessageLoop()

