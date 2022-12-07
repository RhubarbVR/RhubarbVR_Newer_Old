using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine;
using RhuEngine.Linker;
namespace RhuEngine.Commads
{
	public class Register : Command
	{
		public override string HelpMsg => "Register a RhubarbVR account";

		public override async Task RunCommand() {
			Console.WriteLine("Email");
			var email = ReadNextLine();
			Console.WriteLine("Username");
			var username = ReadNextLine();
			Console.WriteLine("Password");
			var Password = PasswordInput();
			Console.WriteLine("Confirm Password");
			var ConfirmPassword = PasswordInput();
			if (Password != ConfirmPassword) {
				Console.WriteLine("Passwords are not the same");
				return;
			}
			var req = await Engine.MainEngine.netApiManager.Client.RegisterAccount(username, email, Password);
			if (!req?.IsDataGood ?? false) {
				Console.WriteLine(req.Data);
			}
			else {
				Console.WriteLine("Failed to Create Account Error " + req?.Data ?? "Error is null");
			}
		}
	}
}

