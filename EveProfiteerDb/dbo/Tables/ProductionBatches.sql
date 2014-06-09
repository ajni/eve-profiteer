﻿CREATE TABLE [dbo].[ProductionBatches]
(
	[Id] INT IDENTITY NOT NULL PRIMARY KEY, 
    [ProductTypeId] INT NOT NULL, 
    [BlueprintTypeId] INT NOT NULL, 
    [ProductionQuantity] INT NOT NULL, 
    [QuantityLeft] INT NOT NULL , 
    [Date] DATETIME2 NOT NULL , 
    [OtherCost] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    [TotalSales] DECIMAL(18, 5) NOT NULL DEFAULT 0, 
    [BlueprintME] INT NOT NULL DEFAULT 0
    CONSTRAINT [FK_ProductionBatches_ToInvTypes] FOREIGN KEY ([ProductTypeId]) REFERENCES [invTypes]([typeID]), 
    [CharacterME] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_ProductionBatches_ToInvBlueprintTypes] FOREIGN KEY ([BlueprintTypeId]) REFERENCES [invBlueprintTypes]([blueprintTypeID]), 
)