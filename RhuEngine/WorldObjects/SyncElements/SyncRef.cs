using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface ISyncRef : ISyncObject
	{
		NetPointer NetValue { get; set; }
	}

	public class SyncRef<T> : SyncObject, ILinkerMember<NetPointer>, ISyncRef, INetworkedObject, IChangeable, ISyncMember where T : class, IWorldObject
	{
		public object Object { get => Value; set => Value = (NetPointer)value; }

		private readonly object _syncRefLock = new();

		protected NetPointer _targetPointer;

		public NetPointer NetValue
		{
			get => _targetPointer;
			set {
				try {
					lock (_syncRefLock) {
						Unbind();
						_targetPointer = value;
						var targetValue = World.GetWorldObject(value);
						if(targetValue == null) {
							_target = null;
						}
						else {
							if (targetValue.GetType().IsAssignableTo(typeof(T))) {
								_target = (T)targetValue;
							}
						}
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
		protected override void OnLoaded() {
			NetValue = _targetPointer;
		}
		[Exposed]
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

		public IWorldObject TargetIWorldObject { get => Target; set { if (value is T data) { Target = data; } else { Target = null; } } }

		[Exposed]
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
				}
				OnChanged();
				Changed?.Invoke(this);
			}
		}

		public event Action<IChangeable> Changed;

		public virtual void Bind() {

		}
		public virtual void Unbind() {

		}

		protected virtual void BroadcastValue() {
			if (IsLinkedTo || NoSync) {
				return;
			}
			World.BroadcastDataToAll(this, new DataNode<NetPointer>(_targetPointer), LiteNetLib.DeliveryMethod.ReliableOrdered);
		}

		public virtual void Received(Peer sender, IDataNode data) {
			if (IsLinkedTo || NoSync) {
				return;
			}
			NetValue = ((DataNode<NetPointer>)data).Value;
		}

		public void RefIDReassign(ulong newID) {
			NetValue = new NetPointer(newID);
		}

		protected override void InitializeMembers(bool networkedObject, bool deserialize, NetPointerUpdateDelegate func) {
		}

		public virtual void OnChanged() {

		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return SyncObjectSerializerObject.CommonRefSerialize(this, _targetPointer);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.RefDeserialize((DataNodeGroup)data, this);
		}

		public bool IsLinkedTo { get; private set; }

		public ILinker drivenFromObj;

		public NetPointer LinkedFrom => drivenFromObj.Pointer;

		public bool NoSync { get; set; }

		public void KillLink() {
			drivenFromObj.RemoveLinkLocation();
			IsLinkedTo = false;
		}

		public void Link(ILinker value) {
			if (!IsLinkedTo) {
				ForceLink(value);
			}
		}
		public void ForceLink(ILinker value) {
			if (IsLinkedTo) {
				KillLink();
			}
			value.SetLinkLocation(this);
			drivenFromObj = value;
			IsLinkedTo = true;
		}

	}
}
