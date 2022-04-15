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
		public object GetBox3D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new BoxShape(boxHalfExtentX,boxHalfExtentY,boxHalfExtentZ);
		}
		public object GetCone(double radius, double height) {
			return new ConeShape(radius, height);
		}
		public object GetConeX(double radius, double height) {
			return new ConeShapeX(radius, height);
		}
		public object GetConeZ(double radius, double height) {
			return new ConeShapeZ(radius, height);
		}
		public object GetCylinderShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new CylinderShape(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
		public object GetCylinderShapeX(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new CylinderShapeX(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}

		public object GetCylinderShapeZ(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new CylinderShapeZ(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
		public object GetSphereShape(double radus) {
			return new SphereShape(radus);
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
