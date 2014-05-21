CREATE VIEW [dbo].[Orders]
	AS SELECT O.*, T.typeName TypeName FROM [OrderData] O LEFT JOIN [$(EveDb)].[dbo].[invTypes] T ON O.TypeId = T.typeID;
