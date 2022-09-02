using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
    public class RCreateRecord
    {
        public long SizeInBytes { get; set; }

        public bool Public { get; set; }

        public string ContentType { get; set; }
    }
}
