CREATE TABLE [dbo].[AssetReductions]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [AssetId] INT NOT NULL, 
    [Quantity] INT NOT NULL, 
    [PostReductionQuantity] INT NOT NULL, 
    [Description] TEXT NOT NULL , 
    [Date] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    CONSTRAINT [FK_AssetReductions_Assets] FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]) 
)
