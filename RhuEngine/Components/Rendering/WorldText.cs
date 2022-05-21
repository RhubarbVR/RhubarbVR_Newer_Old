using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{
	public interface ITextComp : ISyncObject {
		public DynamicTextRender TextRender { get; }
	}

	[Category(new string[] { "Rendering" })]
	public class WorldText : RenderingComponent, ITextComp
	{
		[Default("<color=hsv(240,100,100)>Hello<color=blue><size14>World \n <size5>Trains \n are cool man<size10>\nHello ")]
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

		[Default(true)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<bool> MiddleLines;

		public DynamicTextRender textRender = new();

		public DynamicTextRender TextRender => textRender;
		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, VerticalAlien, HorizontalAlien, MiddleLines);
		}
		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<DefaultFont>();
		}
	}
}
