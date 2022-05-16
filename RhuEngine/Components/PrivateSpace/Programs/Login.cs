using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	public class Login : Program
	{
		public override string ProgramID => "LoginScreen";

		public override Vector2i? Icon => new Vector2i(10,0);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Login";

		public override void LoadUI(Entity uiRoot) {

		}
	}
}
