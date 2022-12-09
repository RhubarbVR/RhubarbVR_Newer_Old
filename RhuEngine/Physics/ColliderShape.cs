﻿using System;
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

		public object GetSingleCompoundShape();
		public void CompoundShapeAdd(object comp, object shape, Matrix matrix);
		public void CompoundShapeMove(object comp, object shape, Matrix matrix);
		public void CompoundShapeRemove(object comp, object shape);

		public object GetBox2D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetBox3D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetCone(double radius, double height);
		public object GetConeX(double radius, double height);
		public object GetConeZ(double radius, double height);
		public object GetCylinderShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetCylinderShapeX(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);

		public object GetConvexMeshShape(IMesh mesh);
		public object GetRawMeshShape(IMesh mesh);

		public object GetCylinderShapeZ(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ);
		public object GetSphereShape(double radus);
	}

	public abstract class ColliderShape:IDisposable
	{
		public static ILinkedColliderShape Manager { get; set; }

		public object obj;

		public RigidBodyCollider GetCollider(PhysicsSim physicsSim, object selfobject = null, bool startActive = true) {
			var col = Manager?.GetCollider(this, physicsSim);
			col.CustomObject = selfobject;
			col.Enabled = startActive;
			return col;
		}
		public RigidBodyCollider GetCollider(PhysicsSim physicsSim, Matrix startingpos, object selfobject = null, bool startActive = true) {
			var col = Manager?.GetCollider(this, physicsSim);
			col.CustomObject = selfobject;
			col.Matrix = startingpos;
			col.Enabled = startActive;
			return col;
		}

		public virtual void Dispose() {
			if(obj is IDisposable disposable) {
				disposable.Dispose();
			}
		}
	}
	public class RCompoundShape : ColliderShape
	{
		private readonly List<ColliderShape> _colliderShapes = new();
		private readonly List<Matrix> _pos = new();

		public bool HasShape(ColliderShape colliderShape) {
			lock (_colliderShapes) {
				return _colliderShapes.Contains(colliderShape);
			}
		}

		public Matrix ShapePos(ColliderShape colliderShapes) {
			lock (_colliderShapes) {
				return _pos[_colliderShapes.IndexOf(colliderShapes)];
			}
		}
		public void RemoveShape(ColliderShape colliderShape) {
			lock (_colliderShapes) {
				Manager?.CompoundShapeRemove(obj, colliderShape.obj);
				var index = _colliderShapes.IndexOf(colliderShape);
				_colliderShapes.RemoveAt(index);
				_pos.RemoveAt(index);
			}
		}
		public void AddShape(ColliderShape colliderShape, Matrix matrix) {
			lock (_colliderShapes) {
				Manager?.CompoundShapeAdd(obj, colliderShape.obj, matrix);
				_colliderShapes.Add(colliderShape);
				_pos.Add(matrix);
			}
		}

		public void MoveShape(ColliderShape colliderShape, Matrix matrix) {
			lock (_colliderShapes) {
				Manager?.CompoundShapeMove(obj, colliderShape.obj, matrix);
				_colliderShapes.Add(colliderShape);
				_pos.Add(matrix);
			}
		}

		public override void Dispose() {
			lock (_colliderShapes) {
				foreach (var item in _colliderShapes) {
					item.Dispose();
				}
				_colliderShapes.Clear();
				_pos.Clear();
			}
			if (obj is IDisposable disposable) {
				disposable.Dispose();
			}
		}

		public RCompoundShape() {
			obj = Manager?.GetSingleCompoundShape();
		}
	}
	public class RConvexMeshShape : ColliderShape
	{
		public RConvexMeshShape(IMesh mesh) {
			obj = Manager?.GetConvexMeshShape(mesh);
		}
	}
	public class RRawMeshShape : ColliderShape
	{
		public RRawMeshShape(IMesh mesh) {
			obj = Manager?.GetRawMeshShape(mesh);
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


	public class RCapsuleShapeX : ColliderShape
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
