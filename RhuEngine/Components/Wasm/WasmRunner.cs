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

namespace RhuEngine.Components
{
	[Category(new string[] { "Wasm" })]
	public sealed partial class WasmRunner : Component
	{
		public abstract partial class WasmFunction : SyncObject
		{
			[Default("Main")]
			public readonly Sync<string> FunctionName;
			public WasmRunner Runner => (WasmRunner)Parent.Parent;
		}

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



		public readonly SyncAbstractObjList<WasmFunction> wasmFunctions;

		public void InvokeMethod(string methodName, params object[] args) {


		}

	}
}
