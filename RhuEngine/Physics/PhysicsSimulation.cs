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
using BepuPhysics.Collidables;
using System.Runtime.CompilerServices;
using BepuPhysics.CollisionDetection;
using RNumerics;

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
		public PhysicsObject GetCollider(StaticHandle handle) {
			return handle.Value < _staticPhysicsObjects.Length ? _staticPhysicsObjects[handle.Value] : null;
		}
		public BodyPhysicsObject GetCollider(BodyHandle handle) {
			return handle.Value < _bodyPhysicsObjects.Length ? _bodyPhysicsObjects[handle.Value] : null;
		}

		public PhysicsObject GetCollider(CollidableReference collidable) {
			return collidable.Mobility == CollidableMobility.Static ? GetCollider(collidable.StaticHandle) : GetCollider(collidable.BodyHandle);
		}

		private PhysicsObject[] _staticPhysicsObjects = Array.Empty<PhysicsObject>();
		private BodyPhysicsObject[] _bodyPhysicsObjects = Array.Empty<BodyPhysicsObject>();

		internal void RegisterPhysicsObject(StaticHandle handle, PhysicsObject physicsShape) {
			RegisterCollider(handle.Value, physicsShape, ref _staticPhysicsObjects);
		}

		internal void RegisterPhysicsObject(BodyHandle handle, BodyPhysicsObject physicsShape) {
			RegisterCollider(handle.Value, physicsShape, ref _bodyPhysicsObjects);
		}

		internal void UnRegisterPhysicsObject(StaticHandle handle) {
			_staticPhysicsObjects[handle.Value] = null;
		}

		internal void UnRegisterPhysicsObject(BodyHandle handle) {
			_bodyPhysicsObjects[handle.Value] = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RegisterCollider<T>(int index, T collider, ref T[] colliders) where T : PhysicsObject {
			if (colliders.Length <= index) {
				colliders = colliders.EnsureSize(index + 1, true);
			}
			colliders[index] = collider;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void UnRegisterCollider(int index, PhysicsObject collider, ref PhysicsObject[] colliders) {
			if (colliders.Length <= index) {
				var minLength = Math.Min(colliders.Length * 2, index + 1);
				colliders = colliders.EnsureSize(minLength, true);
			}
			colliders[index] = collider;
		}
		public bool RayCast(in Vector3f orgin, in Vector3f normal, in float dist, out PhysicsObject collider, out Vector3f hitnormal, out Vector3f hitpointworld, EPhysicsMask rayMask = EPhysicsMask.Normal) {
			var hitTest = new RaycastOneHandler(this, x => x.Group.Value.BasicCheck(rayMask));
			Simulation.RayCast(orgin, normal, dist, ref hitTest);
			hitnormal = hitTest.hitNormal;
			hitpointworld = orgin + (normal * dist);
			collider = hitTest.hasHit ? GetCollider(hitTest.hitCollidable) : null;
			return hitTest.hasHit;
		}

		public bool RayCast(in Vector3f frompos, in Vector3f toPos, out PhysicsObject collider, out Vector3f hitnormal, out Vector3f hitpointworld, EPhysicsMask rayMask = EPhysicsMask.Normal) {
			return RayCast(frompos, toPos - frompos, 1, out collider, out hitnormal, out hitpointworld, rayMask);
		}
	}
}
