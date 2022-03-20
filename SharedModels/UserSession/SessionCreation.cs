using System;
using System.Collections.Generic;
using System.Text;

using SharedModels.Session;

namespace SharedModels.UserSession
{
	public class SessionCreation
	{
		public SessionInfo SessionInfo { get; set; }

		public string[] ForceJoin { get; set; }
		public UserConnectionInfo UserConnectionInfo { get; set; }
	}
}
