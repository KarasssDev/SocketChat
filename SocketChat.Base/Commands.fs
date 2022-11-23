module SocketChat.Base.Commands

open System.Net.Sockets


exception UnexpectedCommand of string

let packString (s: string) =
    let bytes = System.Text.Encoding.UTF8.GetBytes s
    let length = bytes.Length |> byte
    assert (length < System.Byte.MaxValue)
    Array.append [|length|] bytes

let receiveString (socket: Socket) =
    async {
        let sizeArr = Array.zeroCreate<byte> 1
        let! _ = socket.ReceiveAsync(sizeArr, SocketFlags.None) |> Async.AwaitTask
        let buffer = Array.zeroCreate<byte> (int sizeArr.[0])
        let! _ = socket.ReceiveAsync(buffer, SocketFlags.None) |> Async.AwaitTask
        return buffer
    }

let private delimiter = '@'

type Command =
    | Disconnect of string
    | Connect of string
    | Send of string * string
    with
    static member toBytes =
        function
        | Disconnect username -> packString $"DISCONNECT{delimiter}%s{username}"
        | Connect username -> packString $"CONNECT{delimiter}%s{username}"
        | Send (username, message) -> packString $"SEND{delimiter}%s{username}{delimiter}%s{message}"
    static member fromBytes (bytes: byte []) =
        let str = System.Text.Encoding.UTF8.GetString bytes
        str
        |> fun s -> s.Split delimiter
        |> function
            | [| "DISCONNECT"; username |] -> Disconnect username
            | [| "CONNECT"; username |] -> Connect username
            | [| "SEND"; username; message |] -> Send (username, message)
            | _ ->
                Logging.error $"Unexpected command: %s{str}"
                UnexpectedCommand(str) |> raise

type Socket with
    member this.SendCommand command =
        Logging.debug $"Sending command: {command}"
        let bytes = Command.toBytes command
        this.SendAsync(bytes, SocketFlags.None)
        |> Async.AwaitTask
        |> Async.Ignore

    member this.ReceiveCommand () =
        async {
            let! buffer = receiveString this
            let result = Command.fromBytes buffer
            Logging.debug $"Received command: %A{result}"
            return result
        }

    member this.SendMessage (s: string) =
        Logging.debug $"Sending message: %s{s}"
        let bytes = packString s
        this.SendAsync(bytes, SocketFlags.None) |> Async.AwaitTask |> Async.Ignore

    member this.ReceiveMessage () =
        async {
            let! buffer = receiveString this
            let result = System.Text.Encoding.UTF8.GetString buffer
            Logging.debug $"Received message: %s{result}"
            return result
        }


