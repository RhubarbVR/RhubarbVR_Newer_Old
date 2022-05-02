using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI" })]
	public class UICanvas : RenderingComponent
	{
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Sync<Vector3f> scale;

		[Default(true)]
		public readonly Sync<bool> TopOffset;
		[Default(3f)]
		public readonly Sync<float> TopOffsetValue;

		[Default(true)]
		public readonly Sync<bool> FrontBind;
		[Default(10)]
		public readonly Sync<int> FrontBindSegments;
		[Default(180f)]
		public readonly Sync<float> FrontBindAngle;
		[Default(7.5f)]
		public readonly Sync<float> FrontBindRadus;
		public override void OnAttach() {
			base.OnAttach();
			scale.Value = new Vector3f(16, 9,1);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			Entity.UIRectUpdate += Entity_UIRectUpdate;
			Entity_UIRectUpdate(null, Entity.UIRect);
		}

		private void Entity_UIRectUpdate(UIRect last, UIRect uIRect) {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (last is not null) {
				var comp = last.Entity.GetFirstComponent<UICanvas>();
				last.BoundCanvas = comp == this ? null : comp;
				last.RegisterCanvas();
			}
			if (uIRect is not null) {
				uIRect.BoundCanvas = uIRect.Entity.GetFirstComponent<UICanvas>()??this;
				uIRect.RegisterCanvas();
			}
		}

		public void UpdateMeshes() {
			Entity.UIRect?.UpdateMeshes();
		}

		public void RenderUI() {
			if (Entity.UIRect is null) {
				return;
			}
			var uiRect = Entity.UIRect;
			uiRect.Render(Matrix.S(scale.Value/10) * Entity.GlobalTrans);
		}

		public override void Dispose() {
			Entity_UIRectUpdate(Entity.UIRect, null);
			Entity.UIRectUpdate -= Entity_UIRectUpdate;
			base.Dispose();
		}
	}
}
