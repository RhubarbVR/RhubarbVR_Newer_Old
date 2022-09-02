using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
    public class CreateRecordResponses
    {
        public string TempUploadURL { get; set; }

        public Guid RecordID { get; set; }
    }
}
