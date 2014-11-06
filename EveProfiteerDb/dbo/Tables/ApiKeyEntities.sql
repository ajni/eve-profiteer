CREATE TABLE [dbo].[ApiKeyEntities] (
    [Id]        INT IDENTITY NOT NULL PRIMARY KEY,
	[EntityId] INT NOT NULL DEFAULT 0,
    [Name]      NVARCHAR (MAX) NULL,
    [Type]      NVARCHAR (MAX) NULL,
    [ImagePath] NVARCHAR (MAX) NULL,
    [IsActive]  BIT            NOT NULL,
);

