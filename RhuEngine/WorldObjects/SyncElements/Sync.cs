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
			var inputType = typeof(T);
			IDataNode Value;
			if (inputType == typeof(Type)) {
				Value = new DataNode<string>(((Type)(object)_value)?.FullName);
			}
			else if (inputType == typeof(Uri)) {
				Value = new DataNode<string>(((Uri)(object)_value)?.ToString());
			}
			else {
				if (inputType.IsEnum) {
					var unType = inputType.GetEnumUnderlyingType();
					if (unType == typeof(int)) {
						Value = new DataNode<int>((int)(object)_value);
					}
					else if (unType == typeof(uint)) {
						Value = new DataNode<uint>((uint)(object)_value);
					}
					else if (unType == typeof(bool)) {
						Value = new DataNode<bool>((bool)(object)_value);
					}
					else if (unType == typeof(byte)) {
						Value = new DataNode<byte>((byte)(object)_value);
					}
					else if (unType == typeof(sbyte)) {
						Value = new DataNode<sbyte>((sbyte)(object)_value);
					}
					else if (unType == typeof(short)) {
						Value = new DataNode<short>((short)(object)_value);
					}
					else if (unType == typeof(ushort)) {
						Value = new DataNode<ushort>((ushort)(object)_value);
					}
					else if (unType == typeof(long)) {
						Value = new DataNode<long>((long)(object)_value);
					}
					else if (unType == typeof(ulong)) {
						Value = new DataNode<ulong>((ulong)(object)_value);
					}
					else {
						throw new NotSupportedException();
					}
				}
				else {
					Value = new DataNode<T>(_value);
				}
			}
			World.BroadcastDataToAll(this, Value, LiteNetLib.DeliveryMethod.ReliableOrdered);
		}
		public void Received(Peer sender, IDataNode data) {
			if (IsLinkedTo || NoSync) {
				return;
			}
			var inputType = typeof(T);
			T newValue;
			if (inputType == typeof(Type)) {
				newValue = ((DataNode<string>)data).Value is null
					? (T)(object)null
					: (T)(object)Type.GetType(((DataNode<string>)data).Value, false, false);
			}
			else if (inputType == typeof(Uri)) {
				newValue = ((DataNode<string>)data).Value is null
					? (T)(object)null
					: (T)(object)new Uri(((DataNode<string>)data).Value);
			}
			else {
				if (inputType.IsEnum) {
					var unType = inputType.GetEnumUnderlyingType();
					if (unType == typeof(int)) {
						newValue = (T)(object)((DataNode<int>)data).Value;
					}
					else if (unType == typeof(uint)) {
						newValue = (T)(object)((DataNode<uint>)data).Value;
					}
					else if (unType == typeof(bool)) {
						newValue = (T)(object)((DataNode<bool>)data).Value;
					}
					else if (unType == typeof(byte)) {
						newValue = (T)(object)((DataNode<byte>)data).Value;
					}
					else if (unType == typeof(sbyte)) {
						newValue = (T)(object)((DataNode<sbyte>)data).Value;
					}
					else if (unType == typeof(short)) {
						newValue = (T)(object)((DataNode<short>)data).Value;
					}
					else if (unType == typeof(ushort)) {
						newValue = (T)(object)((DataNode<ushort>)data).Value;
					}
					else if (unType == typeof(long)) {
						newValue = (T)(object)((DataNode<long>)data).Value;
					}
					else if (unType == typeof(ulong)) {
						newValue = (T)(object)((DataNode<ulong>)data).Value;
					}
					else {
						throw new NotSupportedException();
					}
				}
				else {
					newValue = (DataNode<T>)data;
				}
			}
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
					if (value?.GetType() == typeof(float) && typeof(T) == typeof(double)) {
						value = (double)(float)value;
					}
					else if (value?.GetType() == typeof(int) && typeof(T) == typeof(uint)) {
						value = (uint)(int)value;
					}
					else if (value?.GetType() == typeof(int) && typeof(T) == typeof(double)) {
						value = (double)(int)value;
					}
					else if (value?.GetType() == typeof(double) && typeof(T) == typeof(float)) {
						value = (float)(double)value;
					}
					else {
						_value = (T)value;
					}
				}
				catch { }
			}
			UpdatedValue();
		}
		protected override void InitializeMembers(bool networkedObject, bool deserializeFunc, NetPointerUpdateDelegate func) {
		}

		public void Lerp(T targetpos, double time = 5f, bool removeOnDone = true) {
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
			if (syncObjectSerializerObject.ValueDeserialize<T>((DataNodeGroup)data, this, out var tempvalue)) {
				_value = tempvalue;
			}
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
			linkedFromObj?.RemoveLinkLocation();
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

		public override void Dispose() {
			Changed = null;
			base.Dispose();
		}
	}
}
