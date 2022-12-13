using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;

using RhuEngine.Components;

namespace RhuEngine.Physics
{
	public struct RaycastOneHandler : IRayHitHandler
	{
		private readonly PhysicsSimulation _simulation;

		private readonly Predicate<PhysicsObject> _filter;

		public float hitT;

		public Vector3 hitNormal;

		public CollidableReference hitCollidable;

		public int hitChildIndex;

		public bool hasHit;

		public RaycastOneHandler(PhysicsSimulation simulation, Predicate<PhysicsObject> filter) {
			_simulation = simulation;
			_filter = filter;
			hitT = float.PositiveInfinity;
			hitNormal = default;
			hitCollidable = default;
			hitChildIndex = 0;
			hasHit = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowTest(CollidableReference collidable) {
			var collider = _simulation.GetCollider(collidable);
			return !(collider?.IsRemoved ?? true) && (collider?.IsEnabled ?? false) && (_filter == null || _filter(collider));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowTest(CollidableReference collidable, int childIndex) {
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex) {
			if (t < hitT) {
				hitT = t;
				hitNormal = normal;
				hitCollidable = collidable;
				hitChildIndex = childIndex;
				maximumT = t;
				hasHit = true;
			}
		}
	}

}
