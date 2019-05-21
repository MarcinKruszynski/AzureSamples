using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ImagesStorage
{
    public interface IImageStorage
    {
        Task UploadImageAsync(byte[] bytes, string blobName);
        Task<bool> CheckIfBlobExistsAsync(string blobName);
    }
}
