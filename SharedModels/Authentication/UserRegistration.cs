using System;

namespace SharedModels
{
	public class UserRegistration
	{
		public string CreateKey { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public DateTime DateOfBirth { get; set; }

		public UserRegistration() {

		}
	}
}