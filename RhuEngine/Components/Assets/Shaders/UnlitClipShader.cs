using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Shaders" })]
	public class UnlitShader : AssetProvider<RShader>
	{
		RShader _shader;
		private void LoadShader() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_shader = RShader.Unlit;
			Load(_shader);
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadShader();
		}
	}
}
