using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient
{
	public class DynamicLocalisation:Localisation
	{
		public DynamicLocalisation(Func<IEnumerable<string>> files, Func<string,string> fileRead,Action reload, string three_Letter_other = null) {
			Files = files;
			FileRead = fileRead;
			Three_Letter_other = three_Letter_other;
			LocalReload += reload;
			try {
				LoadLocal();
			}
			catch (Exception e) {
				Log($"Error to load local {e}");
			}
		}

		public Func<IEnumerable<string>> Files;

		public Func<string,string> FileRead;
		public string Three_Letter_other;

		public override string Three_Letter => Three_Letter_other;


		public override IEnumerable<string> GetFiles() {
			return Files?.Invoke();
		}

		public override void Log(string data) {
			Console.WriteLine(data);
		}

		public override string ReadFile(string item) {
			return FileRead?.Invoke(item);
		}
	}
}
