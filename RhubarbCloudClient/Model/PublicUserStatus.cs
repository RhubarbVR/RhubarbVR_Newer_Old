using System;
using System.Collections.Generic;
using System.Text;

using DataModel.Enums;

namespace RhubarbCloudClient.Model
{
	public class PublicUserStatus
	{
		public Guid Id { get; set; }
		public UserStatus Status { get; set; }
		public string CustomStatusMsg { get; set; }
		public string ClientVersion { get; set; }
		public string Device { get; set; }

	}
}
