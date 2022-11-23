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

type Command =
    | Disconnect of string
    | Connect of string
    | Send of string * string
    with
    static member toBytes =
        function
        | Disconnect username -> packString $"DISCONNECT %s{username}"
        | Connect username -> packString $"CONNECT %s{username}"
        | Send (username, message) -> packString $"SEND %s{username} %s{message}"
    static member fromBytes (bytes: byte []) =
        let str = System.Text.Encoding.UTF8.GetString bytes
        str
        |> fun s -> s.Split ' '
        |> function
            | [| "DISCONNECT"; username |] -> Disconnect username
            | [| "CONNECT"; username |] -> Connect username
            | [| "SEND"; username; message |] -> Send (username, message)
            | _ -> UnexpectedCommand(str) |> raise

type Socket with
    member this.SendCommand command =
        let bytes = Command.toBytes command
        this.SendAsync(bytes, SocketFlags.None)
        |> Async.AwaitTask
        |> Async.Ignore

    member this.ReceiveCommand () =
        async {
            let! buffer = receiveString this
            return Command.fromBytes buffer
        }

    member this.SendString (s: string) =
        let bytes = packString s
        this.SendAsync(bytes, SocketFlags.None) |> Async.AwaitTask |> Async.Ignore

    member this.ReceiveMessage () =
        async {
            let! buffer = receiveString this
            return System.Text.Encoding.UTF8.GetString buffer
        }


