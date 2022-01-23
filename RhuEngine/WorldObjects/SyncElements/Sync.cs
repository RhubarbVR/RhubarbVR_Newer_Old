using System;
using System.Collections.Generic;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ISync
	{
		public void SetValue(object value);
	}
	public class Sync<T> : SyncObject, ILinkerMember<T>, ISync, INetworkedObject, IChangeable
	{
		private readonly object _locker = new();

		private T _value;

		public T Value
		{
			get => _value;
			set {
				lock (_locker) {
					var lastVal = _value;
					_value = value;
					BroadcastValue();
					if (!EqualityComparer<T>.Default.Equals(lastVal, _value)) {
						Changed?.Invoke(this);
					}
				}
			}
		}

		private void BroadcastValue() {
			if (IsLinked || NoSync) {
				return;
			}
			World.BroadcastDataToAll(Pointer, typeof(T).IsEnum ? new DataNode<int>((int)(object)_value) : new DataNode<T>(_value), LiteNetLib.DeliveryMethod.ReliableOrdered);
		}
		public void Received(Peer sender, IDataNode data) {
			if (IsLinked || NoSync) {
				return;
			}
			var newValue = typeof(T).IsEnum ? (T)(object)((DataNode<int>)data).Value : ((DataNode<T>)data).Value;
			lock (_locker) {
				_value = newValue;
				Changed?.Invoke(this);
			}
		}

		public event Action<IChangeable> Changed;

		public void SetValue(object value) {
			lock (_locker) {
				try {
					Value = (T)value;
				}
				catch { }
			}
		}
		public override void InitializeMembers(bool networkedObject, bool deserializeFunc, Func<NetPointer> func) {
		}

		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return SyncObjectSerializerObject.CommonValueSerialize(this, _value);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			_value = syncObjectSerializerObject.ValueDeserialize<T>((DataNodeGroup)data, this);
		}

		public void SetValueNoOnChange(T value) {
			_value = value;
			BroadcastValue();
		}

		public void SetValueNoOnChangeAndNetworking(T value) {
			_value = value;
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
