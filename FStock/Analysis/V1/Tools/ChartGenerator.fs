namespace FStock.Analysis.V1.Tools

open FSVG
open FStock.Analysis.V1.Visualizations.Charts
open Microsoft.FSharp.Core

module ChartGenerator =

    [<RequireQualifiedAccess>]
    type ChartType =
        | Price of PriceChartSettings
        | Volume of VolumeChartSettings
        | Macd of MacdChartSettings
        | Rsi of RsiChartSettings

    and PriceChartSettings = { Height: float }

    and VolumeChartSettings = { Height: float }

    and MacdChartSettings = { Height: float }

    and RsiChartSettings = { Height: float }

    and GeneralSettings =
        { Width: float
          LeftPadding: float
          RightPadding: float
          TopPadding: float
          BottomPadding: float }

    and GeneratorSettings =
        { Settings: GeneralSettings
          Parts: ChartType list }

    type GeneratorState =
        { CurrentY: float
          Elements: Element list }

        static member Create(?yStart: float, ?elements: Element list) =
            { CurrentY = yStart |> Option.defaultValue 0.
              Elements = elements |> Option.defaultValue [] }

        member gs.Update(currentY: float, elements: Element list) =
            { gs with
                CurrentY = currentY
                Elements = gs.Elements @ elements }


    let generate (settings: GeneratorSettings) =
        let initState = GeneratorState.Create(yStart = settings.Settings.TopPadding)

        settings.Parts
        |> List.fold (fun state ct ->
            state) initState




        ()

    let run () = ()



    ()
