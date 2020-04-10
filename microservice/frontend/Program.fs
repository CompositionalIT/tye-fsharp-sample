open Saturn
open Giraffe
open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Http
open System.Net.Http
open FSharp.Control.Tasks
open Thoth.Json.Net

type Weather =
    { Date : DateTime
      TemperatureC : float
      TemperatureF : float
      Summary : string }

module View =
    open GiraffeViewEngine
    let renderTable (weather:Weather array) =
        html [ ] [
            head [ ] [
                title [] [ Text "Weather Forecast" ]
                link [ _rel "stylesheet"; _href "https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css" ]
            ]
            body [] [
                h1 [] [ Text "Weather Forecast:" ]
                table [ _class "table" ] [
                    thead [] [
                        tr [] [
                            th [] [ Text "Date" ]
                            th [] [ Text "Temp (c)" ]
                            th [] [ Text "Temp (f)" ]
                            th [] [ Text "Summary" ]
                        ]
                    ]
                    tbody [] [
                        for forecast in weather do
                            tr [] [
                                td [] [ Text (forecast.Date.ToShortDateString()) ]
                                td [] [ Text (string forecast.TemperatureC) ]
                                td [] [ Text (string forecast.TemperatureF) ]
                                td [] [ Text forecast.Summary ]
                            ]
                    ]
                ]

            ]
        ] |> htmlView

module Data =
    let getWeather (ctx:HttpContext) = task {
        let client = ctx.GetService<IHttpClientFactory>().CreateClient "WeatherClient"
        let! forecast = client.GetAsync "/weatherForecast"
        let! forecast = forecast.Content.ReadAsStringAsync()
        return Decode.Auto.unsafeFromString<Weather array> forecast }

let getWeather next ctx = task {
    let! weather = Data.getWeather ctx
    return! View.renderTable weather next ctx
}

let serviceConfig (services:IServiceCollection) =
    let config = services.BuildServiceProvider().GetService<IConfiguration>()
    services.AddHttpClient("WeatherClient", fun c -> c.BaseAddress <- config.GetServiceUri "backend")
    |> ignore
    services

let app = application {
    service_config serviceConfig
    use_router getWeather
}

run app