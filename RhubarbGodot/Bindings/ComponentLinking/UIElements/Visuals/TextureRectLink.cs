using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
using RhuEngine.Components;
using static Godot.Control;
using RhubarbVR.Bindings.TextureBindings;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class TextureRectLink : UIElementLinkBase<RhuEngine.Components.TextureRect, Godot.TextureRect>
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
			node.Texture = LinkedComp.Texture?.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null;
		}

		private void FlipHorizontal_Changed(IChangeable obj) {
			node.FlipH = LinkedComp.FlipHorizontal.Value;
		}

		private void FlipVertical_Changed(IChangeable obj) {
			node.FlipV = LinkedComp.FlipVertical.Value;
		}

		private void StrechMode_Changed(IChangeable obj) {
			node.StretchMode = LinkedComp.StrechMode.Value switch {
				RStrechMode.Tile => Godot.TextureRect.StretchModeEnum.Tile,
				RStrechMode.Keep => Godot.TextureRect.StretchModeEnum.Keep,
				RStrechMode.KeepCenter => Godot.TextureRect.StretchModeEnum.KeepCentered,
				RStrechMode.KeepAspect => Godot.TextureRect.StretchModeEnum.KeepAspect,
				RStrechMode.KeepAspectCenter => Godot.TextureRect.StretchModeEnum.KeepAspectCentered,
				RStrechMode.KeepAspectCovered => Godot.TextureRect.StretchModeEnum.KeepAspectCovered,
				_ => Godot.TextureRect.StretchModeEnum.Scale,
			};
		}

		private void IgnoreTextureSize_Changed(IChangeable obj) {
			node.ExpandMode = (Godot.TextureRect.ExpandModeEnum)LinkedComp.ExpandedMode.Value;
		}
	}
}
