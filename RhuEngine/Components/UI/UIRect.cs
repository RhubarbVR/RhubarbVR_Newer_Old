using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI" })]
	public class UIRect : Component
	{
		[OnChanged(nameof(UpdateUIMeshes))]
		public Sync<Vector2f> OffsetMin;
		[OnChanged(nameof(UpdateUIMeshes))]
		public Sync<Vector2f> OffsetMax;
		[OnChanged(nameof(UpdateUIMeshes))]
		public Sync<Vector2f> AnchorMin;
		[OnChanged(nameof(UpdateUIMeshes))]
		public Sync<Vector2f> AnchorMax;

		[Default(0.1f)]
		[OnChanged(nameof(DepthChange))]
		public Sync<float> Depth;

		public Vector2f Min => TrueMin + OffsetMin.Value;

		public Vector2f Max => TrueMax + OffsetMax.Value;

		public Vector2f BadMin => (((ParentRect?.BadMin ?? Vector2f.One) - (Vector2f.One - (ParentRect?.TrueMax ?? Vector2f.One))) * (Vector2f.One - AnchorMin.Value)) + (Vector2f.One - (ParentRect?.TrueMax ?? Vector2f.One));

		public Vector2f TrueMin => Vector2f.One - BadMin;

		public Vector2f TrueMax => (((ParentRect?.TrueMax ?? Vector2f.One) - (ParentRect?.TrueMin ?? Vector2f.Zero)) * AnchorMax.Value) + (ParentRect?.TrueMin ?? Vector2f.Zero);

		public float StartPoint => (ParentRect?.StartPoint ?? 0) + (ParentRect?.Depth.Value ?? 0);

		private void DepthChange() {
			UpdateUIMeshes();
		}

		private void UpdateUIMeshes() {
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
					item.UpdateUIMeshes();
				}
			});
		}

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UICanvas RegisteredCanvas { get; internal set; }

		public void RegisterCanvas() {
			RegisteredCanvas = Canvas;
			foreach (Entity item in Entity.children) {
				item.UIRect?.RegisterCanvas();
			}
			UpdateUIMeshes();
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

		private readonly SafeList<RMesh> _meshes = new();

		private readonly SafeList<UIComponent> _uiComponents = new();

		private readonly SafeList<RenderUIComponent> _uiRenderComponents = new();

		private readonly SafeList<UIRect> _childRects = new();

		public void UpdateMeshes() {
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
						if(list[i].MainMesh is null) {
							list[i].RenderTargetChange();
						}
						meshList[i].LoadMesh(list[i].MainMesh);
					}
				});
			});
			_uiComponents.SafeOperation((list) => {
				for (var i = 0; i < _uiComponents.List.Count; i++) {
					list[i].RenderTargetChange();
				}
			});
		}

		public void Render(Matrix matrix) {
			_meshes.SafeOperation((meshList) => {
				_uiRenderComponents.SafeOperation((list) => {
					for (var i = 0; i < _uiRenderComponents.List.Count; i++) {
						meshList[i].Draw(list[i].Pointer.ToString(), list[i].RenderMaterial, matrix,list[i].RenderTint);
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
					item.Render(matrix);
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
			Entity.SetUIRect(Entity.GetFirstComponent<UIRect>()??this);
			Entity.components.Changed += RegisterUIList;
			Entity.children.Changed += Children_Changed;
			Children_Changed(null);
			RegisterUIList(null);
			RegisterCanvas();
		}

		private readonly SafeList<Entity> _boundTo = new();


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
			_childRects.SafeOperation((list) => {
				foreach (Entity item in Entity.children) {
					_boundTo.SafeAdd(item);
					item.components.Changed += Children_Changed;
					list.Add(item.GetFirstComponent<UIRect>());
				}
			});
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
	}
}
