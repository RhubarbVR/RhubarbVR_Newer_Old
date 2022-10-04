using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	public enum EVerticalAlien
	{
		Bottom,
		Center,
		Top,
	}
	public enum EHorizontalAlien
	{
		Left,
		Middle,
		Right,
	}

	[Category(new string[] { "UI/Visuals" })]
	public sealed class UIText : MultiRenderUIComponent
	{
		public override Colorf[] RenderTint => throw new NotImplementedException();

		public RMaterial[] materials = Array.Empty<RMaterial>();

		public override RMaterial[] RenderMaterial => materials;

		public override bool UseSingle => true;

		public override Colorf RenderTintSingle => Colorf.White;

		[Default(true)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<bool> FitText;

		[Default("Text Here")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> Text;
		[Default("")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> EmptyString;
		[Default("<color=rgb(0.9,0.9,0.9)>null")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> NullString;
		[OnAssetLoaded(nameof(ForceUpdate))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<float> Leading;
		[OnChanged(nameof(ForceUpdate))]
		[Default(RFontStyle.None)]
		public readonly Sync<RFontStyle> StartingStyle;

		[OnChanged(nameof(ForceUpdate))]
		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(false)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<bool> Password;

		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Vector2f> MaxClamp;

		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Vector2f> MinClamp;

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		public Matrix textOffset = Matrix.S(1);

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
		
		}

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			StartingColor.Value = Colorf.White;
			MinClamp.Value = Vector2f.MinValue;
			MaxClamp.Value = Vector2f.MaxValue;
		}

		protected override void UpdateMesh() {

		}
	}
}
