﻿CREATE TABLE [dbo].[Transactions] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [TransactionDate] DATETIME        NOT NULL,
    [TransactionId]   BIGINT          NOT NULL,
    [Quantity]        INT             NOT NULL,
    [TypeName]        NVARCHAR (MAX)  NOT NULL,
    [TypeId]          INT          NOT NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [ClientId]        BIGINT          NOT NULL,
    [ClientName]      NVARCHAR (MAX)  NULL,
    [StationId]       BIGINT          NOT NULL,
    [StationName]     NVARCHAR (MAX)  NULL,
    [TransactionType] NVARCHAR(50)    NOT NULL,
    [TransactionFor]  NVARCHAR (MAX)  NULL,
    [ApiKeyEntity_Id] INT             NULL,
    [JournalTransactionId] BIGINT	  NOT NULL, 
    [ClientTypeId]	  INT NOT NULL, 
    CONSTRAINT [PK_dbo.Transactions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Transactions_dbo.ApiKeyEntities_ApiKeyEntity_Id] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [dbo].[ApiKeyEntities] ([Id]), 
    UNIQUE ([TransactionId]),
);

GO
CREATE NONCLUSTERED INDEX [IX_ApiKeyEntity_Id]
    ON [dbo].[Transactions]([ApiKeyEntity_Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_TypeId]
    ON [dbo].[Transactions]([TypeId] ASC);