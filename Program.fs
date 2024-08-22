open System
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Writers // Import this module for setMimeType
open Newtonsoft.Json

type TZInfo = {tzName: string; minDiff: float; localTime: string; utcOffset: float}

// Define the getClosest function
let getClosest () = 
    let tzs = TimeZoneInfo.GetSystemTimeZones()
    let tzList = [
        for tz in tzs do
        let localTz = TimeZoneInfo.ConvertTime(DateTime.Now, tz) 
        let fivePM = DateTime(localTz.Year, localTz.Month, localTz.Day, 17, 0, 0)
        let minDifference = (localTz - fivePM).TotalMinutes
        yield {
                tzName=tz.StandardName;
                minDiff=minDifference;
                localTime=localTz.ToString("hh:mm tt");
                utcOffset=tz.BaseUtcOffset.TotalHours;
             }
    ]
    tzList 
        |> List.filter (fun (i:TZInfo) -> i.minDiff >= 0.0) 
        |> List.sortBy (fun (i:TZInfo) -> i.minDiff) 
        |> List.head

let runWebServer argv = 
    let port = 8080
    let cfg =
          { defaultConfig with
              bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" port]}
    let app =
          choose
            [ GET >=> choose
                [ 
                    path "/" >=> request (fun _ -> OK <| JsonConvert.SerializeObject(getClosest()))
                                >=> setMimeType "application/json; charset=utf-8"
                ]
            ]
    startWebServer cfg app

[<EntryPoint>]
let main argv = 
    printfn "%s" <| JsonConvert.SerializeObject(getClosest())
    runWebServer argv
    0