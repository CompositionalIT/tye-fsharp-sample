open Saturn
open Giraffe
open System

let summaries = [ "Freezing"; "Bracing"; "Chilly"; "Cool"; "Mild"; "Warm"; "Balmy"; "Hot"; "Sweltering"; "Scorching" ]
let rng = Random()
let getForecast() =
    [ for index in 1. .. 5. ->
        let temperature = rng.Next (-20, 55) |> float
        {| Date = DateTime.UtcNow.AddDays index
           TemperatureC = int temperature
           TemperatureF = (32. + (temperature / 0.5556)) |> int
           Summary = summaries.[rng.Next summaries.Length] |}
    ]

let app = application {
    use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
    use_router (route "/weatherForecast" >=> GET >=> warbler (fun _ -> json (getForecast())) )
}

run app