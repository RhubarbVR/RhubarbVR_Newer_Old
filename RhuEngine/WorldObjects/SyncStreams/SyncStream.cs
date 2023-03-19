
using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public abstract partial class SyncStream : SyncObject, INetworkedObject
	{
		public readonly Sync<string> name;

		public User Owner => (User)Parent.Parent;

		public bool NoSync { get; set; }

		public abstract void Received(Peer sender, IDataNode data);
	}
}
