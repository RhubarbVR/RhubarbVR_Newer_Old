using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace Rhubarb_VR_HeadLess.Commads
{
	public class Logout : Command
	{
		public override string HelpMsg => "Logout user if there login";

		public override void RunCommand() {
			if (!Program._app.netApiManager.IsLoggedIn) {
				Console.WriteLine("Need to be Loggedin to logout");
				return;
			}
			Program._app.netApiManager.Logout();
		}
	}
	public class Login : Command
	{
		public override string HelpMsg => "Login a user with email and password";

		public override void RunCommand() {
			if (Program._app.netApiManager.IsLoggedIn) {
				Console.WriteLine("Already login need to logout");
				return;
			}
			Console.WriteLine("Email");
			var email = Console.ReadLine();
			Console.WriteLine("Password");
			var pass = MaskPass();
			Task.Run(async () => {
				var req = await Program._app.netApiManager.Login(email, pass);
				if (req.Login) {
					Console.WriteLine("Login Successfully as " + req.User.UserName);
				}
				else {
					Console.WriteLine("Failed to Login Error " + req?.Message??"Error is null");
				}
			});
		}

		public static string MaskPass() {
			var pass = "";
			ConsoleKeyInfo key;
			do {
				key = Console.ReadKey(true);
				if (key.Key is not ConsoleKey.Backspace and not ConsoleKey.Enter) {
					pass += key.KeyChar;
					Console.Write("*");
				}
				else {
					if (key.Key == ConsoleKey.Backspace && pass.Length > 0) {
						pass = pass.Substring(0, pass.Length - 1);
						Console.Write("\b \b");
					}
					else if (key.Key == ConsoleKey.Enter) {
						break;
					}
				}
			} while (key.Key != ConsoleKey.Enter);
			Console.WriteLine("");
			return pass;
		}
	}
}
