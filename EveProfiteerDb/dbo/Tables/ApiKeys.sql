CREATE TABLE [dbo].[ApiKeys] (
    [Id]       INT         IDENTITY NOT NULL,
    [ApiKeyId] INT         NOT NULL,
    [VCode]    NVARCHAR (MAX) NULL,
    [KeyType]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ApiKeys] PRIMARY KEY CLUSTERED ([Id] ASC)
);

