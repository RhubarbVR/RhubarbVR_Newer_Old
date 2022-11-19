using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
    public sealed class RCreateRecord
    {
        public long SizeInBytes { get; set; }

        public bool Public { get; set; }

		public bool PublicStaticURL { get; set; }

		public string ContentType { get; set; }
    }
}
