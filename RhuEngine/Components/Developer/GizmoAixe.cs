using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{
	[Flags]
	public enum GizmoMode : byte
	{
		None = 0,
		Rotation = 1,
		Scale = 2,
		Position = 4,
		All = Rotation | Scale | Position
	}
	[Flags]
	public enum GizmoDir : byte
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4
	}

	[UpdateLevel(UpdateEnum.Normal)]
	[OverlayOnly]
	public sealed class GizmoAixe : Component
	{
		public readonly Sync<GizmoDir> Direction;
		[OnChanged(nameof(UpdateMeshes))]
		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Rotation;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> RotationCollider;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Scale;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> ScaleCollider;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Scale2;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Scale2Collider;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Position;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> PositionCollider;
		public readonly Linker<Colorf> ColorOfRotationGizmo;
		public readonly Linker<Colorf> ColorOfScaleGizmo;
		public readonly Linker<Colorf> ColorOfPositionGizmo;

		public readonly SyncRef<Gizmo3D> Gizmo3DTarget;

		public readonly SyncRef<PhysicsObject> RotationColliderTarget;
		public readonly SyncRef<PhysicsObject> ScaleColliderTarget;
		public readonly SyncRef<PhysicsObject> Scale2ColliderTarget;
		public readonly SyncRef<PhysicsObject> PositionColliderTarget;

		public float PosStep => Gizmo3DTarget.Target?.PosStep.Value ?? 0f;
		public float AngleStep => Gizmo3DTarget.Target?.AngleStep.Value ?? 0f;
		public float ScaleStep => Gizmo3DTarget.Target?.ScaleStep.Value ?? 0f;

		public bool IsActive => isInRotation | isInScale | isInPos;


		public Matrix StartPos;
		public Vector3f StartData;

		public bool isInRotation;
		public bool isInScale;
		public bool isInPos;
		private Handed _handed;
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

			var side = Sidevec.Rotation * Vector3f.Forward;
			var otherSide = Sidevec.Rotation * Vector3f.Right;

			var newMatrix = StartPos;
			var lazerStart = PrivateSpaceManager.LazerStartPos(_handed);
			var lazerEnd = (PrivateSpaceManager.LazerNormal(_handed) * 50) + lazerStart;
			if (RotationColliderTarget.Target is not null & ColorOfRotationGizmo.Linked) {
				if (!(isInPos | isInScale)) {
					ColorOfRotationGizmo.LinkedValue = RotationColliderTarget.Target.LazeredThisFrame | isInRotation ? GetColor(0.85f) : GetColor();
					if (!isInRotation) {
						_handed = RotationColliderTarget.Target.LazerHand;
						if (InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed) && RotationColliderTarget.Target.LazeredThisFrame) {
							if (PrivateSpaceManager.GetLazer(_handed).Locked.Value) {
								return;
							}
							isInRotation = true;
							newMatrix = StartPos = Gizmo3DTarget.Target?.LocalPos ?? Matrix.Identity;

							var plane = new Plane3f(up, start);
							var intersect = plane.IntersectLine(lazerStart, lazerEnd);
							World.DrawDebugSphere(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f));
							StartData = intersect;
							PrivateSpaceManager.GetLazer(_handed).Locked.Value = true;
						}
					}
					else {
						if (!InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed)) {
							isInRotation = false;
							PrivateSpaceManager.GetLazer(_handed).Locked.Value = false;
						}
						else {
							//Proccess
							var plane = new Plane3f(up, start);
							var intersect = plane.IntersectLine(lazerStart, lazerEnd);
							World.DrawDebugSphere(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f));
							//Todo Apply Rotation

							PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;
						}
					}
				}
			}
			if (ScaleColliderTarget.Target is not null & Scale2ColliderTarget.Target is not null & ColorOfScaleGizmo.Linked) {
				if (!(isInPos | isInRotation)) {
					ColorOfScaleGizmo.LinkedValue = (Scale2ColliderTarget.Target.LazeredThisFrame | ScaleColliderTarget.Target.LazeredThisFrame | isInScale) ? GetColor(0.85f) : GetColor();
					if (!isInScale) {
						_handed = Scale2ColliderTarget.Target.LazeredThisFrame ? Scale2ColliderTarget.Target.LazerHand : ScaleColliderTarget.Target.LazerHand;
						if (InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed) && (Scale2ColliderTarget.Target.LazeredThisFrame | ScaleColliderTarget.Target.LazeredThisFrame)) {
							if (PrivateSpaceManager.GetLazer(_handed).Locked.Value) {
								return;
							}
							isInScale = true;
							var plane = new Plane3f(otherSide, start);
							var intersect = plane.IntersectLine(lazerStart, lazerEnd);
							var localPoint = Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalPointToLocal(intersect) ?? Vector3f.Zero;
							World.DrawDebugSphere(Matrix.T(new Vector3f(0, localPoint.y, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
							World.DrawDebugText(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f), $"{localPoint}");
							StartData = localPoint;
							if (localPoint.IsAnyNanOrInfinity) {
								return;
							}
							newMatrix = StartPos = Gizmo3DTarget.Target?.LocalPos ?? Matrix.Identity;
							PrivateSpaceManager.GetLazer(_handed).Locked.Value = true;
						}
					}
					else {
						if (!InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed)) {
							isInScale = false;
							PrivateSpaceManager.GetLazer(_handed).Locked.Value = false;
						}
						else {
							//Proccess
							var plane = new Plane3f(otherSide, start);
							var intersect = plane.IntersectLine(lazerStart, lazerEnd);
							var localPoint = Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalPointToLocal(intersect) ?? Vector3f.Zero;
							World.DrawDebugText(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f), $"{localPoint}");
							if (localPoint.IsAnyNanOrInfinity) {
								return;
							}
							switch (Direction.Value) {
								case GizmoDir.X: {
									var sidePoint = Matrix.T(new Vector3f(localPoint.x, 0, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
									World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
									PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;

									var defInValue = localPoint.x - StartData.x;
									defInValue -= MathUtil.Clean(defInValue % ScaleStep);
									newMatrix = Matrix.S(new Vector3f(1 + defInValue, 1, 1)) * StartPos;
								}
								break;
								case GizmoDir.Y: {
									var sidePoint = Matrix.T(new Vector3f(0, localPoint.y, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
									World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
									PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;

									var defInValue = localPoint.y - StartData.y;
									defInValue -= MathUtil.Clean(defInValue % ScaleStep);
									newMatrix = Matrix.S(new Vector3f(1, defInValue + 1, 1)) * StartPos;
								}
								break;
								default: {
									var sidePoint = Matrix.T(new Vector3f(0, 0, localPoint.z)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
									World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
									PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;

									var defInValue = localPoint.z - StartData.z;
									defInValue -= MathUtil.Clean(defInValue % ScaleStep);
									newMatrix = Matrix.S(new Vector3f(1, 1, 1 - defInValue)) * StartPos;
								}
								break;
							}

						}
					}
				}
			}
			if (PositionColliderTarget.Target is not null & ColorOfPositionGizmo.Linked) {
				if (!(isInScale | isInRotation)) {
					ColorOfPositionGizmo.LinkedValue = PositionColliderTarget.Target.LazeredThisFrame | isInPos ? GetColor(0.85f) : GetColor();
					if (!isInPos) {
						_handed = PositionColliderTarget.Target.LazerHand;
						if (InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed) && PositionColliderTarget.Target.LazeredThisFrame) {
							if (PrivateSpaceManager.GetLazer(_handed).Locked.Value) {
								return;
							}
							isInPos = true;
							var plane = new Plane3f(otherSide, start);
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
							var plane = new Plane3f(otherSide, start);
							var intersect = plane.IntersectLine(lazerStart, lazerEnd);
							var localPoint = Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalPointToLocal(intersect) ?? Vector3f.Zero;
							World.DrawDebugSphere(Matrix.T(new Vector3f(0, localPoint.y, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
							World.DrawDebugText(Matrix.T(intersect), Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 1, 0.5f), $"{localPoint}");
							switch (Direction.Value) {
								case GizmoDir.X: {
									var sidePoint = Matrix.T(new Vector3f(localPoint.x, 0, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
									World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
									PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;

									var defInValue = localPoint.x - StartData.x;
									defInValue -= MathUtil.Clean(defInValue % PosStep);
									newMatrix = StartPos * Matrix.T(new Vector3f(defInValue, 0, 0));
								}
								break;
								case GizmoDir.Y: {
									var sidePoint = Matrix.T(new Vector3f(0, localPoint.y, 0)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
									World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
									PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;

									var defInValue = localPoint.y - StartData.y;
									defInValue -= MathUtil.Clean(defInValue % PosStep);
									newMatrix = StartPos * Matrix.T(new Vector3f(0, defInValue, 0));
								}
								break;
								default: {
									var sidePoint = Matrix.T(new Vector3f(0, 0, localPoint.z)) * (Gizmo3DTarget.Target?.TransformSpace.Target?.GlobalTrans ?? Matrix.Identity);
									World.DrawDebugSphere(sidePoint, Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 1, 0.5f));
									PrivateSpaceManager.GetLazer(_handed).HitPoint = intersect;

									var defInValue = localPoint.z - StartData.z;
									defInValue -= MathUtil.Clean(defInValue % PosStep);
									newMatrix = StartPos * Matrix.T(new Vector3f(0, 0, defInValue));

								}
								break;
							}

						}
					}
				}
			}
			if (newMatrix != StartPos) {
				Gizmo3DTarget.Target?.SetMatrix(newMatrix);
			}
		}


		private void UpdateMeshes() {
			if (RotationCollider.Linked) {
				RotationCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Rotation);
			}
			if (Rotation.Linked) {
				Rotation.LinkedValue = Mode.Value.HasFlag(GizmoMode.Rotation);
			}
			if (ScaleCollider.Linked) {
				ScaleCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale);
			}
			if (Scale.Linked) {
				Scale.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale);
			}
			if (Scale2Collider.Linked) {
				Scale2Collider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale) & !Mode.Value.HasFlag(GizmoMode.Position);
			}
			if (Scale2.Linked) {
				Scale2.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale);
			}
			if (PositionCollider.Linked) {
				PositionCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
			if (Position.Linked) {
				Position.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
		}

		private Colorf GetColor(float addedValue = 0) {
			return Direction.Value switch {
				GizmoDir.Y => new Colorf(addedValue, 1, addedValue, Gizmo3D.ALPHA),
				GizmoDir.X => new Colorf(1, addedValue, addedValue, Gizmo3D.ALPHA),
				_ => new Colorf(addedValue, addedValue, 1, Gizmo3D.ALPHA),
			};
		}

		protected override void OnAttach() {
			base.OnAttach();
			var rotationMeshRender = Entity.AddChild("Rot").AttachComponent<MeshRender>();
			rotationMeshRender.Entity.rotation.Value = Quaternionf.Pitched;
			Rotation.Target = rotationMeshRender.Enabled;
			var rotMesh = Entity.AttachComponent<TorusMesh>();
			rotMesh.MajorRadius.Value = 1.7f;
			rotMesh.MinorRadius.Value = 0.05f;
			rotationMeshRender.mesh.Target = rotMesh;
			var rotColider = rotationMeshRender.Entity.AttachComponent<MeshShape>();
			rotColider.TargetMesh.Target = rotMesh;
			RotationCollider.Target = rotColider.Enabled;
			RotationColliderTarget.Target = rotColider;

			var mainBoxShapeColider = Entity.AttachComponent<BoxShape>();


			var scaleMeshRender = Entity.AttachComponent<MeshRender>();
			Scale.Target = scaleMeshRender.Enabled;
			var boxStickMesh = Entity.AttachComponent<TrivialBox3Mesh>();
			boxStickMesh.Extent.Value = new Vector3f(0.2f);
			boxStickMesh.Center.Value = new Vector3f(0, 2.6f, 0);
			mainBoxShapeColider.PosOffset.Value = new Vector3f(0, 2.6f, 0);
			mainBoxShapeColider.Size.Value = boxStickMesh.Extent.Value;
			ScaleCollider.Target = mainBoxShapeColider.Enabled;
			scaleMeshRender.mesh.Target = boxStickMesh;

			var scaletwo = Entity.AttachComponent<MeshRender>();
			Scale2.Target = scaletwo.Enabled;
			var scalecyl = Entity.AttachComponent<CylinderMesh>();
			scaletwo.mesh.Target = scalecyl;
			scalecyl.Height.Value = 2.6f;
			scalecyl.BaseRadius.Value = scalecyl.TopRadius.Value = 0.03f;

			var scaleColider = Entity.AttachComponent<CylinderShape>();
			scaleColider.PosOffset.Value = new Vector3f(0, 1.3f, 0);
			scaleColider.Height.Value = 2.6f;
			scaleColider.Radius.Value = 0.03f;
			Scale2Collider.Target = scaleColider.Enabled;

			var positionMeshRender = Entity.AttachComponent<MeshRender>();
			Position.Target = positionMeshRender.Enabled;
			var arrowMesh = Entity.AttachComponent<ArrowMesh>();
			arrowMesh.StickLength.Value = 1.98f;
			arrowMesh.StickRadius.Value = 0.04f;
			arrowMesh.HeadBaseRadius.Value = 0.1f;
			arrowMesh.HeadLength.Value = 0.4f;
			var posColider = Entity.AttachComponent<CylinderShape>();
			posColider.PosOffset.Value = new Vector3f(0, 2.3f / 2, 0);
			posColider.Height.Value = 2.3f;
			posColider.Radius.Value = 0.05f;
			PositionCollider.Target = posColider.Enabled;
			PositionColliderTarget.Target = posColider;

			positionMeshRender.mesh.Target = arrowMesh;

			var rotmit = Entity.AttachComponent<UnlitMaterial>();
			var scalemit = Entity.AttachComponent<UnlitMaterial>();
			var posmit = Entity.AttachComponent<UnlitMaterial>();
			RenderThread.ExecuteOnEndOfFrame(() => {
				rotmit._material.NoDepthTest = true;
				scalemit._material.NoDepthTest = true;
				posmit._material.NoDepthTest = true;
			});

			rotationMeshRender.materials.Add().Target = rotmit;
			scaleMeshRender.materials.Add().Target = scalemit;
			scaletwo.materials.Add().Target = scalemit;
			positionMeshRender.materials.Add().Target = posmit;
			rotmit.Transparency.Value = Transparency.Blend;
			scalemit.Transparency.Value = Transparency.Blend;
			posmit.Transparency.Value = Transparency.Blend;

			ColorOfRotationGizmo.Target = rotmit.Tint;
			ColorOfPositionGizmo.Target = posmit.Tint;
			ColorOfScaleGizmo.Target = scalemit.Tint;
			ScaleColliderTarget.Target = mainBoxShapeColider;
			Scale2ColliderTarget.Target = scaleColider;

		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateMeshes();
		}

	}
}