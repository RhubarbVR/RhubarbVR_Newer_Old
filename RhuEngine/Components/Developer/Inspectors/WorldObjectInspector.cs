using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	public class WorldObjectInspector : Component
	{
		public readonly SyncRef<IWorldObject> TargetObject;
	}
}