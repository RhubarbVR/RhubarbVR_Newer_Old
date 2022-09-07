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
	public class CreateNewWorldProgram : Program
	{
		public override string ProgramID => "CreateNewWorld";

		public override Vector2i? Icon => new Vector2i(17,1);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.CreateNewWorld.Name";

		public override bool LocalName => true;

		[Exposed]
		public void CreateNewWorld(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			var isLogin = Engine.netApiManager.Client.IsLogin;
			if (isLogin) {
				var user = Engine.netApiManager.Client.User;
				var world = WorldManager.CreateNewWorld(World.FocusLevel.Focused, false, $"{user.UserName}'s World");
				world.WorldName.Value = $"{user.UserName}'s World";
				Close();
			}
			else {
				var world = WorldManager.CreateNewWorld(World.FocusLevel.Focused, true, "Local World");
				world.WorldName.Value = "New Local World";
				Close();
			}
		}

		public override void LoadUI(Entity uiRoot) {
			var ma = uiRoot.AttachComponent<UIRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UIBuilder(uiRoot, mit, ma,true);
			uiBuilder.PushRect();
			var button = uiBuilder.AddButton();
			button.ButtonEvent.Target = CreateNewWorld;
			uiBuilder.PopRect();
		}
	}
}
