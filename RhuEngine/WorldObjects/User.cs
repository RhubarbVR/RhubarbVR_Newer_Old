using System;
using System.Linq;
using System.Threading.Tasks;

using LiteNetLib;

using RhubarbCloudClient.Model;

using RhuEngine.Components;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public enum BodyNode
	{
		None,
		UserRoot,
		Head,
		LeftController,
		RightController,
	}
	public class User : SyncObject
	{
		public Matrix GetBodyNodeTrans(BodyNode bodyNode) {
			if (bodyNode == BodyNode.None) {
				return Matrix.Identity;
			}
			if (userRoot.Target is null) {
				return Matrix.Identity;
			}
			switch (bodyNode) {
				case BodyNode.UserRoot:
					return userRoot.Target.Entity.GlobalTrans;
				case BodyNode.Head:
					return userRoot.Target.head.Target?.GlobalTrans ?? Matrix.Identity;
				case BodyNode.LeftController:
					return userRoot.Target.leftController.Target?.GlobalTrans ?? Matrix.Identity;
				case BodyNode.RightController:
					return userRoot.Target.rightController.Target?.GlobalTrans ?? Matrix.Identity;
				default:
					return Matrix.Identity;
			}
		}


		public readonly SyncRef<UserRoot> userRoot;

		public readonly SyncAbstractObjList<SyncStream> syncStreams;
		[Exposed]
		[NoWriteExsposed]
		public string NormalizedUserName { get; private set; }
		[Exposed]
		[NoWriteExsposed]
		public string UserName { get; private set; }
		[Exposed]
		[NoWriteExsposed]
		public Colorb UserColor { get; private set; }
		[NoSyncUpdate]
		[OnChanged(nameof(UserIDLoad))]
		public readonly Sync<string> userID;
		[NoSyncUpdate]
		public readonly Sync<PlatformID> Platform;

		[NoSyncUpdate]
		public readonly Sync<string> PlatformVersion;

		[NoSyncUpdate]
		public readonly Sync<string> BackendID;
		[BindProperty(nameof(UserName))]
		public readonly SyncProperty<string> Username;
		[BindProperty(nameof(NormalizedUserName))]
		public readonly SyncProperty<string> NormalizedUsername;
		[BindProperty(nameof(UserColor))]
		public readonly SyncProperty<Colorb> UserColorProp;
		public void UserIDLoad() {
			Task.Run(async () => {
				if (userID == null) { return; }
				var e = await Engine.netApiManager.Client.GetUser(Guid.Parse(userID));
				e?.BindDataUpdate((userdata) => {
					UserName = userdata.UserName;
					NormalizedUserName = userdata.NormalizedUserName;
					var color = userdata.IconColor.GetColor();
					UserColor = new Colorb(color.r, color.g, color.b, color.a);
				});

			});
		}

		[Default(true)]
		[OnChanged(nameof(PresentChange))]
		public readonly Sync<bool> isPresent;
		public void PresentChange() {
			World.SessionInfoChanged();
		}
		public Peer CurrentPeer { get; set; }

		public bool IsConnected => (CurrentPeer?.NetPeer?.ConnectionState ?? LiteNetLib.ConnectionState.Disconnected) == LiteNetLib.ConnectionState.Connected;

		public bool IsLocalUser => World.GetLocalUser() == this;

		protected override void OnLoaded() {
			base.OnLoaded();
			if (CurrentPeer is null) {
				try {
					var foundPeer = World.peers.Where((val) => val.UserID.ToString() == userID).First();
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
