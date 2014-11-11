CREATE TABLE [dbo].[BatchMaterials]
(
	[Id] INT IDENTITY NOT NULL, 
    [TypeId] INT NOT NULL, 
    [BatchId] INT NOT NULL, 
    [Quantity] INT NOT NULL DEFAULT 0, 
    [TotalCost] DECIMAL(18, 5) NOT NULL DEFAULT 0,
	CONSTRAINT [PK_dbo.BatchMaterials] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BatchMaterials_ToInvTypes] FOREIGN KEY ([TypeId]) REFERENCES [invTypes]([typeID]), 
    CONSTRAINT [FK_BatchMaterials_ToProductionBatches] FOREIGN KEY ([BatchId]) REFERENCES [ProductionBatches]([Id]), 
);
GO
CREATE NONCLUSTERED INDEX [IX_BatchMaterials.BatchId] ON [dbo].[BatchMaterials] ([BatchId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_BatchMaterials.TypeId] ON [dbo].[BatchMaterials] ([TypeId] ASC);