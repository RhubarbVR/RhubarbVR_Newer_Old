using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;

using BepuPhysics.Constraints;

using BepuPhysics;

using BepuUtilities;
using Esprima.Ast;
using RhuEngine.WorldObjects;

namespace RhuEngine.Physics
{
	public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		public PhysicsSimulation Simulation;

		public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

		public readonly bool AllowSubstepsForUnconstrainedBodies => false;
		public readonly bool IntegrateVelocityForKinematics => false;

		public void Initialize(Simulation simulation) {
		}

		public PoseIntegratorCallbacks(PhysicsSimulation sim) : this() {
			Simulation = sim;
		}



		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PrepareForIntegration(float dt) {
			
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity) {

		}
	}
}
