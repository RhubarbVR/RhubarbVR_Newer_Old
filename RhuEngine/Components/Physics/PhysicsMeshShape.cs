using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using BepuPhysics;
using RhuEngine.Linker.MeshAddons;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public abstract class PhysicsMeshShape<T, T2> : PhysicsShape<T> where T : unmanaged, IShape where T2 : PhysicsAddon, new()
	{
		[OnAssetLoaded(nameof(TargetMeshUpdate))]
		public readonly AssetRef<RMesh> TargetMesh;

		protected RMesh _last;

		protected void TargetMeshUpdate() {
			if (_last == TargetMesh.Asset) {
				return;
			}
			if (_last is not null) {
				RemoveRef();
				RemoveData();
			}
			_last = TargetMesh.Asset;
			if (_last is not null) {
				AddRef();
				AddedData();
				UpdateShape();
			}
		}
		protected virtual void RemoveData() {

		}

		protected virtual void AddedData() {

		}

		protected abstract T CreateEmpty(ref float speculativeMargin, float? mass, out BodyInertia inertia);
		protected abstract T CreateShape(T2 addon, ref float speculativeMargin, float? mass, out BodyInertia inertia);

		public override T CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			return _last is null || !(GetAddon?.Loaded ?? false)
				? CreateEmpty(ref speculativeMargin, mass, out inertia)
				: CreateShape(GetAddon,ref speculativeMargin, mass, out inertia);
		}

		protected T2 GetAddon => _last?.GetMeshAddon<T2>(World);

		private void AddRef() {
			GetAddon?.AddRef();
		}
		private void RemoveRef() {
			GetAddon?.RemoveRef();
		}

		public override void RemoveShape() {
			CleanUpShapeData();
			Simulation.Simulation.Shapes.RemoveAndDispose(ShapeIndex.Value, Simulation.BufferPool);
			ShapeIndex = null;
		}

		public override void ShapeDataUpdate() {
			CleanUpShapeData();
		}

		protected abstract void CleanUpShapeData();

		public override void Dispose() {
			RemoveRef();
			base.Dispose();
			GC.SuppressFinalize(this);
		}


	}
}
