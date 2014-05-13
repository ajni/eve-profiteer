﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class TransactionService : RepositoryService<Transaction> {

        public TransactionService(IRepository<Transaction> repository)
            : base(repository) {
        }

        public long GetLatestId(ApiKeyEntity entity) {
            return (from t in Repository.All()
                    where t.ApiKeyEntity.Id == entity.Id
                    orderby t.TransactionId descending
                    select t.TransactionId).FirstOrDefault();
        }

        public IEnumerable<Transaction> RemoveAll(ApiKeyEntity entity) {
            return Repository.RemoveRange(Repository.All().Where(i => i.ApiKeyEntity.Id == entity.Id));
        }

        public IEnumerable<Transaction> AddNew(IEnumerable<Transaction> list) {
            var db = (DbContextRepository<Transaction>)Repository;
            var addNew = list as IList<Transaction> ?? list.ToList();
            db.DbContext.Configuration.ValidateOnSaveEnabled = false;
            db.DbContext.Configuration.AutoDetectChangesEnabled = false;
            db.AddRange(addNew);
            db.SaveChanges();
            //BulkInsert((db.DbContext.Database.Connection.ConnectionString, "dbo.Transactions", addNew);
            return addNew;
        }

        public static void BulkInsert<T>(string connection, string tableName, IList<T> list) {
            using (var bulkCopy = new SqlBulkCopy(connection)) {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))
                    //Dirty hack to make sure we only have system data types 
                    //i.e. filter out the relationships/collections
                                           .Cast<PropertyDescriptor>()
                                           .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                           .ToArray();

                foreach (var propertyInfo in props) {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list) {
                    for (var i = 0; i < values.Length; i++) {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }

    }
}
