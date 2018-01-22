module WillMyMps.Work.Shared.Logging

open System

[<RequireQualifiedAccess>]
type LogSeverity =
    | Debug
    | Info
    | Warning
    | Error

type LogMsg<'a> = LogMsg of LogSeverity * DateTime * 'a

type Logger<'a> = LogMsg<'a> -> unit


let logMsg x y = LogMsg(x, DateTime.UtcNow  ,y)

let debug y = logMsg LogSeverity.Debug y
let info y = logMsg LogSeverity.Info y
let warning y = logMsg LogSeverity.Warning y
let error y = logMsg LogSeverity.Error y

let severity msg =
    msg |> function
    | LogMsg(x, _, _) -> x

let payload msg =
    msg |> function
    | LogMsg(_, _, x) -> x

let time msg =
    msg |> function
    | LogMsg(_, x, _) -> x