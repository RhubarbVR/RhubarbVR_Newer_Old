using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using System;

namespace RhuEngine.Components
{

	[NotLinkedRenderingComponent]
	[Category(new string[] { "Rendering" })]
	public sealed class WorldText : LinkedWorldComponent
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
		[Default(RFontStyle.Regular)]
		public readonly Sync<RFontStyle> StartingStyle;

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

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
		}

		protected override void Render() {
	
		}

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
		}
	}
}
