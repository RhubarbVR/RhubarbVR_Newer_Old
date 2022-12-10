using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using System.Collections.Generic;
using RhuEngine.Linker;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public static class WorldThreadSafty
	{
		[ThreadStatic]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
		public static uint MethodCalls = 0;

		public const uint MAX_CALLS = 25;
	}

	public partial class World
	{
		
	}
}
