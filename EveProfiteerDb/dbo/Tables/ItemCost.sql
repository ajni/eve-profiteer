CREATE TABLE [dbo].[ItemCost]
(
	[Id] INT IDENTITY NOT NULL  PRIMARY KEY CLUSTERED, 
    [InvTypes_TypeId] INT NOT NULL, 
    [MovingAverage] DECIMAL(18, 2) NOT NULL, 
    [Fifo] DECIMAL(18, 2) NULL, 
    [Lifo] DECIMAL(18, 2) NULL,
	[ApiKeyEntities_Id] INT NOT NULL, 
    CONSTRAINT [FK_dbo.ItemCost_dbo.ToInvTypes] FOREIGN KEY ([InvTypes_TypeId]) REFERENCES [invTypes]([typeID]),
    CONSTRAINT [FK_dbo.ItemCost_dbo.ToApiKeyEntities] FOREIGN KEY ([ApiKeyEntities_Id]) REFERENCES [ApiKeyEntities]([Id]),
)
GO
CREATE INDEX [IX_dbo.ItemCost_dbo.InvTypes_TypeId] ON [dbo].[Inventory] ([InvTypes_TypeId])
GO
CREATE INDEX [IX_dbo.ItemCost_dbo.ApiKeyEntity_Id] ON [dbo].[Inventory] ([ApiKeyEntity_Id])
