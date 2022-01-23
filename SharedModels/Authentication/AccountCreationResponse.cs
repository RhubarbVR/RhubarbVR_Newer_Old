using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
	public class AccountCreationResponse
	{
		public string ErrorDetails { get; set; }
		public bool Error { get; set; }
		public string Message { get; set; }

		public AccountCreationResponse() {

		}
	}
}
