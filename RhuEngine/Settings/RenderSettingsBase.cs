using System;
using System.Collections.Generic;
using System.Text;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class NullRenderSettingsBase : RenderSettingsBase
	{
	}

	public abstract class RenderSettingsBase:SettingsObject
	{
		public Action RenderSettingsChange;
	}
}
