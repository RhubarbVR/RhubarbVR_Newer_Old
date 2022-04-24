using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RhuEngine.Components
{
	public interface IRectData
	{
		public UICanvas Canvas { get; }

		public Vector2f Min { get; }

		public Vector2f Max { get; }
		public Vector2f AnchorMinValue { get; }


		public Vector2f AnchorMaxValue { get; }
		public Vector2f BadMin { get; }

		public Vector2f TrueMin { get; }

		public Vector2f TrueMax { get; }

		public float StartPoint { get; }

		public float DepthValue { get; }

	}

	public class BasicRectOvride : IRectData
	{
		public IRectData Child { get; set; }
		public IRectData ParentRect { get; set; }

		public UICanvas Canvas { get; set; }

		public Vector2f AnchorMin { get; set; }

		public Vector2f AnchorMax { get; set; }

		public Vector2f AnchorMinValue => AnchorMin;


		public Vector2f AnchorMaxValue => AnchorMax;

		public Vector2f Min => TrueMin;

		public Vector2f Max => TrueMax;

		public Vector2f BadMin => (((ParentRect?.BadMin ?? Vector2f.One) - (Vector2f.One - (ParentRect?.TrueMax ?? Vector2f.One))) * (Vector2f.One - AnchorMin)) + (Vector2f.One - (ParentRect?.TrueMax ?? Vector2f.One));

		public Vector2f TrueMin => Vector2f.One - BadMin;

		public Vector2f TrueMax => (((ParentRect?.TrueMax ?? Vector2f.One) - (ParentRect?.TrueMin ?? Vector2f.Zero)) * AnchorMax) + (ParentRect?.TrueMin ?? Vector2f.Zero);

		public float StartPoint => (ParentRect?.StartPoint ?? 0) + (ParentRect?.DepthValue ?? 0);

		public float DepthValue { get; set; }
	}

	public struct HitData
	{
		public Vector3f HitPosNoScale;
		public Vector3f HitPosWorld;
		public Vector3f HitPos;
		public Vector3f HitNormalWorld;
		public Vector3f HitNormal;
		public uint Touchindex;
		public bool Laser;
		public bool CustomTouch;
		public float PressForce;

		public void Clean(Matrix parrent,Vector3f canvasScale) {
			var pointNoScale = Matrix.T(HitPosWorld) * (Matrix.S(1/canvasScale) * parrent).Inverse;
			HitPosNoScale = pointNoScale.Translation;
			var point = Matrix.T(HitPosWorld) * parrent.Inverse;
			HitPos = point.Translation;
			HitNormal = point.Rotation.AxisZ;
		}
	}

	[Category(new string[] { "UI\\Rects" })]
	public class UIRect : Component, IRectData
	{
		private readonly List<HitData> _rayHitPoses = new();
		private readonly List<HitData> _lastRayHitPoses = new();

		public void AddHitPoses(HitData hitData) {
			RWorld.ExecuteOnStartOfFrame(() => {
				hitData.Clean(LastRenderPos, Canvas.scale.Value / 10);
				_rayHitPoses.Add(hitData);
				if (_rayHitPoses.Count == 1) {
					RWorld.ExecuteOnEndOfFrame(this, () => {
						_lastRayHitPoses.Clear();
						_lastRayHitPoses.AddRange(_rayHitPoses);
						_rayHitPoses.Clear();
						RWorld.ExecuteOnStartOfFrame(() => {
							RWorld.ExecuteOnEndOfFrame(() => {
								if (_rayHitPoses.Count == 0) {
									_lastRayHitPoses.Clear();
									_rayHitPoses.Clear();
								}
							});
						});
					});
				}
			});
		}

		public IEnumerable<Vector3f> FingerChange(bool ignoreOtherInputZones = false) {
			var lastpoint = LastHitPoses(ignoreOtherInputZones).GetEnumerator();
			var newHitpoin = HitPoses(ignoreOtherInputZones).GetEnumerator();
			var hasData1 = newHitpoin.MoveNext();
			var hasData2 = lastpoint.MoveNext();
			while (hasData1 && hasData2) {
				var currentIndex = Math.Min(lastpoint.Current.Touchindex, newHitpoin.Current.Touchindex);
				if (lastpoint.Current.Touchindex == newHitpoin.Current.Touchindex) {
					yield return lastpoint.Current.HitPos - newHitpoin.Current.HitPos;
				}
				if (lastpoint.Current.Touchindex <= currentIndex) {
					hasData2 = lastpoint.MoveNext();
				}
				if (newHitpoin.Current.Touchindex <= currentIndex) {
					hasData1 = newHitpoin.MoveNext();
				}
			}
		}

		public IEnumerable<Vector3f> ClickFingerChange(float threshold, bool ignoreOtherInputZones = false) {
			var lastpoint = LastHitPosesByFingerID(ignoreOtherInputZones).GetEnumerator();
			var newHitpoin = HitPosesByFingerID(ignoreOtherInputZones).GetEnumerator();
			var hasData1 = newHitpoin.MoveNext();
			var hasData2 = lastpoint.MoveNext();
			while (hasData1 && hasData2) {
				var currentIndex = Math.Min(lastpoint.Current.Touchindex, newHitpoin.Current.Touchindex);
				if(lastpoint.Current.Touchindex == newHitpoin.Current.Touchindex) {
					if (lastpoint.Current.PressForce >= threshold && newHitpoin.Current.PressForce >= threshold) {
						yield return lastpoint.Current.HitPos - newHitpoin.Current.HitPos;
					}
				}
				if (lastpoint.Current.Touchindex <= currentIndex) {
					hasData2 = lastpoint.MoveNext();
				}
				if (newHitpoin.Current.Touchindex <= currentIndex) {
					hasData1 = newHitpoin.MoveNext();
				}
			}
		}
		public IEnumerable<HitData> LastHitPosesByFingerID(bool ignoreOtherInputZones = false) {
			return from hitPoses in LastHitPoses(ignoreOtherInputZones)
				   orderby hitPoses.Touchindex ascending
				   select hitPoses;
		}

		public IEnumerable<HitData> LastHitPoses(bool ignoreOtherInputZones = false) {
			foreach (var item in _lastRayHitPoses) {
				yield return item;
			}
			foreach (var item in _childRects.List) {
				if (!(item._hasInteraction && ignoreOtherInputZones)) {
					foreach (var hitpoits in item.LastHitPoses(ignoreOtherInputZones)) {
						yield return hitpoits;
					}
				}
			}
		}

		private bool _hasInteraction;

		public IEnumerable<HitData> HitPosesByFingerID(bool ignoreOtherInputZones = false) {
			return from hitPoses in HitPoses(ignoreOtherInputZones)
				   orderby hitPoses.Touchindex ascending
				   select hitPoses;
		}

		public IEnumerable<HitData> HitPoses(bool ignoreOtherInputZones = false) {
			foreach (var item in _rayHitPoses) {
				yield return item;
			}
			foreach (var item in _childRects.List) {
				if (!(item._hasInteraction && ignoreOtherInputZones)) {
					foreach (var hitpoits in item.HitPoses(ignoreOtherInputZones)) {
						yield return hitpoits;
					}
				}
			}
		}

		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> OffsetMin;
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> OffsetMax;
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> AnchorMin;

		public Matrix MatrixMove(Matrix matrix) {
			var firstpos = VertMove(matrix.Translation);
			var least = 0f;
			var quaternionf = Quaternionf.Identity;
			for (var i = 0; i < Canvas.FrontBindSegments; i++) {
				var max = i + (1 / Canvas.FrontBindSegments);
				if (max >= firstpos.x && least <= firstpos.x) {
					quaternionf = Quaternionf.Identity;
					break;
				}
				least = max;
			}
			return Matrix.TR(firstpos - matrix.Translation, quaternionf);
		}
		public Vector3f VertMove(Vector3f point) {
			var npoint = point;
			if (Canvas.TopOffset.Value) {
				npoint.z -= point.y * Canvas.TopOffsetValue.Value;
			}
			if (Canvas.FrontBind.Value) {
				var x = (point.x - 0.5) * 2;
				npoint.z -= (float)((1 - (x * x)) * Canvas.FrontBindDist.Value) - Canvas.FrontBindDist.Value;
			}
			return npoint;
		}
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> AnchorMax;
		public Vector2f AnchorMinValue => AnchorMin;


		public Vector2f AnchorMaxValue => AnchorMax;

		[Default(0.1f)]
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<float> Depth;
		public float DepthValue => Depth;

		public virtual Vector2f CutZonesMax => Entity.parent.Target?.UIRect?.CutZonesMax ?? Vector2f.Inf;

		public virtual Vector2f CutZonesMin => Entity.parent.Target?.UIRect?.CutZonesMin ?? Vector2f.NInf;

		public Vector2f Min => TrueMin + (OffsetMin.Value/(Canvas?.scale.Value.Xy??Vector2f.One));

		public Vector2f Max => TrueMax + (OffsetMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));

		public Vector2f BadMin => ((((_rectDataOverride ?? ParentRect)?.BadMin ?? Vector2f.One) - (Vector2f.One - ((_rectDataOverride ?? ParentRect)?.TrueMax ?? Vector2f.One))) * (Vector2f.One - AnchorMin.Value)) + (Vector2f.One - ((_rectDataOverride ?? ParentRect)?.TrueMax ?? Vector2f.One));

		public Vector2f TrueMin => Vector2f.One - BadMin;

		public Vector2f TrueMax => ((((_rectDataOverride ?? ParentRect)?.TrueMax ?? Vector2f.One) - ((_rectDataOverride ?? ParentRect)?.TrueMin ?? Vector2f.Zero)) * AnchorMax.Value) + ((_rectDataOverride ?? ParentRect)?.TrueMin ?? Vector2f.Zero);

		public float StartPoint => ((_rectDataOverride ?? ParentRect)?.StartPoint ?? 0) + ((_rectDataOverride ?? ParentRect)?.DepthValue ?? 0);

		private IRectData _rectDataOverride;
		public virtual bool RemoveFakeRecs => true;

		public Vector3f ScrollOffset { get; set; }

		public void SetOverride(IRectData rectDataOverride) {
			if (rectDataOverride != _rectDataOverride) {
				_rectDataOverride = rectDataOverride;
				RegUpdateUIMeshes();
			}
		}

		public void RegUpdateUIMeshes() {
			RWorld.ExecuteOnStartOfFrame(this, UpdateUIMeshes);
		}

		public virtual void UpdateUIMeshes() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.ProcessBaseMesh();
				}
			});
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					if (RemoveFakeRecs) {
						item?.SetOverride(null);
					}
					item?.RegUpdateUIMeshes();
				}
			});
			_uiComponents.SafeOperation((list) => {
				for (var i = 0; i < _uiComponents.List.Count; i++) {
					list[i].RenderTargetChange();
				}
			});
			UpdateMeshes();
		}

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UICanvas RegisteredCanvas { get; internal set; }

		public void RegisterCanvas() {
			RegisteredCanvas = Canvas;
			foreach (Entity item in Entity.children) {
				item?.UIRect?.RegisterCanvas();
			}
			RegUpdateUIMeshes();
		}

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UICanvas BoundCanvas { get; internal set; }

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UICanvas Canvas => BoundCanvas ?? ParentRect?.Canvas;

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UIRect ParentRect => Entity.parent.Target?.UIRect;

		public readonly SafeList<RMesh> _meshes = new();

		public readonly SafeList<UIComponent> _uiComponents = new();

		public readonly SafeList<RenderUIComponent> _uiRenderComponents = new();

		public readonly SafeList<UIRect> _childRects = new();

		public virtual void UpdateMeshes() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_meshes.SafeOperation((meshList) => {
				_uiRenderComponents.SafeOperation((list) => {
					if (meshList.Count < list.Count) {
						for (var i = 0; i < list.Count - meshList.Count; i++) {
							meshList.Add(new RMesh(null));
						}
					}
					if (meshList.Count > list.Count) {
						for (var i = 0; i < meshList.Count - list.Count; i++) {
							meshList.Remove(new RMesh(null));
						}
					}
					for (var i = 0; i < _uiRenderComponents.List.Count; i++) {
						if(list[i].CutMesh is null) {
							list[i].RenderCutMesh(false);
						}
						meshList[i].LoadMesh(list[i].RenderMesh);
					}
				});
			});
			_uiComponents.SafeOperation((list) => {
				for (var i = 0; i < _uiComponents.List.Count; i++) {
					list[i].RenderTargetChange();
				}
			});
		}

		public bool Culling { get; private set; } = false;

		public void ProcessCutting(bool update = true,bool updatePhysicsMesh = true) {
			var min = Min + ScrollOffset.Xy;
			var max = Max + ScrollOffset.Xy;
			var cutmin = CutZonesMin;
			var cutmax = CutZonesMax;
			Culling = max.y < cutmin.y || min.y > cutmax.y || max.x < cutmin.x || min.x > cutmax.x;
			var cut = !Culling && (max.y > cutmax.y || min.y < cutmin.y || max.x > cutmax.x || min.x < cutmin.x);
			_uiComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.CutElement(cut,update);
				}
			});
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.CutElement(cut,update);
					if (updatePhysicsMesh) {
						item.LoadPhysicsMesh();
					}
				}
			});
		}

		public Matrix LastRenderPos { get; private set; }

		public virtual void Render(Matrix matrix) {
			LastRenderPos = matrix;
			if (Culling) {
				return;
			}
			_meshes.SafeOperation((meshList) => {
				_uiRenderComponents.SafeOperation((list) => {
					for (var i = 0; i < _uiRenderComponents.List.Count; i++) {
						if (list[i].PhysicsCollider is not null) {
							list[i].PhysicsCollider.Matrix = Matrix.T(list[i].PhysicsPose) * matrix;
						}
						meshList[i].Draw(list[i].Pointer.ToString(), list[i].RenderMaterial, matrix, list[i].RenderTint);
					}
				});
			});
			_uiComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.Render(matrix);
				}
			});
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					item?.Render(matrix);
				}
			});
		}

		public override void OnAttach() {
			base.OnAttach();
			AnchorMin.Value = Vector2f.Zero;
			AnchorMax.Value = Vector2f.One;
			OffsetMin.Value = Vector2f.Zero;
			OffsetMax.Value = Vector2f.Zero;
		}

		public override void OnLoaded() {
			base.OnLoaded();
			Entity.SetUIRect(Entity.GetFirstComponent<UIRect>() ?? this);
			Entity.components.Changed += RegisterUIList;
			Entity.children.Changed += Children_Changed;
			Children_Changed(null);
			RegisterUIList(null);
			RegisterCanvas();
			ProcessCutting();
		}

		private readonly SafeList<Entity> _boundTo = new();

		public virtual void ChildAdded(UIRect child) {
			child?.SetOverride(null);
		}

		private void Children_Changed(IChangeable obj) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RWorld.ExecuteOnStartOfFrame(() => {
				_boundTo.SafeOperation((list) => {
					foreach (var item in list) {
						item.components.Changed -= Children_Changed;
					}
					list.Clear();
				});
				_childRects.SafeOperation((list) => list.Clear());
				var added = false;
				_childRects.SafeOperation((list) => {
					foreach (Entity item in Entity.children) {
						_boundTo.SafeAdd(item);
						item.components.Changed += Children_Changed;
						var childadded = item.GetFirstComponent<UIRect>();
						if (childadded != null) {
							ChildAdded(childadded);
							list.Add(childadded);
							childadded.RegUpdateUIMeshes();
							added = true;
						}
					}
				});
				ProcessCutting(false);
				UpdateUIMeshes();
				if (added) {
					ChildRectAdded();
				}
			});
		}

		public virtual void ChildRectAdded() {

		}


		private void RegisterUIList(IChangeable obj) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RWorld.ExecuteOnStartOfFrame(() => {
				_uiComponents.SafeOperation((list) => list.Clear());
				_uiComponents.SafeOperation((list) => {
					foreach (var item in Entity.GetAllComponents<UIComponent>()) {
						if (!typeof(RenderUIComponent).IsAssignableFrom(item.GetType())) {
							list.Add(item);
						}
					}
				});
				_hasInteraction = Entity.GetFirstComponent<UIInteractionComponent>() != null;
				_uiRenderComponents.SafeOperation((list) => list.Clear());
				_uiRenderComponents.SafeOperation((list) => {
					foreach (var item in Entity.GetAllComponents<RenderUIComponent>()) {
						list.Add(item);
					}
				});
				ProcessCutting(false);
				UpdateUIMeshes();
			});
		}



		public override void Dispose() {
			base.Dispose();
			Entity.SetUIRect(Entity.GetFirstComponent<UIRect>());
			Entity.components.Changed -= RegisterUIList;
			Entity.children.Changed -= Children_Changed;
			_boundTo.SafeOperation((list) => {
				foreach (var item in list) {
					item.components.Changed -= Children_Changed;
				}
				list.Clear();
			});
		}

		public void Scroll(Vector3f value) {
			if(value == ScrollOffset) {
				return;
			}
			var phsicsupdate = value.x == ScrollOffset.x && value.y == ScrollOffset.y;
			ScrollOffset = value;
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					item.Scroll(value);
				}
			});
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.RenderScrollMesh(false);
				}
			});
			ProcessCutting(false, false);
			UpdateUIMeshes();
		}
	}
}
