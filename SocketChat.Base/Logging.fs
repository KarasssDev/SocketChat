module SocketChat.Base.Logging

open System

let Error = 1
let Warning = 2
let Info = 3
let Debug = 4

let mutable current_log_level = Error
let mutable current_text_writer = Console.Out
let public ConfigureWriter writer = current_text_writer <- writer
let public ConfigureLogLevel level = current_log_level <- level
let LevelToString = function
    | 1 -> "Error"
    | 2 -> "Warning"
    | 3 -> "Info"
    | 4 -> "Debug"
    | _ -> "Unknown"

let private writeLineString vLevel message =
    let res = $"[%s{LevelToString vLevel}] [%A{DateTime.Now}] %s{message}"
    current_text_writer.WriteLine(res)
    current_text_writer.Flush()

let public printLog vLevel format =
    Printf.ksprintf (fun message -> if current_log_level >= vLevel then writeLineString vLevel message) format

let public error format = printLog Error format
let public warning format = printLog Warning format
let public info format = printLog Info format
let public debug format = printLog Debug format
