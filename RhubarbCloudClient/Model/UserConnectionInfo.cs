using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public sealed class UserConnectionInfo
	{
		public ConnectionType ConnectionType { get; set; }
		public Dictionary<string, int> ServerPingLevels { get; set; }
		public string Data { get; set; }
		public Guid UserID { get; set; }

	}
}
