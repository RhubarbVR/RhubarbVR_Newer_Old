using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using StereoKit;
using System.Collections.Generic;

namespace RhuEngine.WorldObjects
{
	public static class WorldThreadSafty
	{
		[ThreadStatic]
		public static uint MethodCalls = 0;

		public static uint MaxCalls = 100;
	}

	public partial class World
	{
		private readonly List<Action> _actions = new();

		public void AddCoroutine(Action action) {
			_actions.Add(action);
		}

		private void UpdateCoroutine() {
			var e = _actions.GetEnumerator();
			while (e.MoveNext()) {
				try {
					e.Current();
				}
				catch (Exception ex) 
				{
					Log.Err("Failed to update Coroutine in " + WorldDebugName + " Error:" + ex.ToString());
				}
			}
			_actions.Clear();
		}
	}
}
