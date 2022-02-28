using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript\\ScriptBuilders" })]
	public abstract class ScriptBuilder : Component
	{
		[OnChanged(nameof(OnRhuScriptAdded))]
		public SyncRef<RhuScript> script;

		public IScriptNode ScriptNode
		{
			get => script.Target?.MainMethod;
			set {
				if (script.Target is not null) {
					script.Target.MainMethod = value;
				}
			}
		}
		public abstract void OnRhuScriptAdded();

		public abstract void OnGainFocus();

		public abstract void OnLostFocus();
		[Exsposed]
		public void FocusScriptBuilder() {
			if (IsFocused) {
				OnGainFocus();
				return;
			}
			World.FocusedScriptBuilder?.OnLostFocus();
			World.FocusedScriptBuilder = this;
			OnGainFocus();
		}

		public bool IsFocused => World.FocusedScriptBuilder == this;

		[Exsposed]
		public abstract void LoadFromScript();
		[Exsposed]
		public abstract void Compile();

	}
}
