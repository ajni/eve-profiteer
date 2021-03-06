﻿CREATE TABLE [dbo].[Transactions] (
    [Id]              INT             IDENTITY NOT NULL,
    [TransactionDate] DATETIME        NOT NULL,
    [TransactionId]   BIGINT          NOT NULL,
    [Quantity]        INT             NOT NULL,
    [TypeId]          INT          NOT NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [ClientId]        BIGINT          NOT NULL,
    [ClientName]      NVARCHAR (MAX)  NULL,
    [StationId]       INT          NOT NULL,
    [StationName]     NVARCHAR (MAX)  NULL,
    [TransactionType] INT    NOT NULL,
    [TransactionFor]  NVARCHAR (MAX)  NULL,
    [ApiKeyEntity_Id] INT             NOT NULL,
    [JournalTransactionId] BIGINT	  NOT NULL, 
    [ClientTypeId]	  INT NOT NULL, 
    [PerpetualAverageCost] DECIMAL(18, 5) NOT NULL, 
    [PostTransactionStock] INT NOT NULL DEFAULT 0, 
    [UnaccountedQuantity] INT NOT NULL DEFAULT 0, 
    [TaxLiability] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [BrokerFee] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [CogsBrokerFees] DECIMAL(18, 5) NULL, 
    [CogsMaterialCost] DECIMAL(18, 5) NULL, 
    CONSTRAINT [PK_dbo.Transactions] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Transactions_dbo.ApiKeyEntities.ApiKeyEntity_Id] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [dbo].[ApiKeyEntities] ([Id]), 
	CONSTRAINT [FK_dbo.Transactions_dbo.InvType.TypeId] FOREIGN KEY ([TypeId]) REFERENCES [invTypes]([typeID]), 
	CONSTRAINT [FK_dbo.Transactions.StationId] FOREIGN KEY ([StationId]) REFERENCES [staStations]([stationID]),
);

GO
CREATE NONCLUSTERED INDEX [IX_Transactions.ApiKeyEntity_Id]
    ON [dbo].[Transactions]([ApiKeyEntity_Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Transactions.TypeId]
    ON [dbo].[Transactions]([TypeId] ASC);

	GO
CREATE NONCLUSTERED INDEX [IX_Transactions.TransactionDate]
    ON [dbo].[Transactions]([TransactionDate] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Transactions.TransactionId]
    ON [dbo].[Transactions]([TransactionId] ASC);