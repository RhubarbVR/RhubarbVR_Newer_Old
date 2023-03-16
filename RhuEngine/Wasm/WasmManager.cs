using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine.Wasm;

namespace RhuEngine.Managers
{
	public sealed class WasmManager : IManager
	{
		public List<WasmInstance> wasmInstances = new();
		public static List<WasmTask> wasmTasks = new();

		public static void RemoveTask(WasmTask wasmTask) {
			lock (wasmTasks) {
				wasmTasks.Remove(wasmTask);
			}
		}

		public static void AddTask(WasmTask wasmTask) {
			lock (wasmTasks) {
				wasmTasks.Add(wasmTask);
			}
		}

		public void RemoveWasmInst(WasmInstance wasmInst) {
			lock (wasmInstances) {
				wasmInstances.Remove(wasmInst);
			}
		}

		public void AddWasmInst(WasmInstance wasmInst) {
			lock (wasmInstances) {
				wasmInstances.Add(wasmInst);
			}
		}


		public void Dispose() {
		}

		public void Init(Engine engine) {
		}

		public void RenderStep() {
		}

		public void Step() {
		}
	}
}
