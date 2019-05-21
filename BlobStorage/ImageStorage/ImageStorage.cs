using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace ImagesStorage
{
    public class ImageStorage : IImageStorage
    {
        public async Task UploadImageAsync(byte[] bytes, string blobName)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=emkastorage;AccountKey=xNI29xYCrLT7szPqHnZ4Lc+72kz6erRpT867SCkBvomqO5A3T2JqWNi5h3HsdvvAUZ0UqH2AXkXxI9+vO2Hj8w==;EndpointSuffix=core.windows.net");            

            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

            await cloudBlobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, null, null);

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

            await cloudBlockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }

        public async Task<bool> CheckIfBlobExistsAsync(string blobName)
        {
            return false;
        }
    }
}
