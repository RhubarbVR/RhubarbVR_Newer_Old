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
using System.Drawing.Imaging;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ColorPickerContainerLink : BoxContainerBase<RhuEngine.Components.ColorPickerContainer, Godot.ColorPicker>
	{
		public override string ObjectName => "ColorPicker";

		public override void StartContinueInit() {
			LinkedComp.Color.Changed += Color_Changed;
			LinkedComp.EditAlpha.Changed += EditAlpha_Changed;
			LinkedComp.ColorMode.Changed += ColorMode_Changed;
			LinkedComp.DeferredMode.Changed += DeferredMode_Changed;
			LinkedComp.ColorPickerShape.Changed += ColorPickerShape_Changed;
			Color_Changed(null);
			EditAlpha_Changed(null);
			ColorMode_Changed(null);
			DeferredMode_Changed(null);
			ColorPickerShape_Changed(null);
		}

		private void ColorPickerShape_Changed(IChangeable obj) {
			node.PickerShape = LinkedComp.ColorPickerShape.Value switch {
				RColorPickerShape.HSV_Rect_Wheel => ColorPicker.PickerShapeType.HsvWheel,
				RColorPickerShape.VHS_Color => ColorPicker.PickerShapeType.VhsCircle,
				RColorPickerShape.HSL_Circle => ColorPicker.PickerShapeType.OkhslCircle,
				_ => ColorPicker.PickerShapeType.HsvRectangle,
			};
		}

		private void DeferredMode_Changed(IChangeable obj) {
			node.DeferredMode = LinkedComp.DeferredMode.Value;
		}

		private void ColorMode_Changed(IChangeable obj) {
			node.ColorMode = LinkedComp.ColorMode.Value switch {
				RColorMode.HSV => ColorPicker.ColorModeType.Hsv,
				RColorMode.HSL => ColorPicker.ColorModeType.Okhsl,
				RColorMode.RAW => ColorPicker.ColorModeType.Raw,
				_ => ColorPicker.ColorModeType.Rgb,
			};
		}

		private void EditAlpha_Changed(IChangeable obj) {
			node.EditAlpha = LinkedComp.EditAlpha.Value;
		}

		private void Color_Changed(IChangeable obj) {
			node.Color = new Color(LinkedComp.Color.Value.r, LinkedComp.Color.Value.g, LinkedComp.Color.Value.b, LinkedComp.Color.Value.a);
		}
	}
}
