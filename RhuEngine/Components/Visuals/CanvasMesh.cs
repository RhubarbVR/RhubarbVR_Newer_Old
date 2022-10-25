using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using RhuEngine.Physics;
using System.Collections.Generic;

namespace RhuEngine.Components
{

	[SingleComponentLock]
	[Category(new string[] { "Visuals" })]
	[UpdateLevel(UpdateEnum.Rendering)]
	public sealed class CanvasMesh : ProceduralMesh
	{

		[Default(true)]
		public readonly Sync<bool> Laserable;
		[Default(0.5f)]
		public readonly Sync<float> PrimaryNeededForce;
		[Default(0.5f)]
		public readonly Sync<float> GripNeededForce;
		[Default(0.5f)]
		public readonly Sync<float> SecodaryNeededForce;
		[Default(true)]
		public readonly Sync<bool> Touchable;

		[Default(true)]
		public readonly Sync<bool> CustomTochable;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> Resolution;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> MinOffset;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> MaxOffset;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> Scale;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> Min;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> Max;

		public readonly SyncRef<IInputInterface> InputInterface;

		public readonly Sync<Vector2f> Tilt;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> TopOffset;

		[Default(3f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> TopOffsetValue;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> FrontBind;

		[Default(20)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> FrontBindSegments;

		[Default(135f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> FrontBindAngle;

		[Default(7.5f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> FrontBindRadus;

		public RigidBodyCollider PhysicsCollider;

		public override void ComputeMesh() {
			PhysicsCollider?.Remove();
			PhysicsCollider = null;
			var min = Min.Value + ((Vector2f)MinOffset.Value/ (Vector2f)Resolution.Value);
			var max = Max.Value + ((Vector2f)MaxOffset.Value / (Vector2f)Resolution.Value);
			var rectSize = max - min;
			var rectMin = min;
			var newMeshGen = new TrivialRectGenerator {
				IndicesMap = new Index2i(1, 2),
			};
			var NewMesh = newMeshGen.Generate().MakeSimpleMesh();
			NewMesh.Scale(1, -1, 1);
			NewMesh.Translate(0.5f, 0.5f, 0);
			NewMesh = NewMesh.Cut(rectSize + rectMin, rectMin);
			if (TopOffset.Value) {
				NewMesh.OffsetTop(TopOffsetValue.Value);
			}
			if (FrontBind.Value) {
				NewMesh = NewMesh.UIBind(FrontBindAngle.Value, FrontBindRadus.Value, FrontBindSegments.Value, Scale);
			}
			NewMesh.Scale(Scale.Value.x / 10, Scale.Value.y / 10, Scale.Value.z / 10);
			NewMesh.Translate(-(Scale.Value.x / 20), -(Scale.Value.y / 20), 0);
			GenMesh(NewMesh);
			PhysicsCollider = new RRawMeshShape(NewMesh).GetCollider(World.PhysicsSim);
			PhysicsCollider.CustomObject = this;
			PhysicsCollider.Group = ECollisionFilterGroups.UI;
			PhysicsCollider.Mask = ECollisionFilterGroups.UI;
			PhysicsCollider.Active = Entity.IsEnabled;
		}

		protected override void RenderStep() {
			base.RenderStep();
			RenderLocation = Entity.GlobalTrans;
			if (PhysicsCollider is not null) {
				PhysicsCollider.Matrix = RenderLocation;
			}

			foreach (var item in hitDatas) {
				var pos = item.HitPointOnCanvas;
				pos -= Min.Value;
				pos /= Max.Value - Min.Value;
				var isPrime = item.PressForce >= PrimaryNeededForce.Value;
				var isSec = item.GripForces >= GripNeededForce.Value;
				var isMed = InputManager.GetInputAction(InputTypes.Secondary).HandedValue(item.Side) >= SecodaryNeededForce.Value;
				if (item.Lazer) {
					if (Laserable) {
						InputInterface.Target?.SendInput(pos, Tilt.Value, item.PressForce, item.Side, (int)item.TouchUndex, true, isPrime, isSec, isMed);
					}
				}
				else if (item.CustomTouch) {
					if (CustomTochable) {
						InputInterface.Target?.SendInput(pos, Tilt.Value, item.PressForce, item.Side, (int)item.TouchUndex, false, isPrime, isSec, isMed);
					}
				}
				else {
					if (Touchable) {
						InputInterface.Target?.SendInput(pos, Tilt.Value, item.PressForce, item.Side, (int)item.TouchUndex, false, isPrime, isSec, isMed);
					}
				}
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			Scale.Value = new Vector3f(16, 9, 1);
			Max.Value = Vector2f.One;

		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.EnabledChanged += Entity_EnabledChanged;
			Entity_EnabledChanged();
		}

		private void Entity_EnabledChanged() {
			if (PhysicsCollider is not null) {
				PhysicsCollider.Active = Entity.IsEnabled;
			}
		}

		public Matrix RenderLocation;

		protected override void AlwaysStep() {
			RUpdateManager.ExecuteOnEndOfFrame(ClearHitData);
		}

		public List<HitData> hitDatas = new();

		public void ClearHitData() {
			hitDatas.Clear();
		}

		public IEnumerable<HitData> HitDataInVolume(Vector2f min, Vector2f max) {
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
				pos /= Scale.Value.Xy / 10;
				hitData.HitPointOnCanvas = pos;
			}
			else {
				if (LoadedSimpleMesh is null) {
					return;
				}
				var e = RenderLocation.GetLocal(Matrix.T(hitData.Hitpointworld));
				var hitPoint = e.Translation;
				World.DrawDebugSphere(RenderLocation, (Vector3f)hitPoint, new Vector3d(0.01), Colorf.Red, 0.5f);
				var mesh = LoadedSimpleMesh;
				var hittry = mesh.InsideTry(hitPoint);
				if (hittry < 0 || hittry >= mesh.MaxTriangleID) {
					return;
				}
				var tryangle = mesh.GetTriangle(hittry);
				var p1 = mesh.GetVertexAll(tryangle.a);
				var p2 = mesh.GetVertexAll(tryangle.b);
				var p3 = mesh.GetVertexAll(tryangle.c);
				World.DrawDebugSphere(RenderLocation, (Vector3f)p1.v, new Vector3d(0.01), Colorf.Red, 0.5f);
				World.DrawDebugSphere(RenderLocation, (Vector3f)p2.v, new Vector3d(0.01), Colorf.Blue, 0.5f);
				World.DrawDebugSphere(RenderLocation, (Vector3f)p3.v, new Vector3d(0.01), Colorf.Green, 0.5f);
				var uvpos = Vector2f.GetUVPosOnTry(p1.v, p1.uv[0], p2.v, p2.uv[0], p3.v, p3.uv[0], hitPoint);
				World.DrawDebugText(RenderLocation, (Vector3f)hitPoint + new Vector3f(0, 0.1f, 0.1f), new Vector3f(0.1f), Colorf.BlueMetal, uvpos, 0.1f);
				hitData.HitPointOnCanvas = uvpos;
			}
			hitDatas.Add(hitData);
		}

		public void ProcessHitTouch(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, Handed handed) {
			AddHitData(new HitData(touchUndex, hitnormal, hitpointworld, handed));
		}
		public void ProcessHitLazer(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForces, Handed side) {
			AddHitData(new HitData(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side));
		}

		public override void Dispose() {
			base.Dispose();
			PhysicsCollider?.Remove();
		}
	}
}
