using System;

using RhuEngine.Physics;
using BulletSharp;
namespace RBullet
{
	public class BPhysicsSim
	{
		public readonly DefaultCollisionConfiguration _collisionConfiguration;

		public readonly CollisionDispatcher _dispatcher;

		public readonly DbvtBroadphase _broadphase;

		public readonly DiscreteDynamicsWorld _physicsWorld;

		public readonly ConstraintSolverPoolMultiThreaded _constraintSolver;

		public BPhysicsSim() {
			_collisionConfiguration = new DefaultCollisionConfiguration();
			_dispatcher = new CollisionDispatcher(_collisionConfiguration);
			_broadphase = new DbvtBroadphase();
			_constraintSolver = new ConstraintSolverPoolMultiThreaded(Environment.ProcessorCount);
			_physicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _constraintSolver, _collisionConfiguration);
		}
	}


	public class BulletPhysicsSim : ILinkedPhysicsSim
	{
		public object NewSim() {
			return new BPhysicsSim();
		}

		public void UpdateSim(object obj,float DeltaSeconds) {
			((BPhysicsSim)obj)._physicsWorld.StepSimulation(DeltaSeconds);
			((BPhysicsSim)obj)._physicsWorld.ComputeOverlappingPairs();
		}
	}
}
