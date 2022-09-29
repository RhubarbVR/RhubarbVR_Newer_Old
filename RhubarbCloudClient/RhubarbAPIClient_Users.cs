using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using DataModel.Enums;

using Microsoft.AspNetCore.SignalR.Client;

using Newtonsoft.Json;

using RhubarbCloudClient.Model;

namespace RhubarbCloudClient
{
	public sealed partial class RhubarbAPIClient : IDisposable
	{
		public const string PRIVATEPATH = "PrivateUser/";
		public const string USERINFOPATH = "User/";

		public class ManagedUser
		{
			private readonly RhubarbAPIClient _client;
			public Guid UserID => UserData?.Id??Guid.Empty;

			public UserRelation Relation { get; private set; }

			public PublicUser UserData { get; private set; }

			public PublicUserStatus UserStatus { get; private set; }

			public async Task UpdateUserData() {
				var data = await _client.SendGetServerResponses<PublicUser>(API_PATH + USERINFOPATH + UserID);
				if (!data.Error) {
					UserData = data.Data;
					UserDataChanged?.Invoke(UserData);
				}
			}
			public async Task UpdateUserStatus() {
				var data = await _client.SendGetServerResponses<PublicUserStatus>(API_PATH + USERINFOPATH + UserID + "/Status");
				if (!data.Error) {
					UserStatus = data.Data;
					UserStatusChanged?.Invoke(UserStatus);
				}
			}
			public async Task UpdateUserRelation() {
				var data = await _client.SendGetServerResponses<UserRelation>(API_PATH + PRIVATEPATH + "GetRelation/" + UserID);
				if (!data.Error) {
					Relation = data.Data;
					UserRelationChanged?.Invoke(Relation);
				}
			}

			public async Task SetUserRelation(UserRelation userRelation) {
				var data = await _client.SendGetServerResponses<UserRelation>(API_PATH + PRIVATEPATH + "SetRelation/" + UserID + "?relation=" + (int)userRelation);
				if (!data.Error) {
					Relation = data.Data;
					UserRelationChanged?.Invoke(Relation);
				}
			}

			public void BindRelationUpdate(Action<UserRelation> action) {
				UserRelationChanged += action;
				action(Relation);
			}

			public void BindDataUpdate(Action<PublicUser> action) {
				UserDataChanged += action;
				if (UserData is not null) {
					action(UserData);
				}
			}

			public void BindStatusUpdate(Action<PublicUserStatus> action) {
				UserStatusChanged += action;
				if(UserStatus is not null) {
					action(UserStatus);
				}
			}

			public event Action<UserRelation> UserRelationChanged;
			public event Action<PublicUser> UserDataChanged;
			public event Action<PublicUserStatus> UserStatusChanged;

			internal ManagedUser(UserRelation userRelation, RhubarbAPIClient client) {
				Relation = userRelation;
				_client = client;
			}
			internal ManagedUser(RhubarbAPIClient client) {
				_client = client;
			}
		}


		private readonly Dictionary<Guid, ManagedUser> _loadedUsers = new();

		private async Task LoadStartUsers() {
			_loadedUsers.Clear();
			var following = (await SendGetServerResponses<Guid[]>(API_PATH + PRIVATEPATH + "GetFollowing")).Data ?? Array.Empty<Guid>();
			var friends = (await SendGetServerResponses<Guid[]>(API_PATH + PRIVATEPATH + "GetFriend")).Data ?? Array.Empty<Guid>();
			var blocked = (await SendGetServerResponses<Guid[]>(API_PATH + PRIVATEPATH + "GetBlocked")).Data ?? Array.Empty<Guid>();
			foreach (var item in following) {
				var newData = new ManagedUser(UserRelation.Follower, this);
				_loadedUsers.Add(item, newData);
				await newData.UpdateUserStatus();
				await newData.UpdateUserData();
			}
		}


		public async Task<ManagedUser> GetUser(Guid userID) {
			if (_loadedUsers.TryGetValue(userID, out var val)) {
				return val;
			}
			else {
				var newData = new ManagedUser(UserRelation.None, this);
				_loadedUsers.Add(userID, newData);
				await newData.UpdateUserStatus();
				await newData.UpdateUserData();
				await newData.UpdateUserRelation();
				await _hub.InvokeAsync("UserUpdateListen", userID);
				return newData;
			}
		}

		private async Task UserDataUpdate(Guid userID, bool statusUpdate) {
			var targetUser = await GetUser(userID);
			if(targetUser is null) {
				Console.WriteLine("Target user was null");
				return;
			}
			if (statusUpdate) {
				await targetUser.UpdateUserStatus();
			}
			else {
				await targetUser.UpdateUserData();
			}
		}
	}
}
