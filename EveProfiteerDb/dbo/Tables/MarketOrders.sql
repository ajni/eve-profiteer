CREATE TABLE [dbo].[MarketOrders]
(
	[Id] INT IDENTITY NOT NULL, 
    [OrderId] BIGINT NOT NULL, 
    [CharacterId] INT NOT NULL, 
    [StationId] INT NOT NULL, 
    [VolumeEntered] INT NOT NULL, 
    [VolumeRemaining] INT NOT NULL, 
    [MinVolume] INT NOT NULL, 
    [OrderState] INT NOT NULL, 
    [TypeId] INT NOT NULL, 
    [Range] INT NOT NULL, 
    [AccountKey] INT NOT NULL, 
    [Duration] INT NOT NULL, 
    [Escrow] DECIMAL(18, 2) NOT NULL, 
    [Price] DECIMAL(18, 2) NOT NULL, 
    [Bid] BIT NOT NULL, 
    [Issued] DATETIME2 NOT NULL,
	CONSTRAINT [PK_dbo.MarketOrders] PRIMARY KEY NONCLUSTERED ([Id] ASC),
	CONSTRAINT UQ_OrderId UNIQUE ([OrderId])
)
GO
CREATE  INDEX [IX_MarketOrders.CharacterId] ON [dbo].[MarketOrders] ([CharacterId] ASC)

GO
CREATE NONCLUSTERED INDEX [IX_MarketOrders.OrderId] ON [dbo].[MarketOrders] ([OrderId] ASC)
