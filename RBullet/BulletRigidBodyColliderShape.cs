using System;

using BulletSharp;

using RhuEngine.Physics;

namespace RBullet
{
	public class BulletColliderShape : ILinkedColliderShape
	{
		public object GetBox2D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new Box2DShape(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}

		public object GetCapsuleShape(double radius, double height) {
			return new CapsuleShape(radius, height);
		}

		public object GetCapsuleShapeX(double radius, double height) {
			return new CapsuleShapeX(radius, height);
		}

		public object GetCapsuleShapeZ(double radius, double height) {
			return new CapsuleShapeZ(radius, height);
		}

		public RigidBodyCollider GetCollider(ColliderShape obj, PhysicsSim physicsSim) {
			var collider = new RigidBodyCollider {
				CollisionShape = obj,
				PhysicsSim = physicsSim
			};
			new BRigidBodyCollider(collider);
			return collider;
		}

	}
}
