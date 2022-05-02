using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedModels.UserInfo;

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
			User = new PrivateUser(user);
		}

	}

	public class PublicUser
	{

		public PublicUser(IRhubarbIdentity user) {
			EmailConfirmed = user?.EmailConfirmed??false;
			UserName = user?.UserName;
			Id = user?.Id;
			NormalizedUserName = user?.NormalizedUserName;
		}

		public PublicUser() {
		}
		public bool EmailConfirmed { get; set; }

		public string NormalizedUserName { get; set; }

		public string UserName { get; set; }

		public List<string> Roles { get; set; }

		public string Id { get; set; }
	}

	public class PrivateUser : IRhubarbIdentity
	{

		public PrivateUser(IRhubarbIdentity rhubarbIdentity) {
			TwoFactorEnabled = rhubarbIdentity.TwoFactorEnabled;
			PhoneNumberConfirmed = rhubarbIdentity.PhoneNumberConfirmed;
			PhoneNumber = rhubarbIdentity.PhoneNumber;
			EmailConfirmed = rhubarbIdentity.EmailConfirmed;
			NormalizedEmail = rhubarbIdentity.NormalizedEmail;
			Email = rhubarbIdentity.Email;
			NormalizedUserName = rhubarbIdentity.NormalizedUserName;
			UserName=rhubarbIdentity.UserName;
			Id=rhubarbIdentity.Id;
			DateOfBirth = rhubarbIdentity.DateOfBirth;
			Friends = rhubarbIdentity.Friends;
			FriendRequests	= rhubarbIdentity.FriendRequests;
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

		public string[] Friends { get; set; }
		public FriendRequest[] FriendRequests { get; set; }
	}
}