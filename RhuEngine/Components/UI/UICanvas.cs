using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[NotLinkedRenderingComponent]
	[UpdateLevel(UpdateEnum.Normal)]
	[Category(new string[] { "UI" })]
	public class UICanvas : RenderingComponent
	{

		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<Vector3f> scale;

		[Default(false)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<bool> TopOffset;

		[Default(3f)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<float> TopOffsetValue;

		[Default(false)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<bool> FrontBind;

		[Default(10)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<int> FrontBindSegments;

		[Default(135f)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<float> FrontBindAngle;

		[Default(7.5f)]
		[OnChanged(nameof(UpdatePyhsicsMesh))]
		public readonly Sync<float> FrontBindRadus;

		public RigidBodyCollider PhysicsCollider;
		public Matrix PhysicsColliderOffset = Matrix.Identity;
		public void UpdatePyhsicsMesh() {
			RWorld.ExecuteOnEndOfFrame(this, () => {
				var isJustBloxMesh = !(TopOffset.Value || FrontBind.Value);
				if (isJustBloxMesh) {
					var size = scale.Value / 10;
					size *= new Vector3f(1, 1, 0.25);
					PhysicsCollider = new RBoxShape(size / 2).GetCollider(World.PhysicsSim);
					PhysicsCollider.CustomObject = this;
					PhysicsCollider.Group = ECollisionFilterGroups.UI;
					PhysicsCollider.Mask = ECollisionFilterGroups.UI;
					PhysicsCollider.Active = Entity.IsEnabled;
					PhysicsColliderOffset = Matrix.T(size / 2);
				}
				else {
					var newMeshGen = new TrivialBox3Generator {
						Box = new Box3d(Vector3d.One / 2, Vector3d.One / 2)
					};
					var NewMesh = newMeshGen.Generate().MakeSimpleMesh();
					if (TopOffset.Value) {
						NewMesh.OffsetTop(TopOffsetValue.Value);
					}
					if (FrontBind.Value) {
						NewMesh = NewMesh.UIBind(FrontBindAngle.Value, FrontBindRadus.Value, FrontBindSegments.Value, scale);
					}
					NewMesh.Scale(scale.Value.x / 10, scale.Value.y / 10, scale.Value.z / 10);
					PhysicsCollider = new RRawMeshShape(NewMesh).GetCollider(World.PhysicsSim);
					PhysicsCollider.CustomObject = this;
					PhysicsCollider.Group = ECollisionFilterGroups.UI;
					PhysicsCollider.Mask = ECollisionFilterGroups.UI;
					PhysicsCollider.Active = Entity.IsEnabled;
					PhysicsColliderOffset = Matrix.Identity;
				}
			});
		}

		public override void OnAttach() {
			base.OnAttach();
			scale.Value = new Vector3f(16, 9, 1);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			Entity.UIRect?.CanvasUpdate();
			UpdatePyhsicsMesh();
			Entity.EnabledChanged += Entity_EnabledChanged;
		}

		private void Entity_EnabledChanged() {
			if (PhysicsCollider is not null) {
				PhysicsCollider.Active = Entity.IsEnabled;
			}
		}

		public Matrix RenderLocation;

		public override void Render() {
			RenderLocation = Entity.GlobalTrans;
			if (PhysicsCollider is not null) {
				PhysicsCollider.Matrix = PhysicsColliderOffset * RenderLocation;
			}
			Entity.UIRect?.RenderRect(RenderLocation);
		}

		public override void AlwaysStep() {
			RWorld.ExecuteOnEndOfFrame(ClearHitData);
		}

		public List<HitData> hitDatas = new(); 

		public void ClearHitData() {
			hitDatas.Clear();
		}

		public IEnumerable<HitData> HitDataInVolume(Vector2f min,Vector2f max) {
			for (var i = 0; i < hitDatas.Count; i++) {
				var hitData = hitDatas[i];
				if (hitData.HitPointOnCanvas.IsWithIn(min, max)) {
					yield return hitData;
				}
			}
		}

		public struct HitData
		{
			public bool Lazer;

			public uint TouchUndex;
			
			public Vector3f Hitnormal;

			public Vector3f Hitpointworld;

			public float PressForce;
			
			public float GripForces;

			public Handed Side;

			public Vector2f HitPointOnCanvas;
			public bool CustomTouch;

			public HitData(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, Handed handed) {
				CustomTouch = false;
				HitPointOnCanvas = Vector2f.Zero;
				Lazer = false;
				TouchUndex = touchUndex;
				Hitnormal = hitnormal;
				Hitpointworld = hitpointworld;
				PressForce = 0f;
				GripForces = 0f;
				Side = handed;
			}
			public HitData(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForces, Handed side) {
				CustomTouch = false;
				HitPointOnCanvas = Vector2f.Zero;
				Lazer = true;
				TouchUndex = touchUndex;
				Hitnormal = hitnormal;
				Hitpointworld = hitpointworld;
				PressForce = pressForce;
				GripForces = gripForces;
				Side = side;
			}
		}

		private void AddHitData(HitData hitData) {
			var localPoint = RenderLocation.GetLocal(Matrix.T(hitData.Hitpointworld));
			var isJustBloxMesh = !(TopOffset.Value || FrontBind.Value);
			if (isJustBloxMesh) {
				var pos = localPoint.Translation.Xy;
				pos /= scale.Value.Xy / 10;
				hitData.HitPointOnCanvas = pos;
			}
			else {
				//Todo: Dont Think this is good enough 
				var pos = localPoint.Translation.Xy;
				pos /= scale.Value.Xy / 10;
				hitData.HitPointOnCanvas = pos;
			}
			hitDatas.Add(hitData);
		}

		public void ProcessHitTouch(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld,Handed handed) {
			AddHitData(new HitData(touchUndex, hitnormal, hitpointworld, handed));
		}
		public void ProcessHitLazer(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForces, Handed side) {
			AddHitData(new HitData(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side));
		}
	}
}
