using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface ISyncProperty
	{
		void Bind(string value, object from);
	}
	public sealed partial class SyncProperty<T> : SyncObject, ILinkerMember<T>,ISyncProperty, ISyncMember
	{
		public Action<T> SetValue;
		public Func<T> GetValue;

		// Todo: could be removed for more performance
		public void Bind(string value,object from) {
			var property = from.GetType().GetProperty(value);
			if (property.PropertyType != typeof(T)) {
				throw new Exception($"Not vailed Property Type {value}");
			}
			if (property != null) {
				if (property.CanWrite) {
					SetValue = (v) => property.SetValue(from, v);
				}
				if (property.CanRead) {
					GetValue = () => (T)property.GetValue(from);
				}
			}
		}

		[Exposed]
		public T Value
		{
			get => GetValue.Invoke();
			set {
				Changed.Invoke(this);
				SetValue.Invoke(value);
			}
		}
		public object Object { get => GetValue.Invoke(); set => SetValue.Invoke((T)value); }

		protected override void InitializeMembers(bool networkedObject, bool deserializeFunc, NetPointerUpdateDelegate func) {
		}

		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			_hasBeenNetSynced |= syncObjectSerializerObject.NetSync;
			return SyncObjectSerializerObject.CommonSerialize(this);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.BindPointer((DataNodeGroup)data, this);
		}

		public bool IsLinkedTo { get; private set; }

		public ILinker linkedFromObj;

		public event Action<IChangeable> Changed;
		public event Action<ILinker> OnLinked;

		public NetPointer LinkedFrom => linkedFromObj.Pointer;

		public bool NoSync { get; set; }

		public void KillLink() {
			linkedFromObj.RemoveLinkLocation();
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
			linkedFromObj = value;
			IsLinkedTo = true;
			OnLinked?.Invoke(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(SyncProperty<T> data) => data.Value;

		public override void Dispose() {
			SetValue = null;
			GetValue = null;
			base.Dispose();
		}
	}
}
