CREATE VIEW [dbo].[Transactions]
	AS SELECT TD.*, T.typeName Name FROM [TransactionData] TD LEFT JOIN [$(EveDb)].[dbo].[invTypes] T ON TD.TypeId = T.typeID;
