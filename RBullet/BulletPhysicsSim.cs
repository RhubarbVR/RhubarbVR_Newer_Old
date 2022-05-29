using System;

using RhuEngine.Physics;
using BulletSharp;
using RNumerics;
using System.Numerics;

namespace RBullet
{
	public class BPhysicsSim
	{
		public readonly DefaultCollisionConfiguration _collisionConfiguration;

		public readonly CollisionDispatcher _dispatcher;

		public readonly DbvtBroadphase _broadphase;

		public readonly DiscreteDynamicsWorld _physicsWorld;
		
		public readonly ConstraintSolverPoolMultiThreaded _solverPool;
		public readonly SequentialImpulseConstraintSolverMultiThreaded _parallelSolver;

		public BPhysicsSim() {
			//var collisionConfigurationInfo = new DefaultCollisionConstructionInfo {
			//	DefaultMaxPersistentManifoldPoolSize = 80000,
			//	DefaultMaxCollisionAlgorithmPoolSize = 80000
			//};
			_collisionConfiguration = new DefaultCollisionConfiguration();
			_dispatcher = new CollisionDispatcher(_collisionConfiguration);
			_broadphase = new DbvtBroadphase();
			_solverPool = new ConstraintSolverPoolMultiThreaded(Math.Min(Environment.ProcessorCount - 1,1));
			_parallelSolver = new SequentialImpulseConstraintSolverMultiThreaded();
			_physicsWorld = new DiscreteDynamicsWorldMultiThreaded(_dispatcher, _broadphase, _solverPool, _parallelSolver, _collisionConfiguration);
			//collisionConfigurationInfo.Dispose();
		}

		public SafeList<BRigidBodyCollider> Updates = new SafeList<BRigidBodyCollider>();

		public void AddPhysicsCallBack(BRigidBodyCollider bRigidBodyCollider) {
			Updates.SafeOperation((list) => {
				if (!list.Contains(bRigidBodyCollider)) {
					list.Add(bRigidBodyCollider);
				}
			});
		}
		public void RemovePhysicsCallBack(BRigidBodyCollider bRigidBodyCollider) {
			Updates.SafeOperation((list) => {
				if (list.Contains(bRigidBodyCollider)) {
					list.Remove(bRigidBodyCollider);
				}
			});
		}

		public delegate void BOverlapCallback(Vector3 PositionWorldOnA, Vector3 PositionWorldOnB, Vector3 NormalWorldOnB, double Distance, double Distance1,BRigidBodyCollider hit);

		public class Tester : ContactResultCallback
		{
			public BOverlapCallback callback1;
			public Tester(BOverlapCallback bOverlapCallback) {
				callback1 = bOverlapCallback;
			}

			public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1) {
				callback1.Invoke(cp.PositionWorldOnA, cp.PositionWorldOnB, cp.NormalWorldOnB, cp.Distance, cp.Distance1, (BRigidBodyCollider)colObj0Wrap.CollisionObject.UserObject);
				return 0;
			}
		}

		public void RunCallBacks() {
			Updates.SafeOperation((list) => {
				foreach (var item in list) {
					if (item.collisionObject != null) {
						_physicsWorld.ContactTest(item.collisionObject, new Tester((Vector3 PositionWorldOnA, Vector3 PositionWorldOnB, Vector3 NormalWorldOnB, double Distance, double Distance1, BRigidBodyCollider hit) => {
							item.Call(new Vector3f((float)PositionWorldOnA.X, (float)PositionWorldOnA.Y, (float)PositionWorldOnA.Z),
								new Vector3f((float)PositionWorldOnB.X, (float)PositionWorldOnB.Y, (float)PositionWorldOnB.Z),
								new Vector3f((float)NormalWorldOnB.X, (float)NormalWorldOnB.Y, (float)NormalWorldOnB.Z), Distance, Distance1, hit.Collider);
						}));
					}
				}
			});
		}
	}


	public class BulletPhysicsSim : ILinkedPhysicsSim
	{
		public bool ConvexRayTest(object sem, ColliderShape colliderShape, ref RNumerics.Matrix from, ref RNumerics.Matrix to, out RigidBodyCollider rigidBodyCollider, out Vector3f HitNormalWorld, out Vector3f HitPointWorld, ECollisionFilterGroups mask, ECollisionFilterGroups group) {
			var frome = new Vector3(from.Translation.x, from.Translation.y, from.Translation.z);
			var toe = new Vector3(to.Translation.x, to.Translation.y, to.Translation.z);
			var fromm = BRigidBodyCollider.CastMet(from);
			var tom = BRigidBodyCollider.CastMet(to);
			var callback = new ClosestConvexResultCallback(ref frome, ref toe) {
				CollisionFilterGroup = (int)group,
				CollisionFilterMask = (int)group
			};
			((BPhysicsSim)sem)._physicsWorld.ConvexSweepTestRef((ConvexShape)colliderShape.obj,ref fromm, ref tom, callback);
			try {
				rigidBodyCollider = null;
				if (callback.HitCollisionObject != null) {
					if (callback.HitCollisionObject.UserObject != null) {
						rigidBodyCollider = ((BRigidBodyCollider)callback.HitCollisionObject.UserObject).Collider;
					}
				}
			}
			catch {
				rigidBodyCollider = null;
			}
			HitPointWorld = new Vector3f(callback.HitPointWorld.X, callback.HitPointWorld.Y, callback.HitPointWorld.Z);
			HitNormalWorld = new Vector3f(callback.HitNormalWorld.X, callback.HitNormalWorld.Y, callback.HitNormalWorld.Z);
			return callback.HasHit;
		}

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
				rigidBodyCollider = null;
				if (callback.CollisionObject != null) {
					if (callback.CollisionObject.UserObject != null) {
						rigidBodyCollider = ((BRigidBodyCollider)callback.CollisionObject.UserObject).Collider;
					}
				}
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
			((BPhysicsSim)obj).RunCallBacks();
		}
	}
}
