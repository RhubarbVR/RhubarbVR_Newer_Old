using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Shaders" })]
	public class UnlitClipShader : AssetProvider<RShader>
	{
		RShader _shader;
		private void LoadShader() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_shader = RShader.UnlitClip;
			Load(_shader);
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadShader();
		}
	}
}
