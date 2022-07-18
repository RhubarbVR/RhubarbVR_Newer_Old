using System;
using System.Collections.Generic;
using System.Text;

using SharedModels.Session;

namespace SharedModels.UserSession
{
	public class JoinSession
	{
		public Guid SessionID { get; set; }

		public UserConnectionInfo UserConnectionInfo { get; set; }
	}
}
