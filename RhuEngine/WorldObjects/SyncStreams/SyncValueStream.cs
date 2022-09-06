using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public class SyncValueStream<T> : SyncStream, ILinkerMember<T>, ISync, INetworkedObject, IChangeable
	{
		private readonly object _locker = new();

		private T _value;

		public T Value
		{
			get => _value;
			set {
				lock (_locker) {
					_value = value;
					BroadcastValue();
					Changed?.Invoke(this);
				}
			}
		}

		private void BroadcastValue() {
			if (IsLinkedTo || NoSync) {
				return;
			}
			World.BroadcastDataToAllStream(this, typeof(T).IsEnum ? new DataNode<int>((int)(object)_value) : new DataNode<T>(_value), LiteNetLib.DeliveryMethod.Unreliable);
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

		public void SetValueForce(object value) {
			lock (_locker) {
				try {
					Value = (T)value;
				}
				catch { }
			}
		}

		public void SetValueNoOnChange(T value) {
			_value = value;
			BroadcastValue();
		}

		public void SetValueNoOnChangeAndNetworking(T value) {
			_value = value;
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

		public void SetStartingObject() {
		}
		public virtual T OnSave(SyncObjectSerializerObject serializerObject) {
			return _value;
		}

		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return SyncObjectSerializerObject.CommonValueSerialize(this, OnSave(syncObjectSerializerObject));
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			_value = syncObjectSerializerObject.ValueDeserialize<T>((DataNodeGroup)data, this);
		}

		public object GetValue() {
			return _value;
		}

		public void SetValue(object data) {
			Value = (T)data;
		}
	}
}
