CREATE TABLE [dbo].[ApiKeyEntities] (
    [Id]        INT IDENTITY NOT NULL,
	[EntityId] INT NOT NULL DEFAULT 0,
    [Name]      NVARCHAR (MAX) NULL,
    [Type]      NVARCHAR (MAX) NULL,
    [ImagePath] NVARCHAR (MAX) NULL,
    [IsActive]  BIT            NOT NULL,
	CONSTRAINT [PK_dbo.ApiKeyEntities] PRIMARY KEY NONCLUSTERED ([Id] ASC),
)

GO
CREATE NONCLUSTERED INDEX [IX_dbo.ApiKeyEntities.EntityId] ON [dbo].[ApiKeyEntities] ([EntityId])
