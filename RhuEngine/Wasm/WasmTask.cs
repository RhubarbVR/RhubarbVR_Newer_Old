using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.Managers;

using SharedModels.GameSpecific;

namespace RhuEngine.Wasm
{
	public sealed class WasmTask : IDisposable
	{
		[ThreadStatic]
		public static WasmTask CurrentTask;

		public Task Task { get; private set; }

		private readonly List<WasmInstance> _wasmInstances = new(1);
		private readonly List<string> _methodNames = new(1);

		public object ReturnData;

		public CancellationToken cancellationToken = new();

		public bool HasWasmInstance(WasmInstance wasmInstance) {
			return _wasmInstances.Contains(wasmInstance);
		}

		public WasmTask(WasmInstance wasmInstance, string methodName) {
#if DEBUG
			RLog.Info("Wasm Task Created");
#endif
			_wasmInstances.Add(wasmInstance);
			_methodNames.Add(methodName);
			WasmManager.AddTask(this);
		}

		public void AddSubFunction(WasmInstance wasmInstance, string name) {
			_methodNames.Add(name);
			_wasmInstances.Add(wasmInstance);
		}


		public static WasmTask RunWasmCompileTask(WasmInstance wasmInstance, Action action) {
			if (CurrentTask is not null) {
				throw new Exception("Wasm script can not compile another wasm script");
			}
			else {
				var newTask = new WasmTask(wasmInstance, "main");
				newTask.Task = new Task(() => {
					CurrentTask = newTask;
					action();
					newTask.Dispose();
					CurrentTask = null;
				}, newTask.cancellationToken);
				newTask.Task.Start();
				return newTask;
			}
		}

		public static WasmTask RunWasmTask(WasmInstance wasmInstance, string methodName, params object[] prams) {
			if (CurrentTask is not null) {
				CurrentTask.AddSubFunction(wasmInstance, methodName);
				CurrentTask.ReturnData = wasmInstance.InvokeMethod(methodName, prams);
				return CurrentTask;
			}
			else {
				var newTask = new WasmTask(wasmInstance, methodName);
				newTask.Task = new Task(() => {
					CurrentTask = newTask;
					CurrentTask.ReturnData = wasmInstance.InvokeMethod(methodName, prams);
					newTask.Dispose();
					CurrentTask = null;
				}, newTask.cancellationToken);
				newTask.Task.Start();
				return newTask;
			}
		}

		public void Dispose() {
#if DEBUG
			RLog.Info("Wasm Task Ended");
#endif
			WasmManager.RemoveTask(this);
			ReturnData = null;
			foreach (var item in _wasmInstances) {
				item.wasmRunner?.RemoveTask(this);
			}
		}
	}
}
