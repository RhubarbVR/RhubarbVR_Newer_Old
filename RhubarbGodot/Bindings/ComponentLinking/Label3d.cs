using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhubarbVR.Bindings.FontBindings;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;

using static System.Net.Mime.MediaTypeNames;
using static Godot.Control;
using static Godot.GeometryInstance3D;
using static Godot.TextServer;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class LinkedLabel3D : GeometryInstance3DBase<TextLabel3D, Label3D>
	{
		public override string ObjectName => "TextLabel";

		public override void StartContinueInit() {
			LinkedComp.PixelSize.Changed += PixelSize_Changed;
			LinkedComp.TextOffset.Changed += TextOffset_Changed;
			LinkedComp.Billboard.Changed += Billboard_Changed;
			LinkedComp.Shaded.Changed += Shaded_Changed;
			LinkedComp.DoubleSided.Changed += DoubleSided_Changed;
			LinkedComp.NoDepthTest.Changed += NoDepthTest_Changed;
			LinkedComp.FixSize.Changed += FixSize_Changed;
			LinkedComp.AlphaCutout.Changed += AlphaCutout_Changed;
			LinkedComp.AlphaScissorThreshold.Changed += AlphaScissorThreshold_Changed;
			LinkedComp.TextureFilter.Changed += TextureFilter_Changed;
			LinkedComp.RenderPriority.Changed += RenderPriority_Changed;
			LinkedComp.OutlineRenderPriority.Changed += OutlineRenderPriority_Changed;
			LinkedComp.Modulate.Changed += Modulate_Changed;
			LinkedComp.OutlineModulate.Changed += OutlineModulate_Changed;
			LinkedComp.Text.Changed += Text_Changed;
			LinkedComp.Font.LoadChange += Font_LoadChange;
			LinkedComp.FontSize.Changed += FontSize_Changed;
			LinkedComp.OutLineSize.Changed += OutLineSize_Changed;
			LinkedComp.HorizontalAlignment.Changed += HorizontalAlignment_Changed;
			LinkedComp.VerticalAlignment.Changed += VerticalAlignment_Changed;
			LinkedComp.Uppercase.Changed += Uppercase_Changed;
			LinkedComp.LineSpacing.Changed += LineSpacing_Changed;
			LinkedComp.TextAutoWrap.Changed += TextAutoWrap_Changed;
			LinkedComp.Width.Changed += Width_Changed;
			LinkedComp.TextDir.Changed += Dir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			PixelSize_Changed(null);
			TextOffset_Changed(null);
			Billboard_Changed(null);
			Shaded_Changed(null);
			DoubleSided_Changed(null);
			NoDepthTest_Changed(null);
			FixSize_Changed(null);
			AlphaCutout_Changed(null);
			AlphaScissorThreshold_Changed(null);
			TextureFilter_Changed(null);
			RenderPriority_Changed(null);
			OutlineRenderPriority_Changed(null);
			Modulate_Changed(null);
			OutlineModulate_Changed(null);
			Text_Changed(null);
			Font_LoadChange(null);
			FontSize_Changed(null);
			OutLineSize_Changed(null);
			HorizontalAlignment_Changed(null);
			VerticalAlignment_Changed(null);
			Uppercase_Changed(null);
			LineSpacing_Changed(null);
			TextAutoWrap_Changed(null);
			Width_Changed(null);
			Dir_Changed(null);
			Language_Changed(null);
		}

		private void Font_LoadChange(RFont obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Font = LinkedComp.Font.Asset?.Inst is GodotFont font ? (font?.FontFile) : null);
		}

		private void Language_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Language = LinkedComp.Language.Value);
		}

		private void Dir_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextDirection = LinkedComp.TextDir.Value switch { RTextDirection.Ltr => Direction.Ltr, RTextDirection.Rtl => Direction.Rtl, _ => TextServer.Direction.Auto, });
		}

		private void Width_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Width = LinkedComp.Width.Value);
		}

		private void TextAutoWrap_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AutowrapMode = LinkedComp.TextAutoWrap.Value switch { RAutowrapMode.Arbitrary => AutowrapMode.Arbitrary, RAutowrapMode.Word => AutowrapMode.Word, RAutowrapMode.WordSmart => AutowrapMode.WordSmart, _ => AutowrapMode.Off, });
		}

		private void LineSpacing_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LineSpacing = LinkedComp.LineSpacing.Value);
		}

		private void Uppercase_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Uppercase = LinkedComp.Uppercase.Value);
		}

		private void VerticalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VerticalAlignment = LinkedComp.VerticalAlignment.Value switch { RTextVerticalAlignment.Top => VerticalAlignment.Top, RTextVerticalAlignment.Bottom => VerticalAlignment.Bottom, _ => VerticalAlignment.Center, });
		}

		private void HorizontalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.HorizontalAlignment = LinkedComp.HorizontalAlignment.Value switch { RHorizontalAlignment.Left => HorizontalAlignment.Left, RHorizontalAlignment.Center => HorizontalAlignment.Center, RHorizontalAlignment.Right => HorizontalAlignment.Right, _ => HorizontalAlignment.Fill, });
		}

		private void OutLineSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.OutlineSize = LinkedComp.OutLineSize.Value);
		}

		private void FontSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FontSize = LinkedComp.FontSize.Value);
		}

		private void Text_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Text = LinkedComp.Text.Value);
		}

		private void OutlineModulate_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.OutlineModulate = new Color(LinkedComp.OutlineModulate.Value.r, LinkedComp.OutlineModulate.Value.g, LinkedComp.OutlineModulate.Value.b, LinkedComp.OutlineModulate.Value.a));
		}

		private void Modulate_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Modulate = new Color(LinkedComp.Modulate.Value.r, LinkedComp.Modulate.Value.g, LinkedComp.Modulate.Value.b, LinkedComp.Modulate.Value.a));
		}

		private void OutlineRenderPriority_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.OutlineRenderPriority = LinkedComp.OutlineRenderPriority.Value);
		}

		private void RenderPriority_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RenderPriority = LinkedComp.RenderPriority.Value);
		}

		private void TextureFilter_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureFilter = LinkedComp.TextureFilter.Value switch { RElementTextureFilter.Linear => BaseMaterial3D.TextureFilterEnum.Linear, RElementTextureFilter.LinearMipmap => BaseMaterial3D.TextureFilterEnum.LinearWithMipmaps, RElementTextureFilter.NearestMipmap => BaseMaterial3D.TextureFilterEnum.NearestWithMipmaps, RElementTextureFilter.NearestMipmapAnisotropic => BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic, RElementTextureFilter.LinearMipmapAnisotropic => BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic, _ => BaseMaterial3D.TextureFilterEnum.Nearest, });
		}

		private void AlphaScissorThreshold_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AlphaScissorThreshold = LinkedComp.AlphaScissorThreshold);
		}

		private void AlphaCutout_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AlphaCut = LinkedComp.AlphaCutout.Value switch { RTextAlphaCutout.Discard => Label3D.AlphaCutMode.Discard, RTextAlphaCutout.Opaque_PrePass => Label3D.AlphaCutMode.OpaquePrepass, _ => Label3D.AlphaCutMode.Disabled, });
		}

		private void FixSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FixedSize = LinkedComp.FixSize.Value);
		}

		private void NoDepthTest_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.NoDepthTest = LinkedComp.NoDepthTest.Value);
		}

		private void DoubleSided_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DoubleSided = LinkedComp.DoubleSided.Value);
		}

		private void Shaded_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Shaded = LinkedComp.Shaded.Value);
		}

		private void Billboard_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Billboard = LinkedComp.Billboard.Value switch { RBillboardOptions.Enabled => BaseMaterial3D.BillboardModeEnum.Enabled, RBillboardOptions.YBillboard => BaseMaterial3D.BillboardModeEnum.FixedY, _ => BaseMaterial3D.BillboardModeEnum.Disabled, });
		}

		private void TextOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Offset = new Vector2(LinkedComp.TextOffset.Value.x, LinkedComp.TextOffset.Value.y));
		}

		private void PixelSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PixelSize = LinkedComp.PixelSize.Value);
		}
	}

}
