using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

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

	[Category(new string[] { "UI" })]
	public class UIRect : Component, IRectData
	{
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> OffsetMin;
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> OffsetMax;
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<Vector2f> AnchorMin;
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

		public Vector2f Min => TrueMin + OffsetMin.Value;

		public Vector2f Max => TrueMax + OffsetMax.Value;

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
			RWorld.ExecuteOnMain(this, UpdateUIMeshes);
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
						meshList[i].LoadMesh(list[i].CutMesh);
					}
				});
			});
			_uiComponents.SafeOperation((list) => {
				for (var i = 0; i < _uiComponents.List.Count; i++) {
					list[i].RenderTargetChange();
				}
			});
		}

		private bool _cull = false;
		public void ProcessCutting() {
			var min = Min + ScrollOffset.Xy;
			var max = Max + ScrollOffset.Xy;
			var cutmin = CutZonesMin;
			var cutmax = CutZonesMax;
			_cull = max.y < cutmin.y || min.y > cutmax.y || max.x < cutmin.x || min.x > cutmax.x;
			var cut = !_cull && (max.y > cutmax.y || min.y < cutmin.y || max.x > cutmax.x || min.x < cutmin.x);
			_uiComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.CutElement(cut);
				}
			});
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.CutElement(cut);
				}
			});
		}

		public virtual void Render(Matrix matrix) {
			if (_cull) {
				return;
			}
			_meshes.SafeOperation((meshList) => {
				_uiRenderComponents.SafeOperation((list) => {
					for (var i = 0; i < _uiRenderComponents.List.Count; i++) {
						if (list[i].PhysicsCollider is not null) {
							list[i].PhysicsCollider.Matrix = matrix;
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
		}

		private readonly SafeList<Entity> _boundTo = new();

		public virtual void ChildAdded(UIRect child) {
			child?.SetOverride(null);
		}

		private void Children_Changed(IChangeable obj) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
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
			ProcessCutting();
			UpdateMeshes();
			if (added) {
				ChildRectAdded();
			}
		}

		public virtual void ChildRectAdded() {

		}


		private void RegisterUIList(IChangeable obj) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_uiComponents.SafeOperation((list) => list.Clear());
			_uiComponents.SafeOperation((list) => {
				foreach (var item in Entity.GetAllComponents<UIComponent>()) {
					if (!typeof(RenderUIComponent).IsAssignableFrom(item.GetType())) {
						list.Add(item);
					}
				}
			});
			_uiRenderComponents.SafeOperation((list) => list.Clear());
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in Entity.GetAllComponents<RenderUIComponent>()) {
					list.Add(item);
				}
			});
			ProcessCutting();
			UpdateMeshes();
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
			ScrollOffset = value;
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					item.Scroll(value);
				}
			});
			ProcessCutting();
			_uiRenderComponents.SafeOperation((list) => {
				foreach (var item in list) {
					item.RenderScrollMesh(false);
					item.RenderCutMesh(false);
				}
			});
			UpdateUIMeshes();
		}
	}
}
