using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ISyncObject : IWorldObject
	{
		bool IsDestroying { get; set; }

		public event Action<object> OnDispose;

		public void ChangeName(string name);
		public void Initialize(World world, IWorldObject parent, string name, bool networkedObject, bool deserialize, Func<NetPointer> netPointer = null);
		public void OnSave();
		public void Destroy();
		public void OnLoaded();
		public IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject);
		public void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject);

	}
}
