using System;
using System.Collections.Generic;
using System.Text;

namespace TableStorageApp
{
    public class Song
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
