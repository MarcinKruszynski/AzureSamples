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
            //var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=emkastorage;AccountKey=gkDT876QKKBoYLT/ghPmQU8TU+TnhWErtr0d9al0vq3bQftlWYhT6hnn7Ygxakf1r6m8QL5kia3lBftB0VZM5A==;EndpointSuffix=core.windows.net");

            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=emkacosmos;AccountKey=3Ai2tZsCCbJKvK1ud5msljU68JTibmU4buXlhimWlh8iMS1ZucLFzyg1SXmkuLrxtj1NE8YgOtBejCE6xvaI4g==;TableEndpoint=https://emkacosmos.table.cosmos.azure.com:443/");

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
