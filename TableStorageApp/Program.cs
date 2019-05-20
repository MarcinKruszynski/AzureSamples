using System;
using System.Linq;

namespace TableStorageApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new SongRepository();

            var rowKey = Guid.NewGuid().ToString();

            repository.CreateOrUpdate(new SongEntity
            {
                PartitionKey = "Rap",
                RowKey = rowKey,
                Title = "Gwiazdy"
            });

            var entities = repository.All();

            var songs = entities.Select(x => new Song
            {
                Id = x.RowKey,
                Group = x.PartitionKey,
                Title = x.Title,
                Artist = x.Artist,
                Timestamp = x.Timestamp
            }).ToList();

            var item = repository.Get("Rap", rowKey);

            repository.Delete(item);


            entities = repository.All();

            songs = entities.Select(x => new Song
            {
                Id = x.RowKey,
                Group = x.PartitionKey,
                Title = x.Title,
                Artist = x.Artist,
                Timestamp = x.Timestamp
            }).ToList();

            Console.WriteLine("Hello World!");
        }
    }
}
