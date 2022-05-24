using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace Rhubarb_VR_HeadLess.Commads
{
	public class Register : Command
	{
		public override string HelpMsg => "Register a RhubarbVR account";

		public override void RunCommand() {
			Console.WriteLine("Email");
			var email = Console.ReadLine();
			Console.WriteLine("Username");
			var username = Console.ReadLine();
			Console.WriteLine("Password");
			var Password = Login.MaskPass();
			Console.WriteLine("Confirm Password");
			var ConfirmPassword = Login.MaskPass();
			if (Password != ConfirmPassword) {
				Console.WriteLine("Passwords are not the same");
				return;
			}
			Task.Run(async () => {
				var req = await Program._app.netApiManager.SignUp(username, email,Password,DateTime.UtcNow);
				if (!req?.Error??false) {
					Console.WriteLine(req.Message);
				}
				else {
					Console.WriteLine("Failed to Create Account Error " + req?.Message ?? "Error is null");
				}
			});
		}
	}
}
