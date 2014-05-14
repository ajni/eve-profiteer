CREATE TABLE [dbo].[ApiKeyApiKeyEntities] (
    [ApiKey_Id]       INT NOT NULL,
    [ApiKeyEntity_Id] INT NOT NULL,
    CONSTRAINT [PK_dbo.ApiKeyApiKeyEntities] PRIMARY KEY CLUSTERED ([ApiKey_Id] ASC, [ApiKeyEntity_Id] ASC),
    CONSTRAINT [FK_dbo.ApiKeyApiKeyEntities_dbo.ApiKeyEntities_ApiKeyEntity_Id] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [dbo].[ApiKeyEntities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.ApiKeyApiKeyEntities_dbo.ApiKeys_ApiKey_Id] FOREIGN KEY ([ApiKey_Id]) REFERENCES [dbo].[ApiKeys] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_ApiKey_Id]
    ON [dbo].[ApiKeyApiKeyEntities]([ApiKey_Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ApiKeyEntity_Id]
    ON [dbo].[ApiKeyApiKeyEntities]([ApiKeyEntity_Id] ASC);

