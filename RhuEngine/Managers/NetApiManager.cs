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
	/// <summary>
	/// Handles all networking with the rhubarb cloud
	/// </summary>
	public sealed class NetApiManager : IManager
	{
		public WorldManager WorldManager { get; private set; }
		/// <summary>
		/// Handles all requests to and from the rhubarb cloud
		/// </summary>
		public RhubarbAPIClient Client { get; private set; }
		/// <summary>
		/// Constructs a new API client with the specified path for the cookies
		/// </summary>
		/// <param name="path">Path of the API</param>
		public NetApiManager(string path) {
#if false && DEBUG
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

		public void ChangeTargetApi(Uri targetAPI) {
			if(targetAPI.Host == "rhubarbvr") {
				WorldManager.Engine.MainSettings.TargetAPI = null;
				Client.ChangeApi(new Uri("https://api.rhubarbvr.net/"));
			}
			else {
				var uriBuilder = new UriBuilder(targetAPI);
				uriBuilder.Host = "rhubarbapi." + uriBuilder.Host;
				Client.ChangeApi(uriBuilder.Uri);
			}

		}

		/// <summary>
		/// Called when the api returns an error from the session
		/// </summary>
		/// <param name="data">The error message.</param>
		/// <param name="session">the session it corresponds to.</param>
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
		/// <summary>
		/// This is called when a user connection is made.
		/// </summary>
		/// <param name="connectToUser">An object containing the user info that is attempting to connect.</param>
		/// <param name="session">The session that the connection is being made to.</param>
		public async Task UserConnection(ConnectToUser connectToUser, Guid session) {
			var targetWorld = WorldManager.GetWorldBySessionID(session);
			RLog.Info($"UserConnection {session} ConnectToUser:{connectToUser.UserID}");
			if (targetWorld is null) {
				RLog.Info($"Failed To find session {session}");
				return;
			}
			await targetWorld.ConnectToUser(connectToUser);
		}
		/// <summary>
		/// Fires when the server sends a new session id
		/// </summary>
		/// <param name="newID"></param>
		/// <param name="session"></param>
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
			if (WorldManager.Engine.MainSettings.TargetAPI is not null) {
				Client.ChangeApi(new Uri(WorldManager.Engine.MainSettings.TargetAPI));
			}
		}


		public void UpdateHash() {
			var hasher = SHA256.Create();
			var concatenatedStream = new ConcatenatedStream();
			concatenatedStream.Enqueue(new MemoryStream(BitConverter.GetBytes(10)));
			//AddExtraToHash concatenatedStream.Enqueue(DataFormClassesInDLLS)
			var inArray = hasher.ComputeHash(concatenatedStream);
			Client.ClientCompatibility = Convert.ToBase64String(inArray);
			RLog.Info("Client Compatibility: " + Client.ClientCompatibility);
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
