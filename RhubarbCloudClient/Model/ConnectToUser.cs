using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public sealed class ConnectToUser
	{
		public ConnectionType ConnectionType { get; set; }
		public string Data { get; set; }
		public Guid UserID { get; set; }
		public string Server { get; set; }
	}
}
