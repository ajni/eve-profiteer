﻿--CREATE VIEW [dbo].[Orders]
--	AS SELECT O.*, T.typeName TypeName FROM [dbo].[OrderData] O INNER JOIN [dbo].[invTypes] T ON O.TypeId = T.typeID;
--	GO;
--	--CREATE UNIQUE CLUSTERED INDEX [IDX_dbo.TypeId] ON [dbo].[Orders] ([TypeId]);
--	GO;
