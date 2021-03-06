﻿using eZet.EveLib.Modules;
using eZet.EveLib.Modules.Models;
using eZet.EveLib.Modules.Models.Character;
using eZet.EveLib.Modules.Models.Misc;
using eZet.EveProfiteer.Models;
using OrderType = eZet.EveLib.Modules.Models.OrderType;

namespace eZet.EveProfiteer.Util {
    public static class ApiEntityMapper {

        public static ApiKeyEntity Map(Character source, ApiKeyEntity target) {
            target.EntityId = (int)source.CharacterId;
            target.Name = source.CharacterName;
            target.Type = "Character";
            return target;
        }

        public static Transaction Map(WalletTransactions.Transaction source, Transaction target) {
            target.TransactionId = source.TransactionId;
            target.TransactionDate = source.TransactionDate;
            target.Quantity = source.Quantity;
            target.TypeId = source.TypeId;
            target.Price = source.Price;
            target.ClientId = source.ClientId;
            target.ClientName = source.ClientName;
            target.StationId = source.StationId;
            target.StationName = source.StationName;
            target.TransactionType = source.TransactionType == OrderType.Buy
                ? TransactionType.Buy
                : TransactionType.Sell;
            target.TransactionFor = source.TransactionFor;
            target.JournalTransactionId = source.JournalTransactionId;
            target.ClientTypeId = source.ClientTypeId;
            return target;
        }

        public static MarketOrder Map(MarketOrders.MarketOrder source, MarketOrder target) {
            target.OrderId = source.OrderId;
            target.ApiKeyEntityId = (int)source.CharacterId;
            target.StationId = source.StationId;
            target.VolumeEntered = source.VolumeEntered;
            target.VolumeRemaining = source.VolumeRemaining;
            target.MinVolume = source.MinVolume;
            target.OrderState = (OrderState)source.OrderState;
            target.TypeId = source.TypeId;
            target.Range = source.Range;
            target.AccountKey = source.AccountKey;
            target.Duration = source.Duration;
            target.Escrow = source.Escrow;
            target.Price = source.Price;
            target.Bid = source.Bid != 0;
            target.Issued = source.IssuedDate;
            return target;
        }

        public static JournalEntry Map(WalletJournal.JournalEntry source, JournalEntry target) {
            target.Date = source.Date;
            target.RefId = source.RefId;
            target.RefTypeId = source.RefTypeId;
            target.OwnerId = source.OwnerId;
            target.OwnerName = source.OwnerName;
            target.ParticipantId = source.ParticipantId;
            target.ParticipantName = source.ParticipantName;
            target.ArgumentId = source.ArgumentId;
            target.ArgumentName = source.ArgumentName;
            target.Amount = source.Amount;
            target.BalanceAfter = source.BalanceAfter;
            target.Reason = source.Reason;
            target.TaxReceiverId = source.TaxReceiverId;
            target.TaxAmount = source.TaxAmount;
            target.OwnerTypeId = source.OwnerTypeId;
            target.ParticipantTypeId = source.ParticipantTypeId;
            return target;
        }

        public static MarketBrowserOrder Map(EmdItemOrders.ItemOrderEntry source, MarketBrowserOrder target) {
            target.TypeId = source.TypeId;
            target.StationId = source.StationId;
            target.SolarSystemId = source.SolarSystemId;
            target.RegionId = source.RegionId;
            target.Price = source.Price;
            target.OrderId = source.OrderId;
            target.VolEntered = source.VolEntered;
            target.VolRemaining = source.VolRemaining;
            target.MinVolume = source.MinVolume;
            target.Range = source.Range;
            target.ExpiresDate = source.ExpiresDate;
            target.IssuedDate = source.IssuedDate;
            target.CreatedDate = source.CreatedDate;
            return target;
        }

        public static MarketHistoryAggregateEntry Map(CrestMarketHistory.MarketHistoryEntry source, MarketHistoryAggregateEntry target) {
            target.Volume = source.Volume;
            target.OrderCount = source.OrderCount;
            target.LowPrice = source.LowPrice;
            target.HighPrice = source.HighPrice;target.AvgPrice = source.AvgPrice;
            target.Date = source.Date;
            return target;
        }

        public static RefType Map(ReferenceTypes.ReferenceType source, RefType target) {
            target.Id = source.RefTypeId;
            target.Name = source.RefTypeName;
            return target;
        }
    }
}