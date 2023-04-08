using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RhuEngine.Components
{

	[SingleComponentLock]
	[Category(new string[] { "Visuals" })]
	[UpdateLevel(UpdateEnum.Rendering)]
	public sealed partial class UIMeshShape : MeshShape
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
		public readonly Sync<bool> FlipY;
		[Default(true)]
		public readonly Sync<bool> CustomTochable;

		public readonly SyncRef<IInputInterface> InputInterface;

		public readonly Sync<Vector2f> Tilt;

		public Handed LastHand { get; private set; }

		protected override void RenderStep() {
			base.RenderStep();
			if (hitDatas.Count == 0) {
				InputInterface.Target?.SendNoInput();
			}
			else {
				foreach (var item in hitDatas) {
					var pos = item.HitPointOnCanvas;
					if (FlipY) {
						pos = new Vector2f(pos.X, 1 - pos.Y);
					}
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
			ClearHitData();
		}

		protected override void OnAttach() {
			base.OnAttach();
			Group.Value = Mask.Value = EPhysicsMask.UI;
		}

		public List<HitData> hitDatas = new();

		public void ClearHitData() {
			hitDatas.Clear();
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float DistanceToTriangle(Vector3f point, params Vector3f[] triangle) {
			var normal = Vector3f.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]);
			var planeDistance = Vector3f.Dot(normal, triangle[0]);

			var distance = Vector3f.Dot(normal, point) - planeDistance;

			var projection = point + normal * distance;
			return Vector3f.Dot(Vector3f.Cross(triangle[1] - triangle[0], projection - triangle[0]), normal) > 0 &&
				Vector3f.Dot(Vector3f.Cross(triangle[2] - triangle[1], projection - triangle[1]), normal) > 0 &&
				Vector3f.Dot(Vector3f.Cross(triangle[0] - triangle[2], projection - triangle[2]), normal) > 0
				? Math.Abs(distance)
				: Math.Min(DistanceToSegment(point, triangle[0], triangle[1]),
				Math.Min(DistanceToSegment(point, triangle[1], triangle[2]),
				DistanceToSegment(point, triangle[2], triangle[0])));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float DistanceToSegment(Vector3f point, Vector3f a, Vector3f b) {
			var nearestPoint = ClosestPointOnSegment(point, a, b);
			return point.Distance(nearestPoint);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Vector3f ClosestPointOnSegment(Vector3f point, Vector3f a, Vector3f b) {
			var ab = b - a;
			var t = Vector3f.Dot(point - a, ab) / Vector3f.Dot(ab, ab);
			return t < 0 ? a : t > 1 ? b : a + t * ab;
		}


		private bool AddHitData(HitData hitData) {
			var RenderLocation = Entity.GlobalTrans * Matrix.T(PosOffset.Value);
			var localPoint = RenderLocation.GetLocal(Matrix.T(hitData.Hitpointworld));
			if (_last?.LoadedMesh is null) {
				return false;
			}
			var e = RenderLocation.GetLocal(Matrix.T(hitData.Hitpointworld));
			var hitPoint = e.Translation;
			World.DrawDebugSphere(RenderLocation, (Vector3f)hitPoint, new Vector3d(0.01), Colorf.Red, 0.5f);
			var mesh = _last?.LoadedMesh;
			var hittry = mesh.InsideTry(hitPoint);
			if (hittry < 0 || hittry >= mesh.MaxTriangleID) {
				return false;
			}

			var tryangle = mesh.GetTriangle(hittry);
			var p1 = mesh.GetVertexAll(tryangle.a);
			var p2 = mesh.GetVertexAll(tryangle.b);
			var p3 = mesh.GetVertexAll(tryangle.c);
			var dis = DistanceToTriangle(hitPoint, (Vector3f)p1.v, (Vector3f)p2.v, (Vector3f)p3.v);
			if (dis > 0.001f | dis < -0.001f) {
				return false;
			}

			World.DrawDebugSphere(RenderLocation, (Vector3f)p1.v, new Vector3d(0.01), Colorf.Red, 0.5f);
			World.DrawDebugSphere(RenderLocation, (Vector3f)p2.v, new Vector3d(0.01), Colorf.Blue, 0.5f);
			World.DrawDebugSphere(RenderLocation, (Vector3f)p3.v, new Vector3d(0.01), Colorf.Green, 0.5f);
			var uvpos = Vector2f.GetUVPosOnTry(p1.v, p1.uv[0], p2.v, p2.uv[0], p3.v, p3.uv[0], hitPoint);
			World.DrawDebugText(RenderLocation, (Vector3f)hitPoint + new Vector3f(0, 0.1f, 0.1f), new Vector3f(0.1f), Colorf.BlueMetal, uvpos, 0.1f);
			hitData.HitPointOnCanvas = uvpos;
			hitDatas.Add(hitData);
			return true;
		}

		public override void Touch(uint touchUndex, Vector3f hitnormal, Vector3f hitpointworld, Handed handed) {
			AddHitData(new HitData(touchUndex, hitnormal, hitpointworld, handed));
			base.Touch(touchUndex, hitnormal, hitpointworld, handed);
		}
		public override void Lazer(uint v, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForce, Handed hand) {
			if (!AddHitData(new HitData(v, hitnormal, hitpointworld, pressForce, gripForce, hand))) {
				base.Lazer(v, hitnormal, hitpointworld, pressForce, gripForce, hand);
			}
		}
	}
}
