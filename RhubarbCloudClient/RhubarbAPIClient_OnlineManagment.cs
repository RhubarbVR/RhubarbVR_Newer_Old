using System;
using System.Diagnostics;
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
	public sealed partial class RhubarbAPIClient : IDisposable
	{
		public bool _isOnline = false;

		public event Action HasGoneOfline;
		public event Action HasGoneOnline;

		public event Action<int> PingChange;

		private int _ping;

		public int Ping
		{
			get => _ping;
			set { _ping = value; PingChange?.Invoke(value); }
		}

		public bool IsOnline
		{
			get => _isOnline;
			set {
				if (_isOnline == value) {
					return;
				}
				if (!value) {
					IsGoneOffline();
					HasGoneOfline?.Invoke();
				}
				if (value) {
					IsGoneOnline();
					HasGoneOnline?.Invoke();
				}
				_isOnline = value;
			}
		}

		private void IsGoneOffline() {
			Console.WriteLine("Has gone offline");
			LogOutPros();
		}

		private void IsGoneOnline() {
			Console.WriteLine("Has gone Online");
			GetMe().ConfigureAwait(false);
		}

		private async Task<int> CheckForInternetConnection() {
			try {
				var sw = Stopwatch.StartNew();
				var req = await HttpClient.GetAsync("/");
				sw.Stop();
				return (int)sw.ElapsedMilliseconds;
			}
			catch {
				return -1;
			}
		}

		private void UpdateCheckForInternetConnection() {
			try {
				Task.Run(CheckForInternetConnectionLoop);
			}
			catch(Exception e) {
				Console.WriteLine(e);	
			}
		}

		public async Task CheckForInternetConnectionLoop() {
			Ping = await CheckForInternetConnection();
			IsOnline = Ping != -1;
		}
	}
}
