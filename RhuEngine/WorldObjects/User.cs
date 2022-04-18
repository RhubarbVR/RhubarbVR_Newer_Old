using System;
using System.Linq;
using System.Threading.Tasks;

using LiteNetLib;

using RhuEngine.Components;


namespace RhuEngine.WorldObjects
{
	public class User : SyncObject
	{
		public SyncRef<UserRoot> userRoot;

		public SyncAbstractObjList<SyncStream> syncStreams;
		[Exsposed]
		[NoWriteExsposed]
		public string NormalizedUserName { get; private set; }
		[Exsposed]
		[NoWriteExsposed]
		public string UserName { get;private set; }
		[Exsposed]
		[NoWriteExsposed]
		public string[] Roles { get; private set; }

		[NoSyncUpdate]
		[OnChanged(nameof(UserIDLoad))]
		public Sync<string> userID;
		[NoSyncUpdate]
		public Sync<PlatformID> Platform;

		[NoSyncUpdate]
		public Sync<string> PlatformVersion;

		[NoSyncUpdate]
		public Sync<string> BackendID;

		public void UserIDLoad() {
			Task.Run(async () => {
				if (userID == null) { return; }
				var e = await Engine.netApiManager.GetUserInfo(userID);
				UserName = e?.UserName;
				NormalizedUserName = e?.NormalizedUserName;
				Roles = e?.Roles.ToArray();
			});
		}

		[Default(true)]
		[OnChanged(nameof(PresentChange))]
		public Sync<bool> isPresent;
		public void PresentChange() {
			World.SessionInfoChanged();
		}
		public Peer CurrentPeer { get; set; }

		public bool IsConnected  => (CurrentPeer?.NetPeer?.ConnectionState ?? LiteNetLib.ConnectionState.Disconnected) == LiteNetLib.ConnectionState.Connected;

		public bool IsLocalUser => World.GetLocalUser() == this;

		public override void OnLoaded() {
			base.OnLoaded();
			if(CurrentPeer is null) {
				try {
					var foundPeer = World.peers.Where((val) => val.UserID == userID).First();
					if (foundPeer is not null) {
						if (foundPeer.User is null) {
							CurrentPeer = foundPeer;
							CurrentPeer.User = this;
						}
					}
				}
				catch { }
			}
			UserIDLoad();
		}

		public T FindSyncStream<T>(string name) where T : SyncStream {
			foreach (var item in syncStreams) {
				if (((SyncStream)item).name.Value == name) {
					try {
						return (T)item;
					}
					catch { }
				}
			}
			return null;
		}

		public T FindOrCreateSyncStream<T>(string name) where T : SyncStream, new() {
			var thing = FindSyncStream<T>(name);
			if (thing == null) {
				var stream = syncStreams.Add<T>();
				if (stream is null) {
					throw new Exception("Stream is null");
				}
				stream.name.Value = name;
				return stream;
			}
			else {
				return thing;
			}
		}
	}
}
