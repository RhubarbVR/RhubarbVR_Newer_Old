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
		public int ZDepth { get; }


		public Vector2f TrueMax { get; }

		public float StartPoint { get; }

		public float DepthValue { get; }

		public void UpdateMinMaxNoPross();

		public void UpdateMinMax();

	}

	public class BasicRectOvride : IRectData
	{
		public int ZDepth => ((ParentRect)?.ZDepth ?? 0) + 1;

		public UIRect Child { get; set; }
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

		public void UpdateMinMax() {
			Child?.UpdateMinMax();
		}

		public void UpdateMinMaxNoPross() {
			Child?.UpdateMinMaxNoPross();
		}
	}

	public struct HitData
	{
		public Handed Handed;
		public Vector3f HitPosNoScale;
		public Vector3f HitPosWorld;
		public Vector3f HitPos;
		public Vector3f HitNormalWorld;
		public Vector3f HitNormal;
		public uint Touchindex;
		public bool Laser;
		public bool CustomTouch;
		public float PressForce;
		public float GripForce;

		public void Clean(Matrix parrent, Vector3f canvasScale) {
			var pointNoScale = Matrix.T(HitPosWorld) * (Matrix.S(1 / canvasScale) * parrent).Inverse;
			HitPosNoScale = pointNoScale.Translation;
			var point = Matrix.T(HitPosWorld) * parrent.Inverse;
			HitPos = point.Translation;
			HitNormal = point.Rotation.AxisZ;
		}
	}

	[Category(new string[] { "UI/Rects" })]
	public class UIRect : Component, IRectData
	{
		public bool PysicsLock { get; private set; }
		public void PhysicsLock() {
			PysicsLock = true;
		}
		public void PhysicsUnLock() {
			PysicsLock = false;
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.LoadPhysicsMesh();
				}
			});
		}

		private readonly List<HitData> _rayHitPoses = new();
		private readonly List<HitData> _lastRayHitPoses = new();

		public void AddHitPoses(HitData hitData) {
			RWorld.ExecuteOnStartOfFrame(() => {
				hitData.Clean(LastRenderPos, (Canvas?.scale.Value ?? Vector3f.One) / 10);
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
				if (lastpoint.Current.Touchindex == newHitpoin.Current.Touchindex) {
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

		public IEnumerable<Vector3f> ClickGripChange(float threshold, bool ignoreOtherInputZones = false) {
			var lastpoint = LastHitPosesByFingerID(ignoreOtherInputZones).GetEnumerator();
			var newHitpoin = HitPosesByFingerID(ignoreOtherInputZones).GetEnumerator();
			var hasData1 = newHitpoin.MoveNext();
			var hasData2 = lastpoint.MoveNext();
			while (hasData1 && hasData2) {
				var currentIndex = Math.Min(lastpoint.Current.Touchindex, newHitpoin.Current.Touchindex);
				if (lastpoint.Current.Touchindex == newHitpoin.Current.Touchindex) {
					if (lastpoint.Current.GripForce >= threshold && newHitpoin.Current.GripForce >= threshold) {
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

		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<Vector2f> OffsetLocalMin;
		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<Vector2f> OffsetLocalMax;
		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<Vector2f> OffsetMin;
		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<Vector2f> OffsetMax;
		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<Vector2f> AnchorMin;
		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<Vector2f> AnchorMax;

		public Matrix MatrixMove(Matrix matrix) {
			var firstpos = VertMove(matrix.Translation);
			var anglecalfirst = 0f;
			if (Canvas.FrontBind.Value) {
				var adder = 1;
				if (matrix.Translation.x < 0.5f) {
					adder = 0;
				}
				anglecalfirst = Canvas.FrontBindAngle.Value * (((float)Math.Floor(matrix.Translation.x * Canvas.FrontBindSegments.Value) + adder) / Canvas.FrontBindSegments.Value);
				anglecalfirst = (anglecalfirst * -1) + 90;
			}
			var rollangle = 0f;
			var transform = firstpos - matrix.Translation;
			if (Canvas.TopOffset.Value) {
				rollangle = Canvas.TopOffsetValue.Value * matrix.Translation.y * 45;
				transform += new Vector3f(0, Canvas.TopOffsetValue.Value * 0.03f, 0);
			}
			return Matrix.TR(transform, Quaternionf.CreateFromEuler(anglecalfirst, rollangle, 0));
		}
		public Vector3f VertMove(Vector3f point) {
			var npoint = point;
			if (Canvas.TopOffset.Value) {
				npoint.z -= point.y * Canvas.TopOffsetValue.Value;
			}
			if (Canvas.FrontBind.Value) {
				var data = (Vector3d)npoint;
				data.Bind(Canvas.FrontBindAngle, Canvas.FrontBindRadus, Canvas.scale.Value, Canvas.FrontBindSegments.Value);
				npoint = (Vector3f)data;
			}
			return npoint;
		}

		public Vector2f AnchorMinValue => AnchorMin;


		public Vector2f AnchorMaxValue => AnchorMax;

		[Default(0.05f)]
		[OnChanged(nameof(UpdateMinMax))]
		public readonly Sync<float> Depth;
		public float DepthValue => Depth;

		public IRectData ParrentRect => _rectDataOverride ?? ParentRect;

		public virtual Vector2f CutZonesMax => Entity.parent.Target?.UIRect?.CutZonesMax ?? Vector2f.Inf;

		public virtual Vector2f CutZonesMin => Entity.parent.Target?.UIRect?.CutZonesMin ?? Vector2f.NInf;

		Vector2f _cachedMin;
		Vector2f _cachedMax;
		Vector2f _cachedBadMin;

		public void UpdateMinMax() {
			UpdateMinMaxNoPross();
			RegUpdateUIMeshes();
		}
		public virtual void UpdateMinMaxNoPross() {
			_cachedBadMin = CompBadMin;
			_cachedMin = CompMin;
			_cachedMax = CompMax;
			ZDepth = CompZDepth;
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					item.UpdateMinMaxNoPross();
				}
			});
		}
		public int ZDepth { get; private set; }

		public int CompZDepth => ((_rectDataOverride ?? ParentRect)?.ZDepth ?? 0) + 1;

		public Vector2f AddedSize { get; set; }

		public event Action AddedSizeCHange;

		public void UpdateAddedSize(Vector2f size) {
			AddedSize = size;
			AddedSizeCHange?.Invoke();
		}

		public Vector2f Min => _cachedMin;

		public Vector2f Max => _cachedMax;
		
		public Vector2f CompMin => TrueMin + (OffsetLocalMin.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));

		public Vector2f CompMax => TrueMax + (OffsetLocalMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));

		public Vector2f BadMin => _cachedBadMin;

		public Vector2f CompBadMin => ((((_rectDataOverride ?? ParentRect)?.BadMin ?? Vector2f.One) - (Vector2f.One - ((_rectDataOverride ?? ParentRect)?.TrueMax ?? Vector2f.One))) * (Vector2f.One - AnchorMin.Value)) + (Vector2f.One - ((_rectDataOverride ?? ParentRect)?.TrueMax ?? Vector2f.One)) - (OffsetMin.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));

		public Vector2f TrueMin => Vector2f.One - BadMin;

		public Vector2f TrueMax => ((((_rectDataOverride ?? ParentRect)?.TrueMax ?? Vector2f.One) - ((_rectDataOverride ?? ParentRect)?.TrueMin ?? Vector2f.Zero)) * AnchorMax.Value) + ((_rectDataOverride ?? ParentRect)?.TrueMin ?? Vector2f.Zero) + (OffsetMax.Value / (Canvas?.scale.Value.Xy ?? Vector2f.One));

		public float StartPoint => ((_rectDataOverride ?? ParentRect)?.StartPoint ?? 0) + ((_rectDataOverride ?? ParentRect)?.DepthValue ?? 0);

		private IRectData _rectDataOverride;
		public virtual bool RemoveFakeRecs => true;

		public Vector3f ScrollOffset { get; set; }

		public void SetOverride(IRectData rectDataOverride) {
			if (rectDataOverride != _rectDataOverride) {
				_rectDataOverride = rectDataOverride;
			}
		}

		public void RegUpdateUIMeshes() {
			RWorld.ExecuteOnStartOfFrame(this, UpdateUIMeshes);
		}

		public virtual void UpdateUIMeshes() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (Canvas is null) {
				return;
			}
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.ProcessMesh();
				}
			});

			_uiComponents.SafeOperation((list) => {
				for (var i = 0; i < _uiComponents.List.Count; i++) {
					list[i].RenderTargetChange();
				}
			});
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					if (RemoveFakeRecs) {
						item?.SetOverride(null);
					}
					item?.UpdateUIMeshes();
				}
			});
			UpdateMeshes();
		}
		[UnExsposed]
		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UICanvas RegisteredCanvas;

		public void RegisterCanvas() {
			RegisteredCanvas = Canvas;
			foreach (Entity item in Entity.children) {
				item?.UIRect?.RegisterCanvas();
			}
			UpdateMinMax();
		}
		[UnExsposed]
		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UICanvas BoundCanvas;

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
			if (Canvas is null) {
				return;
			}
			_meshes.SafeOperation((meshList) => {
				_uiRenderComponents.SafeOperation((list) => {
					if (meshList.Count < list.Count) {
						for (var i = 0; i < list.Count - meshList.Count; i++) {
							meshList.Add(new RMesh(null, true));
						}
					}
					if (meshList.Count > list.Count) {
						for (var i = 0; i < meshList.Count - list.Count; i++) {
							meshList.Remove(new RMesh(null,true));
						}
					}
					for (var i = 0; i < meshList.Count; i++) {
						if (list[i].CutMesh is null) {
							list[i].RenderCutMesh(false);
						}
						meshList[i].LoadMesh(list[i].RenderMesh);
					}
				});
			});
		}

		public bool Culling { get; private set; } = false;

		public void ProcessCutting(bool update = true, bool updatePhysicsMesh = true) {
			if (Canvas is null) {
				return;
			}
			var min = Min + ScrollOffset.Xy;
			var max = Max + ScrollOffset.Xy;
			var cutmin = CutZonesMin;
			var cutmax = CutZonesMax;
			Culling = max.y < cutmin.y || min.y > cutmax.y || max.x < cutmin.x || min.x > cutmax.x;
			var cut = !Culling && (max.y > cutmax.y || min.y < cutmin.y || max.x > cutmax.x || min.x < cutmin.x);
			_uiComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.CutElement(cut, update);
				}
			});
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.CutElement(cut, update);
					if (updatePhysicsMesh) {
						item.LoadPhysicsMesh();
					}
				}
			});
		}

		public Matrix WorldPos => Matrix.T(Min.XY_* (Canvas.scale.Value / 10)) * LastRenderPos;

		public Matrix LastRenderPos { get; private set; }

		public virtual void Render(Matrix matrix) {
			LastRenderPos = matrix;
			if (Culling) {
				return;
			}
			_meshes.SafeOperation((meshList) => {
				_uiRenderComponents.SafeOperation((list) => {
					for (var i = 0; i < _uiRenderComponents.List.Count; i++) {
						var mataddon = list[i].BoxBased ? Matrix.S((Canvas?.scale.Value ?? Vector3f.One) / 10) : Matrix.Identity;
						if (list[i].PhysicsCollider is not null) {
							list[i].PhysicsCollider.Matrix = list[i].PhysicsPose * mataddon * matrix;
						}
						if (list[i].RenderMaterial is not null) {
							meshList[i].Draw(list[i].Pointer.ToString(), list[i].RenderMaterial, mataddon * matrix, list[i].RenderTint, ZDepth);
						}
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
					if (item.Entity.IsEnabled) {
						item?.Render(matrix);
					}
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
			RegisterCanvas();
			Entity.components.Changed += RegisterUIList;
			Entity.children.Changed += Children_Changed;
			Children_Changed(null);
			RegisterUIList(null);
			ProcessCutting();
		}

		private readonly SafeList<Entity> _boundTo = new();

		public virtual void ChildAdded(UIRect child) {
			child?.SetOverride(null);
		}

		public object children_ChangedClass = Guid.NewGuid();

		private void Children_Changed(IChangeable obj) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RWorld.ExecuteOnStartOfFrame(children_ChangedClass, () => {
				_boundTo.SafeOperation((alist) => {
					foreach (var item in alist) {
						item.components.Changed -= Children_Changed;
					}
					alist.Clear();
					_childRects.SafeOperation((list) => {
						list.Clear();
						foreach (Entity item in Entity.children) {
							item.components.Changed += Children_Changed;
							alist.Add(item);
							var childadded = item.UIRect;
							if (childadded is not null) {
								list.Add(childadded);
								childadded.RegisterCanvas();
								childadded.UpdateMinMax();
							}
						}
					});
				});
				UpdateUIMeshes();
				Scroll(ScrollOffset, true, true);
			});
		}

		public object RegisterUIListClass = Guid.NewGuid();

		private void RegisterUIList(IChangeable obj) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RWorld.ExecuteOnStartOfFrame(RegisterUIListClass, () => {
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
				Scroll(ScrollOffset, true, true);
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

		public void Scroll(Vector3f value, bool forceUpdate = false, bool forcePhsics = false) {
			if (value == ScrollOffset && !forceUpdate) {
				return;
			}
			var phsicsupdate = !(value.x == ScrollOffset.x && value.y == ScrollOffset.y);
			ScrollOffset = value;
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.RenderScrollMesh(false);
				}
			});
			_uiComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.RenderTargetChange();
				}
			});
			ProcessCutting(false, phsicsupdate || forcePhsics);
			UpdateMeshes();
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					item.Scroll(value, forceUpdate, forcePhsics);
				}
			});
		}
	}
}
