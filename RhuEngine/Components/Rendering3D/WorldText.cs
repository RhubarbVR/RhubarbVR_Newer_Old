using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	public enum RBillboardOptions {
		Disabled, 
		Enabled, 
		YBillboard,
	}
	public enum RTextAlphaCutout
	{
		Disabled,
		Discard,
		Opaque_PrePass
	}
	public enum RTextVerticalAlignment
	{
		//
		// Summary:
		//     Vertical top alignment, usually for text-derived classes.
		Top,
		//
		// Summary:
		//     Vertical center alignment, usually for text-derived classes.
		Center,
		//
		// Summary:
		//     Vertical bottom alignment, usually for text-derived classes.
		Bottom,
	}

	[Category(new string[] { "Rendering3D" })]
	public sealed partial class TextLabel3D : GeometryInstance3D
	{
		[Default(0.005f)]
		public readonly Sync<float> PixelSize;
		public readonly Sync<Vector2i> TextOffset;
		public readonly Sync<RBillboardOptions> Billboard;
		public readonly Sync<bool> Shaded;
		[Default(true)]
		public readonly Sync<bool> DoubleSided;
		public readonly Sync<bool> NoDepthTest;
		public readonly Sync<bool> FixSize;
		public readonly Sync<RTextAlphaCutout> AlphaCutout;
		[Default(0.5f)]
		public readonly Sync<float> AlphaScissorThreshold;
		public readonly Sync<RElementTextureFilter> TextureFilter;
		public readonly Sync<int> RenderPriority;
		[Default(-1)]
		public readonly Sync<int> OutlineRenderPriority;
		public readonly Sync<Colorf> Modulate;
		public readonly Sync<Colorf> OutlineModulate;
		[Default("Text")]
		public readonly Sync<string> Text;
		public readonly AssetRef<RFont> Font;
		[Default(96)]
		public readonly Sync<int> FontSize;
		[Default(12)]
		public readonly Sync<int> OutLineSize;
		[Default(RHorizontalAlignment.Center)]
		public readonly Sync<RHorizontalAlignment> HorizontalAlignment;
		[Default(RTextVerticalAlignment.Center)]
		public readonly Sync<RTextVerticalAlignment> VerticalAlignment;
		public readonly Sync<bool> Uppercase;
		public readonly Sync<int> LineSpacing;
		public readonly Sync<RAutowrapMode> TextAutoWrap;
		[Default(500)]
		public readonly Sync<int> Width;
		public readonly Sync<RTextDirection> TextDir;
		public readonly Sync<string> Language;

		protected override void OnAttach() {
			base.OnAttach();
			Modulate.Value = Colorf.White;
			OutlineModulate.Value = Colorf.Black;
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
		}
	}
}
