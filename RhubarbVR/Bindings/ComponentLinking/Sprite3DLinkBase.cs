using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;

using RNumerics;

using static GDExtension.GeometryInstance3D;

namespace RhubarbVR.Bindings.ComponentLinking
{

	public abstract class Sprite3DLinkBase<T, T2> : GeometryInstance3DBase<T, T2> where T : RhuEngine.Components.Sprite3DBase, new() where T2 : GDExtension.Sprite3D, new()
	{
		public override string ObjectName => "Sprite3D";

		public override void Init() {
			base.Init();
			LinkedComp.Centered.Changed += Centered_Changed;
			LinkedComp.OffsetPos.Changed += OffsetPos_Changed;
			LinkedComp.FlipH.Changed += FlipH_Changed;
			LinkedComp.FlipV.Changed += FlipY_Changed;
			LinkedComp.Moduluate.Changed += Moduluate_Changed;
			LinkedComp.PixelSize.Changed += PixelSize_Changed;
			LinkedComp.Axis.Changed += Axis_Changed;
			LinkedComp.Billboard.Changed += Billboard_Changed;
			LinkedComp.Transparent.Changed += Transparrent_Changed;
			LinkedComp.Shaded.Changed += Shaded_Changed;
			LinkedComp.DoubleSided.Changed += DoubleSided_Changed;
			LinkedComp.NoDepthTest.Changed += NoDepthTest_Changed;
			LinkedComp.FixedSize.Changed += FixedSize_Changed;
			LinkedComp.AlphaMode.Changed += AlphaMode_Changed;
			LinkedComp.RenderPriority.Changed += RenderPriority_Changed;
			Centered_Changed(null);
			OffsetPos_Changed(null);
			FlipH_Changed(null);
			FlipY_Changed(null);
			Moduluate_Changed(null);
			PixelSize_Changed(null);
			Axis_Changed(null);
			Billboard_Changed(null);
			Transparrent_Changed(null);
			Shaded_Changed(null);
			DoubleSided_Changed(null);
			NoDepthTest_Changed(null);
			FixedSize_Changed(null);
			AlphaMode_Changed(null);
			RenderPriority_Changed(null);
		}

		private void RenderPriority_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RenderPriority = LinkedComp.RenderPriority.Value);
		}

		private void AlphaMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AlphaCut = LinkedComp.AlphaMode.Value switch {
				RSprite3DAlphaCut.Discard => SpriteBase3D.AlphaCutMode.Discard,
				RSprite3DAlphaCut.OpaquePrePass => SpriteBase3D.AlphaCutMode.OpaquePrepass,
				_ => SpriteBase3D.AlphaCutMode.Disabled,
			});
		}

		private void FixedSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FixedSize = LinkedComp.FixedSize.Value);
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

		private void Transparrent_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Transparent = LinkedComp.Transparent.Value);
		}

		private void Billboard_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Billboard = LinkedComp.Billboard.Value switch { RBillboardOptions.Enabled => BaseMaterial3D.BillboardMode.Enabled, RBillboardOptions.YBillboard => BaseMaterial3D.BillboardMode.FixedY, _ => BaseMaterial3D.BillboardMode.Disabled, });
		}

		private void Axis_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Axis = LinkedComp.Axis.Value switch { RSprite3DDir.Y => Vector3.Axis.Y, RSprite3DDir.Z => Vector3.Axis.Z, _ => Vector3.Axis.X, });
		}

		private void PixelSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PixelSize = LinkedComp.PixelSize.Value);
		}

		private void Moduluate_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Modulate = new Color(LinkedComp.Moduluate.Value.r, LinkedComp.Moduluate.Value.g, LinkedComp.Moduluate.Value.b, LinkedComp.Moduluate.Value.a));
		}

		private void FlipY_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FlipV = LinkedComp.FlipV.Value);
		}

		private void FlipH_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FlipH = LinkedComp.FlipH.Value);
		}

		private void OffsetPos_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Offset = new Vector2(LinkedComp.OffsetPos.Value.x, LinkedComp.OffsetPos.Value.y));
		}

		private void Centered_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Centered = LinkedComp.Centered.Value);
		}
	}

	public sealed class Sprite3DLink : Sprite3DLinkBase<RhuEngine.Components.Sprite3D, GDExtension.Sprite3D>
	{
		public override void StartContinueInit() {
			LinkedComp.texture.LoadChange += Texture_LoadChange;
			LinkedComp.HFrames.Changed += HFrames_Changed;
			LinkedComp.VFrames.Changed += VFrames_Changed;
			LinkedComp.Frame.Changed += Frame_Changed;
			LinkedComp.RegionEnabled.Changed += RegionEnabled_Changed;
			LinkedComp.MinRect.Changed += Rect_Changed;
			LinkedComp.MaxRect.Changed += Rect_Changed;
			Texture_LoadChange(null);
			HFrames_Changed(null);
			VFrames_Changed(null);
			Frame_Changed(null);
			RegionEnabled_Changed(null);
			Rect_Changed(null);
		}

		private void Texture_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Texture = LinkedComp.texture?.Asset?.Inst is GodotTexture2D texture2D ? texture2D.Texture2D : null);
		}

		private void Rect_Changed(IChangeable obj) {
			var min = LinkedComp.MinRect.Value;
			var max = LinkedComp.MaxRect.Value;
			var pos = ((min - max) / 2) + min;
			var size = min - max;
			RenderThread.ExecuteOnEndOfFrame(() => 
				node.RegionRect = new Rect2(
				new Vector2(pos.x, pos.y), new Vector2(size.x, size.y)
			));
		}


		private void RegionEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RegionEnabled = LinkedComp.RegionEnabled.Value);
		}

		private void Frame_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Frame = LinkedComp.Frame.Value);
		}

		private void VFrames_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Vframes = LinkedComp.VFrames.Value);
		}

		private void HFrames_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Hframes = LinkedComp.HFrames.Value);
		}
	}
}
