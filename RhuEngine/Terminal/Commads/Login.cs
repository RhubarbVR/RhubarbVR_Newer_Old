using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
namespace RhuEngine.Commads
{
	public class Logout : Command
	{
		public override string HelpMsg => "Logout user if there login";

		public override async Task RunCommand() {
			if (!EngineHelpers.MainEngine.netApiManager.Client.IsLogin) {
				Console.WriteLine("Need to be Loggedin to logout");
				return;
			}
			await EngineHelpers.MainEngine.netApiManager.Client.LogOut();
		}
	}
	public class Login : Command
	{
		public override string HelpMsg => "Login a user with email and password";

		public override async Task RunCommand() {
			if (EngineHelpers.MainEngine.netApiManager.Client.IsLogin) {
				Console.WriteLine("Already login need to logout");
				return;
			}
			Console.WriteLine("Email");
			var email = ReadNextLine();
			Console.WriteLine("Password");
			var pass = PasswordInput();
			var req = await EngineHelpers.MainEngine.netApiManager.Client.Login(email, pass, null);
			if (req.Error) {
				Console.WriteLine("Login Successfully as " + req.Data.UserName);
			}
			else {
				Console.WriteLine("Failed to Login Error " + req.MSG ?? "Error is null");
			}
		}
	}
}
