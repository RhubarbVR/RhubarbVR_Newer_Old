using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace RhuEngine.Commads
{
	public class Logout : Command
	{
		public override string HelpMsg => "Logout user if there login";

		public override void RunCommand() {
			if (!Engine.MainEngine.netApiManager.IsLoggedIn) {
				Console.WriteLine("Need to be Loggedin to logout");
				return;
			}
			Engine.MainEngine.netApiManager.Logout();
		}
	}
	public class Login : Command
	{
		public override string HelpMsg => "Login a user with email and password";

		public override void RunCommand() {
			if (Engine.MainEngine.netApiManager.IsLoggedIn) {
				Console.WriteLine("Already login need to logout");
				return;
			}
			Console.WriteLine("Email");
			var email = ReadNextLine();
			Console.WriteLine("Password");
			var pass = PasswordInput();
			Task.Run(async () => {
				var req = await Engine.MainEngine.netApiManager.Login(email, pass);
				if (req.Login) {
					Console.WriteLine("Login Successfully as " + req.User.UserName);
				}
				else {
					Console.WriteLine("Failed to Login Error " + req?.Message??"Error is null");
				}
			});
		}
	}
}
