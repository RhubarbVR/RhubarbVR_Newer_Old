using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using GDExtension;
using RhuEngine.Components;
using static GDExtension.Control;
using RhubarbVR.Bindings.TextureBindings;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class TextureRectLink : UIElementLinkBase<RhuEngine.Components.TextureRect, GDExtension.TextureRect>
	{
		public override string ObjectName => "TextureRect";

		public override void StartContinueInit() {

			LinkedComp.Texture.LoadChange += Texture_LoadChange;
			LinkedComp.ExpandedMode.Changed += IgnoreTextureSize_Changed;
			LinkedComp.StrechMode.Changed += StrechMode_Changed;
			LinkedComp.FlipVertical.Changed += FlipVertical_Changed;
			LinkedComp.FlipHorizontal.Changed += FlipHorizontal_Changed;
			Texture_LoadChange(null);
			IgnoreTextureSize_Changed(null);
			StrechMode_Changed(null);
			FlipVertical_Changed(null);
			FlipHorizontal_Changed(null);
		}

		private void Texture_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Texture = LinkedComp.Texture?.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void FlipHorizontal_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FlipH = LinkedComp.FlipHorizontal.Value);
		}

		private void FlipVertical_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FlipV = LinkedComp.FlipVertical.Value);
		}

		private void StrechMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.StretchModeValue = LinkedComp.StrechMode.Value switch {
				RStrechMode.Tile => GDExtension.TextureRect.StretchMode.Tile,
				RStrechMode.Keep => GDExtension.TextureRect.StretchMode.Keep,
				RStrechMode.KeepCenter => GDExtension.TextureRect.StretchMode.KeepCentered,
				RStrechMode.KeepAspect => GDExtension.TextureRect.StretchMode.KeepAspect,
				RStrechMode.KeepAspectCenter => GDExtension.TextureRect.StretchMode.KeepAspectCentered,
				RStrechMode.KeepAspectCovered => GDExtension.TextureRect.StretchMode.KeepAspectCovered,
				_ => GDExtension.TextureRect.StretchMode.Scale,
			});
		}

		private void IgnoreTextureSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ExpandModeValue = (GDExtension.TextureRect.ExpandMode)LinkedComp.ExpandedMode.Value);
		}
	}
}
