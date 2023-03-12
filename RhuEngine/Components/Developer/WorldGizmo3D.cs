using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer" })]
	public sealed partial class WorldGizmo3D : Component
	{
		[OnChanged(nameof(UpdateModeData))]
		public readonly Sync<GizmoMode> Mode;

		public readonly SyncRef<Entity> TargetEntity;

		[OnChanged(nameof(UpdateModeData))]
		public readonly SyncRef<Entity> TransformSpace;
		[OnChanged(nameof(UpdateModeData))]
		public readonly SyncRef<Entity> ParentEntity;

		[OnChanged(nameof(UpdateModeData))]
		public readonly SyncRef<IValueSource<Vector3f>> Pos;
		[OnChanged(nameof(UpdateModeData))]
		public readonly SyncRef<IValueSource<Vector3f>> Scale;
		[OnChanged(nameof(UpdateModeData))]
		public readonly SyncRef<IValueSource<Quaternionf>> Rot;
		[OnChanged(nameof(UpdateModeData))]
		public readonly Sync<bool> UseLocalRot;
		[OnChanged(nameof(UpdateModeData))]
		public readonly Sync<float> PosStep;
		[OnChanged(nameof(UpdateModeData))]
		public readonly Sync<float> ScaleStep;
		[OnChanged(nameof(UpdateModeData))]
		public readonly Sync<float> AngleStep;

		private Gizmo3D _privateGizmo;
		protected override void FirstCreation() {
			base.FirstCreation();
			Mode.Value = GizmoMode.All;
		}
		[Exposed]
		public void SetUpWithEntity(Entity entity) {
			if (entity is null) {
				return;
			}
			Pos.Target = entity.position;
			Scale.Target = entity.scale;
			Rot.Target = entity.rotation;
			TargetEntity.Target = entity;
			ParentEntity.Target = TransformSpace.Target = entity.InternalParent;
		}

		public void UpdateModeData() {
			if (_privateGizmo is null) {
				return;
			}
			_privateGizmo.Scale.Target = Scale.Target;
			_privateGizmo.Rot.Target = Rot.Target;
			_privateGizmo.Pos.Target = Pos.Target;
			_privateGizmo.TransformSpace.Target = TransformSpace.Target;
			_privateGizmo.ParentEntity.Target = ParentEntity.Target;
			_privateGizmo.UseLocalRot.Value = UseLocalRot.Value;

			_privateGizmo.PosStep.Value = PosStep.Value;
			_privateGizmo.ScaleStep.Value = ScaleStep.Value;
			_privateGizmo.AngleStep.Value = AngleStep.Value;

			var AllowedModes = GizmoMode.None;
			if (Pos.Target is not null) {
				AllowedModes |= GizmoMode.Position;
			}
			if (Scale.Target is not null) {
				AllowedModes |= GizmoMode.Scale;
			}
			if (Rot.Target is not null) {
				AllowedModes |= GizmoMode.Rotation;
			}
			_privateGizmo.Mode.Value = Mode.Value & AllowedModes;

		}

		protected override void OnLoaded() {
			base.OnLoaded();
			if (Pointer.GetOwnerID() != World.LocalUserID || !Engine.EngineLink.CanRender) {
				return;
			}
			var gizmoRoot = WorldManager.OverlayWorld.RootEntity.FindChildOrAddChild("GizmosRoot");
			_privateGizmo = gizmoRoot.AddChild(Pointer.HexString()).AttachComponent<Gizmo3D>();
			UpdateModeData();
		}

		public override void Dispose() {
			_privateGizmo?.Entity?.Destroy();
			_privateGizmo = null;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

	}
}