CREATE TABLE [dbo].[JournalEntries] (
    [Id]              INT             IDENTITY NOT NULL,
    [Date]            DATETIME        NOT NULL,
    [RefId]           BIGINT          NOT NULL,
    [RefTypeId]       INT          NOT NULL,
    [OwnerName]       NVARCHAR (MAX)  NULL,
    [OwnerId]         BIGINT          NOT NULL,
    [ParticipantName] NVARCHAR (MAX)  NULL,
    [ParticipantId]   BIGINT          NOT NULL,
    [ArgumentName]    NVARCHAR (MAX)  NULL,
    [ArgumentId]      BIGINT          NOT NULL,
    [Amount]          DECIMAL (18, 2) NOT NULL,
    [BalanceAfter]    DECIMAL (18, 2) NOT NULL,
    [Reason]          NVARCHAR (MAX)  NULL,
    [TaxReceiverId]   NVARCHAR (MAX)  NULL,
    [TaxAmount]       NVARCHAR (MAX)  NULL,
    [ApiKeyEntity_Id] INT             NULL,
    [OwnerTypeId] INT NOT NULL, 
    [ParticipantTypeId] INT NOT NULL, 
    CONSTRAINT [PK_dbo.JournalEntries] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.JournalEntries_dbo.ApiKeyEntities_ApiKeyEntity_Id] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [dbo].[ApiKeyEntities] ([Id])
);

GO
CREATE CLUSTERED INDEX [IX_ApiKeyEntity_Id] ON [dbo].[JournalEntries]([ApiKeyEntity_Id] ASC);

