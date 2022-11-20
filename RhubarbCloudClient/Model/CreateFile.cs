using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace RhubarbCloudClient.Model
{
	public class CreateFile
	{
		public string Name { get; set; }
		public Guid Thumbnail { get; set; }
		public Guid MainRecordId { get; set; }

		public Guid[] AllOtherRecords { get; set; }

		public CreateFile() {

		}
	}
}
