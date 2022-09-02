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
	public partial class RhubarbAPIClient : IDisposable
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
					IsGoneOfline();
					HasGoneOfline?.Invoke();
				}
				if (value) {
					IsGoneOnline();
					HasGoneOnline?.Invoke();
				}
				_isOnline = value;
			}
		}

		private void IsGoneOfline() {
			LogOutPros();
		}

		private void IsGoneOnline() {
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
				Console.WriteLine("Is Offline");
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

		private async Task CheckForInternetConnectionLoop() {
			Ping = await CheckForInternetConnection();
			IsOnline = Ping != -1;
			await Task.Delay(5000);
			if (!IsOnline) {
				await CheckForInternetConnectionLoop();
			}
		}
	}
}
