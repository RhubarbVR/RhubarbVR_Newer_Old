using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[NotLinkedRenderingComponent]
	[Category(new string[] { "UI" })]
	public class UICanvas : RenderingComponent
	{
		public readonly Sync<Vector3f> scale;

		[Default(false)]
		public readonly Sync<bool> TopOffset;
		[Default(3f)]
		public readonly Sync<float> TopOffsetValue;

		[Default(false)]
		public readonly Sync<bool> FrontBind;
		[Default(10)]
		public readonly Sync<int> FrontBindSegments;
		[Default(135f)]
		public readonly Sync<float> FrontBindAngle;
		[Default(7.5f)]
		public readonly Sync<float> FrontBindRadus;
		public override void OnAttach() {
			base.OnAttach();
			scale.Value = new Vector3f(16, 9,1);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			Entity.UIRect?.CanvasUpdate();
		}
		public override void Render() {

		}
	}
}
