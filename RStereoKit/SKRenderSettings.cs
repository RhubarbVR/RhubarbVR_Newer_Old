using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine;
using RhuSettings;
using RhuEngine.Settings;
using StereoKit;
namespace RStereoKit
{
	public class SKRenderSettings:RenderSettingsBase
	{
		[SettingsField("Fov")]
		public float Fov = 60f;

		public SKRenderSettings() {
			RenderSettingsChange = () => Renderer.SetFOV(Fov);
		}
	}
}
