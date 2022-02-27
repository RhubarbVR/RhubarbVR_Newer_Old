using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ISyncRef : IWorldObject
	{
		void OnLoaded();
		NetPointer NetValue { get; set; }
	}

	public class SyncRef<T> : SyncObject, ILinkerMember<NetPointer>, ISyncRef, INetworkedObject, IChangeable where T : class, IWorldObject
	{
		private readonly object _syncRefLock = new();

		private NetPointer _targetPointer;

		public NetPointer NetValue
		{
			get => _targetPointer;
			set {
				try {
					lock (_syncRefLock) {
						Unbind();
						_targetPointer = value;
						_target = (T)World.GetWorldObject(value);
						Bind();
					}
				}
				catch {
					_target = null;
				}
				Changed?.Invoke(this);
				OnChanged();
			}
		}
		public override void OnLoaded() {
			NetValue = _targetPointer;
		}
		[Exsposed]
		public virtual NetPointer Value
		{
			get => _targetPointer;
			set {
				try {
					lock (_syncRefLock) {
						Unbind();
						_targetPointer = value;
						BroadcastValue();
						_target = (T)World.GetWorldObject(value);
						Bind();
					}
				}
				catch {
					_target = null;
				}
				Changed?.Invoke(this);
				OnChanged();
			}
		}


		private T _target;

		public IWorldObject TargetIWorldObject { get => Target; set { if (value != null) { Value = value.Pointer; } } }
		[Exsposed]
		public virtual T Target
		{
			get => _target == null || (_target?.IsRemoved ?? false) || _target?.World != World ? null : _target;
			set {
				lock (_syncRefLock) {
					Unbind();
					_targetPointer = value == null ? default : value.Pointer;
					_target = value;
					BroadcastValue();
					Bind();
					Changed?.Invoke(this);
					OnChanged();
				}
			}
		}
		public event Action<IChangeable> Changed;

		public virtual void Bind() {

		}
		public virtual void Unbind() {

		}

		private void BroadcastValue() {
			if (IsLinked || NoSync) {
				return;
			}
			World.BroadcastDataToAll(this, new DataNode<NetPointer>(_targetPointer), LiteNetLib.DeliveryMethod.ReliableOrdered);
		}

		public void Received(Peer sender, IDataNode data) {
			if (IsLinked || NoSync) {
				return;
			}
			NetValue = ((DataNode<NetPointer>)data).Value;
		}

		public void RefIDReassign(ulong newID) {
			NetValue = new NetPointer(newID);
		}

		public override void InitializeMembers(bool networkedObject, bool deserialize, Func<NetPointer> func) {
		}

		public virtual void OnChanged() {

		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return SyncObjectSerializerObject.CommonRefSerialize(this, _targetPointer);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.RefDeserialize((DataNodeGroup)data, this);
		}

		public bool IsLinked { get; private set; }

		public ILinker drivenFromObj;

		public NetPointer LinkedFrom => drivenFromObj.Pointer;

		public bool NoSync { get; set; }

		public void KillLink() {
			drivenFromObj.RemoveLinkLocation();
			IsLinked = false;
		}

		public void Link(ILinker value) {
			if (!IsLinked) {
				ForceLink(value);
			}
		}
		public void ForceLink(ILinker value) {
			if (IsLinked) {
				KillLink();
			}
			value.SetLinkLocation(this);
			drivenFromObj = value;
			IsLinked = true;
		}

	}
}
