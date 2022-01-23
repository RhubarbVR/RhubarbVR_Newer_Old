using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels
{
	public enum ConnectionType
	{
		None,
		Direct,
		HolePunch,
		Relay
	}
	public class UserSessionInfo
	{
		public ConnectionType ConnectionType { get; set; }
		public string Data { get; set; }
	}

	public class ConnectToUser
	{
		public string UserID { get; set; }
		public ConnectionType ConnectionType { get; set; }
		public string Data { get; set; }
	}
}
