using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	public sealed class SettingsProgram : Program
	{
		public override string ProgramID => "Settings";

		public override Vector2i? Icon => new Vector2i(0,0);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.Settings.Name";

		public override bool LocalName => true;

		public override void LoadUI(Entity uiRoot) {
			var ma = uiRoot.AttachComponent<UIRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UIBuilder(uiRoot, mit, ma,true);
			uiBuilder.PushRect(new Vector2f(.25f), new Vector2f(.75f));
			uiBuilder.AddGenaricCheckBox(window.IconMit.Target, window.IconSprite.Target);
		}
	}
}
