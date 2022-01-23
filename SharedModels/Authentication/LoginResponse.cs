using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
	public class LoginResponse
	{
		public bool Login { get; set; }

		public string Message { get; set; }

		public PrivateUser User { get; set; }

		public LoginResponse() {

		}

		public LoginResponse(string FailMessage) {
			Login = false;
			Message = FailMessage;
		}

		public LoginResponse(IRhubarbIdentity user) {
			Login = true;
			Message = $"Login as {user.UserName}";
			User = new PrivateUser(user.TwoFactorEnabled, user.PhoneNumberConfirmed, user.PhoneNumber, user.EmailConfirmed, user.NormalizedEmail, user.Email, user.NormalizedUserName, user.UserName, user.Id, user.DateOfBirth);
		}

	}



	public class PrivateUser : IRhubarbIdentity
	{

		public PrivateUser(bool twoFactorEnabled, bool phoneNumberConfirmed, string phoneNumber, bool emailConfirmed, string normalizedEmail, string email, string normalizedUserName, string userName, string id, DateTime dateof) {
			TwoFactorEnabled = twoFactorEnabled;
			PhoneNumberConfirmed = phoneNumberConfirmed;
			PhoneNumber = phoneNumber;
			EmailConfirmed = emailConfirmed;
			NormalizedEmail = normalizedEmail;
			Email = email;
			NormalizedUserName = normalizedUserName;
			UserName = userName;
			Id = id;
			DateOfBirth = dateof;
		}

		public PrivateUser() {
		}

		public bool TwoFactorEnabled { get; set; }

		public bool PhoneNumberConfirmed { get; set; }

		public string PhoneNumber { get; set; }

		public bool EmailConfirmed { get; set; }

		public string NormalizedEmail { get; set; }

		public string Email { get; set; }

		public string NormalizedUserName { get; set; }

		public string UserName { get; set; }

		public string Id { get; set; }

		public DateTime DateOfBirth { get; set; }
	}
}