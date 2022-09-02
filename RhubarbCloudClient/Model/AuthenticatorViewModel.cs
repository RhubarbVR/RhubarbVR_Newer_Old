using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public class AuthenticatorViewModel
	{
		public string SecretKey { get; set; }

		public string BarcodeUrl { get; set; }

		public string Code { get; set; }
	}
}
