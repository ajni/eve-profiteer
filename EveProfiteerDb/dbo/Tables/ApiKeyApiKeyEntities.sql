CREATE TABLE [dbo].[ApiKeyApiKeyEntities] (
    [ApiKey_Id]       INT NOT NULL,
    [ApiKeyEntity_Id] INT NOT NULL,
    CONSTRAINT [PK_dbo.ApiKeyApiKeyEntities] PRIMARY KEY CLUSTERED ([ApiKey_Id] ASC, [ApiKeyEntity_Id] ASC),
    CONSTRAINT [FK_dbo.ApiKeyApiKeyEntities_dbo.ApiKeyEntities.ApiKeyEntity_Id] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [dbo].[ApiKeyEntities] ([Id]),
    CONSTRAINT [FK_dbo.ApiKeyApiKeyEntities_dbo.ApiKeys.ApiKey_Id] FOREIGN KEY ([ApiKey_Id]) REFERENCES [dbo].[ApiKeys] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_dbo.ApiKeyApiKeyEntities.ApiKey_Id]
    ON [dbo].[ApiKeyApiKeyEntities]([ApiKey_Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_dbo.ApiKeyApiKeyEntities.ApiKeyEntity_Id]
    ON [dbo].[ApiKeyApiKeyEntities]([ApiKeyEntity_Id] ASC);

