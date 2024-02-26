namespace FStock.Analysis.V1.Tools

open System
open FSVG
open FStock.Analysis.V1.Core.Persistence
open FStock.Analysis.V1.TechnicalIndicators
open FStock.Analysis.V1.Visualizations.Charts

open Freql.Sqlite
open Microsoft.Data.Sqlite
open Microsoft.FSharp.Core

module ChartGenerator =

    [<RequireQualifiedAccess>]
    type ChartType =
        | Price of PriceChartSettings
        | Volume of VolumeChartSettings
        | Macd of MacdChartSettings
        | Rsi of RsiChartSettings

    and PriceChartSettings =
        { Height: float
          ShortMovingAverageTableName: string
          LongMovingAverageTableName: string }

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

    let fetchStockData
        (storeCtx: SqliteContext)
        (tableName: string)
        (symbol: string)
        (endDate: DateTime)
        (entryCount: int)
        =

        let sql, parameters =
            $"""SELECT * FROM {tableName} WHERE symbol = @0 AND DATE(entry_date) <= DATE(@1) ORDER BY DATE(entry_date) DESC LIMIT @2""",
            [ box symbol; box endDate; box entryCount ]

        Operations.selectStockEntryRecords storeCtx [ sql ] parameters

    let fetchSimpleMovingAverageData
        (storeCtx: SqliteContext)
        (tableName: string)
        (symbol: string)
        (endDate: DateTime)
        (entryCount: int)
        =
        let sql, parameters =
            $"""SELECT * FROM {tableName} WHERE symbol = @0 AND DATE(entry_date) <= DATE(@1) ORDER BY DATE(entry_date) DESC LIMIT @2""",
            [ box symbol; box endDate; box entryCount ]

        storeCtx.SelectAnon<SimpleMovingAverage.SmaItem>(sql, parameters)
        |> List.sortBy (fun sma -> sma.EntryDate)

    let fetchRsiData
        (storeCtx: SqliteContext)
        (tableName: string)
        (symbol: string)
        (endDate: DateTime)
        (entryCount: int)
        =

        let sql, parameters =
            $"""SELECT * FROM {tableName} WHERE symbol = @0 AND DATE(entry_date) <= DATE(@1) ORDER BY DATE(entry_date) DESC LIMIT @2""",
            [ box symbol; box endDate; box entryCount ]

        storeCtx.SelectAnon<RelativeStrengthIndex.RsiItem>(sql, parameters)
        |> List.sortBy (fun rsi -> rsi.EntryDate)

    let fetchMacdData
        (storeCtx: SqliteContext)
        (tableName: string)
        (symbol: string)
        (endDate: DateTime)
        (entryCount: int)
        =

        let sql, parameters =
            $"""SELECT * FROM {tableName} WHERE symbol = @0 AND DATE(entry_date) <= DATE(@1) ORDER BY DATE(entry_date) DESC LIMIT @2""",
            [ box symbol; box endDate; box entryCount ]

        storeCtx.SelectAnon<MovingAverageConvergenceDivergence.MacdItem>(sql, parameters)
        |> List.sortBy (fun ma -> ma.EntryDate)


    let generatePriceChart (settings: GeneratorSettings) (chartSettings: PriceChartSettings) (state: GeneratorState) =
        
        let cs = ({
            MinimumX = 0.
            MaximumX = failwith "todo"
            MinimumY = failwith "todo"
            MaximumY = failwith "todo"
            LeftYAxis = true
            RightYAxis = true
            XAxisStartOverride = failwith "todo"
            XAxisEndOverride = failwith "todo"
            AxisStyle = failwith "todo" 
        }: MacdChart.ChartSettings)
        
        MacdChart.create
        
        
        ()


    let generate (settings: GeneratorSettings) =
        let initState = GeneratorState.Create(yStart = settings.Settings.TopPadding)

        settings.Parts |> List.fold (fun state ct ->
            match ct with
            | ChartType.Price priceChartSettings -> failwith "todo"
            | ChartType.Volume volumeChartSettings -> failwith "todo"
            | ChartType.Macd macdChartSettings -> failwith "todo"
            | ChartType.Rsi rsiChartSettings -> failwith "todo"
            
            state) initState




        ()

    let run () = ()



    ()
