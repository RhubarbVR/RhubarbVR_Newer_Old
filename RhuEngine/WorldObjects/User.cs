using System;
using System.Linq;

using LiteNetLib;

using RhuEngine.Components;

namespace RhuEngine.WorldObjects
{
	public class User : SyncObject
	{
		public SyncRef<UserRoot> userRoot;

		public SyncAbstractObjList<SyncStream> syncStreams;
		
		[NoSyncUpdate]
		public Sync<string> userID;
		[Default(true)]
		public Sync<bool> isPresent;

		public Peer CurrentPeer { get; set; }

		public bool IsConnected  => (CurrentPeer?.NetPeer?.ConnectionState ?? ConnectionState.Disconnected) != ConnectionState.Connected;

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
