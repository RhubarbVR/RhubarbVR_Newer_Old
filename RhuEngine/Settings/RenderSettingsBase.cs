using System;
using System.Collections.Generic;
using System.Text;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class NullRenderSettingsBase : RenderSettingsBase
	{
		public override bool RenderSettingsUpdate() {
			return false;
		}
	}

	public abstract class RenderSettingsBase:SettingsObject
	{
		public RenderSettingsBase() {
		}

		public abstract bool RenderSettingsUpdate();
	}
}
