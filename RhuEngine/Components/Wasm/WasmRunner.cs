using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using WebAssembly;
using WebAssembly.Instructions;
using WebAssembly.Runtime;
using RhuEngine.Commads;
using RhuEngine.Wasm;
using System.IO;

namespace RhuEngine.Components
{
	[Category(new string[] { "Wasm" })]
	public sealed partial class WasmRunner : Component
	{
		public abstract partial class WasmFunction : SyncObject
		{
			[Default("WasmFunction")]
			public readonly Sync<string> FunctionName;
			public WasmRunner Runner => (WasmRunner)Parent.Parent;
		}

		#region WasmFunction
		public sealed partial class VoidWasmFunction : WasmFunction
		{
			[Exposed]
			public void Execute() {
				Runner.InvokeMethod(FunctionName);
			}

		}
		public sealed partial class VoidWasmFunction<T1> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1) {
				Runner.InvokeMethod(FunctionName, t1);
			}
		}

		public sealed partial class VoidWasmFunction<T1, T2> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2) {
				Runner.InvokeMethod(FunctionName, t1, t2);
			}
		}

		public sealed partial class VoidWasmFunction<T1, T2, T3> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2, T3 t3) {
				Runner.InvokeMethod(FunctionName, t1, t2, t3);
			}
		}

		public sealed partial class VoidWasmFunction<T1, T2, T3, T4> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2, T3 t3, T4 t4) {
				Runner.InvokeMethod(FunctionName, t1, t2, t3, t4);
			}
		}

		public sealed partial class VoidWasmFunction<T1, T2, T3, T4, T5> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
				Runner.InvokeMethod(FunctionName, t1, t2, t3, t4, t5);
			}
		}

		public sealed partial class VoidWasmFunction<T1, T2, T3, T4, T5, T6> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
				Runner.InvokeMethod(FunctionName, t1, t2, t3, t4, t5, t6);
			}
		}

		public sealed partial class VoidWasmFunction<T1, T2, T3, T4, T5, T6, T7> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
				Runner.InvokeMethod(FunctionName, t1, t2, t3, t4, t5, t6, t7);
			}
		}
		public sealed partial class VoidWasmFunction<T1, T2, T3, T4, T5, T6, T7, T8> : WasmFunction
		{
			[Exposed]
			public void Execute(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
				Runner.InvokeMethod(FunctionName, t1, t2, t3, t4, t5, t6, t7, t8);
			}
		}
		#endregion WasmFunction

		public readonly SyncAbstractObjList<WasmFunction> wasmFunctions;

		[OnAssetLoaded(nameof(AssetLoaded))]
		public readonly AssetRef<IBinaryAsset> TargetScript;

		private bool _needsToCompile = true;

		private WasmInstance _wasmInstance;

		private readonly List<WasmTask> _wasmTasks = new();

		private void AssetLoaded(IBinaryAsset _) {
			_needsToCompile = true;
		}

		private void Compile() {
			if (TargetScript.Asset is null) {
				return;
			}
			lock (_wasmTasks) {
				_wasmTasks.Add(_wasmInstance.CompileScript(TargetScript.Asset.CreateStream()));
				_needsToCompile = false;
			}
		}

		protected override void OnLoaded() {
			_wasmInstance = new() {
				wasmRunner = this
			};
			Engine.wasmManager.AddWasmInst(_wasmInstance);
			Compile();
			base.OnLoaded();
		}

		public override void Dispose() {
			Engine.wasmManager.RemoveWasmInst(_wasmInstance);
			base.Dispose();
		}

		internal void RemoveTask(WasmTask wasmTask) {
			lock (_wasmTasks) {
				if (_wasmTasks.Contains(wasmTask)) {
					_wasmTasks.Remove(wasmTask);
				}
			}
		}

		public void InvokeMethod(string methodName, params object[] args) {
			CallCompile();
			lock (_wasmTasks) {
				_wasmTasks.Add(WasmTask.RunWasmTask(_wasmInstance, methodName, args));
			}
		}

		public void CallCompile() {
			if (_needsToCompile) {
				Compile();
			}
		}
	}
}
