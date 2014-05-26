CREATE TABLE [dbo].[Inventory]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [InvTypes_TypeId] INT NOT NULL, 
    [Stock] INT NOT NULL, 
    [Cost] DECIMAL NOT NULL,
	[ApiKeyEntity_Id] INT NOT NULL, 
    CONSTRAINT [FK_dbo.Inventory_dbo.ToInvTypes] FOREIGN KEY ([InvTypes_TypeId]) REFERENCES [invTypes]([typeID]),
    CONSTRAINT [FK_dbo.Inventory_dbo.ToApiKeyEntities] FOREIGN KEY ([ApiKeyEntity_Id]) REFERENCES [ApiKeyEntities]([Id]),

)
GO
CREATE INDEX [IX_dbo.Inventory_dbo.InvTypes_TypeId] ON [dbo].[Inventory] ([InvTypes_TypeId])
GO
CREATE INDEX [IX_dbo.Inventory_dbo.ApiKeyEntity_Id] ON [dbo].[Inventory] ([ApiKeyEntity_Id])
