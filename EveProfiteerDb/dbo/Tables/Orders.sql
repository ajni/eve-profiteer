CREATE TABLE [dbo].[Orders] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [ItemName]          NVARCHAR (MAX)  NULL,
    [ItemId]            BIGINT          NOT NULL,
    [BuyQuantity]       INT             NOT NULL,
    [MaxBuyPrice]       DECIMAL (18, 2) NOT NULL,
    [TotalMaxBuyPrice]  DECIMAL (18, 2) NOT NULL,
    [MinSellQuantity]   INT             NOT NULL,
    [MinSellPrice]      DECIMAL (18, 2) NOT NULL,
    [TotalMinSellPrice] DECIMAL (18, 2) NOT NULL,
    [MaxSellQuantity]   INT             NOT NULL,
    [TotalMaxSellPrice] DECIMAL (18, 2) NOT NULL,
    [UpdateTime]        DATETIME        NOT NULL,
    [AvgVolume]         FLOAT (53)      NOT NULL,
    [CurrentBuyPrice]   DECIMAL (18, 2) NOT NULL,
    [CurrentSellPrice]  DECIMAL (18, 2) NOT NULL,
    [AvgPrice]          DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_dbo.Orders] PRIMARY KEY CLUSTERED ([Id] ASC)
);

