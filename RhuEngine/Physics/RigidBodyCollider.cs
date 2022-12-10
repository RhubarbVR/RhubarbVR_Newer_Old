using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Physics
{
	public interface ILinkedRigidBodyCollider
	{
		public bool ActivateGet(object obj);

		public void ActivateSet(object obj, bool val);
		public bool EnabledGet(object obj);

		public void EnabledSet(object obj, bool val);

		public float MassGet(object obj);
		public void MassSet(object obj, float val);

		public ECollisionFilterGroups GroupGet(object obj);
		public void GroupSet(object obj, ECollisionFilterGroups val);

		public ECollisionFilterGroups MaskGet(object obj);
		public void MaskSet(object obj, ECollisionFilterGroups val);

		public void Remove(object obj);

		public bool NoneStaticBodyGet(object obj);
		public void NoneStaticBodySet(object obj, bool val);

		public Matrix MatrixGet(object obj);
		public void MatrixSet(object obj, Matrix val);

		public void AddOverlapCallback(object obj, RigidBodyCollider.OverlapCallback overlapCallback);
		public void RemoveOverlapCallback(object obj, RigidBodyCollider.OverlapCallback overlapCallback);

	}

	public class RigidBodyCollider : IDisposable
	{
		public delegate void OverlapCallback(Vector3f PositionWorldOnA, Vector3f PositionWorldOnB, Vector3f NormalWorldOnB, double Distance, double Distance1, RigidBodyCollider hit);

		public event OverlapCallback Overlap { add => Manager.AddOverlapCallback(obj, value); remove => Manager.RemoveOverlapCallback(obj, value); }

		public object obj;
		public object CustomObject { get; set; }

		public bool Enabled
		{
			get => Manager.EnabledGet(obj);
			set => Manager.EnabledSet(obj, value);
		}

		public bool Activate
		{
			get => Manager.ActivateGet(obj);
			set => Manager.ActivateSet(obj, value);
		}

		public float Mass
		{
			get => Manager.MassGet(obj);
			set => Manager.MassSet(obj, value);
		}

		public ECollisionFilterGroups Group
		{
			get => Manager.GroupGet(obj);
			set => Manager?.GroupSet(obj, value);
		}

		public ECollisionFilterGroups Mask
		{
			get => Manager.MaskGet(obj);
			set => Manager?.MaskSet(obj, value);
		}
		public bool NoneStaticBody
		{
			get => Manager.NoneStaticBodyGet(obj);
			set => Manager.NoneStaticBodySet(obj, value);
		}

		public Matrix Matrix
		{
			get => Manager.MatrixGet(obj);
			set => Manager.MatrixSet(obj, value);
		}

		public void Remove() {
			Manager.Remove(obj);
		}

		public void Dispose() {
			if (obj is IDisposable disposable) {
				disposable.Dispose();
			}
			GC.SuppressFinalize(this);
		}

		public ColliderShape CollisionShape { get; set; }

		public PhysicsSim PhysicsSim { get; set; }

		public static ILinkedRigidBodyCollider Manager { get; set; }

	}
}
