CREATE TABLE [dbo].[Assets]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [InvTypes_TypeId] INT NOT NULL, 
    [Quantity] INT NOT NULL DEFAULT 0, 
    [ActualQuantity] INT NOT NULL DEFAULT 0, 
    [MaterialCost] DECIMAL(18, 5) NOT NULL DEFAULT 0,
	[ApiKeyEntity_Id] INT NOT NULL, 
    [UnaccountedQuantity] INT NOT NULL DEFAULT 0, 
    [LatestAverageCost] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    [BrokerFees] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_dbo.Assets_dbo.ToInvTypes] FOREIGN KEY ([InvTypes_TypeId]) REFERENCES [invTypes]([typeID]),
    CONSTRAINT [FK_dbo.Assets_dbo.ToApiKeyEntities] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [ApiKeyEntities]([Id]),

)
GO
CREATE INDEX [IX_dbo.Assets_dbo.InvTypes_TypeId] ON [dbo].[Assets] ([InvTypes_TypeId])
GO
CREATE INDEX [IX_dbo.Assets_dbo.ApiKeyEntity_Id] ON [dbo].[Assets] ([ApiKeyEntity_Id])
