using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels
{
	public class SessionInfo
	{
		public string SessionId { get; set; }
		public string SessionName { get; set; }
		public DateTime CreatedDate { get; set; }
		public string Creator { get; set; }
		public int UserCount { get; set; }
	}
}
