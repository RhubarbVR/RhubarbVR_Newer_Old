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
	public sealed class NinePatchRectLink : UIElementLinkBase<RhuEngine.Components.NinePatchRect, Godot.NinePatchRect>
	{
		public override string ObjectName => "TextLabel";

		public override void StartContinueInit() {

			LinkedComp.Texture.LoadChange += Texture_LoadChange;
			LinkedComp.DrawCenter.Changed += DrawCenter_Changed;
			LinkedComp.RegionMin.Changed += RegionMin_Changed;
			LinkedComp.RegionMax.Changed += RegionMax_Changed;
			LinkedComp.MarginMin.Changed += MarginMin_Changed;
			LinkedComp.MarginMax.Changed += MarginMax_Changed;
			LinkedComp.Horizontal.Changed += Horizontal_Changed;
			LinkedComp.Vertical.Changed += Vertical_Changed;
			Texture_LoadChange(null);
			DrawCenter_Changed(null);
			RegionMin_Changed(null);
			RegionMax_Changed(null);
			MarginMin_Changed(null);
			MarginMax_Changed(null);
			Horizontal_Changed(null);
			Vertical_Changed(null);

		}

		private void Texture_LoadChange(RTexture2D obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Texture = LinkedComp.Texture.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null);
		}

		private void Vertical_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AxisStretchVertical = LinkedComp.Vertical.Value switch { RNinePatchRectStretch.Tile => Godot.NinePatchRect.AxisStretchMode.Tile, RNinePatchRectStretch.TileFit => Godot.NinePatchRect.AxisStretchMode.TileFit, _ => Godot.NinePatchRect.AxisStretchMode.Stretch, });
		}

		private void Horizontal_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AxisStretchHorizontal = LinkedComp.Horizontal.Value switch { RNinePatchRectStretch.Tile => Godot.NinePatchRect.AxisStretchMode.Tile, RNinePatchRectStretch.TileFit => Godot.NinePatchRect.AxisStretchMode.TileFit, _ => Godot.NinePatchRect.AxisStretchMode.Stretch, });
		}

		private void MarginMax_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.PatchMarginTop = LinkedComp.MarginMax.Value.y;
				node.PatchMarginRight = LinkedComp.MarginMax.Value.x;
			});
		}

		private void MarginMin_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				node.PatchMarginBottom = LinkedComp.MarginMin.Value.y;
				node.PatchMarginLeft = LinkedComp.MarginMin.Value.x;
			});
		}

		private void RegionMax_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				var size = LinkedComp.RegionMax.Value - LinkedComp.RegionMin.Value;
				var pos = LinkedComp.RegionMin.Value - (size / 2);
				node.RegionRect = new Rect2(pos.x, pos.y, size.x, size.y);
			});
		}

		private void RegionMin_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				var size = LinkedComp.RegionMax.Value - LinkedComp.RegionMin.Value;
				var pos = LinkedComp.RegionMin.Value - (size / 2);
				node.RegionRect = new Rect2(pos.x, pos.y, size.x, size.y);
			});
		}

		private void DrawCenter_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DrawCenter = LinkedComp.DrawCenter.Value);
		}
	}
}
