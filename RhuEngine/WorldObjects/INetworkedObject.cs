
using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public interface INetworkedObject : ISyncObject
	{
		public bool NoSync { get; set; }
		public void Received(Peer sender, IDataNode data);
	}
}
