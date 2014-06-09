CREATE TABLE [dbo].[BatchMaterials]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [TypeId] INT NOT NULL, 
    [BatchId] INT NOT NULL, 
    [Quantity] INT NOT NULL DEFAULT 0, 
    [TotalCost] DECIMAL(18, 5) NOT NULL DEFAULT 0,
    CONSTRAINT [FK_BatchMaterials_ToInvTypes] FOREIGN KEY ([TypeId]) REFERENCES [invTypes]([typeID]), 
    CONSTRAINT [FK_BatchMaterials_ToProductionBatches] FOREIGN KEY ([BatchId]) REFERENCES [ProductionBatches]([Id]), 
);
GO
