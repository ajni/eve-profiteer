CREATE TABLE [dbo].[AssetReductions]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [AssetId] INT NOT NULL, 
    [Quantity] INT NOT NULL, 
    [PostModificationQuantity] INT NOT NULL, 
    [Description] TEXT NOT NULL , 
    [TransactionValue] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    [Timestamp] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    [Date] DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    CONSTRAINT [FK_AssetReductions_Assets] FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]) 
)
