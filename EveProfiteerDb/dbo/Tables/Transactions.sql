CREATE TABLE [dbo].[Transactions] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [TransactionDate] DATETIME        NOT NULL,
    [TransactionId]   BIGINT          NOT NULL,
    [Quantity]        INT             NOT NULL,
    [TypeId]          INT          NOT NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [ClientId]        BIGINT          NOT NULL,
    [ClientName]      NVARCHAR (MAX)  NULL,
    [StationId]       BIGINT          NOT NULL,
    [StationName]     NVARCHAR (MAX)  NULL,
    [TransactionType] INT    NOT NULL,
    [TransactionFor]  NVARCHAR (MAX)  NULL,
    [ApiKeyEntity_Id] INT             NOT NULL,
    [JournalTransactionId] BIGINT	  NOT NULL, 
    [ClientTypeId]	  INT NOT NULL, 
    [PerpetualAverageCost] DECIMAL(18, 5) NOT NULL, 
    [CurrentStock] INT NOT NULL DEFAULT 0, 
    [UnaccountedStock] INT NOT NULL DEFAULT 0, 
    [TaxLiability] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [BrokerFee] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Transactions_dbo.ApiKeyEntities_ApiKeyEntity_Id] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [dbo].[ApiKeyEntities] ([Id]), 
	CONSTRAINT [FK_Transaction_ToInvType] FOREIGN KEY ([TypeId]) REFERENCES [invTypes]([typeID]), 
    UNIQUE ([TransactionId]),
);

GO
CREATE NONCLUSTERED INDEX [IX_ApiKeyEntity_Id]
    ON [dbo].[Transactions]([ApiKeyEntity_Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TypeId]
    ON [dbo].[Transactions]([TypeId] ASC);