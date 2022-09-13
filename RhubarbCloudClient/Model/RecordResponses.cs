using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
    public sealed class RecordResponses
    {
        public Guid RecordID { get; set; }

        public bool Private { get; set; }

        public long SizeInBytes { get; set; }
    }
}
