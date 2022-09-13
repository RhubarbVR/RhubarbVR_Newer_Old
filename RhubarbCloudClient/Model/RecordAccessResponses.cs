using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
    public sealed class RecordAccessResponses
    {
        public Guid RecordID { get; set; }

        public long SizeInBytes { get; set; }

        public string TempURL { get; set; }
    }
}
