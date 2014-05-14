CREATE TABLE [dbo].[ApiKeys] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [ApiKeyId] BIGINT         NOT NULL,
    [VCode]    NVARCHAR (MAX) NULL,
    [KeyType]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ApiKeys] PRIMARY KEY CLUSTERED ([Id] ASC)
);

