using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using BepuUtilities.Memory;
using BepuUtilities;

using RhuEngine.WorldObjects;
using BepuPhysics.Constraints;
using System.Numerics;
using RhuEngine.Components;

namespace RhuEngine.Physics
{
	public sealed class PhysicsSimulation : IDisposable
	{

		public Vector3 WorldGravity => World.WorldGravity.Value;

		public World World { get; private set; }

		public Simulation Simulation { get; private set; }

		public BufferPool BufferPool { get; private set; }

		public void Dispose() {
			Simulation.Dispose();
			BufferPool.Clear();
			GC.SuppressFinalize(this);
		}

		public void Init(World world) {
			World = world;
			BufferPool = new BufferPool();
			Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(new SpringSettings(30, 1)), new PoseIntegratorCallbacks(this), new SolveDescription(8, 1));

		}

		public void Update(double elapsed) {
			Simulation.Timestep((float)elapsed);
		}
	}
}
