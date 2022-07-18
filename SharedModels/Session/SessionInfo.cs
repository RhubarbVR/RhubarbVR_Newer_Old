using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels.Session
{
	public enum SessionAccessLevel
	{
		Private,
		Public,
		Friends,
		FriendsOfFriends,
		GroupOnly
	}
	public class SessionInfo
	{
		public bool IsHidden { get; set; }
		public SessionAccessLevel SessionAccessLevel { get; set; }
		public virtual Guid SessionId { get; set; }
		public Guid AssociatedGroup { get; set; }
		public string SessionName { get; set; }
		public string NormalizedSessionName { get; set; }
		public string ThumNail { get; set; }
		public virtual string[] SessionTags { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime LastUpdated { get; set; }
		public Guid Creator { get; set; }
		public int UserCount { get; set; }
		public int ActiveUsers { get; set; }
		public int MaxUsers { get; set; }
		public virtual Guid[] Users { get; set; }
		public virtual Guid[] Admins { get; set; }
		public string ClientVersion { get; set; }
	}
}
