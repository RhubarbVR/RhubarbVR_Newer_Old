using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using RhuEngine.Components;

namespace RhuEngine.Physics
{
	public struct StaticChangeAwakeningFilter : IStaticChangeAwakeningFilter
	{
		public PhysicsSimulation simulation;

		public bool isEnabled;

		public EPhysicsMask ePhysicsMask;

		public StaticChangeAwakeningFilter(PhysicsSimulation simulation, bool isEnabled, EPhysicsMask ePhysicsMask) {
			this.simulation = simulation;
			this.isEnabled = isEnabled;
			this.ePhysicsMask = ePhysicsMask;
		}

		public bool AllowAwakening => isEnabled;

		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ShouldAwaken(BodyReference body) {
			var shape = simulation.GetCollider(body.Handle);
			return shape.IsEnabled && shape.CollisionEnabled && ePhysicsMask.BasicCheck(shape.Mask);
		}
	}

}
