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
		
	}
}
