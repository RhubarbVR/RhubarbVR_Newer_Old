using System;
using System.Reflection;

using RhuEngine.Commads;
using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public sealed class SyncDelegate : SyncDelegate<Action>
	{
		public void Invoke() {
			Target?.Invoke();
		}
	}

	public class SyncDelegate<T> : SyncRef<IWorldObject>, ISyncMember where T : Delegate
	{
		private Type _type;
		private string _method;

		private T _delegateTarget;

		public new T Target
		{
			get => base.Target != null && base.Target.IsRemoved ? null : _delegateTarget;
			set {
				if (value == Target) {
					return;
				}
				Delegate @delegate = value;
				if (@delegate?.Target == null) {
					_type = @delegate?.Method.DeclaringType;
					_method = @delegate?.Method.Name;
					base.Target = null;
				}
				else {
					if (@delegate.Target is not IWorldObject worldObject) {
						throw new Exception("Delegate doesn't belong to a WorldObject");
					}
					if (worldObject.World != World) {
						throw new Exception("Delegate owner belongs to a different world");
					}
					if(@delegate.GetMethodInfo().GetCustomAttribute<ExposedAttribute>() is null) {
						throw new Exception("Method does not have Exsposed Attribute");
					}
					_type = @delegate.Method.DeclaringType;
					_method = @delegate.Method.Name;
					base.Target = worldObject;
				}
				_delegateTarget = value;
			}
		}

		public override void Bind() {
			base.Bind();
			BuildDelegate();
		}

		public override void OnChanged() {
			BuildDelegate();
		}

		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			var nodeGroup = (DataNodeGroup)base.Serialize(syncObjectSerializerObject);
			nodeGroup.SetValue("Method", new DataNode<string>(_method));
			nodeGroup.SetValue("Type", new DataNode<string>(_type?.FullName));
			return nodeGroup;
		}

		protected override void BroadcastValue() {
			if (IsLinkedTo || NoSync) {
				return;
			}
			var data = new DataNodeGroup();
			data["target"] = new DataNode<NetPointer>(_targetPointer);
			data["_method"] = new DataNode<string>(_method);
			data["_type"] = new DataNode<string>(_type?.FullName);
			World.BroadcastDataToAll(this, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
		}

		public override void Received(Peer sender, IDataNode data) {
			if (IsLinkedTo || NoSync) {
				return;
			}
			var datagroup = data as DataNodeGroup;
			NetValue = ((DataNode<NetPointer>)datagroup["target"]).Value;
			_method = ((DataNode<string>)datagroup["_method"]).Value;
			var hello = ((DataNode<string>)datagroup["_type"]).Value;
			_type = string.IsNullOrEmpty(hello) ? null : Type.GetType(hello, false, true);
			BuildDelegate();
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			var loadedData = (DataNodeGroup)data;
			base.Deserialize(loadedData, syncObjectSerializerObject);
			_method = ((DataNode<string>)loadedData.GetValue("Method")).Value;
			var hello = ((DataNode<string>)loadedData.GetValue("Type")).Value;
			_type =  string.IsNullOrEmpty(hello) ? null : Type.GetType(hello,false,true);
			BuildDelegate();
		}

		public void BuildDelegate() {
			if (_type == null || string.IsNullOrEmpty(_method) || base.Target == null) {
				return;
			}
			try {
				var _delegate = Delegate.CreateDelegate(typeof(T), base.Target, _method, false, false);
				if (_delegate.GetMethodInfo().GetCustomAttribute<ExposedAttribute>() is null) {
					throw new Exception("Method does not have Exsposed Attribute");
				}
				_delegateTarget = _delegate as T;
			}
			catch (Exception e) {
				RLog.Err($"Failed To load Delegate Type {_type}  Method {_method} Del{typeof(T).GetFormattedName()} Error" + e.ToString());
				_type = null;
				_method = "";
				base.Target = null;
				_delegateTarget = null;
			}
		}
	}
}
