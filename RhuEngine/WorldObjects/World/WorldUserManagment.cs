using RhuEngine.Datatypes;
using RhuEngine.Linker;

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
				RLog.Info("Peer already has peer");
			}
			else {
				var user = GetUserFromID(peer.UserID);
				if (user == null) {
					RLog.Info($"User built from peer {Users.Count + 1}");
					var userid = (ushort)(Users.Count + 1);
					var pos = 176u; 
					user = Users.AddWithCustomRefIds(false, false, () => {
						lock (_buildRefIDLock) {
							var netPointer = NetPointer.BuildID(pos, userid);
							pos++;
							return netPointer;
						}
					});
					user.userID.Value = peer.UserID;
					user.CurrentPeer = peer;
					peer.User = user;
				}
				else {
					RLog.Info($"User found from peer UserID:{peer.UserID}");
					if (user.CurrentPeer == peer) {
						RLog.Info("Already bond to user");
						return;
					}
					if ((user.CurrentPeer?.NetPeer?.ConnectionState ?? LiteNetLib.ConnectionState.Disconnected) == LiteNetLib.ConnectionState.Connected) {
						RLog.Err("User already loaded can only join a world once");
						peer.NetPeer.Disconnect();
					}
					else {
						user.CurrentPeer = peer;
					}
				}
			}
		}
		[Exsposed]
		public User GetMasterUser() {
			return Users[MasterUser];
		}
		[Exsposed]
		public User GetHostUser() {
			return Users[0];
		}
		[Exsposed]
		public User GetLocalUser() {
			return Users is null ? null : LocalUserID <= 0 ? null : (LocalUserID - 1) < Users.Count ? Users[LocalUserID - 1] : null;
		}
	}
}
