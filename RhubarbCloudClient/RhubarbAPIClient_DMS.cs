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
	public partial class RhubarbAPIClient : IDisposable
	{
		public const string DMSPATH = "DM/";

		public class DMManaged
		{
			public bool IsUserDm => !DM.IsGorupDM;

			public Guid DMid => DM.Id;
			public UserDM DM { get; private set; }

			public async Task<ManagedUser> GetUser() {
				return await Client.GetUser(DM.Users.Where((x) => x != Client.User.Id).FirstOrDefault());
			}

			public async Task<Uri> DMAsset() {
				if(IsUserDm) {
					var user = await Client.GetUser(DM.Users.Where((x) => x != Client.User.Id).FirstOrDefault());
					return await Client.GetRecordDownloadURL(Guid.Parse(user.UserData.ProfileIcon));
				}
				else {
					return await Client.GetRecordDownloadURL(Guid.Parse(DM.Thumbnail));
				}
			}

			public RhubarbAPIClient Client { get; private set; }

			public async Task<ServerResponse<UserDM.MSG>> SendMsg(MessageType messageType, string messageData) {
				return await Client.SendMsg(DMid, messageType, messageData);
			}
			public async Task<ServerResponse<UserDM.MSG>> SendURLmsg(Uri message) {
				return await Client.SendURLmsg(DMid, message);
			}
			public async Task<ServerResponse<UserDM.MSG>> SendTextMsg(string message) {
				return await Client.SendTextMsg(DMid, message);
			}
			public async Task LoadMsgs() {
				await Client.LoadMsgs(DMid,DM.Msgs?.Length??0);
			}
			internal DMManaged(UserDM DM, RhubarbAPIClient client) {
				this.DM = DM;
				Client = client;
			}
		}

		private readonly Dictionary<Guid, DMManaged> _dms = new();

		public IEnumerable<DMManaged> GetDms() {
			foreach (var item in _dms.Values) {
				yield return item;
			}
		}

		public DMManaged GetDM(Guid targetDM) {
			return _dms[targetDM];
		}

		private async Task LoadStartDM() {
			_dms.Clear();
			var req = await SendGetServerResponses<UserDM[]>(API_PATH + DMSPATH + "DMs");
			if (!req.Error) {
				foreach (var item in req.Data) {
					var newDm = new DMManaged(item, this);
					_dms.Add(item.Id, newDm);
				}
			}
			else {
				Console.WriteLine("Error LoadingDms " + req.MSG);
			}
		}
		public async Task<DMManaged> CreateDM(params Guid[] users) {
			var res = await SendPostServerResponses<UserDM, Guid[]>(API_PATH + DMSPATH + "DMs", users);
			if (!res.Error) {
				var newDm = new DMManaged(res.Data, this);
				_dms.Add(res.Data.Id, newDm);
				return newDm;
			}
			return null;
		}
		public async Task<ServerResponse<UserDM.MSG>> SendMsg(Guid targetDM, MessageType messageType, string messageData) {
			return await SendPostServerResponses<UserDM.MSG, RSendMsg>(API_PATH + DMSPATH + "Messages/" + targetDM.ToString() + "/Send", new RSendMsg { MessageType = messageType, Data = messageData });
		}
		public async Task<ServerResponse<UserDM.MSG>> SendURLmsg(Guid targetDM, Uri message) {
			return await SendMsg(targetDM, MessageType.URL, message.ToString());
		}
		public async Task<ServerResponse<UserDM.MSG>> SendTextMsg(Guid targetDM, string message) {
			return await SendMsg(targetDM, MessageType.Text, message);
		}
		public async Task LoadMsgs(Guid targetDM, int start = 0) {
			var data = await SendGetServerResponses<UserDM.MSG[]>(API_PATH + DMSPATH + "Messages/" + targetDM.ToString() + $"?start={start}");
			if (!data.Error) {

			}
		}
		private void ReceivedMsg(UserDM.MSG mSG) {

		}
	}
}
