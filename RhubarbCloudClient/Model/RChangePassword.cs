using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public sealed class RChangePassword
	{
		public string Email { get; set; }
		public string Token { get; set; }
		public string NewPassword { get; set; }
	}
}
