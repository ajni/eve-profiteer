CREATE TABLE [dbo].[Orders] (
    [Id]                INT             IDENTITY NOT NULL,
    [TypeId]            INT          NOT NULL,
    [BuyQuantity]       INT             NOT NULL,
    [MaxBuyPrice]       DECIMAL (18, 2) NOT NULL,
    [MinSellQuantity]   INT             NOT NULL,
    [MinSellPrice]      DECIMAL (18, 2) NOT NULL,
    [MaxSellQuantity]   INT             NOT NULL,
    [UpdateTime]        DATETIME2        NOT NULL,
    [AvgVolume]         FLOAT (53)      NOT NULL,
    [CurrentBuyPrice]   DECIMAL (18, 2) NOT NULL,
    [CurrentSellPrice]  DECIMAL (18, 2) NOT NULL,
    [AvgPrice]          DECIMAL (18, 2) NOT NULL,
    [ApiKeyEntity_Id] INT NOT NULL, 
    [IsSellOrder] BIT NOT NULL, 
    [IsBuyOrder] BIT NOT NULL, 
    [Notes] TEXT NULL, 
    StationId INT NULL, 
    [AutoProcess] BIT NOT NULL DEFAULT 1, 
    [MapRegion_Id] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.Orders] PRIMARY KEY NONCLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_dbo.Orders.ApiKeyEntities] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [ApiKeyEntities]([Id]), 
    CONSTRAINT [FK_dbo.Orders.InvTypes] FOREIGN KEY ([TypeId]) REFERENCES [invTypes]([typeID]), 
    CONSTRAINT [FK_dbo.Orders.StaStations] FOREIGN KEY ([StationId]) REFERENCES [staStations]([stationID]), 
)
GO
CREATE CLUSTERED INDEX [IX_dbo.Orders.ApiKeyEntity_Id] ON [dbo].[Orders] ([ApiKeyEntity_Id] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_dbo.Orders.InvType_Id] ON [dbo].[Orders] ([TypeId] ASC);