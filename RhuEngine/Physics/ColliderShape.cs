using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Physics
{
	public interface ILinkedColliderShape
	{
		public RigidBodyCollider GetCollider(ColliderShape obj, PhysicsSim physicsSim);

		public object GetCapsuleShapeX(double radius, double height);
		public object GetCapsuleShapeZ(double radius, double height);
		public object GetCapsuleShape(double radius, double height);

		public object GetBox2D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetBox3D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetCone(double radius, double height);
		public object GetConeX(double radius, double height);
		public object GetConeZ(double radius, double height);
		public object GetCylinderShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetCylinderShapeX(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);

		public object GetConvexMeshShape(IMesh mesh);

		public object GetCylinderShapeZ(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetSphereShape(double radus);
	}

	public abstract class ColliderShape
	{
		public static ILinkedColliderShape Manager { get; set; }

		public object obj;

		public RigidBodyCollider GetCollider(PhysicsSim physicsSim,object selfobject = null,bool startActive = true) {
			var col =  Manager?.GetCollider(this, physicsSim);
			col.CustomObject = obj;
			col.Active = startActive;
			return col;
		}
		public RigidBodyCollider GetCollider(PhysicsSim physicsSim,Matrix startingpos, object selfobject = null, bool startActive = true) {
			var col = Manager?.GetCollider(this, physicsSim);
			col.CustomObject = selfobject;
			col.Matrix = startingpos;
			col.Active = startActive;
			return col;
		}
	}

	public class RConvexMeshShape : ColliderShape
	{
		public RConvexMeshShape(IMesh mesh) {
			obj = Manager?.GetConvexMeshShape(mesh);
		}
	}

	public class RConeShape : ColliderShape
	{
		public RConeShape(double radius, double height) {
			obj = Manager?.GetCone(radius, height);
		}
	}
	public class RConeXShape : ColliderShape
	{
		public RConeXShape(double radius, double height) {
			obj = Manager?.GetConeX(radius, height);
		}
	}
	public class RConeZShape : ColliderShape
	{
		public RConeZShape(double radius, double height) {
			obj = Manager?.GetConeZ(radius, height);
		}
	}
	public class RBoxShape : ColliderShape
	{
		public RBoxShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			obj = Manager?.GetBox3D(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
		public RBoxShape(double boxHalfExtentXYZ) {
			obj = Manager?.GetBox3D(boxHalfExtentXYZ, boxHalfExtentXYZ, boxHalfExtentXYZ);
		}

		public RBoxShape(Vector3d collidersize) {
			obj = Manager?.GetBox3D(collidersize.x, collidersize.y, collidersize.z);
		}
	}

	public class RCylinderShape : ColliderShape
	{
		public RCylinderShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			obj = Manager?.GetCylinderShape(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
	}
	public class RCylinderXShape : ColliderShape
	{
		public RCylinderXShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			obj = Manager?.GetCylinderShapeX(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
	}
	public class RCylinderZShape : ColliderShape
	{
		public RCylinderZShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			obj = Manager?.GetCylinderShapeX(boxHalfExtentX, boxHalfExtentY, boxHalfExtentZ);
		}
	}
	public class RSphereShape : ColliderShape
	{
		public RSphereShape(double radus) {
			obj = Manager?.GetSphereShape(radus);
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
