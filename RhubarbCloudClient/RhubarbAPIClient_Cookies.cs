using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RhubarbCloudClient.Model;

namespace RhubarbCloudClient
{
	public partial class RhubarbAPIClient : IDisposable
	{
		private static void WriteCookiesToDisk(string file, CookieContainer cookieJar) {
			if (!Directory.Exists(Path.GetDirectoryName(file))) {
				Directory.CreateDirectory(Path.GetDirectoryName(file));
			}
			using Stream stream = File.Create(file);
			try {
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, cookieJar);
			}
			catch {
			}
		}

		private static CookieContainer ReadCookiesFromDisk(string file) {
			try {
				if (file is null) {
					return new CookieContainer();
				}
				using Stream stream = File.Open(file, FileMode.Open);
				var formatter = new BinaryFormatter();
				return (CookieContainer)formatter.Deserialize(stream);
			}
			catch {
				return new CookieContainer();
			}
		}
	}
}
