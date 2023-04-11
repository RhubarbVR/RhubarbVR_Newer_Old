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
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class TexturedButtonLink : ButtonBase<TexturedButton, TextureButton>
	{
		public override string ObjectName => "TexturedButton";

		public override void StartContinueInit() {
			LinkedComp.IgnoreTextureSize.Changed += IgnoreTextureSize_Changed;
			LinkedComp.StretchMode.Changed += StretchMode_Changed;
			LinkedComp.FlipHorizontal.Changed += FlipHorizontal_Changed;
			LinkedComp.FlipVertical.Changed += FlipVertical_Changed;
			LinkedComp.Texture_Normal.LoadChange += Texture_Normal_LoadChange;
			LinkedComp.Texture_Press.LoadChange += Texture_Press_LoadChange;
			LinkedComp.Texture_Hover.LoadChange += Texture_Hover_LoadChange;
			LinkedComp.Texture_Disabled.LoadChange += Texture_Disabled_LoadChange;
			LinkedComp.Texture_Focused.LoadChange += Texture_Focused_LoadChange;
			LinkedComp.Texture_ClickMask.LoadChange += Texture_ClickMask_LoadChange;
			IgnoreTextureSize_Changed(null);
			StretchMode_Changed(null);
			FlipHorizontal_Changed(null);
			FlipVertical_Changed(null);
			Texture_Normal_LoadChange(null);
			Texture_Press_LoadChange(null);
			Texture_Hover_LoadChange(null);
			Texture_Disabled_LoadChange(null);
			Texture_Focused_LoadChange(null);
			Texture_ClickMask_LoadChange(null);
		}

		//Todo Add with bitmap support
		private void Texture_ClickMask_LoadChange(RTexture2D obj) {
			var texture = LinkedComp.Texture_ClickMask.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null;
		}

		private void Texture_Focused_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureFocused = LinkedComp.Texture_Focused.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Texture_Disabled_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureDisabled = LinkedComp.Texture_Disabled.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Texture_Hover_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureHover = LinkedComp.Texture_Hover.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Texture_Press_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TexturePressed = LinkedComp.Texture_Press.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Texture_Normal_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureNormal = LinkedComp.Texture_Normal.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void FlipVertical_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FlipV = LinkedComp.FlipVertical.Value);
		}

		private void FlipHorizontal_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FlipH = LinkedComp.FlipHorizontal.Value);
		}

		private void StretchMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.StretchMode = LinkedComp.StretchMode.Value switch { RTexturedButtonStretchMode.Tile => TextureButton.StretchModeEnum.Tile, RTexturedButtonStretchMode.Keep => TextureButton.StretchModeEnum.Keep, RTexturedButtonStretchMode.KeepCenter => TextureButton.StretchModeEnum.KeepCentered, RTexturedButtonStretchMode.KeepAsspect => TextureButton.StretchModeEnum.KeepAspect, RTexturedButtonStretchMode.KeepAsspectCenter => TextureButton.StretchModeEnum.KeepAspectCentered, RTexturedButtonStretchMode.KeepAsspectCovered => TextureButton.StretchModeEnum.KeepAspectCovered, _ => TextureButton.StretchModeEnum.Scale, });
		}

		private void IgnoreTextureSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.IgnoreTextureSize = LinkedComp.IgnoreTextureSize.Value);
		}
	}
}
