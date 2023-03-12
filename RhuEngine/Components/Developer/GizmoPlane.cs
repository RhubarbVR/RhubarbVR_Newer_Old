using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.Normal)]
	[OverlayOnly]
	public sealed partial class GizmoPlane : Component
	{
		public readonly SyncRef<Gizmo3D> Gizmo3DTarget;

		public readonly Sync<GizmoDir> Direction;

		[OnChanged(nameof(UpdateMeshes))]
		public readonly Sync<GizmoMode> Mode;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Position;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> PositionCollider;

		public readonly Linker<Colorf> ColorOfPositionGizmo;

		public readonly SyncRef<PhysicsObject> PositionColliderTarget;
		public float PosStep => Gizmo3DTarget.Target?.PosStep.Value ?? 0f;
		public float AngleStep => Gizmo3DTarget.Target?.AngleStep.Value ?? 0f;
		public float ScaleStep => Gizmo3DTarget.Target?.ScaleStep.Value ?? 0f;

		public Matrix StartPos;
		public Vector3f StartData;
		public bool isInPos;
		private Handed _handed;
		protected override void FirstCreation() {
			base.FirstCreation();
			Mode.Value = GizmoMode.All;
		}
		protected override void Step() {
			base.Step();
			if (Gizmo3DTarget.Target?.GetIfOtherIsActive(this) ?? false) {
				return;
			}
			var global = Entity.GlobalTrans;
			var UPvec = Matrix.T(new Vector3f(0, 1, 0)) * Entity.GlobalTrans;
			var Sidevec = Matrix.T(new Vector3f(1, 0, 0)) * Entity.GlobalTrans;
			var start = global.Translation;
			var up = UPvec.Rotation * Vector3f.Up;
			var lazerStart = PrivateSpaceManager.LazerStartPos(_handed);
			var lazerEnd = (PrivateSpaceManager.LazerNormal(_handed) * 50) + lazerStart;
			var newMatrix = StartPos;
			if (PositionColliderTarget.Target is not null & ColorOfPositionGizmo.Linked) {
				ColorOfPositionGizmo.LinkedValue = PositionColliderTarget.Target.LazeredThisFrame | isInPos ? GetColor(0.85f) : GetColor();
				if (!isInPos) {
					_handed = PositionColliderTarget.Target.LazerHand;
					if (InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed) && PositionColliderTarget.Target.LazeredThisFrame) {
						if (PrivateSpaceManager.GetLazer(_handed).Locked.Value) {
							return;
						}
						isInPos = true;

						var plane = new Plane3f(up, start);
						var intersect = plane.IntersectLine(lazerStart, lazerEnd);
						var localPoint = Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalPointToLocal(intersect) ?? Vector3f.Zero;
						World.DrawDebugSphere(Matrix.T(new Vector3f(0, localPoint.y, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
						World.DrawDebugText(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f), $"{localPoint}");
						StartData = localPoint;

						newMatrix = StartPos = Gizmo3DTarget.Target?.LocalPos ?? Matrix.Identity;
						PrivateSpaceManager.GetLazer(_handed).Locked.Value = true;
					}
				}
				else {
					if (!InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed)) {
						isInPos = false;
						PrivateSpaceManager.GetLazer(_handed).Locked.Value = false;
					}
					else {
						//Proccess
						var plane = new Plane3f(up, start);
						var intersect = plane.IntersectLine(lazerStart, lazerEnd);
						var localPoint = Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalPointToLocal(intersect) ?? Vector3f.Zero;
						var sidePoint = Matrix.T(new Vector3f(localPoint.x, localPoint.y, localPoint.z)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
						World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
						PrivateSpaceManager.GetLazer(_handed).HitPoint = sidePoint.Translation;
						World.DrawDebugText(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f), $"{localPoint}");
						switch (Direction.Value) {
							case GizmoDir.X: {
								var defInValueY = localPoint.y - StartData.y;
								defInValueY -= MathUtil.Clean(defInValueY % PosStep);
								var defInValueZ = localPoint.z - StartData.z;
								defInValueZ -= MathUtil.Clean(defInValueZ % PosStep);
								newMatrix = Matrix.T(new Vector3f(0, defInValueY, defInValueZ)) * StartPos;
							}
							break;
							case GizmoDir.Y: {
								var defInValueX = localPoint.x - StartData.x;
								defInValueX -= MathUtil.Clean(defInValueX % PosStep);
								var defInValueZ = localPoint.z - StartData.z;
								defInValueZ -= MathUtil.Clean(defInValueZ % PosStep);
								newMatrix = Matrix.T(new Vector3f(defInValueX, 0, defInValueZ)) * StartPos;
							}
							break;
							default: {
								var defInValueX = localPoint.x - StartData.x;
								defInValueX -= MathUtil.Clean(defInValueX % PosStep);
								var defInValueY = localPoint.y - StartData.y;
								defInValueY -= MathUtil.Clean(defInValueY % PosStep);
								newMatrix = Matrix.T(new Vector3f(defInValueX, defInValueY, 0)) * StartPos;
							}
							break;
						}

					}
				}
			}
			if (newMatrix != StartPos) {
				Gizmo3DTarget.Target?.SetMatrix(newMatrix);
			}
		}

		private Colorf GetColor(float addedValue = 0) {
			return Direction.Value switch {
				GizmoDir.Y => new Colorf(addedValue, 1, addedValue, Gizmo3D.ALPHA),
				GizmoDir.X => new Colorf(1, addedValue, addedValue, Gizmo3D.ALPHA),
				_ => new Colorf(addedValue, addedValue, 1, Gizmo3D.ALPHA),
			};
		}

		private void UpdateMeshes() {
			if (PositionCollider.Linked) {
				PositionCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
			if (Position.Linked) {
				Position.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var plane = Entity.AddChild("Plane");
			plane.position.Value = new Vector3f(0.6f, 0, 0.6f);
			var positionMeshRender = plane.AttachComponent<MeshRender>();
			Position.Target = positionMeshRender.Enabled;
			var planeMesh = plane.AttachComponent<RectangleMesh>();
			planeMesh.Dimensions.Value = new Vector2f(0.5f);

			var posColider = plane.AttachComponent<BoxShape>();
			posColider.Size.Value = new Vector3f(0.5f, 0.02f, 0.5f);
			PositionCollider.Target = posColider.Enabled;
			positionMeshRender.mesh.Target = planeMesh;
			var posmit = Entity.AttachComponent<UnlitMaterial>();
			positionMeshRender.materials.Add().Target = posmit;
			posmit.Transparency.Value = Transparency.Blend;
			RenderThread.ExecuteOnEndOfFrame(() => posmit._material.NoDepthTest = true);
			ColorOfPositionGizmo.Target = posmit.Tint;
			PositionColliderTarget.Target = posColider;
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateMeshes();
		}

	}
}