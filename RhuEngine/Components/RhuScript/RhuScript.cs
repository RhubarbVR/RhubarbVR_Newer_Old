using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript" })]
	public class RhuScript : Component,IUpdatingComponent
	{
		[Exsposed]
		public (Action, SafeCall) ClearErrorsSafe(SafeCall safeCall) {
			return (() => ClearError(), SafeCall.MakeResponses(safeCall._number,Engine));
		}

		public void ClearError() {
			Log.Info("RhuScript Errors Cleared");
			Error = null;
			OnClearError.Target?.Invoke();
		}
		public SyncDelegate OnError;
		
		public SyncDelegate OnClearError;

		public Exception Error;

		public IScriptNode _MainMethod;

		public IScriptNode MainMethod
		{
			get => _MainMethod;
			set {
				_MainMethod = value;
				_MainMethod.LoadIntoWorld(World, this);
				LoadLocalValues();
			}
		}

		public uint AmountOfLocalValues { get; set; }

		public ScriptNodeWrite[] LocalValueNode = new ScriptNodeWrite[0];

		public void LoadLocalValues() {
			LocalValueNode = new ScriptNodeWrite[0];
			var nodeList = new List<IScriptNode>();
			_MainMethod.GetChildrenAll(nodeList);
			var e = from values in from value in nodeList
									where value is ScriptNodeWrite
									select (value as ScriptNodeWrite)
					orderby values.NodeIndex descending
					select values;
			foreach (var item in e) {
				if(LocalValueNode.Length < item.NodeIndex) {
					LocalValueNode = new ScriptNodeWrite[item.NodeIndex + 1];
					AmountOfLocalValues = item.NodeIndex + 1;
				}
				LocalValueNode[item.NodeIndex] = item;
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
				WorldThreadSafty.MethodCalls++;
				if (WorldThreadSafty.MethodCalls > WorldThreadSafty.MaxCalls) {
					throw new StackOverflowException();
				}
				var method = MainMethod?.Invoke(new ScriptNodeDataHolder(AmountOfLocalValues));
				WorldThreadSafty.MethodCalls--;
				return method;
			}
			catch (Exception e) 
			{
				Error = e;
				Log.Err("Error in RhuScript Error: " + e.ToString());
				OnError.Target?.Invoke();
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
