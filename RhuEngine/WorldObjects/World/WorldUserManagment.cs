using RhuEngine.Datatypes;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public partial class World
	{
		public ushort LocalUserID { get; private set; } = 1;

		[NoSyncUpdate]
		[NoSave]
		public SyncObjList<User> Users;
		
		public User GetUserFromID(string id) {
			foreach (User item in Users) {
				if (item.userID.Value == id) {
					return item;
				}
			}
			return null;
		}
		
		private void LoadUserIn(Peer peer) {
			if (peer.User is not null) {
				Log.Info("Peer already has peer");
			}
			else {
				var user = GetUserFromID(peer.UserID);
				if (user == null) {
					Log.Info($"User built from peer {Users.Count + 1}");
					var userid = (ushort)(Users.Count + 1);
					var pos = 176u; 
					user = Users.AddWithCustomRefIds(false, false, () => {
						var netPointer = NetPointer.BuildID(pos, userid);
						pos++;
						return netPointer;
					});
					user.userID.Value = peer.UserID;
					user.CurrentPeer = peer;
					peer.User = user;
				}
				else {
					Log.Info("User found from peer");
					if (user.CurrentPeer == peer) {
						Log.Info("Already bond to user");
						return;
					}
					if ((user.CurrentPeer?.NetPeer?.ConnectionState ?? LiteNetLib.ConnectionState.Disconnected) == LiteNetLib.ConnectionState.Connected) {
						Log.Err("User already loaded can only join a world once");
						peer.NetPeer.Disconnect();
					}
					else {
						user.CurrentPeer = peer;
					}
				}
			}
		}

		public User GetHostUser() {
			return Users[0];
		}

		public User GetLocalUser() {
			return Users is null ? null : LocalUserID <= 0 ? null : (LocalUserID - 1) < Users.Count ? Users[Users.Count - LocalUserID] : null;
		}
	}
}
