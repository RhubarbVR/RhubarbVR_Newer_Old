using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels.UserInfo
{
	public class FriendRequest
	{
		public int Attempts { get; set; }

		public bool Block { get; set; }

		public string FromUser { get; set; }

		public DateTime TimeRequested { get; set; }

	}
}
