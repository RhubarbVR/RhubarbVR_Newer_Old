using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
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
		public const string SESSION_PATH = "Sessions/";


		public async Task<string[]> GetRelayHoleServers() {
			var data = await SendGet<string[]>(API_PATH + "Relays/GetRelays");
			return data.IsDataGood ? data.Data : Array.Empty<string>();
		}
		public Action<string, Guid> SessionErrorBind;
		public Func<ConnectToUser,Guid,Task> UserConnectionBind;
		public Action<Guid,Guid> SessionIDBind;

		public void SessionError(string data, Guid session) {
			 SessionErrorBind?.Invoke(data, session);
		}
		public async Task UserConnection(ConnectToUser connectToUser,Guid session) {
			await UserConnectionBind?.Invoke(connectToUser, session);
		}

		public void SessionIDupdate(Guid newID, Guid session) {
			SessionIDBind?.Invoke(newID, session);
		}

		public async Task JoinSession(JoinSession joinSession) {
			await _hub.InvokeAsync("JoinSession", joinSession);
		}

		public async Task CreateSession(SessionCreation sessionCreation) {
			await _hub.InvokeAsync("CreateSession", sessionCreation);
		}
		public async Task LeaveSession(Guid id) {
			await _hub.InvokeAsync("LeaveSession", id);
		}
		public async Task<SessionInfo[]> GetTopPublicSessions() {
			var data = await SendGet<SessionInfo[]>(API_PATH + SESSION_PATH + "GetTopPublicSessions");
			if (data.IsDataGood) {
				return data.Data;
			}
			else {
				return Array.Empty<SessionInfo>();
			}
		}
	}
}
