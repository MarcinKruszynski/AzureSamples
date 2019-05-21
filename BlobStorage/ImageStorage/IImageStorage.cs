using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ImagesStorage
{
    public interface IImageStorage
    {
        Task<CloudBlockBlob> UploadImageAsync(byte[] bytes, string blobName);
        Task<bool> CheckIfBlobExistsAsync(string blobName);
        Task<IEnumerable<CloudBlockBlob>> ListImageBlobsAsync(string prefix = null);
    }
}
