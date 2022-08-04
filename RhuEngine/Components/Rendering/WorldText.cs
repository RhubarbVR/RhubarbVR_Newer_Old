using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using SixLabors.Fonts;

namespace RhuEngine.Components
{
	public interface ITextComp : ISyncObject
	{
		public DynamicTextRender TextRender { get; }
	}

	[NotLinkedRenderingComponent]
	[Category(new string[] { "Rendering" })]
	public class WorldText : RenderingComponent, ITextComp, IWorldBoundingBox
	{
		[Default(1f)]
		public readonly Sync<float> Width;
		[Default(1f)]
		public readonly Sync<float> Height;
		[Default("Text Here")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> Text;
		[OnAssetLoaded(nameof(UpdateText))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<float> Leading;
		[OnChanged(nameof(UpdateText))]
		[Default(FontStyle.Regular)]
		public readonly Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(UpdateText))]
		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		[Default(RenderLayer.Text)]
		public readonly Sync<RenderLayer> TargetRenderLayer;

		public DynamicTextRender textRender = new();

		public DynamicTextRender TextRender => textRender;

		public AxisAlignedBox3f Bounds => textRender.axisAlignedBox3F;

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, VerticalAlien, HorizontalAlien);
		}

		public override void Render() {
			float y;
			switch (VerticalAlien.Value) {
				case EVerticalAlien.Bottom:
					y = ((textRender.Height) * 0.1f) - (Height.Value / 2);
					break;
				case EVerticalAlien.Center:
					y = (textRender.Height / 2) * 0.1f;
					break;
				case EVerticalAlien.Top:
					y = Height.Value / 2;
					break;
				default:
					y = 0;
					break;
			}
			float x;
			switch (HorizontalAlien.Value) {
				case EHorizontalAlien.Left:
					x = -(Width.Value / 2);
					break;
				case EHorizontalAlien.Middle:
					x = -(textRender.Width / 2 * 0.1f);
					break;
				case EHorizontalAlien.Right:
					x = (Width.Value / 2) - (textRender.Width * 0.1f);
					break;
				default:
					x = 0;
					break;
			}
			var offSet = Matrix.T(new Vector3f(x,y));
			textRender.Render(Matrix.S(0.1f) * offSet, Entity.GlobalTrans, TargetRenderLayer);
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
		}
	}
}
