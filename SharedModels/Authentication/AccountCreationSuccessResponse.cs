using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedModels
{
	public class AccountCreationSuccessResponse : AccountCreationResponse
	{
		public static AccountCreationResponse AccountCreatedResponse(string email) {
			return new AccountCreationResponse() {
				Error = false,
				Message = $"Your account was created. We have sent an email to '{email}', Please verify your email."
			};
		}
	}
}
