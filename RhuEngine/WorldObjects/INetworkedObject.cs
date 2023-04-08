
using System;
using System.Collections.Generic;

using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public interface ICreationDeletionNetworkedObject : INetworkedObject
	{
		public List<Action> ReceivedCreationDelete(Peer sender, IDataNode data);
	}

	public interface IDropOldNetworkedObject : INetworkedObject
	{
		public IDataNode GetUpdateData();
	}

	public interface INetworkedObject : ISyncObject
	{
		public bool NoSync { get; set; }
		public void Received(Peer sender, IDataNode data);
	}
}
