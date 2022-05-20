using System;
using System.Reflection;

using RhuEngine.DataStructure;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;


namespace RhuEngine.WorldObjects
{
	public class SyncDelegate: SyncDelegate<Action>
	{
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
					if(@delegate.GetMethodInfo().GetCustomAttribute<ExsposedAttribute>() is null) {
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
			if (_type != null) {
				nodeGroup.SetValue("Type", new DataNode<string>(_type.FullName));
			}
			else {
				nodeGroup.SetValue("Type", new DataNode<string>(""));
			}
			return nodeGroup;
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			var loadedData = (DataNodeGroup)data;
			base.Deserialize(loadedData, syncObjectSerializerObject);
			_method = ((DataNode<string>)loadedData.GetValue("Method")).Value;
			var hello = ((DataNode<string>)loadedData.GetValue("Type")).Value;
			_type = hello == "" ? null : Type.GetType(hello);
			BuildDelegate();
		}

		public void BuildDelegate() {
			if (_type == null || _method == "" || _method == null || base.Target == null) {
				return;
			}
			try {
				var _delegate = Delegate.CreateDelegate(typeof(T), base.Target, _method, false, true);
				_delegateTarget = _delegate as T;
			}
			catch (Exception e) {
				RLog.Err($"Failed To load Delegate Type {_type}  Method {_method} Error" + e.ToString());
				_type = null;
				_method = "";
				base.Target = null;
				_delegateTarget = null;
			}
		}
	}
}
