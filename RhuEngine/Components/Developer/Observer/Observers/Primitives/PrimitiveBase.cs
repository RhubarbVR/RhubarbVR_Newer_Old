using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;

namespace RhuEngine.Components
{
	public abstract class PrimitiveBase<T>:ObserverBase<Sync<T>>
	{
	}
}
