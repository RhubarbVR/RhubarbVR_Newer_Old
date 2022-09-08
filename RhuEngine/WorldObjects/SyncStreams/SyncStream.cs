﻿
using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public abstract class SyncStream : SyncObject, INetworkedObject
	{
		public readonly Sync<string> name;

		public bool NoSync { get; set; }

		public abstract void Received(Peer sender, IDataNode data);
	}
}
