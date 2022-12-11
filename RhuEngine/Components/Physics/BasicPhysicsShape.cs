using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using BepuPhysics;
using RhuEngine.Physics;
using System.Runtime.InteropServices;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using BepuUtilities;
using System.Numerics;
using static RNumerics.RoundRectGenerator;
using System.Runtime.CompilerServices;

namespace RhuEngine.Components
{
	public abstract class BasicPhysicsShape<T> : PhysicsShape<T> where T : unmanaged, IShape
	{
		public override void RemoveShape() {
			Simulation.Simulation.Shapes.RemoveAndDispose(ShapeIndex, Simulation.BufferPool);
			ShapeIndex = default;
		}

	}
}
