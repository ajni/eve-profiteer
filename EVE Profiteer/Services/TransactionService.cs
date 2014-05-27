using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class TransactionService {
        public TransactionService(EveProfiteerDbEntities db) {
            Db = db;
        }

        public EveProfiteerDbEntities Db { get; set; }

        public long GetLatestId(ApiKeyEntity entity) {
            return (from t in Db.Transactions
                where t.ApiKeyEntity.Id == entity.Id
                orderby t.TransactionId descending
                select t.TransactionId).FirstOrDefault();
        }


        public async Task ProcessInventory(IEnumerable<Transaction> transactions) {
            Db.Configuration.AutoDetectChangesEnabled = false;
            List<Asset> assets =
                await Db.Assets.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync();
            Dictionary<int, Asset> assetLookup = assets.ToDictionary(t => t.InvTypes_TypeId, t => t);
            foreach (Transaction transaction in transactions.OrderBy(t => t.TransactionDate)) {
                Asset asset;
                if (!assetLookup.TryGetValue(transaction.TypeId, out asset)) {
                    asset = new Asset();
                    asset.InvTypes_TypeId = transaction.TypeId;
                    asset.ApiKeyEntity_Id = transaction.ApiKeyEntity_Id;
                    Db.Assets.Add(asset);
                    assetLookup.Add(asset.InvTypes_TypeId, asset);
                }
                if (transaction.TransactionType == TransactionType.Buy) {
                    asset.TotalCost += transaction.Price*transaction.Quantity;
                    asset.Quantity += transaction.Quantity;
                    asset.LatestAverageCost = asset.TotalCost/asset.Quantity;
                    transaction.PerpetualAverageCost = asset.LatestAverageCost;
                    transaction.CurrentStock = asset.Quantity;
                }
                else if (transaction.TransactionType == TransactionType.Sell) {
                    if (asset.Quantity > 0) {
                        transaction.PerpetualAverageCost = asset.TotalCost/asset.Quantity;
                        asset.TotalCost -= transaction.Quantity*(asset.TotalCost/asset.Quantity);
                    }
                    else {
                        transaction.PerpetualAverageCost = asset.LatestAverageCost;
                    }
                    asset.Quantity -= transaction.Quantity;
                    transaction.CurrentStock = asset.Quantity;
                    if (asset.Quantity < 0) {
                        transaction.UnaccountedStock += Math.Abs(asset.Quantity);
                        transaction.CurrentStock = 0;
                        asset.UnaccountedQuantity += Math.Abs(asset.Quantity);
                        asset.TotalCost = 0;
                        asset.Quantity = 0;
                    }
                }
            }
            Db.ChangeTracker.DetectChanges();
            await Db.SaveChangesAsync();
            Db.Configuration.AutoDetectChangesEnabled = true;
        }

        public async Task ProcessTransactions(ICollection<Transaction> transactions) {
            await ProcessInventory(transactions);
            Db.Configuration.AutoDetectChangesEnabled = false;
            Db.Configuration.ValidateOnSaveEnabled = false;
            IList<Transaction> list = transactions as IList<Transaction> ?? transactions.ToList();
            int count = 0;
            foreach (Transaction transaction in list) {
                ++count;
                if (count%5000 == 0) {
                    Db.ChangeTracker.DetectChanges();
                    await Db.SaveChangesAsync();
                    Debug.WriteLine("Saved transactions:" + count);
                    //db.CreateNewContext();
                }
                Db.Transactions.Add(transaction);
            }
            Db.ChangeTracker.DetectChanges();
            await Db.SaveChangesAsync();
            Db.Configuration.AutoDetectChangesEnabled = true;
            Db.Configuration.ValidateOnSaveEnabled = true;
        }

        public static void ProcessTransactions<T>(string connection, string tableName, IList<T> list) {
            using (var bulkCopy = new SqlBulkCopy(connection)) {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                PropertyDescriptor[] props = TypeDescriptor.GetProperties(typeof (T))
                    //Dirty hack to make sure we only have system data types 
                    //i.e. filter out the relationships/collections
                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                    .ToArray();

                foreach (PropertyDescriptor propertyInfo in props) {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name,
                        Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (T item in list) {
                    for (int i = 0; i < values.Length; i++) {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }
    }
}