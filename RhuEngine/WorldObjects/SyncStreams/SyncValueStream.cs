using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public partial class SyncValueStream<T> : SyncStream, ILinkerMember<T>, ISync, INetworkedObject, IChangeable
	{
		private readonly object _locker = new();

		private T _value;

		public T Value
		{
			get => _value;
			set {
				lock (_locker) {
					_value = value;
					Changed?.Invoke(this);
				}
			}
		}

		public override void Received(Peer sender, IDataNode data) {
			if (NoSync) {
				return;
			}
			var newValue = typeof(T).IsEnum ? (T)(object)((DataNode<int>)data).Value : ((DataNode<T>)data).Value;
			lock (_locker) {
				_value = newValue;
				Changed?.Invoke(this);
			}
		}

		public event Action<IChangeable> Changed;
		public event Action<ILinker> OnLinked;

		public void SetValueForce(object value) {
			lock (_locker) {
				try {
					Value = (T)value;
				}
				catch { }
			}
		}

		public bool IsLinkedTo { get; private set; }

		[NoSave]
		[NoSync]
		[NoShow]
		public ILinker drivenFromObj;

		public NetPointer LinkedFrom => drivenFromObj.Pointer;

		public object Object { get => _value; set => _value = (T)value; }

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

		public void SetStartingValue() {
		}
		public virtual T OnSave(SyncObjectSerializerObject serializerObject) {
			return _value;
		}

		public object GetValue() {
			return _value;
		}
		public Type GetValueType() {
			return typeof(T);
		}
		public void SetValue(object data) {
			Value = (T)data;
		}

		public override void Dispose() {
			Changed = null;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		public void SetStartingValueNetworked() {
		}


		public override void StreamUpdateOther() {

		}

		public override void StreamUpdateOwner() {
			if (IsLinkedTo || NoSync) {
				return;
			}
			World.StreamToAll(this, typeof(T).IsEnum ? new DataNode<int>((int)(object)_value) : new DataNode<T>(_value), LiteNetLib.DeliveryMethod.Unreliable);
		}
	}
}
