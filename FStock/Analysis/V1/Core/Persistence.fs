namespace FStock.Analysis.V1.Core.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 22/02/2024 21:50:35 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `artifacts`.
    type Artifact =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("bucket")>] Bucket: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("artifactBlob")>] ArtifactBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("fileType")>] FileType: string }
    
        static member Blank() =
            { Id = String.Empty
              Bucket = String.Empty
              Name = String.Empty
              ArtifactBlob = BlobField.Empty()
              Hash = String.Empty
              FileType = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE artifacts (
	id TEXT NOT NULL,
	bucket TEXT NOT NULL,
	name TEXT NOT NULL,
	artifact_blob BLOB NOT NULL,
	hash TEXT NOT NULL,
	file_type TEXT NOT NULL
)
        """
    
        static member SelectSql() = """
        SELECT
              artifacts.`id`,
              artifacts.`bucket`,
              artifacts.`name`,
              artifacts.`artifact_blob`,
              artifacts.`hash`,
              artifacts.`file_type`
        FROM artifacts
        """
    
        static member TableName() = "artifacts"
    
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
    
    /// A record representing a row in the table `stock_entries`.
    type StockEntry =
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
        CREATE TABLE stock_entries (
	symbol TEXT NOT NULL,
	entry_date TEXT NOT NULL,
	open_value REAL NOT NULL,
	high_value REAL NOT NULL,
	low_value REAL NOT NULL,
	close_value REAL NOT NULL,
	adjusted_close_value REAL NOT NULL,
	volume_value REAL NOT NULL,
	CONSTRAINT stock_entries_PK PRIMARY KEY (symbol,entry_date)
)
        """
    
        static member SelectSql() = """
        SELECT
              stock_entries.`symbol`,
              stock_entries.`entry_date`,
              stock_entries.`open_value`,
              stock_entries.`high_value`,
              stock_entries.`low_value`,
              stock_entries.`close_value`,
              stock_entries.`adjusted_close_value`,
              stock_entries.`volume_value`
        FROM stock_entries
        """
    
        static member TableName() = "stock_entries"
    
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
    
    /// A record representing a row in the table `table_listings`.
    type TableListingItem =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("tableType")>] TableType: string
          [<JsonPropertyName("parametersBlob")>] ParametersBlob: BlobField }
    
        static member Blank() =
            { Name = String.Empty
              TableType = String.Empty
              ParametersBlob = BlobField.Empty() }
    
        static member CreateTableSql() = """
        CREATE TABLE table_listings (
	name TEXT NOT NULL,
	table_type TEXT NOT NULL,
	parameters_blob BLOB NOT NULL,
	CONSTRAINT table_listings_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              table_listings.`name`,
              table_listings.`table_type`,
              table_listings.`parameters_blob`
        FROM table_listings
        """
    
        static member TableName() = "table_listings"
    

/// Module generated on 22/02/2024 21:50:35 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `artifacts`.
    type NewArtifact =
        { [<JsonPropertyName("id")>] Id: string
          [<JsonPropertyName("bucket")>] Bucket: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("artifactBlob")>] ArtifactBlob: BlobField
          [<JsonPropertyName("hash")>] Hash: string
          [<JsonPropertyName("fileType")>] FileType: string }
    
        static member Blank() =
            { Id = String.Empty
              Bucket = String.Empty
              Name = String.Empty
              ArtifactBlob = BlobField.Empty()
              Hash = String.Empty
              FileType = String.Empty }
    
    
    /// A record representing a new row in the table `metadata`.
    type NewMetadataItem =
        { [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: decimal }
    
        static member Blank() =
            { ItemKey = String.Empty
              ItemValue = 0m }
    
    
    /// A record representing a new row in the table `stock_entries`.
    type NewStockEntry =
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
    
    
    /// A record representing a new row in the table `stocks`.
    type NewStock =
        { [<JsonPropertyName("symbol")>] Symbol: string
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Symbol = String.Empty
              Name = String.Empty }
    
    
    /// A record representing a new row in the table `table_listings`.
    type NewTableListingItem =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("tableType")>] TableType: string
          [<JsonPropertyName("parametersBlob")>] ParametersBlob: BlobField }
    
        static member Blank() =
            { Name = String.Empty
              TableType = String.Empty
              ParametersBlob = BlobField.Empty() }
    
    
/// Module generated on 22/02/2024 21:50:35 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.Artifact` from the table `artifacts`.
    /// Internally this calls `context.SelectSingleAnon<Records.Artifact>` and uses Records.Artifact.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArtifactRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectArtifactRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Artifact.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Artifact>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Artifact>` and uses Records.Artifact.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectArtifactRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectArtifactRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Artifact.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Artifact>(sql, parameters)
    
    let insertArtifact (context: SqliteContext) (parameters: Parameters.NewArtifact) =
        context.Insert("artifacts", parameters)
    
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
    
    /// Select a `Records.StockEntry` from the table `stock_entries`.
    /// Internally this calls `context.SelectSingleAnon<Records.StockEntry>` and uses Records.StockEntry.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectStockEntryRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectStockEntryRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.StockEntry.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.StockEntry>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.StockEntry>` and uses Records.StockEntry.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectStockEntryRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectStockEntryRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.StockEntry.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.StockEntry>(sql, parameters)
    
    let insertStockEntry (context: SqliteContext) (parameters: Parameters.NewStockEntry) =
        context.Insert("stock_entries", parameters)
    
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
    
    /// Select a `Records.TableListingItem` from the table `table_listings`.
    /// Internally this calls `context.SelectSingleAnon<Records.TableListingItem>` and uses Records.TableListingItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTableListingItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTableListingItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TableListingItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.TableListingItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.TableListingItem>` and uses Records.TableListingItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTableListingItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTableListingItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TableListingItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.TableListingItem>(sql, parameters)
    
    let insertTableListingItem (context: SqliteContext) (parameters: Parameters.NewTableListingItem) =
        context.Insert("table_listings", parameters)
    