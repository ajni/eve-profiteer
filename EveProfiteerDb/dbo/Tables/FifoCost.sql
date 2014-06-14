CREATE TABLE [dbo].[FifoCost]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [BuyTransactionId] INT NOT NULL, 
    [SellTransactionId] INT NOT NULL, 
    [Quantity] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_FifoCost_BuyTranscation_ToTransactions] FOREIGN KEY ([BuyTransactionId]) REFERENCES [Transactions]([Id]),
    CONSTRAINT [FK_FifoCost_SellTranscation_ToTransactions] FOREIGN KEY ([SellTransactionId]) REFERENCES [Transactions]([Id])
)
