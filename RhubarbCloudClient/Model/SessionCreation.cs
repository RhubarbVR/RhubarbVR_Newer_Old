using System;
using System.Collections.Generic;
using System.Text;

using DataModel.Enums;

namespace RhubarbCloudClient.Model
{
	public sealed class SessionCreation
	{
		public UserConnectionInfo UserConnectionInfo { get; set; }
		public string SessionName { get; set; }
		public string[] SessionTags { get; set; }
		public AccessLevel SessionAccessLevel { get; set; }
		public int MaxUsers { get; set; }
		public bool IsHidden { get; set; }
		public string ThumNail { get; set; }
		public bool IsAssociatedToGroup { get; set; }
		public Guid AssociatedGroup { get; set; }
		public Guid TempSessionID { get; set; }

		public string WorldID { get; set; }
	}
}
