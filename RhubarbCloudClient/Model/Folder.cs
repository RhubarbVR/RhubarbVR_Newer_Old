using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace RhubarbCloudClient.Model
{
	public class SyncFolder
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public DateTimeOffset CreationDate { get; set; }
		public DateTimeOffset UpdateDate { get; set; }
		public Guid? ParrentFolderId { get; set; }
		public Guid? AssociatedGroupId { get; set; }
		public Guid? AssociatedUserId { get; set; }

		public SyncFolder[] Folders { get; set; }

		public SyncFile[] Files { get; set; }

		public SyncFolder() {

		}
	}
}
