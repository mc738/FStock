namespace FStock.Data

open System
open System.IO
open FStock.Data.Persistence
open Freql.Csv
open Freql.Sqlite

module Import =

    [<AutoOpen>]
    module private Utils =

        let readLines (path: string) =
            try
                File.ReadAllLines path |> Ok
            with
            | exn -> Error $"Error reading file `{path}`: {exn.Message}"

        let strToOption (str: string) =
            match System.String.IsNullOrWhiteSpace str with
            | true -> None
            | false -> Some str

        let ynToBool (v: string) =
            System.String.Equals(v, "Y", StringComparison.OrdinalIgnoreCase)

        let ynToBoolOption (v: string) = strToOption v |> Option.map ynToBool

        let collectResults (results: Result<'a, 'b> list) =
            results
            |> List.fold
                (fun (s, e) v ->
                    match v with
                    | Ok r -> s @ [ r ], e
                    | Error r -> s, e @ [ r ])
                ([], [])

    module Logging =
        
        [<RequireQualifiedAccess>]
        type LogItemType =
            | Information
            | Success
            | Error
            | Warning
        
        type Log = LogItemType -> string -> string -> unit
        
            
        let logInfo (log: Log) (from: string) (message: string) =
            log LogItemType.Information from message
            
        let logSuccess (log: Log) (from: string) (message: string) =
            log LogItemType.Success from message
            
        let logError (log: Log) (from: string) (message: string) =
            log LogItemType.Error from message
            
        let logWarning (log: Log) (from: string) (message: string) =
            log LogItemType.Warning from message

    open Logging        
        
    type Metadata =
        { NasdaqTraded: string
          Symbol: string
          SecurityName: string
          ListingExchange: string
          Category: string
          Etf: string
          RoundLotSize: decimal
          TestIssue: string
          FinancialStatus: string
          CqsSymbol: string
          NasdaqSymbol: string
          NextShares: string }

    type Entry =
        { [<CsvValueFormat("yyyy-MM-dd")>]
          Date: DateTime
          Open: Decimal
          High: Decimal
          Low: Decimal
          Close: Decimal
          AdjClose: Decimal
          Volume: Decimal }

    let initialize (log: Logging.Log) (path: string) =
        match File.Exists path with
        | true -> SqliteContext.Open path
        | false ->
            let ctx = SqliteContext.Create path

            [ Records.SymbolMetadataItem.CreateTableSql()
              Records.Etf.CreateTableSql()
              Records.Stock.CreateTableSql()
              Records.FileImport.CreateTableSql()
              Records.ImportError.CreateTableSql() ]
            |> List.map ctx.ExecuteSqlNonQuery
            |> ignore
            
            log LogItemType.Information "init" "Store created."

            ctx

    let loadMetadata (path: string) =
        CsvParser.parseFile<Metadata> true path
        |> collectResults

    let importMetadata (ctx: SqliteContext) (data: Metadata list) =
        ctx.ExecuteInTransaction (fun t ->
            data
            |> List.iter (fun md ->
                ({ Symbol = md.Symbol
                   SecurityName = md.SecurityName
                   ListingExchange = md.ListingExchange |> strToOption
                   Category = md.Category |> strToOption
                   IsEft = md.Etf |> ynToBool
                   RoundLotSize = md.RoundLotSize
                   TestIssue = md.TestIssue |> ynToBool
                   FinancialStatus = md.FinancialStatus |> strToOption
                   IsNasdaqTraded = md.NasdaqTraded |> ynToBool
                   CqsSymbol = md.CqsSymbol |> strToOption
                   NasdaqSymbol = md.NasdaqSymbol
                   NextShares = md.NextShares |> ynToBool }: Parameters.NewSymbolMetadataItem)
                |> Operations.insertSymbolMetadataItem t))

    let importStockFile (log: Log) (ctx: SqliteContext) (path: string) =
        let (entries, errors) =
            CsvParser.parseFile<Entry> true path
            |> collectResults

        ctx.ExecuteInTransaction (fun t ->
            let symbol =
                Path.GetFileNameWithoutExtension path

            entries
            |> List.iter (fun e ->

                ({ Symbol = symbol
                   EntryDate = e.Date
                   OpenValue = e.Open
                   HighValue = e.High
                   LowValue = e.Low
                   CloseValue = e.Close
                   AdjustedCloseValue = e.AdjClose
                   VolumeValue = e.Volume }: Parameters.NewStock)
                |> Operations.insertStock t)

            ({ FilePath = path
               Symbol = symbol
               Successful = errors.IsEmpty }: Parameters.NewFileImport)
            |> Operations.insertFileImport t

            if errors.IsEmpty |> not then
                errors
                |> List.iter (fun e ->
                    ({ Symbol = symbol
                       Line = e.Line
                       Message = e.Error }: Parameters.NewImportError)
                    |> Operations.insertImportError t))
          
    let importEtfFile (log: Log) (ctx: SqliteContext) (path: string) =
        let (entries, errors) =
            CsvParser.parseFile<Entry> true path
            |> collectResults

        ctx.ExecuteInTransaction (fun t ->
            let symbol =
                Path.GetFileNameWithoutExtension path

            entries
            |> List.iter (fun e ->

                ({ Symbol = symbol
                   EntryDate = e.Date
                   OpenValue = e.Open
                   HighValue = e.High
                   LowValue = e.Low
                   CloseValue = e.Close
                   AdjustedCloseValue = e.AdjClose
                   VolumeValue = e.Volume }: Parameters.NewEtf)
                |> Operations.insertEtf t)

            ({ FilePath = path
               Symbol = symbol
               Successful = errors.IsEmpty }: Parameters.NewFileImport)
            |> Operations.insertFileImport t

            if errors.IsEmpty |> not then
                errors
                |> List.iter (fun e ->
                    ({ Symbol = symbol
                       Line = e.Line
                       Message = e.Error }: Parameters.NewImportError)
                    |> Operations.insertImportError t))
        
    let importStockFiles (log: Log) (ctx: SqliteContext) (paths: string list) =
        paths
        |> List.iteri (fun i p ->
            logInfo log "import-stocks" $"Importing file {i + 1} of {paths.Length}"
            match importStockFile log ctx p with
            | Ok _ -> logSuccess log "import-stocks" "Complete"
            | Error e ->
                let symbol = Path.GetFileNameWithoutExtension p
                ({ FilePath = p
                   Symbol = symbol
                   Successful = false }: Parameters.NewFileImport)
                |> Operations.insertFileImport ctx
                logError log "import-stocks" $"{p} - {e}")
        
    
    let importEtfFiles (log: Log) (ctx: SqliteContext) (paths: string list) =
        paths
        |> List.iteri (fun i p ->
            logInfo log "import-etfs" $"Importing file {i + 1} of {paths.Length}"
            match importEtfFile log ctx p with
            | Ok _ -> logSuccess log "import-etfs" "Complete"
            | Error e ->
                let symbol = Path.GetFileNameWithoutExtension p
                ({ FilePath = p
                   Symbol = symbol
                   Successful = false }: Parameters.NewFileImport)
                |> Operations.insertFileImport ctx
                logError log "import-etfs" $"{p} - {e}")
        
    let build (log: Log) path =
        let storePath =
            Path.Combine(path, "fstock_store.db")

        let metadataPath =
            Path.Combine(path, "raw", "symbols_valid_meta.csv")
            
        let stockFiles = Directory.EnumerateFiles(Path.Combine(path, "raw", "stocks")) |> List.ofSeq
        let eftPaths = Directory.EnumerateFiles(Path.Combine(path, "raw", "etfs")) |> List.ofSeq

        let ctx = initialize log storePath

        let (metadata, mdErrors) =
            loadMetadata metadataPath

        match mdErrors.IsEmpty with
        | true -> ()
        | false ->
            logError log "build" $"{mdErrors.Length} error(s) importing metadata."
        
        match importMetadata ctx metadata with
        | Ok _ -> logSuccess log "build" "Metadata successfully imported."
        | Error e -> logError log "build" $"Error importing metadata: {e}"
        
        importStockFiles log ctx stockFiles
        importEtfFiles log ctx eftPaths
        
    
    let logToConsole (itemType: LogItemType) (from: string) (message: string) =
        let (color, t) = 
            match itemType with
            | LogItemType.Information -> ConsoleColor.Gray, "info "
            | LogItemType.Success -> ConsoleColor.Green, "ok  "
            | LogItemType.Error -> ConsoleColor.Red, "error"
            | LogItemType.Warning -> ConsoleColor.Magenta, "warn "
            
        Console.ForegroundColor <- color
        printfn $"[{DateTime.UtcNow} {t}] {from} - {message}"
        Console.ResetColor()    
