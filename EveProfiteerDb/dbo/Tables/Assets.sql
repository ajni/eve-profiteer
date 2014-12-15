CREATE TABLE [dbo].[Assets]
(
	[Id] INT IDENTITY NOT NULL, 
    [InvTypes_TypeId] INT NOT NULL, 
    [CalculatedQuantity] INT NOT NULL DEFAULT 0, 
    [InventoryQuantity] INT NOT NULL DEFAULT 0, 
    [MaterialCost] DECIMAL(18, 5) NOT NULL DEFAULT 0,
	[ApiKeyEntity_Id] INT NOT NULL, 
    [UnaccountedQuantity] INT NOT NULL DEFAULT 0, 
    [LatestAverageCost] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    [BrokerFees] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    [MarketQuantity] INT NOT NULL DEFAULT 0, 
    [LastSellTransaction] INT NULL, 
    [LastBuyTransaction] INT NULL, 
    [StationId] INT NOT NULL , 
    CONSTRAINT [PK_dbo.Assets] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Assets_dbo.ToInvTypes] FOREIGN KEY ([InvTypes_TypeId]) REFERENCES [invTypes]([typeID]),
    CONSTRAINT [FK_dbo.Assets_dbo.ToApiKeyEntities] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [ApiKeyEntities]([Id]), 
	CONSTRAINT [FK_dbo.Assets.LastSellTransaction] FOREIGN KEY ([LastSellTransaction]) REFERENCES [Transactions]([Id]),
	CONSTRAINT [FK_dbo.Assets.LastBuyTransaction] FOREIGN KEY ([LastBuyTransaction]) REFERENCES [Transactions]([Id]),
	CONSTRAINT [FK_dbo.Assets.StationId] FOREIGN KEY ([StationId]) REFERENCES [staStations]([stationID]),
)
GO
CREATE NONCLUSTERED INDEX [IX_dbo.Assets.ApiKeyEntity_Id] ON [dbo].[Assets] ([ApiKeyEntity_Id] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_dbo.Assets.InvTypes_TypeId] ON [dbo].[Assets] ([InvTypes_TypeId] ASC)
