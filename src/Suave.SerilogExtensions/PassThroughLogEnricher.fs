namespace Suave.SerilogExtensions

open System.Diagnostics
open Suave
open Serilog.Core
open Serilog.Events

type PassThroughLogEnricher(context: HttpContext) = 
    interface ILogEventEnricher with 
        member this.Enrich(logEvent: LogEvent, _: ILogEventPropertyFactory) = 
            let anyOf xs = fun x -> List.exists ((=) x) xs 

            match Dictionary.tryFind "Stopwatch" context.userState with
            | None -> failwith "No Stopwatch found"
            | Some v -> 
                v |> unbox<Stopwatch> 
                |> fun stopwatch -> 
                    stopwatch.Stop()
                    stopwatch.ElapsedMilliseconds
                    |> int
                    |> Enrichers.eventProperty "Duration"
                    |> logEvent.AddOrUpdateProperty

                match Dictionary.tryFind "RequestId" context.userState with
                | None -> failwith "No RequestId found"
                | Some v -> 
                    v |> unbox<string> 
                    |> Enrichers.eventProperty "RequestId"
                    |> logEvent.AddOrUpdateProperty            