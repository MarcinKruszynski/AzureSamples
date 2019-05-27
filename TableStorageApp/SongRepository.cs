using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableStorageApp
{
    public class SongRepository
    {
        private CloudTable songsTable = null;

        public SongRepository()
        {
            //var storageAccount = CloudStorageAccount.Parse("");

            var storageAccount = CloudStorageAccount.Parse("");

            var tableClient = storageAccount.CreateCloudTableClient();

            songsTable = tableClient.GetTableReference("Songs");
        }

        public IEnumerable<SongEntity> All()
        {
            var isRap = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Rap");

            var query = new TableQuery<SongEntity>().Where(isRap);

            var entities = songsTable.ExecuteQuery(query);

            return entities;
        }

        public void CreateOrUpdate(SongEntity entity)
        {
            var operation = TableOperation.InsertOrReplace(entity);

            songsTable.Execute(operation);
        }

        public void Delete(SongEntity entity)
        {
            var operation = TableOperation.Delete(entity);

            songsTable.Execute(operation);
        }

        public SongEntity Get(string partitionKey, string rowKey)
        {
            var operation = TableOperation.Retrieve<SongEntity>(partitionKey, rowKey);

            var result = songsTable.Execute(operation);

            return result.Result as SongEntity;
        }
    }

    public class SongEntity: TableEntity
    {
        public string Title { get; set; }
        public string Artist { get; set; }
    }
}
