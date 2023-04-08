using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface ISyncObject : IWorldObject
	{

		internal bool HasBeenNetSynced { get; set; }

		bool IsDestroying { get; set; }

		public event Action<object> OnDispose;
		public event Action<string> NameChange;
		public void ChangeName(string name);
		public void Initialize(World world, IWorldObject parent, string name, bool networkedObject, bool deserialize, NetPointerUpdateDelegate netPointer = null);
		public void Destroy();
		public IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject);
		public void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject);
		internal void CallFirstCreation();
		internal void RunOnLoad();
		internal void RunOnSave();

	}
}
