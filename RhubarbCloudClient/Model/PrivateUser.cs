using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public sealed class PrivateUser : PublicUser
	{
		public bool TwoFactorEnabled { get; set; }
		public long UsedBytes { get; set; }
		public long TotalBytes { get; set; }

		public PrivateUser() {

		}
	}
}
