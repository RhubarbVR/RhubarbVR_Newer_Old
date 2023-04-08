using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface ISyncRef : ISyncObject, IChangeable, INetworkedObject, ISyncMember
	{
		NetPointer RawPointer { set; }

		NetPointer NetValue { get; set; }

		IWorldObject TargetIWorldObject { get; set; }

		Type GetRefType { get; }
	}

	public partial class SyncRef<T> : SyncObject, ILinkerMember<NetPointer>, ISyncRef, IDropOldNetworkedObject, IChangeable, ISyncMember where T : class, IWorldObject
	{
		public object Object { get => Value; set => Value = (NetPointer)value; }

		private readonly object _syncRefLock = new();

		protected NetPointer _targetPointer;

		public NetPointer RawPointer { set => _targetPointer = value; }

		public NetPointer NetValue
		{
			get => _targetPointer;
			set {
				Unbind();
				try {
					lock (_syncRefLock) {
						_targetPointer = value;
						var targetValue = World.GetWorldObject(value);
						_target = targetValue == null ? null : targetValue.GetType().IsAssignableTo(typeof(T)) ? (T)targetValue : null;
					}
				}
				catch {
					_target = null;
				}
				finally {
					Bind();
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
				lock (_syncRefLock) {
					Unbind();
					try {
						_targetPointer = value;
						BroadcastValue();
						var targetValue = World.GetWorldObject(value);
						_target = targetValue == null ? null : targetValue.GetType().IsAssignableTo(typeof(T)) ? (T)targetValue : null;
					}
					catch {
						_target = null;
					}
					finally {
						Bind();
					}
				}
				Changed?.Invoke(this);
				OnChanged();
			}
		}


		private T _target;
		internal void SetTargetNoChange(T value) {
			lock (_syncRefLock) {
				Unbind();
				_targetPointer = value == null ? default : value.Pointer;
				_target = value;
				BroadcastValue();
				Bind();
			}
		}
		internal void SetTargetNoNetworkOrChange(T value) {
			lock (_syncRefLock) {
				Unbind();
				_targetPointer = value == null ? default : value.Pointer;
				_target = value;
				Bind();
			}
		}

		public IWorldObject TargetIWorldObject { get => Target; set => Target = value is T data ? data : null; }

		private bool _allowCrossWorld = false;

		/// <summary>
		/// This is not to be used enlesss you know the value will never be serialized and networked
		/// </summary>
		internal void AllowCrossWorld() {
			_allowCrossWorld = true;
		}

		[Exposed]
		public virtual T Target
		{
			get => _target?.IsRemoved ?? true ? null : _target;
			set {
				if (value?.World != World && value is not null && (!(_allowCrossWorld && (World.IsPersonalSpace || World.IsOverlayWorld)))) {
					throw new NotSupportedException("World not the same");
				}
				lock (_syncRefLock) {
					Unbind();
					try {
						_targetPointer = value == null ? default : value.Pointer;
						_target = value;
						BroadcastValue();
					}
					catch {
						_target = null;
					}
					finally {
						Bind();
					}
				}
				OnChanged();
				Changed?.Invoke(this);
			}
		}

		public event Action<IChangeable> Changed;
		public event Action<ILinker> OnLinked;

		public virtual void Bind() {

		}
		public virtual void Unbind() {

		}

		public virtual IDataNode GetUpdateData() {
			return new DataNode<NetPointer>(_targetPointer);
		}

		protected void BroadcastValue() {
			if (IsLinkedTo || NoSync) {
				return;
			}
			if (!_hasBeenNetSynced) {
				return;
			}
			World.BroadcastObjectUpdate(this);
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
			_hasBeenNetSynced |= syncObjectSerializerObject.NetSync;
			return SyncObjectSerializerObject.CommonRefSerialize(this, _targetPointer);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.RefDeserialize((DataNodeGroup)data, this);
		}

		public bool IsLinkedTo { get; private set; }

		public ILinker drivenFromObj;

		public NetPointer LinkedFrom => drivenFromObj.Pointer;

		public bool NoSync { get; set; }

		public Type GetRefType => typeof(T);

		public void KillLink() {
			drivenFromObj.RemoveLinkLocation();
			IsLinkedTo = false;
			OnLinked?.Invoke(null);
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
			OnLinked?.Invoke(value);
		}
		public override void Dispose() {
			Changed = null;
			base.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
