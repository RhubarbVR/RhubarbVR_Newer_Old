using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{
#if DEBUG
	[Category(new string[] { "RhuScript\\ScriptBuilders" })]
	public class NullScriptBuilder : ScriptBuilder
	{
		public override void Compile() {
		}

		public override void LoadFromScript() {
		}

		public override void OnClearError() {
		}

		public override void OnError() {
		}

		public override void OnGainFocus() {
		}

		public override void OnLostFocus() {
		}

		public override void OnRhuScriptAdded() {
		}
	}
#endif
	[Category(new string[] { "RhuScript\\ScriptBuilders" })]
	public abstract class ScriptBuilder : Component
	{
		[OnChanged(nameof(OnRhuScriptAddedInt))]
		public readonly SyncRef<RhuScript> script;

		public IScriptNode ScriptNode
		{
			get => script.Target?.MainMethod;
			set {
				if (script.Target is not null) {
					script.Target.MainMethod = value;
				}
			}
		}
		public void OnRhuScriptAddedInt() {
			if (script.Target is not null) {
				script.Target.OnClearError.Target = OnClearError;
				script.Target.OnError.Target = OnError;
			}
			OnRhuScriptAdded();
		}

		public abstract void OnRhuScriptAdded();

		public abstract void OnGainFocus();

		public abstract void OnLostFocus();
		[Exposed]
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

		[Exposed]
		public abstract void LoadFromScript();
		[Exposed]
		public abstract void Compile();
		[Exposed]

		public abstract void OnError();

		[Exposed]

		public abstract void OnClearError();

	}
}
