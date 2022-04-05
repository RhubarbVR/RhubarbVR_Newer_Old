using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/PrimitiveVisuals" })]
	public class UIText : UIComponent
	{
		[Default("<style=regular><color=red>Hell>o<size5><color=blue> W<>or\nld<size10><color=red>!!<!</color></color></color></color></color>")]
		[OnChanged(nameof(UpdateText))]
		public Sync<string> Text;
		[OnChanged(nameof(UpdateText))]
		public AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateText))]
		public Sync<Colorf> StartingColor;

		[OnChanged(nameof(UpdateText))]
		[Default(FontStyle.Regular)]
		public Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(UpdateText))]
		[Default(10f)]
		public Sync<float> StatingSize;

		public UITextRender textRender = new();

		private void UpdateText() {
			textRender.LoadText(Pointer.ToString(), Text, Font.Asset, StartingColor, StartingStyle, StatingSize);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			UpdateText();
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<DefaultFont>();
		}

		public override void Render(Matrix matrix) {
			var rootmat = Matrix.T(new Vector3f(0, 0, Rect.StartPoint + 0.01f)) * matrix;
			textRender.Render(rootmat);
		}
	}
}
