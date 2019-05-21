using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesStorage
{
    public class ImageStorage : IImageStorage
    {
        public async Task<CloudBlockBlob> UploadImageAsync(byte[] bytes, string blobName)
        {
            CloudBlobContainer cloudBlobContainer = await GetImagesContainerAsync();

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

            cloudBlockBlob.Properties.ContentType = "image/png";

            await cloudBlockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);

            return cloudBlockBlob;
        }

        private static async Task<CloudBlobContainer> GetImagesContainerAsync()
        {
            var cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=emkastorage;AccountKey=xNI29xYCrLT7szPqHnZ4Lc+72kz6erRpT867SCkBvomqO5A3T2JqWNi5h3HsdvvAUZ0UqH2AXkXxI9+vO2Hj8w==;EndpointSuffix=core.windows.net");
            //var cloudStorageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");

            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

            await cloudBlobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, null, null);
            return cloudBlobContainer;
        }

        public async Task<bool> CheckIfBlobExistsAsync(string blobName)
        {
            CloudBlobContainer cloudBlobContainer = await GetImagesContainerAsync();

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

            return await cloudBlockBlob.ExistsAsync();            
        }

        public async Task<IEnumerable<CloudBlockBlob>> ListImageBlobsAsync(string prefix = null)
        {
            var cloudBlockBlobs = new List<CloudBlockBlob>();
            CloudBlobContainer cloudBlobContainer = await GetImagesContainerAsync();

            BlobContinuationToken token = null;
            do
            {
                var blobResultSegment = await cloudBlobContainer.ListBlobsSegmentedAsync(prefix, token);
                //var blobResultSegment = await cloudBlobContainer.ListBlobsSegmentedAsync(null, true, BlobListingDetails.None, 2, token, null, null);
                token = blobResultSegment.ContinuationToken;
                cloudBlockBlobs.AddRange(blobResultSegment.Results.OfType<CloudBlockBlob>());
            }
            while (token != null);            

            return cloudBlockBlobs;
        }
    }
}
