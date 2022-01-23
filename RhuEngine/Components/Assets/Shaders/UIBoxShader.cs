using RhuEngine.WorldObjects.ECS;

using StereoKit;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Shaders" })]
	public class UIBoxShader : AssetProvider<Shader>
	{
		Shader _shader;
		private void LoadShader() {
			_shader = Shader.UIBox;
			Load(_shader);
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadShader();
		}
	}
}
