using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesApp
{
    public class ImageBlob
    {
        public string BlobName { get; set; }
        public string BlobUri { get; set; }

        public CloudBlockBlob Blob { get; set; }
    }
}
