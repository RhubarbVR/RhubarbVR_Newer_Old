using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using Newtonsoft.Json;

using RhubarbCloudClient.Model;

namespace RhubarbCloudClient
{
	public sealed partial class RhubarbAPIClient : IDisposable
	{
		public const string AUTHPATH = "auth/";
		public string EMAILPATH = "Email/";

		public PrivateUser User { get; private set; }
		public bool IsLogin => User is not null;

		public event Action<PrivateUser> OnLogin;

		public event Action OnLogout;
		private async Task LogInPros(PrivateUser privateUser) {
			User = privateUser;
			if (IsLogin) {
				OnLogin?.Invoke(User);
				await InitSignlR();
				await LoadStartDM();
				await LoadStartGroups();
				await LoadStartUsers();
			}
			else {
				LogOutPros();
			}
		}

		HubConnection _hub;
		private async Task InitSignlR() {
			Console.WriteLine("Starting SignelR");
			try {
				_hub = new HubConnectionBuilder()
					.WithUrl(new Uri(HttpClient.BaseAddress, "hub"), (x) => x.Cookies = Cookies)
					.WithAutomaticReconnect()
					.Build();
			}
			catch {
				_hub = new HubConnectionBuilder()
					.WithUrl(new Uri(HttpClient.BaseAddress, "hub"))
					.WithAutomaticReconnect()
					.Build();
			}
			//_hub.On(nameof(MessageNotify), MessageNotify);
			_hub.On<UserDM.MSG>(nameof(ReceivedMsg), ReceivedMsg);
			_hub.On<PrivateUserStatus>(nameof(LoadInStatusInfo), LoadInStatusInfo);
			_hub.On<Guid,bool>(nameof(UserDataUpdate), UserDataUpdate);
			_hub.On<string,Guid>(nameof(SessionError), SessionError);
			_hub.On<ConnectToUser, Guid>(nameof(UserConnection), UserConnection);
			_hub.On<Guid, Guid>(nameof(SessionIDupdate), SessionIDupdate);
			_hub.Closed += Hub_Closed;
			await _hub.StartAsync();
			Console.WriteLine("SignelR started");
		}

		private Task Hub_Closed(Exception arg) {
			Console.WriteLine($"SignelR Closed {arg?.Message??"null"}");
			return Task.CompletedTask;
		}

		public async Task UpdateUserStatus(PrivateUserStatus status) {
			if (_hub is null) {
				return;
			}
			await _hub.InvokeAsync("SetStatus", status);
		}

		public string ClientCompatibility = "WEB";
		public PrivateUserStatus Status { get; private set; }

		public async Task UpdateStatus() {
			await UpdateUserStatus(Status);
		}

		private async Task LoadInStatusInfo(PrivateUserStatus status) {
			Console.WriteLine("User Status Loaded");
			Status = status;
			status.ClientVersion = Environment.Version.ToString();
			status.Device = Environment.OSVersion.ToString();
			status.ClientCompatibility = ClientCompatibility;
			await UpdateUserStatus(status);
		}

		private void LogOutPros() {
			_hub?.DisposeAsync();
			_hub = null;
			User = null;
			OnLogout?.Invoke();

		}

		private async Task ProccessUserLoadin(ServerResponse<PrivateUser> res) {
			if (res.Error) {
				LogOutPros();
			}
			else {
				await LogInPros(res.Data);
			}
		}
		public async Task<HttpDataResponse<string>> RegisterAccount(string userName, string Password, string Email) {
			return await RegisterAccount(new RUserRegistration { Email = Email, UserName = userName, Password = Password });
		}
		public async Task<HttpDataResponse<string>> RegisterAccount(RUserRegistration rUserRegistration) {
			return await SendPost(API_PATH + AUTHPATH + "Register", rUserRegistration);
		}

		public async Task<ServerResponse<PrivateUser>> Login(string Email, string Password, string Code = null) {
			return await Login(new RAccountLogin { Code = Code, Password = Password, Email = Email });
		}

		public async Task<ServerResponse<PrivateUser>> Login(RAccountLogin rUserLogin) {
			var req = await SendPostServerResponses<PrivateUser, RAccountLogin>(API_PATH + AUTHPATH + "Login", rUserLogin);
			await ProccessUserLoadin(req);
			return req;
		}

		public async Task<string> SendForgotPassword(string targetEmail) {
			var req = await SendGet(API_PATH + EMAILPATH + "SendForgotPassword?email=" + targetEmail);
			return req.Data;
		}

		public async Task<string> ChangePassword(RChangePassword rChangePassword) {
			var req = await SendPost<RChangePassword>(API_PATH + EMAILPATH + "ChangePassword", rChangePassword);
			return req.Data;
		}

		public async Task GetMe() {
			var req = await SendGetServerResponses<PrivateUser>(API_PATH + AUTHPATH + "GetMe");
			await ProccessUserLoadin(req);
		}

		public async Task LogOut() {
			await SendGet(API_PATH + AUTHPATH + "LogOut");
			LogOutPros();
		}
	}
}
