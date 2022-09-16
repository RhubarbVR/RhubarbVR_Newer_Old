using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RhuEngine.Components;
using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface ISync : ISyncMember
	{
		public void SetStartingObject();
		public void SetValue(object value);

		public void SetValueForce(object value);
		public object GetValue();

		public Type GetValueType();
	}
	[GenericTypeConstraint()]
	public class Sync<T> : SyncObject, ILinkerMember<T>, ISync, INetworkedObject, IChangeable, ISyncMember
	{
		private readonly object _locker = new();

		private T _value;

		[Exposed]
		public T Value
		{
			get => _value;
			set {
				lock (_locker) {
					var lastVal = _value;
					_value = value;
					BroadcastValue();
					UpdatedValue();
					if (!EqualityComparer<T>.Default.Equals(lastVal, _value)) {
						Changed?.Invoke(this);
					}
				}
			}
		}
		public virtual void UpdatedValue() {
		}

		public object Object { get => _value; set => _value = (T)value; }
		private void BroadcastValue() {
			if (IsLinkedTo || NoSync) {
				return;
			}
			World.BroadcastDataToAll(this, typeof(T).IsEnum ? new DataNode<int>((int)(object)_value) : new DataNode<T>(_value), LiteNetLib.DeliveryMethod.ReliableOrdered);
		}
		public void Received(Peer sender, IDataNode data) {
			if (IsLinkedTo || NoSync) {
				return;
			}
			var newValue = typeof(T).IsEnum ? (T)(object)((DataNode<int>)data).Value : ((DataNode<T>)data).Value;
			lock (_locker) {
				var lastVal = _value;
				_value = newValue;
				UpdatedValue();
				if (!EqualityComparer<T>.Default.Equals(lastVal, _value)) {
					Changed?.Invoke(this);
				}
			}
		}

		public event Action<IChangeable> Changed;

		public void SetValueForce(object value) {
			lock (_locker) {
				try {
					_value = (T)value;
				}
				catch { }
			}
			UpdatedValue();
		}
		protected override void InitializeMembers(bool networkedObject, bool deserializeFunc, NetPointerUpdateDelegate func) {
		}

		public void Lerp(T targetpos,double time = 5f,bool removeOnDone = true) {
			var lerp = this.GetClosedEntityOrRoot().AttachComponent<Lerp<T>>();
			lerp.StartLerp(this, targetpos, time, removeOnDone);
		}
		public void SmoothLerp(T targetpos, double multiply = 5f) {
			var lerp = this.GetClosedEntityOrRoot().AttachComponent<SmoothLerp<T>>();
			lerp.StartSmoothLerp(this, targetpos, multiply);
		}

		public virtual T OnSave(SyncObjectSerializerObject serializerObject) {
			return _value;
		}

		public virtual void OnLoad(SyncObjectDeserializerObject serializerObject) {
		}


		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return SyncObjectSerializerObject.CommonValueSerialize(this, OnSave(syncObjectSerializerObject));
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			_value = syncObjectSerializerObject.ValueDeserialize<T>((DataNodeGroup)data, this);
			OnLoad(syncObjectSerializerObject);
		}

		public void SetValueNoOnChange(T value) {
			_value = value;
			BroadcastValue();
			UpdatedValue();
		}

		public void SetValueNoOnChangeAndNetworking(T value) {
			_value = value;
			UpdatedValue();
		}

		public bool IsLinkedTo { get; private set; }

		public ILinker linkedFromObj;

		public NetPointer LinkedFrom => linkedFromObj.Pointer;

		public bool NoSync { get; set; }

		public virtual T StartingValue => default;

		public void KillLink() {
			linkedFromObj.RemoveLinkLocation();
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
			linkedFromObj = value;
			IsLinkedTo = true;
		}

		public void SetStartingObject() {
			_value = StartingValue;
			UpdatedValue();
		}

		public void SetValue(object data) {
			Value = (T)data;
		}

		public object GetValue() {
			return Value;
		}

		public Type GetValueType() {
			return typeof(T);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(Sync<T> data) => data.Value;
	}
}
