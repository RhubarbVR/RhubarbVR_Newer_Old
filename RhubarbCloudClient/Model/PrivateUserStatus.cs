using System;
using System.Collections.Generic;
using System.Text;

using DataModel.Enums;

namespace RhubarbCloudClient.Model
{
	public class PrivateUserStatus : PublicUserStatus
	{
		public PrivateUserStatus() {

		}
		public string ClientCompatibility { get; set; }
		public AccessLevel UserStatusAccess { get; set; }
	}
}
