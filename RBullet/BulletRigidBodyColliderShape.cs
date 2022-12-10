using System;
using System.Linq;
using System.Numerics;

using BulletSharp;


using RhuEngine.Physics;

using RNumerics;

namespace RBullet
{
	public sealed class BulletColliderShape : ILinkedColliderShape
	{
		public object GetBox2D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new Box2DShape((float)boxHalfExtentX, (float)boxHalfExtentY, (float)boxHalfExtentZ);
		}

		public object GetCapsuleShape(double radius, double height) {
			return new CapsuleShape((float)radius, (float)height);
		}

		public object GetCapsuleShapeX(double radius, double height) {
			return new CapsuleShapeX((float)radius, (float)height);
		}

		public object GetCapsuleShapeZ(double radius, double height) {
			return new CapsuleShapeZ((float)radius, (float)height);
		}
		public object GetBox3D(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new BoxShape((float)boxHalfExtentX, (float)boxHalfExtentY, (float)boxHalfExtentZ);
		}
		public object GetCone(double radius, double height) {
			return new ConeShape((float)radius, (float)height);
		}
		public object GetConeX(double radius, double height) {
			return new ConeShapeX((float)radius, (float)height);
		}
		public object GetConeZ(double radius, double height) {
			return new ConeShapeZ((float)radius, (float)height);
		}
		public object GetCylinderShape(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new CylinderShape((float)boxHalfExtentX, (float)boxHalfExtentY, (float)boxHalfExtentZ);
		}
		public object GetCylinderShapeX(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new CylinderShapeX((float)boxHalfExtentX, (float)boxHalfExtentY, (float)boxHalfExtentZ);
		}

		public object GetCylinderShapeZ(double boxHalfExtentX, double boxHalfExtentY, double boxHalfExtentZ) {
			return new CylinderShapeZ((float)boxHalfExtentX, (float)boxHalfExtentY, (float)boxHalfExtentZ);
		}
		public object GetSphereShape(double radus) {
			return new SphereShape((float)radus);
		}

		public RigidBodyCollider GetCollider(ColliderShape obj, PhysicsSim physicsSim) {
			var collider = new RigidBodyCollider {
				CollisionShape = obj,
				PhysicsSim = physicsSim
			};
			collider.obj = new BRigidBodyCollider(collider);
			return collider;
		}

		public object GetConvexMeshShape(IMesh mesh) {
			if (mesh == null) {
				return new EmptyShape();
			}
			var verts = mesh.VertexPos().Select((val) => new Vector3(val.x, val.y, val.z)).ToArray();
			var trys = new ConvexHullShape(verts);
			return trys;
		}

		public object GetRawMeshShape(IMesh mesh) {
			if (mesh == null) {
				return new EmptyShape();
			}
			if (mesh.TriangleCount <= 0) {
				return new EmptyShape();
			}

			var indces = mesh.RenderIndices().ToArray();
			var verts = mesh.VertexPos().Select((val) => new Vector3(val.x, val.y, val.z)).ToArray();
			var indexVertexArray2 = new TriangleIndexVertexArray();
			var _initialMesh = new IndexedMesh();
			_initialMesh.Allocate(indces.Length / 3, verts.Length);
			_initialMesh.SetData(indces, verts);
			indexVertexArray2.AddIndexedMesh(_initialMesh);
			var trys = new BvhTriangleMeshShape(indexVertexArray2, false);
			return trys;
		}

		public object GetSingleCompoundShape() {
			return new CompoundShape();
		}

		public void CompoundShapeAdd(object comp, object shape, Matrix matrix) {
			if (comp is CompoundShape compound) {
				compound.AddChildShape(matrix.m, (CollisionShape)shape);
			}
		}

		public void CompoundShapeMove(object comp, object shape, Matrix matrix) {
			if (comp is CompoundShape compound) {
				compound.RemoveChildShape((CollisionShape)shape);
				compound.AddChildShape(matrix.m, (CollisionShape)shape);
			}
		}

		public void CompoundShapeRemove(object comp, object shape) {
			if (comp is CompoundShape compound) {
				compound.RemoveChildShape((CollisionShape)shape);
			}
		}
	}
}
