using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using SixLabors.Fonts;
using System;

namespace RhuEngine.Components
{
	public interface ITextComp : ISyncObject
	{
		public DynamicTextRender TextRender { get; }
	}

	[NotLinkedRenderingComponent]
	[Category(new string[] { "Rendering" })]
	public sealed class WorldText : LinkedWorldComponent, ITextComp, IWorldBoundingBox
	{
		[Default(false)]
		public readonly Sync<bool> FitText;
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
			textRender.LoadText(Text, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, HorizontalAlien);
		}

		protected override void Render() {
			var scalerValue = 1f;
			if (FitText) {
				scalerValue = Math.Min(Width / (textRender.Width * 0.1f), Height/ (textRender.Height * 0.1f));
			}
			var y = VerticalAlien.Value switch {
				EVerticalAlien.Bottom => (textRender.Height * 0.1f * scalerValue) - (Height.Value / 2),
				EVerticalAlien.Center => textRender.Height / 2 * 0.1f * scalerValue,
				EVerticalAlien.Top => Height.Value / 2 ,
				_ => 0,
			};
			var x = HorizontalAlien.Value switch {
				EHorizontalAlien.Left => -(Width.Value / 2),
				EHorizontalAlien.Middle => -(textRender.Width / 2 * 0.1f * scalerValue),
				EHorizontalAlien.Right => (Width.Value / 2) - (textRender.Width * 0.1f * scalerValue),
				_ => 0,
			};
			var offSet = Matrix.TS(new Vector3f(x,y),new Vector3f(scalerValue, scalerValue));
			textRender.Render(Matrix.S(0.1f) * offSet, Entity.GlobalTrans, TargetRenderLayer);
		}

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
		}
	}
}
