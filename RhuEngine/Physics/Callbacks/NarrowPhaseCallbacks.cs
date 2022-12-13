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

namespace RhuEngine.Physics
{
	public unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
	{
		public SpringSettings ContactSpringiness;
		public float MaximumRecoveryVelocity;
		public float FrictionCoefficient;

		public NarrowPhaseCallbacks(SpringSettings contactSpringiness, float maximumRecoveryVelocity = 2f, float frictionCoefficient = 1f) {
			ContactSpringiness = contactSpringiness;
			MaximumRecoveryVelocity = maximumRecoveryVelocity;
			FrictionCoefficient = frictionCoefficient;
		}

		public void Initialize(Simulation simulation) {
			if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0) {
				ContactSpringiness = new(30, 1);
				MaximumRecoveryVelocity = 2f;
				FrictionCoefficient = 1f;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin) {
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) {
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold> {
			pairMaterial.FrictionCoefficient = FrictionCoefficient;
			pairMaterial.MaximumRecoveryVelocity = MaximumRecoveryVelocity;
			pairMaterial.SpringSettings = ContactSpringiness;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold) {
			return true;
		}

		public void Dispose() {
		}
	}
}
