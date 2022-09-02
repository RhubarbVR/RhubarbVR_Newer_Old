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
		public class HttpDataResponse<T>
		{
			public T Data { get; set; }

			public HttpResponseMessage HttpResponseMessage { get; set; }

			public async Task<string> RawData() {
				return await HttpResponseMessage.Content.ReadAsStringAsync();
			}

			public async static Task<HttpDataResponse<T>> Build(HttpResponseMessage httpResponseMessage) {
				var httpDataResponse = new HttpDataResponse<T> {
					HttpResponseMessage = httpResponseMessage,
					Data = JsonConvert.DeserializeObject<T>(await httpResponseMessage.Content.ReadAsStringAsync())
				};
				return httpDataResponse;
			}

			public async static Task<HttpDataResponse<string>> BuildString(HttpResponseMessage httpResponseMessage) {
				var httpDataResponse = new HttpDataResponse<string> {
					HttpResponseMessage = httpResponseMessage,
					Data = await httpResponseMessage.Content.ReadAsStringAsync()
				};
				return httpDataResponse;
			}
			public bool IsDataNull => Data is null;

			public bool IsDataGood => HttpResponseMessage?.IsSuccessStatusCode ?? false & Data is not null;
		}


		public async Task<HttpDataResponse<string>> SendPost<T>(string path, T value) {
			var httpContent = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
			try {
				var sw = Stopwatch.StartNew();
				var request = await HttpClient.PostAsync(path, httpContent);
				sw.Stop();
				Ping = (int)sw.ElapsedMilliseconds;
				return await HttpDataResponse<string>.BuildString(request);
			}
			catch {
				await Check();
				return new HttpDataResponse<string>();
			}
		}
		public async Task<HttpDataResponse<R>> SendPost<R, T>(string path, T value) {
			var httpContent = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
			try {
				var sw = Stopwatch.StartNew();
				var request = await HttpClient.PostAsync(path, httpContent);
				sw.Stop();
				Ping = (int)sw.ElapsedMilliseconds;
				return await HttpDataResponse<R>.Build(request);
			}
			catch {
				await Check();
				return new HttpDataResponse<R>();
			}
		}
		public async Task<HttpDataResponse<string>> SendGet(string path) {
			try {
				var sw = Stopwatch.StartNew();
				var request = await HttpClient.GetAsync(path);
				sw.Stop();
				Ping = (int)sw.ElapsedMilliseconds;
				return await HttpDataResponse<string>.BuildString(request);
			}
			catch {
				await Check();
				return new HttpDataResponse<string>();
			}
		}
		public async Task<HttpDataResponse<R>> SendGet<R>(string path) {
			try {
				var sw = Stopwatch.StartNew();
				var request = await HttpClient.GetAsync(path);
				sw.Stop();
				Ping = (int)sw.ElapsedMilliseconds;
				return await HttpDataResponse<R>.Build(request);
			}
			catch {
				await Check();
				return new HttpDataResponse<R>();
			}
		}

		public async Task<ServerResponse<T>> SendGetServerResponses<T>(string path) {
			try {
				var sw = Stopwatch.StartNew();
				var request = await HttpClient.GetAsync(path);
				sw.Stop();
				Ping = (int)sw.ElapsedMilliseconds;
				var build = await HttpDataResponse<ServerResponse<T>>.Build(request);
				return build.IsDataNull ? new ServerResponse<T>(request.StatusCode.ToString()) : build.Data;
			}
			catch (Exception e) {
				await Check();
				return new ServerResponse<T>(e.ToString());
			}
		}

		public async Task Check() {
			Ping = await CheckForInternetConnection();
			IsOnline = Ping != -1;
			if (!IsOnline) {
				UpdateCheckForInternetConnection();
			}
		}

		public async Task<ServerResponse<T>> SendPostServerResponses<T, P>(string path, P value) {
			try {
				var httpContent = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
				var sw = Stopwatch.StartNew();
				var req = await HttpClient.PostAsync(path, httpContent);
				sw.Stop();
				Ping = (int)sw.ElapsedMilliseconds;
				var request =  await HttpDataResponse<ServerResponse<T>>.Build(req);
				return request.IsDataNull ? new ServerResponse<T>(request.HttpResponseMessage.StatusCode.ToString()) : request.Data;
			}
			catch (Exception e) {
				await Check();
				return new ServerResponse<T>(e.ToString());
			}
		}
	}
}
