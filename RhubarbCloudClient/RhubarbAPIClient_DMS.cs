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
			public Guid DMid => DM.Id;
			public UserDM DM { get; private set; }

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
					await newDm.LoadMsgs();
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
				await newDm.LoadMsgs();
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
				AddMsgsToStart(targetDM, data.Data);
			}
		}

		private void AddMsgsToStart(Guid targetDM, params UserDM.MSG[] mSGs) {
			try {
				var field = _dms[targetDM].DM.Msgs;
				field ??= Array.Empty<UserDM.MSG>();
				var startSize = _dms[targetDM].DM.Msgs?.Length ?? 0;
				Array.Resize(ref field, startSize + mSGs.Length);
				for (var i = 0; i < mSGs.Length; i++) {
					field[i] = mSGs[i];
				}
				for (var i = mSGs.Length; i < startSize + mSGs.Length; i++) {
					field[i] = _dms[targetDM].DM.Msgs[i - mSGs.Length];
				}
				_dms[targetDM].DM.Msgs = field;
			}
			catch {
				Console.WriteLine("Error Loading in DM MSG to start" + targetDM);
			}
		}
		private void AddMsgs(Guid targetDM, params UserDM.MSG[] mSGs) {
			try {
				var field = _dms[targetDM].DM.Msgs;
				field ??= Array.Empty<UserDM.MSG>();
				var startSize = _dms[targetDM].DM.Msgs?.Length ?? 0;
				Array.Resize(ref field, startSize + mSGs.Length);
				for (var i = 0; i < mSGs.Length; i++) {
					var curentIndex = i + startSize;
					field[curentIndex] = mSGs[i];
				}
				_dms[targetDM].DM.Msgs = field;
			}
			catch {
				Console.WriteLine("Error Loading in DM MSG " + targetDM);
			}
		}

		private void ReceivedMsg(UserDM.MSG mSG) {
			AddMsgs(mSG.DMId, mSG);
		}
	}
}
