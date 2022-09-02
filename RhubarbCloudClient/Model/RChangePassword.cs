using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public class RChangePassword
	{
		public string Email { get; set; }
		public string Token { get; set; }
		public string NewPassword { get; set; }
	}
}
