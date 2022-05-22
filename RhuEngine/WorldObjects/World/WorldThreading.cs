using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using System.Collections.Generic;
using RhuEngine.Linker;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public static class WorldThreadSafty
	{
		[ThreadStatic]
		public static uint MethodCalls = 0;

		public static uint MaxCalls = 25;
	}

	public partial class World
	{
		private readonly List<Action> _actions = new();

		public void AddCoroutineEnd(Action action) {
			_actions.Add(action);
		}
		public void AddCoroutine(Action action) {
			_actions.Add(Task.Run(action).Wait);
		}
		private void UpdateCoroutine() {
			var e = _actions.GetEnumerator();
			while (e.MoveNext()) {
				try {
					e.Current();
				}
				catch (Exception ex) 
				{
					RLog.Err("Failed to update Coroutine in " + WorldDebugName + " Error:" + ex.ToString());
				}
			}
			_actions.Clear();
		}
	}
}
