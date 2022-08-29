namespace FStock.Data.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 29/08/2022 10:47:58 (utc) via Freql.Sqlite.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `day_reports`.
    type DayReport =
        { [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("reportBlob")>] ReportBlob: BlobField }
    
        static member Blank() =
            { EntryDate = DateTime.UtcNow
              ReportBlob = BlobField.Empty() }
    
        static member CreateTableSql() = """
        CREATE TABLE day_reports (
	entry_date TEXT NOT NULL,
	report_blob BLOB NOT NULL,
	CONSTRAINT day_reports_PK PRIMARY KEY (entry_date)
)
        """
    
        static member SelectSql() = """
        SELECT
              entry_date,
              report_blob
        FROM day_reports
        """
    
        static member TableName() = "day_reports"
    
    /// A record representing a row in the table `etfs`.
    type Etf =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("openValue")>] OpenValue: decimal
          [<JsonPropertyName("highValue")>] HighValue: decimal
          [<JsonPropertyName("lowValue")>] LowValue: decimal
          [<JsonPropertyName("closeValue")>] CloseValue: decimal
          [<JsonPropertyName("adjustedCloseValue")>] AdjustedCloseValue: decimal
          [<JsonPropertyName("volumeValue")>] VolumeValue: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              OpenValue = 0m
              HighValue = 0m
              LowValue = 0m
              CloseValue = 0m
              AdjustedCloseValue = 0m
              VolumeValue = 0m }
    
        static member CreateTableSql() = """
        CREATE TABLE etfs (
	symbol TEXT NOT NULL,
	entry_date TEXT NOT NULL,
	open_value REAL NOT NULL,
	high_value REAL NOT NULL,
	low_value REAL NOT NULL,
	close_value REAL NOT NULL,
	adjusted_close_value REAL NOT NULL,
	volume_value REAL NOT NULL,
	CONSTRAINT etfs_FK FOREIGN KEY (symbol) REFERENCES symbol_metadata(symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              symbol,
              entry_date,
              open_value,
              high_value,
              low_value,
              close_value,
              adjusted_close_value,
              volume_value
        FROM etfs
        """
    
        static member TableName() = "etfs"
    
    /// A record representing a row in the table `file_imports`.
    type FileImport =
        { [<JsonPropertyName("filePath")>] FilePath: string
          [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("successful")>] Successful: bool }
    
        static member Blank() =
            { FilePath = String.Empty
              Symbol = String.Empty
              Successful = true }
    
        static member CreateTableSql() = """
        CREATE TABLE file_imports (
	file_path TEXT NOT NULL,
	symbol TEXT NOT NULL,
	successful INTEGER NOT NULL,
	CONSTRAINT file_imports_FK FOREIGN KEY (symbol) REFERENCES symbol_metadata(symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              file_path,
              symbol,
              successful
        FROM file_imports
        """
    
        static member TableName() = "file_imports"
    
    /// A record representing a row in the table `import_errors`.
    type ImportError =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("line")>] Line: int
          [<JsonPropertyName("message")>] Message: string }
    
        static member Blank() =
            { Symbol = String.Empty
              Line = 0
              Message = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE import_errors (
	symbol TEXT NOT NULL,
	line INTEGER NOT NULL,
	message TEXT NOT NULL,
	CONSTRAINT import_errors_FK FOREIGN KEY (symbol) REFERENCES symbol_metadata(symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              symbol,
              line,
              message
        FROM import_errors
        """
    
        static member TableName() = "import_errors"
    
    /// A record representing a row in the table `moving_averages`.
    type MovingAverage =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("movingAverage50Days")>] MovingAverage50Days: decimal
          [<JsonPropertyName("movingAverage200Days")>] MovingAverage200Days: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              MovingAverage50Days = 0m
              MovingAverage200Days = 0m }
    
        static member CreateTableSql() = """
        CREATE TABLE moving_averages (
	symbol TEXT NOT NULL,
	entry_date TEXT NOT NULL,
	moving_average50_days REAL NOT NULL,
	moving_average200_days REAL NOT NULL,
	CONSTRAINT moving_averages_PK PRIMARY KEY (symbol,entry_date),
	CONSTRAINT moving_averages_FK FOREIGN KEY (symbol) REFERENCES symbol_metadata(symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              symbol,
              entry_date,
              moving_average50_days,
              moving_average200_days
        FROM moving_averages
        """
    
        static member TableName() = "moving_averages"
    
    /// A record representing a row in the table `stocks`.
    type Stock =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("openValue")>] OpenValue: decimal
          [<JsonPropertyName("highValue")>] HighValue: decimal
          [<JsonPropertyName("lowValue")>] LowValue: decimal
          [<JsonPropertyName("closeValue")>] CloseValue: decimal
          [<JsonPropertyName("adjustedCloseValue")>] AdjustedCloseValue: decimal
          [<JsonPropertyName("volumeValue")>] VolumeValue: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              OpenValue = 0m
              HighValue = 0m
              LowValue = 0m
              CloseValue = 0m
              AdjustedCloseValue = 0m
              VolumeValue = 0m }
    
        static member CreateTableSql() = """
        CREATE TABLE stocks (
	symbol TEXT NOT NULL,
	entry_date TEXT NOT NULL,
	open_value REAL NOT NULL,
	high_value REAL NOT NULL,
	low_value REAL NOT NULL,
	close_value REAL NOT NULL,
	adjusted_close_value REAL NOT NULL,
	volume_value REAL NOT NULL,
	CONSTRAINT stocks_FK FOREIGN KEY (symbol) REFERENCES symbol_metadata(symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              symbol,
              entry_date,
              open_value,
              high_value,
              low_value,
              close_value,
              adjusted_close_value,
              volume_value
        FROM stocks
        """
    
        static member TableName() = "stocks"
    
    /// A record representing a row in the table `symbol_metadata`.
    type SymbolMetadataItem =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("securityName")>] SecurityName: string
          [<JsonPropertyName("listingExchange")>] ListingExchange: string option
          [<JsonPropertyName("category")>] Category: string option
          [<JsonPropertyName("isEft")>] IsEft: bool
          [<JsonPropertyName("roundLotSize")>] RoundLotSize: decimal
          [<JsonPropertyName("testIssue")>] TestIssue: bool
          [<JsonPropertyName("financialStatus")>] FinancialStatus: string option
          [<JsonPropertyName("isNasdaqTraded")>] IsNasdaqTraded: bool
          [<JsonPropertyName("cqsSymbol")>] CqsSymbol: string option
          [<JsonPropertyName("nasdaqSymbol")>] NasdaqSymbol: string
          [<JsonPropertyName("nextShares")>] NextShares: bool }
    
        static member Blank() =
            { Symbol = String.Empty
              SecurityName = String.Empty
              ListingExchange = None
              Category = None
              IsEft = true
              RoundLotSize = 0m
              TestIssue = true
              FinancialStatus = None
              IsNasdaqTraded = true
              CqsSymbol = None
              NasdaqSymbol = String.Empty
              NextShares = true }
    
        static member CreateTableSql() = """
        CREATE TABLE symbol_metadata (
	symbol TEXT NOT NULL,
	security_name TEXT NOT NULL,
	listing_exchange TEXT,
	category TEXT,
	is_eft INTEGER NOT NULL,
	round_lot_size REAL NOT NULL,
	test_issue INTEGER NOT NULL,
	financial_status TEXT,
	is_nasdaq_traded INTEGER NOT NULL,
	cqs_symbol TEXT,
	nasdaq_symbol TEXT NOT NULL,
	next_shares INTEGER NOT NULL,
	CONSTRAINT symbol_metadata_PK PRIMARY KEY (symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              symbol,
              security_name,
              listing_exchange,
              category,
              is_eft,
              round_lot_size,
              test_issue,
              financial_status,
              is_nasdaq_traded,
              cqs_symbol,
              nasdaq_symbol,
              next_shares
        FROM symbol_metadata
        """
    
        static member TableName() = "symbol_metadata"
    

/// Module generated on 29/08/2022 10:47:58 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `day_reports`.
    type NewDayReport =
        { [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("reportBlob")>] ReportBlob: BlobField }
    
        static member Blank() =
            { EntryDate = DateTime.UtcNow
              ReportBlob = BlobField.Empty() }
    
    
    /// A record representing a new row in the table `etfs`.
    type NewEtf =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("openValue")>] OpenValue: decimal
          [<JsonPropertyName("highValue")>] HighValue: decimal
          [<JsonPropertyName("lowValue")>] LowValue: decimal
          [<JsonPropertyName("closeValue")>] CloseValue: decimal
          [<JsonPropertyName("adjustedCloseValue")>] AdjustedCloseValue: decimal
          [<JsonPropertyName("volumeValue")>] VolumeValue: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              OpenValue = 0m
              HighValue = 0m
              LowValue = 0m
              CloseValue = 0m
              AdjustedCloseValue = 0m
              VolumeValue = 0m }
    
    
    /// A record representing a new row in the table `file_imports`.
    type NewFileImport =
        { [<JsonPropertyName("filePath")>] FilePath: string
          [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("successful")>] Successful: bool }
    
        static member Blank() =
            { FilePath = String.Empty
              Symbol = String.Empty
              Successful = true }
    
    
    /// A record representing a new row in the table `import_errors`.
    type NewImportError =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("line")>] Line: int
          [<JsonPropertyName("message")>] Message: string }
    
        static member Blank() =
            { Symbol = String.Empty
              Line = 0
              Message = String.Empty }
    
    
    /// A record representing a new row in the table `moving_averages`.
    type NewMovingAverage =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("movingAverage50Days")>] MovingAverage50Days: decimal
          [<JsonPropertyName("movingAverage200Days")>] MovingAverage200Days: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              MovingAverage50Days = 0m
              MovingAverage200Days = 0m }
    
    
    /// A record representing a new row in the table `stocks`.
    type NewStock =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("openValue")>] OpenValue: decimal
          [<JsonPropertyName("highValue")>] HighValue: decimal
          [<JsonPropertyName("lowValue")>] LowValue: decimal
          [<JsonPropertyName("closeValue")>] CloseValue: decimal
          [<JsonPropertyName("adjustedCloseValue")>] AdjustedCloseValue: decimal
          [<JsonPropertyName("volumeValue")>] VolumeValue: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              OpenValue = 0m
              HighValue = 0m
              LowValue = 0m
              CloseValue = 0m
              AdjustedCloseValue = 0m
              VolumeValue = 0m }
    
    
    /// A record representing a new row in the table `symbol_metadata`.
    type NewSymbolMetadataItem =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("securityName")>] SecurityName: string
          [<JsonPropertyName("listingExchange")>] ListingExchange: string option
          [<JsonPropertyName("category")>] Category: string option
          [<JsonPropertyName("isEft")>] IsEft: bool
          [<JsonPropertyName("roundLotSize")>] RoundLotSize: decimal
          [<JsonPropertyName("testIssue")>] TestIssue: bool
          [<JsonPropertyName("financialStatus")>] FinancialStatus: string option
          [<JsonPropertyName("isNasdaqTraded")>] IsNasdaqTraded: bool
          [<JsonPropertyName("cqsSymbol")>] CqsSymbol: string option
          [<JsonPropertyName("nasdaqSymbol")>] NasdaqSymbol: string
          [<JsonPropertyName("nextShares")>] NextShares: bool }
    
        static member Blank() =
            { Symbol = String.Empty
              SecurityName = String.Empty
              ListingExchange = None
              Category = None
              IsEft = true
              RoundLotSize = 0m
              TestIssue = true
              FinancialStatus = None
              IsNasdaqTraded = true
              CqsSymbol = None
              NasdaqSymbol = String.Empty
              NextShares = true }
    
    
/// Module generated on 29/08/2022 10:47:58 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.DayReport` from the table `day_reports`.
    /// Internally this calls `context.SelectSingleAnon<Records.DayReport>` and uses Records.DayReport.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDayReportRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectDayReportRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.DayReport.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.DayReport>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.DayReport>` and uses Records.DayReport.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDayReportRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectDayReportRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.DayReport.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.DayReport>(sql, parameters)
    
    let insertDayReport (context: SqliteContext) (parameters: Parameters.NewDayReport) =
        context.Insert("day_reports", parameters)
    
    /// Select a `Records.Etf` from the table `etfs`.
    /// Internally this calls `context.SelectSingleAnon<Records.Etf>` and uses Records.Etf.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectEtfRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectEtfRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Etf.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Etf>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Etf>` and uses Records.Etf.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectEtfRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectEtfRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Etf.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Etf>(sql, parameters)
    
    let insertEtf (context: SqliteContext) (parameters: Parameters.NewEtf) =
        context.Insert("etfs", parameters)
    
    /// Select a `Records.FileImport` from the table `file_imports`.
    /// Internally this calls `context.SelectSingleAnon<Records.FileImport>` and uses Records.FileImport.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectFileImportRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectFileImportRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.FileImport.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.FileImport>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.FileImport>` and uses Records.FileImport.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectFileImportRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectFileImportRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.FileImport.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.FileImport>(sql, parameters)
    
    let insertFileImport (context: SqliteContext) (parameters: Parameters.NewFileImport) =
        context.Insert("file_imports", parameters)
    
    /// Select a `Records.ImportError` from the table `import_errors`.
    /// Internally this calls `context.SelectSingleAnon<Records.ImportError>` and uses Records.ImportError.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectImportErrorRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectImportErrorRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ImportError.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.ImportError>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.ImportError>` and uses Records.ImportError.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectImportErrorRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectImportErrorRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.ImportError.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.ImportError>(sql, parameters)
    
    let insertImportError (context: SqliteContext) (parameters: Parameters.NewImportError) =
        context.Insert("import_errors", parameters)
    
    /// Select a `Records.MovingAverage` from the table `moving_averages`.
    /// Internally this calls `context.SelectSingleAnon<Records.MovingAverage>` and uses Records.MovingAverage.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectMovingAverageRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectMovingAverageRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.MovingAverage.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.MovingAverage>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.MovingAverage>` and uses Records.MovingAverage.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectMovingAverageRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectMovingAverageRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.MovingAverage.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.MovingAverage>(sql, parameters)
    
    let insertMovingAverage (context: SqliteContext) (parameters: Parameters.NewMovingAverage) =
        context.Insert("moving_averages", parameters)
    
    /// Select a `Records.Stock` from the table `stocks`.
    /// Internally this calls `context.SelectSingleAnon<Records.Stock>` and uses Records.Stock.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectStockRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectStockRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Stock.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Stock>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Stock>` and uses Records.Stock.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectStockRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectStockRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Stock.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Stock>(sql, parameters)
    
    let insertStock (context: SqliteContext) (parameters: Parameters.NewStock) =
        context.Insert("stocks", parameters)
    
    /// Select a `Records.SymbolMetadataItem` from the table `symbol_metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.SymbolMetadataItem>` and uses Records.SymbolMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSymbolMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectSymbolMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SymbolMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.SymbolMetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.SymbolMetadataItem>` and uses Records.SymbolMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectSymbolMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectSymbolMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.SymbolMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.SymbolMetadataItem>(sql, parameters)
    
    let insertSymbolMetadataItem (context: SqliteContext) (parameters: Parameters.NewSymbolMetadataItem) =
        context.Insert("symbol_metadata", parameters)
    