using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript" })]
	public class RhuScript : Component,IUpdatingComponent
	{
		public Exception Error;

		public IScriptNode _MainMethod;

		public IScriptNode MainMethod
		{
			get => _MainMethod;
			set {
				_MainMethod = value;
				_MainMethod.LoadIntoWorld(World, this);
			}
		}
		[Exsposed]
		public void InfoLog(string msg) {
			Log.Info("[RhuScript] "+ msg);
		}
		[Exsposed]
		public void ErrorLog(string msg) {
			Log.Err("[RhuScript] " + msg);
		}
		[Exsposed]
		public void CallMainMethod() {
			CallMainMethodAndReturn();
		}

		[Exsposed]
		public object CallMainMethodAndReturn() {
			if (Error != null) {
				return null;
			}
			try {
				return MainMethod?.Invoke();
			}
			catch (Exception e) 
			{
				Error = e;
				Log.Err("Error in RhuScript Error: " + e.ToString());
			}
			return null;
		}

		[Exsposed]
		public T CallAndReturn<T>() {
			return (T)CallMainMethodAndReturn();
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			base.Deserialize(data, syncObjectSerializerObject);
			MainMethod = Serializer.Read<IScriptNode>(((DataNode<byte[]>)((DataNodeGroup)data).GetValue("Code")).Value);
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			var dataNodeGroup = (DataNodeGroup)base.Serialize(syncObjectSerializerObject);
			dataNodeGroup.SetValue("Code", new DataNode<byte[]>(Serializer.Save(_MainMethod)));
			return dataNodeGroup;
		}
	}
}
