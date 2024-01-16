namespace FStock.Analysis.V1.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 16/01/2024 23:08:18 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `day_values`.
    type DayValue =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("openValue")>] OpenValue: decimal
          [<JsonPropertyName("highValue")>] HighValue: decimal
          [<JsonPropertyName("lowValue")>] LowValue: decimal
          [<JsonPropertyName("closeValue")>] CloseValue: decimal
          [<JsonPropertyName("adjCloseValue")>] AdjCloseValue: decimal
          [<JsonPropertyName("volumeValue")>] VolumeValue: decimal
          [<JsonPropertyName("maPrev7OpenValue")>] MaPrev7OpenValue: decimal
          [<JsonPropertyName("maPrev7HighValue")>] MaPrev7HighValue: decimal
          [<JsonPropertyName("maPrev7LowValue")>] MaPrev7LowValue: decimal
          [<JsonPropertyName("maPrev7CloseValue")>] MaPrev7CloseValue: decimal
          [<JsonPropertyName("maPrev7AdjCloseValue")>] MaPrev7AdjCloseValue: decimal
          [<JsonPropertyName("maPrev7VolumeValue")>] MaPrev7VolumeValue: decimal
          [<JsonPropertyName("maPrev20OpenValue")>] MaPrev20OpenValue: decimal
          [<JsonPropertyName("maPrev20HighValue")>] MaPrev20HighValue: decimal
          [<JsonPropertyName("maPrev20LowValue")>] MaPrev20LowValue: decimal
          [<JsonPropertyName("maPrev20CloseValue")>] MaPrev20CloseValue: decimal
          [<JsonPropertyName("maPrev20AdjCloseValue")>] MaPrev20AdjCloseValue: decimal
          [<JsonPropertyName("maPrev20VolumeValue")>] MaPrev20VolumeValue: decimal
          [<JsonPropertyName("maPrev50OpenValue")>] MaPrev50OpenValue: decimal
          [<JsonPropertyName("maPrev50HighValue")>] MaPrev50HighValue: decimal
          [<JsonPropertyName("maPrev50LowValue")>] MaPrev50LowValue: decimal
          [<JsonPropertyName("maPrev50CloseValue")>] MaPrev50CloseValue: decimal
          [<JsonPropertyName("maPrev50AdjCloseValue")>] MaPrev50AdjCloseValue: decimal
          [<JsonPropertyName("maPrev50VolumeValue")>] MaPrev50VolumeValue: decimal
          [<JsonPropertyName("maPrev100OpenValue")>] MaPrev100OpenValue: decimal
          [<JsonPropertyName("maPrev100HighValue")>] MaPrev100HighValue: decimal
          [<JsonPropertyName("maPrev100LowValue")>] MaPrev100LowValue: decimal
          [<JsonPropertyName("maPrev100CloseValue")>] MaPrev100CloseValue: decimal
          [<JsonPropertyName("maPrev100AdjCloseValue")>] MaPrev100AdjCloseValue: decimal
          [<JsonPropertyName("maPrev100VolumeValue")>] MaPrev100VolumeValue: decimal
          [<JsonPropertyName("maPrev200OpenValue")>] MaPrev200OpenValue: decimal
          [<JsonPropertyName("maPrev200HighValue")>] MaPrev200HighValue: decimal
          [<JsonPropertyName("maPrev200LowValue")>] MaPrev200LowValue: decimal
          [<JsonPropertyName("maPrev200CloseValue")>] MaPrev200CloseValue: decimal
          [<JsonPropertyName("maPrev200AdjCloseValue")>] MaPrev200AdjCloseValue: decimal
          [<JsonPropertyName("maPrev200VolumeValue")>] MaPrev200VolumeValue: decimal
          [<JsonPropertyName("maPrev252OpenValue")>] MaPrev252OpenValue: decimal
          [<JsonPropertyName("maPrev252HighValue")>] MaPrev252HighValue: decimal
          [<JsonPropertyName("maPrev252LowValue")>] MaPrev252LowValue: decimal
          [<JsonPropertyName("maPrev252CloseValue")>] MaPrev252CloseValue: decimal
          [<JsonPropertyName("maPrev252AdjCloseValue")>] MaPrev252AdjCloseValue: decimal
          [<JsonPropertyName("maPrev252VolumeValue")>] MaPrev252VolumeValue: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              OpenValue = 0m
              HighValue = 0m
              LowValue = 0m
              CloseValue = 0m
              AdjCloseValue = 0m
              VolumeValue = 0m
              MaPrev7OpenValue = 0m
              MaPrev7HighValue = 0m
              MaPrev7LowValue = 0m
              MaPrev7CloseValue = 0m
              MaPrev7AdjCloseValue = 0m
              MaPrev7VolumeValue = 0m
              MaPrev20OpenValue = 0m
              MaPrev20HighValue = 0m
              MaPrev20LowValue = 0m
              MaPrev20CloseValue = 0m
              MaPrev20AdjCloseValue = 0m
              MaPrev20VolumeValue = 0m
              MaPrev50OpenValue = 0m
              MaPrev50HighValue = 0m
              MaPrev50LowValue = 0m
              MaPrev50CloseValue = 0m
              MaPrev50AdjCloseValue = 0m
              MaPrev50VolumeValue = 0m
              MaPrev100OpenValue = 0m
              MaPrev100HighValue = 0m
              MaPrev100LowValue = 0m
              MaPrev100CloseValue = 0m
              MaPrev100AdjCloseValue = 0m
              MaPrev100VolumeValue = 0m
              MaPrev200OpenValue = 0m
              MaPrev200HighValue = 0m
              MaPrev200LowValue = 0m
              MaPrev200CloseValue = 0m
              MaPrev200AdjCloseValue = 0m
              MaPrev200VolumeValue = 0m
              MaPrev252OpenValue = 0m
              MaPrev252HighValue = 0m
              MaPrev252LowValue = 0m
              MaPrev252CloseValue = 0m
              MaPrev252AdjCloseValue = 0m
              MaPrev252VolumeValue = 0m }
    
        static member CreateTableSql() = """
        CREATE TABLE day_values (
	symbol TEXT NOT NULL,
	entry_date TEXT NOT NULL,
	open_value REAL NOT NULL,
	high_value REAL NOT NULL,
	low_value REAL NOT NULL,
	close_value REAL NOT NULL,
	adj_close_value REAL NOT NULL,
	volume_value REAL NOT NULL,
	ma_prev_7_open_value REAL NOT NULL,
	ma_prev_7_high_value REAL NOT NULL,
	ma_prev_7_low_value REAL NOT NULL,
	ma_prev_7_close_value REAL NOT NULL,
	ma_prev_7_adj_close_value REAL NOT NULL,
	ma_prev_7_volume_value REAL NOT NULL,
	ma_prev_20_open_value REAL NOT NULL,
	ma_prev_20_high_value REAL NOT NULL,
	ma_prev_20_low_value REAL NOT NULL,
	ma_prev_20_close_value REAL NOT NULL,
	ma_prev_20_adj_close_value REAL NOT NULL,
	ma_prev_20_volume_value REAL NOT NULL,
	ma_prev_50_open_value REAL NOT NULL,
	ma_prev_50_high_value REAL NOT NULL,
	ma_prev_50_low_value REAL NOT NULL,
	ma_prev_50_close_value REAL NOT NULL,
	ma_prev_50_adj_close_value REAL NOT NULL,
	ma_prev_50_volume_value REAL NOT NULL,
	ma_prev_100_open_value REAL NOT NULL,
	ma_prev_100_high_value REAL NOT NULL,
	ma_prev_100_low_value REAL NOT NULL,
	ma_prev_100_close_value REAL NOT NULL,
	ma_prev_100_adj_close_value REAL NOT NULL,
	ma_prev_100_volume_value REAL NOT NULL,
	ma_prev_200_open_value REAL NOT NULL,
	ma_prev_200_high_value REAL NOT NULL,
	ma_prev_200_low_value REAL NOT NULL,
	ma_prev_200_close_value REAL NOT NULL,
	ma_prev_200_adj_close_value REAL NOT NULL,
	ma_prev_200_volume_value REAL NOT NULL,
	ma_prev_252_open_value REAL NOT NULL,
	ma_prev_252_high_value REAL NOT NULL,
	ma_prev_252_low_value REAL NOT NULL,
	ma_prev_252_close_value REAL NOT NULL,
	ma_prev_252_adj_close_value REAL NOT NULL,
	ma_prev_252_volume_value REAL NOT NULL,
	CONSTRAINT day_values_PK PRIMARY KEY (symbol,entry_date)
)
        """
    
        static member SelectSql() = """
        SELECT
              day_values.`symbol`,
              day_values.`entry_date`,
              day_values.`open_value`,
              day_values.`high_value`,
              day_values.`low_value`,
              day_values.`close_value`,
              day_values.`adj_close_value`,
              day_values.`volume_value`,
              day_values.`ma_prev_7_open_value`,
              day_values.`ma_prev_7_high_value`,
              day_values.`ma_prev_7_low_value`,
              day_values.`ma_prev_7_close_value`,
              day_values.`ma_prev_7_adj_close_value`,
              day_values.`ma_prev_7_volume_value`,
              day_values.`ma_prev_20_open_value`,
              day_values.`ma_prev_20_high_value`,
              day_values.`ma_prev_20_low_value`,
              day_values.`ma_prev_20_close_value`,
              day_values.`ma_prev_20_adj_close_value`,
              day_values.`ma_prev_20_volume_value`,
              day_values.`ma_prev_50_open_value`,
              day_values.`ma_prev_50_high_value`,
              day_values.`ma_prev_50_low_value`,
              day_values.`ma_prev_50_close_value`,
              day_values.`ma_prev_50_adj_close_value`,
              day_values.`ma_prev_50_volume_value`,
              day_values.`ma_prev_100_open_value`,
              day_values.`ma_prev_100_high_value`,
              day_values.`ma_prev_100_low_value`,
              day_values.`ma_prev_100_close_value`,
              day_values.`ma_prev_100_adj_close_value`,
              day_values.`ma_prev_100_volume_value`,
              day_values.`ma_prev_200_open_value`,
              day_values.`ma_prev_200_high_value`,
              day_values.`ma_prev_200_low_value`,
              day_values.`ma_prev_200_close_value`,
              day_values.`ma_prev_200_adj_close_value`,
              day_values.`ma_prev_200_volume_value`,
              day_values.`ma_prev_252_open_value`,
              day_values.`ma_prev_252_high_value`,
              day_values.`ma_prev_252_low_value`,
              day_values.`ma_prev_252_close_value`,
              day_values.`ma_prev_252_adj_close_value`,
              day_values.`ma_prev_252_volume_value`
        FROM day_values
        """
    
        static member TableName() = "day_values"
    
    /// A record representing a row in the table `metadata`.
    type MetadataItem =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: decimal }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = 0m }
    
        static member CreateTableSql() = """
        CREATE TABLE metadata (
	item_key TEXT NOT NULL,
	item_value TEXT NOT NULL,
	CONSTRAINT metadata_PK PRIMARY KEY (item_key)
)
        """
    
        static member SelectSql() = """
        SELECT
              metadata.`item_key`,
              metadata.`item_value`
        FROM metadata
        """
    
        static member TableName() = "metadata"
    
    /// A record representing a row in the table `stocks`.
    type Stock =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Symbol = String.Empty
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE stocks (
	symbol TEXT NOT NULL,
	name TEXT NOT NULL,
	CONSTRAINT stocks_PK PRIMARY KEY (symbol)
)
        """
    
        static member SelectSql() = """
        SELECT
              stocks.`symbol`,
              stocks.`name`
        FROM stocks
        """
    
        static member TableName() = "stocks"
    

/// Module generated on 16/01/2024 23:08:18 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `day_values`.
    type NewDayValue =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("entryDate")>] EntryDate: DateTime
          [<JsonPropertyName("openValue")>] OpenValue: decimal
          [<JsonPropertyName("highValue")>] HighValue: decimal
          [<JsonPropertyName("lowValue")>] LowValue: decimal
          [<JsonPropertyName("closeValue")>] CloseValue: decimal
          [<JsonPropertyName("adjCloseValue")>] AdjCloseValue: decimal
          [<JsonPropertyName("volumeValue")>] VolumeValue: decimal
          [<JsonPropertyName("maPrev7OpenValue")>] MaPrev7OpenValue: decimal
          [<JsonPropertyName("maPrev7HighValue")>] MaPrev7HighValue: decimal
          [<JsonPropertyName("maPrev7LowValue")>] MaPrev7LowValue: decimal
          [<JsonPropertyName("maPrev7CloseValue")>] MaPrev7CloseValue: decimal
          [<JsonPropertyName("maPrev7AdjCloseValue")>] MaPrev7AdjCloseValue: decimal
          [<JsonPropertyName("maPrev7VolumeValue")>] MaPrev7VolumeValue: decimal
          [<JsonPropertyName("maPrev20OpenValue")>] MaPrev20OpenValue: decimal
          [<JsonPropertyName("maPrev20HighValue")>] MaPrev20HighValue: decimal
          [<JsonPropertyName("maPrev20LowValue")>] MaPrev20LowValue: decimal
          [<JsonPropertyName("maPrev20CloseValue")>] MaPrev20CloseValue: decimal
          [<JsonPropertyName("maPrev20AdjCloseValue")>] MaPrev20AdjCloseValue: decimal
          [<JsonPropertyName("maPrev20VolumeValue")>] MaPrev20VolumeValue: decimal
          [<JsonPropertyName("maPrev50OpenValue")>] MaPrev50OpenValue: decimal
          [<JsonPropertyName("maPrev50HighValue")>] MaPrev50HighValue: decimal
          [<JsonPropertyName("maPrev50LowValue")>] MaPrev50LowValue: decimal
          [<JsonPropertyName("maPrev50CloseValue")>] MaPrev50CloseValue: decimal
          [<JsonPropertyName("maPrev50AdjCloseValue")>] MaPrev50AdjCloseValue: decimal
          [<JsonPropertyName("maPrev50VolumeValue")>] MaPrev50VolumeValue: decimal
          [<JsonPropertyName("maPrev100OpenValue")>] MaPrev100OpenValue: decimal
          [<JsonPropertyName("maPrev100HighValue")>] MaPrev100HighValue: decimal
          [<JsonPropertyName("maPrev100LowValue")>] MaPrev100LowValue: decimal
          [<JsonPropertyName("maPrev100CloseValue")>] MaPrev100CloseValue: decimal
          [<JsonPropertyName("maPrev100AdjCloseValue")>] MaPrev100AdjCloseValue: decimal
          [<JsonPropertyName("maPrev100VolumeValue")>] MaPrev100VolumeValue: decimal
          [<JsonPropertyName("maPrev200OpenValue")>] MaPrev200OpenValue: decimal
          [<JsonPropertyName("maPrev200HighValue")>] MaPrev200HighValue: decimal
          [<JsonPropertyName("maPrev200LowValue")>] MaPrev200LowValue: decimal
          [<JsonPropertyName("maPrev200CloseValue")>] MaPrev200CloseValue: decimal
          [<JsonPropertyName("maPrev200AdjCloseValue")>] MaPrev200AdjCloseValue: decimal
          [<JsonPropertyName("maPrev200VolumeValue")>] MaPrev200VolumeValue: decimal
          [<JsonPropertyName("maPrev252OpenValue")>] MaPrev252OpenValue: decimal
          [<JsonPropertyName("maPrev252HighValue")>] MaPrev252HighValue: decimal
          [<JsonPropertyName("maPrev252LowValue")>] MaPrev252LowValue: decimal
          [<JsonPropertyName("maPrev252CloseValue")>] MaPrev252CloseValue: decimal
          [<JsonPropertyName("maPrev252AdjCloseValue")>] MaPrev252AdjCloseValue: decimal
          [<JsonPropertyName("maPrev252VolumeValue")>] MaPrev252VolumeValue: decimal }
    
        static member Blank() =
            { Symbol = String.Empty
              EntryDate = DateTime.UtcNow
              OpenValue = 0m
              HighValue = 0m
              LowValue = 0m
              CloseValue = 0m
              AdjCloseValue = 0m
              VolumeValue = 0m
              MaPrev7OpenValue = 0m
              MaPrev7HighValue = 0m
              MaPrev7LowValue = 0m
              MaPrev7CloseValue = 0m
              MaPrev7AdjCloseValue = 0m
              MaPrev7VolumeValue = 0m
              MaPrev20OpenValue = 0m
              MaPrev20HighValue = 0m
              MaPrev20LowValue = 0m
              MaPrev20CloseValue = 0m
              MaPrev20AdjCloseValue = 0m
              MaPrev20VolumeValue = 0m
              MaPrev50OpenValue = 0m
              MaPrev50HighValue = 0m
              MaPrev50LowValue = 0m
              MaPrev50CloseValue = 0m
              MaPrev50AdjCloseValue = 0m
              MaPrev50VolumeValue = 0m
              MaPrev100OpenValue = 0m
              MaPrev100HighValue = 0m
              MaPrev100LowValue = 0m
              MaPrev100CloseValue = 0m
              MaPrev100AdjCloseValue = 0m
              MaPrev100VolumeValue = 0m
              MaPrev200OpenValue = 0m
              MaPrev200HighValue = 0m
              MaPrev200LowValue = 0m
              MaPrev200CloseValue = 0m
              MaPrev200AdjCloseValue = 0m
              MaPrev200VolumeValue = 0m
              MaPrev252OpenValue = 0m
              MaPrev252HighValue = 0m
              MaPrev252LowValue = 0m
              MaPrev252CloseValue = 0m
              MaPrev252AdjCloseValue = 0m
              MaPrev252VolumeValue = 0m }
    
    
    /// A record representing a new row in the table `metadata`.
    type NewMetadataItem =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: decimal }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = 0m }
    
    
    /// A record representing a new row in the table `stocks`.
    type NewStock =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Symbol = String.Empty
              Name = String.Empty }
    
    
/// Module generated on 16/01/2024 23:08:18 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.DayValue` from the table `day_values`.
    /// Internally this calls `context.SelectSingleAnon<Records.DayValue>` and uses Records.DayValue.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDayValueRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectDayValueRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.DayValue.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.DayValue>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.DayValue>` and uses Records.DayValue.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDayValueRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectDayValueRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.DayValue.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.DayValue>(sql, parameters)
    
    let insertDayValue (context: SqliteContext) (parameters: Parameters.NewDayValue) =
        context.Insert("day_values", parameters)
    
    /// Select a `Records.MetadataItem` from the table `metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.MetadataItem>` and uses Records.MetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.MetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.MetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.MetadataItem>` and uses Records.MetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.MetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.MetadataItem>(sql, parameters)
    
    let insertMetadataItem (context: SqliteContext) (parameters: Parameters.NewMetadataItem) =
        context.Insert("metadata", parameters)
    
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
    