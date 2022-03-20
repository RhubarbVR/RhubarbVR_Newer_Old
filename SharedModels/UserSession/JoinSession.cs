using System;
using System.Collections.Generic;
using System.Text;

using SharedModels.Session;

namespace SharedModels.UserSession
{
	public class JoinSession
	{
		public string SessionID { get; set; }

		public UserConnectionInfo UserConnectionInfo { get; set; }
	}
}
