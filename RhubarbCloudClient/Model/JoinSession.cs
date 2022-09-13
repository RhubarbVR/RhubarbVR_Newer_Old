using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public sealed class JoinSession
	{
		public Guid SessionID { get; set; }
		public UserConnectionInfo UserConnectionInfo { get; set; }
	}
}
