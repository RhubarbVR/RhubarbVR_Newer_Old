using System;

using RhuEngine.Physics;
using BulletSharp;
using RNumerics;
using BulletSharp.Math;

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

		public bool RayTest(object sem, ref Vector3f rayFromWorld, ref Vector3f rayToWorld, out RigidBodyCollider rigidBodyCollider, out Vector3f HitNormalWorld, out Vector3f HitPointWorld, ECollisionFilterGroups mask, ECollisionFilterGroups group) {
			var frome = new Vector3(rayFromWorld.x, rayFromWorld.y, rayFromWorld.z);
			var toe = new Vector3(rayToWorld.x, rayToWorld.y, rayToWorld.z);

			var callback = new ClosestRayResultCallback(ref frome, ref toe) {
				CollisionFilterGroup = (int)group,
				CollisionFilterMask = (int)group
			};
			((BPhysicsSim)sem)._physicsWorld.RayTestRef(ref frome,ref toe,callback);
			try {
				rigidBodyCollider = ((BRigidBodyCollider)callback.CollisionObject.UserObject).Collider;
			}
			catch {
				rigidBodyCollider = null;
			}
			HitPointWorld = new Vector3f(callback.HitPointWorld.X, callback.HitPointWorld.Y, callback.HitPointWorld.Z);
			HitNormalWorld = new Vector3f(callback.HitNormalWorld.X, callback.HitNormalWorld.Y, callback.HitNormalWorld.Z);
			return callback.HasHit;
		}

		public void UpdateSim(object obj,float DeltaSeconds) {
			((BPhysicsSim)obj)._physicsWorld.StepSimulation(DeltaSeconds);
			((BPhysicsSim)obj)._physicsWorld.ComputeOverlappingPairs();
		}
	}
}
