using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels.Session
{
	public enum ConnectionType
	{
		None,
		Direct,
		HolePunch,
		Relay
	}
	public class UserConnectionInfo
	{
		public virtual Dictionary<string,int> ServerPingLevels { get; set; }
		public ConnectionType ConnectionType { get; set; }
		public string Data { get; set; }
	}

	public class ConnectToUser
	{
		public string UserID { get; set; }
		public string Server { get; set; }
		public ConnectionType ConnectionType { get; set; }
		public string Data { get; set; }
	}
}
