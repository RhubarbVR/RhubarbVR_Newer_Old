using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Physics
{
	public interface ILinkedColliderShape
	{
		public RigidBodyCollider GetCollider(ColliderShape obj, PhysicsSim physicsSim);

		public object GetCapsuleShapeX(double radius, double height);
		public object GetCapsuleShapeZ(double radius, double height);
		public object GetCapsuleShape(double radius, double height);

		public object GetBox2D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
	}

	public abstract class ColliderShape
	{
		public static ILinkedColliderShape Manager { get; set; }

		public object obj;

		public object CustomObject { get; set; }

		public RigidBodyCollider GetCollider(PhysicsSim physicsSim) {
			var col =  Manager?.GetCollider(this, physicsSim);
			return col;
		}
	}

	public class RBox2DShape : ColliderShape
	{
		public RBox2DShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			obj = Manager?.GetBox2D(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
	}


	public class RCapsuleShapeX:ColliderShape
	{
		public RCapsuleShapeX(double radius, double height) { 
			obj = Manager?.GetCapsuleShapeX(radius, height);
		}
	}

	public class RCapsuleShapeZ : ColliderShape
	{
		public RCapsuleShapeZ(double radius, double height) {
			obj = Manager?.GetCapsuleShapeZ(radius, height);
		}
	}
	public class RCapsuleShape : ColliderShape
	{
		public RCapsuleShape(double radius, double height) {
			obj = Manager?.GetCapsuleShape(radius, height);
		}
	}
}
