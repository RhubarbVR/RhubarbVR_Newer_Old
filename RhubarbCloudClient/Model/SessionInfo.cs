using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using DataModel.Enums;

namespace RhubarbCloudClient.Model
{
	public sealed class SessionInfo
	{
		public Guid ID { get; set; }
		public string SessionName { get; set; }

		public int MaxUsers { get; set; }
		public int UsersInSession { get; set; }

		public bool IsHidden { get; set; }
		public string ThumNail { get; set; }
		public bool IsAssociatedToGroup { get; set; }
		public Guid AssociatedGroup { get; set; }
		public Guid AssociatedWorld { get; set; }
		public DateTimeOffset CreatedDate { get; set; }
		public DateTimeOffset LastUpdated { get; set; }
		public string[] Tags { get; set; }
		public Guid[] PresentUsers { get; set; }
		public Guid[] Users { get; set; }
		public Guid Owner { get; set; }
		public Guid[] Admins { get; set; }
		public Guid[] Mods { get; set; }

		public AccessLevel SessionAccessLevel { get; set; }

	}
}
