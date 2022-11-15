using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RNumerics;
using Newtonsoft.Json;

using RhubarbCloudClient;
using RhubarbCloudClient.Model;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;


namespace RhuEngine.Managers
{
	public sealed class NetApiManager : IManager
	{
		public WorldManager WorldManager { get; private set; }
		public RhubarbAPIClient Client { get; private set; }

		public NetApiManager(string path) {
#if !DEBUG
			Client = new RhubarbAPIClient(RhubarbAPIClient.BaseUri, path) {
				UserConnectionBind = UserConnection,
				SessionErrorBind = SessionError,
				SessionIDBind = SessionIDupdate
			};
#else
			Client = new RhubarbAPIClient(new Uri("https://api.rhubarbvr.net/"), path) {
				UserConnectionBind = UserConnection,
				SessionErrorBind = SessionError,
				SessionIDBind = SessionIDupdate,
			};
#endif
		}
		public void SessionError(string data, Guid session) {
			var targetWorld = WorldManager.GetWorldBySessionID(session);
			RLog.Info($"Error With session {session} MSG:{data}");
			if (targetWorld is null) {
				RLog.Info($"Failed To find session {session}");
				return;
			}
			targetWorld.HasError = true;
			targetWorld.LoadMsg = data;
		}
		public async Task UserConnection(ConnectToUser connectToUser, Guid session) {
			var targetWorld = WorldManager.GetWorldBySessionID(session);
			RLog.Info($"UserConnection {session} ConnectToUser:{connectToUser.UserID}");
			if (targetWorld is null) {
				RLog.Info($"Failed To find session {session}");
				return;
			}
			await targetWorld.ConnectToUser(connectToUser);
		}

		public void SessionIDupdate(Guid newID, Guid session) {
			var targetWorld = WorldManager.GetWorldBySessionID(session);
			RLog.Info($"LoadedSessionID {session} NewID:{newID}");
			if (targetWorld is null) {
				RLog.Info($"Failed To find session {session}");
				return;
			}
			if (targetWorld.SessionID.Value != newID.ToString()) {
				targetWorld.SessionID.Value = newID.ToString();
				RLog.Info("Loaded Session ID");
			}
			else {
				targetWorld.IsDeserializing = true;
				targetWorld.IsLoadingNet = false;
				RLog.Info("Already Loaded Session ID");
			}
		}
		public void Init(Engine engine) {
			WorldManager = engine.worldManager;
			UpdateHash();
			Client.ClientVersion = engine.version.ToString();
			Task.Run(Client.Check);
		}


		public void UpdateHash() {
			using (var mD5CryptoServiceProvider = new MD5CryptoServiceProvider()) {
				var concatenatedStream = new ConcatenatedStream();
				concatenatedStream.Enqueue(new MemoryStream(BitConverter.GetBytes(10)));
				//AddExtraToHash concatenatedStream.Enqueue(DataFormClassesInDLLS)
				var inArray = mD5CryptoServiceProvider.ComputeHash(concatenatedStream);
				Client.ClientCompatibility = Convert.ToBase64String(inArray);
				RLog.Info("Client Compatibility: " + Client.ClientCompatibility);
			}
		}

		public void Step() {
		}

		public void RenderStep() {
		}

		public void Dispose() {
			Client?.Dispose();
		}
	}
}
