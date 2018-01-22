namespace WillMyMps.Work.Backend.Logger
#nowarn "64"
open WillMyMps.Work.Shared.Logging

module Console =

    let private write conv msg  =
        let prefix =
            match msg |> severity with
            | LogSeverity.Debug -> "Debug"
            | LogSeverity.Info -> "Info"
            | LogSeverity.Warning -> "Warning"
            | LogSeverity.Error -> "Error"
        let instant = (msg |> time)
        printfn "[%s] - %s: %s" (instant.ToLongDateString()) prefix (msg |> payload |> conv)


    type ConsoleLogger = ConsoleLogger with

        static member Write (ConsoleLogger, msg:LogMsg<string> ) = write id msg

        static member Write (ConsoleLogger, msg:LogMsg<int> ) = write (sprintf "%d") msg

    let inline logConsole (x:'N) : unit = ((^ConsoleLogger or ^N) : (static member Write: ^ConsoleLogger * ^N -> _) ConsoleLogger, x)
