using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class RenderUIComponent : UIComponent
	{
		public SimpleMesh MainMesh { get; set; }

		public SimpleMesh ScrollMesh { get; set; }
		public SimpleMesh CutMesh { get; set; }

		public abstract RMaterial RenderMaterial { get; }
		public abstract Colorf RenderTint { get; }

		public abstract bool HasPhysics { get; }

		public RigidBodyCollider PhysicsCollider;

		public abstract void ProcessBaseMesh();

		public void ProcessMesh() {
			ProcessBaseMesh();
			Entity.UIRect?.UpdateMeshes();
		}

		public override void RenderTargetChange() {
			ProcessBaseMesh();
		}
		public override void Render(Matrix matrix) {
			//Never Runs
		}

		public void RenderScrollMesh(bool updateMesh = true) {
			if (MainMesh is null) {
				ProcessBaseMesh();
			}
			if (Rect.ScrollOffset == Vector3f.Zero) {
				ScrollMesh = MainMesh;
			}
			else {
				var scrollMesh = new SimpleMesh(MainMesh);
				scrollMesh.Translate(Rect.ScrollOffset);
				ScrollMesh = scrollMesh;
			}
			if (updateMesh) {
				RWorld.ExecuteOnMain(this, Rect.UpdateMeshes);
			}
		}

		private bool _isCut;

		public override void CutElement(bool cut) {
			_isCut = cut;
			RenderCutMesh();
		}


		public void LoadPhysicsMesh() {
			PhysicsCollider?.Remove();
			PhysicsCollider = null;
			PhysicsCollider = new RMeshShape(CutMesh).GetCollider(World.PhysicsSim);
			PhysicsCollider.CustomObject = this;
			PhysicsCollider.Group = ECollisionFilterGroups.UI;
			PhysicsCollider.Mask = ECollisionFilterGroups.UI;
			PhysicsCollider.Active = true;
		}

		public void RenderCutMesh(bool updateMesh = true) {
			if (ScrollMesh is null) {
				RenderScrollMesh(false);
			}
			SimpleMesh cutMesh;
			if (_isCut) {
				cutMesh = new SimpleMesh();
				var cutmax = Rect.CutZonesMax;
				var cutmin = Rect.CutZonesMin;
				var vertsThatNeedCap = new List<NewVertexInfo>();
				foreach (var item in ScrollMesh.Triangles) {
					var tryangle = ScrollMesh.GetTriangle(item);
					var v1 = ScrollMesh.GetVertexAll(tryangle.a);
					var v2 = ScrollMesh.GetVertexAll(tryangle.b);
					var v3 = ScrollMesh.GetVertexAll(tryangle.c);
					var v1inbox = cutmax.IsInBox(cutmin, v1.v.Xy);
					var v2inbox = cutmax.IsInBox(cutmin, v2.v.Xy);
					var v3inbox = cutmax.IsInBox(cutmin, v3.v.Xy);
					void TryAdd(NewVertexInfo vert, NewVertexInfo vert2, NewVertexInfo vert3) {
						var intesect1 = Vector2f.MinMaxIntersect(vert2.v.Xy, cutmin, cutmax);
						var intesect2 = Vector2f.MinMaxIntersect(vert3.v.Xy, cutmin, cutmax);
						var present1 = (MathUtil.Abs(vert.v.Xy - intesect1) / MathUtil.Abs(vert.v.Xy - vert2.v.Xy)).Clean;
						var present2 = (MathUtil.Abs(vert.v.Xy - intesect2) / MathUtil.Abs(vert.v.Xy - vert3.v.Xy)).Clean;
						var uv1 = ((vert2.uv[0] - vert.uv[0]) * present1) + vert.uv[0];
						var uv2 = ((vert3.uv[0] - vert.uv[0]) * present2) + vert.uv[0];
						var new2 = new NewVertexInfo(new Vector3d(intesect1.x, intesect1.y, vert2.v.z), vert2.n, vert2.c, (Vector2f)uv1);
						var new3 = new NewVertexInfo(new Vector3d(intesect2.x, intesect2.y, vert3.v.z), vert3.n, vert3.c, (Vector2f)uv2);
						cutMesh.AppendTriangle(vert, new2, new3);
						vertsThatNeedCap.Add(new2);
						vertsThatNeedCap.Add(new3);
					}
					void QuadAdd(NewVertexInfo vert, NewVertexInfo vert2, NewVertexInfo outvert) {
						//To only works with rect any complex shape will brake
						var intesect1 = Vector2f.MinMaxIntersect(vert2.v.Xy, cutmin, cutmax);
						var intesect2 = Vector2f.MinMaxIntersect(outvert.v.Xy, cutmin, cutmax);
						var present1 = (MathUtil.Abs(vert.v.Xy - intesect1) / MathUtil.Abs(vert.v.Xy - vert2.v.Xy)).Clean;
						var present2 = (MathUtil.Abs(vert.v.Xy - intesect2) / MathUtil.Abs(vert.v.Xy - outvert.v.Xy)).Clean;
						var uv1 = ((vert2.uv[0] - vert.uv[0]) * present1) + vert.uv[0];
						var uv2 = ((outvert.uv[0] - vert.uv[0]) * present2) + vert.uv[0];
						var new2 = new NewVertexInfo(new Vector3d(intesect1.x, intesect1.y, vert2.v.z), vert2.n, vert2.c, (Vector2f)uv1);
						var new3 = new NewVertexInfo(new Vector3d(intesect2.x, intesect2.y, outvert.v.z), outvert.n, outvert.c, (Vector2f)uv2);
						cutMesh.AppendTriangle(vert, new2, new3);
						vertsThatNeedCap.Add(new2);
						vertsThatNeedCap.Add(new3);
					}
					if (v1inbox && v2inbox && v3inbox) {
						cutMesh.AppendTriangle(v1, v2, v3);
					}
					else if(!(v1inbox || v2inbox || v3inbox)) {
						continue;
					}
					else if (v1inbox) {
						if (v2inbox) {
							QuadAdd(v1, v2, v3);
						}
						else if(v3inbox) {
							QuadAdd(v1, v3, v2);
						}
						else {
							TryAdd(v1, v2, v3);
						}
					}
					else if (v2inbox) {
						if (v1inbox) {
							QuadAdd(v2, v1, v3);
						}
						else if (v3inbox) {
							QuadAdd(v2, v3, v1);
						}
						else {
							TryAdd(v2, v1, v2);
						}
					}
					else {
						if (v2inbox) {
							QuadAdd(v3, v2, v1);
						}
						else if (v1inbox) {
							QuadAdd(v3, v1, v2);
						}
						else {
							TryAdd(v3, v1, v2);
						}
					}
				}
				if(vertsThatNeedCap.Count != 0) {
					var firstvert = vertsThatNeedCap[0];
					var min = firstvert.v;
					var max = firstvert.v;
					foreach (var item in vertsThatNeedCap) {
						max = MathUtil.Max(item.v, max);
						min = MathUtil.Min(item.v, min);
					}
					var new11 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(min.x, max.y, min.z) };
					var new21 = new NewVertexInfo { bHaveN = true, bHaveC = true, n = Vector3f.Up, c = firstvert.c, v = new Vector3d(max.x, max.y, min.z) };
					var new31 = new NewVertexInfo { bHaveN = true, bHaveC = true, n = Vector3f.Up, c = firstvert.c, v = new Vector3d(min.x, max.y, max.z) };
					var new41 = new NewVertexInfo { bHaveN = true, bHaveC = true, n = Vector3f.Up, c = firstvert.c, v = new Vector3d(max.x, max.y, max.z) };
					cutMesh.AppendTriangle(new31, new21, new11);
					cutMesh.AppendTriangle(new41, new21, new31);
					var new1 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true,c = firstvert.c,v = new Vector3d(min.x,min.y,min.z)};
					var new2 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(max.x, min.y, min.z) };
					var new3 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(min.x, min.y, max.z) };
					var new4 = new NewVertexInfo { bHaveN = true, n = Vector3f.Up, bHaveC = true, c = firstvert.c, v = new Vector3d(max.x, min.y, max.z) };
					cutMesh.AppendTriangle(new1, new2, new3);
					cutMesh.AppendTriangle(new3, new2, new4);
				}
			}
			else {
				cutMesh = ScrollMesh;
			}
			LoadPhysicsMesh();
			CutMesh = cutMesh;
			if (updateMesh) {
				RWorld.ExecuteOnMain(this, Rect.UpdateMeshes);
			}
		}
	}
}
