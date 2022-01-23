using RhuEngine.WorldObjects.ECS;

using StereoKit;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Shaders" })]
	public class PBRClipShader : AssetProvider<Shader>
	{
		Shader _shader;
		private void LoadShader() {
			_shader = Shader.PBRClip;
			Load(_shader);
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadShader();
		}
	}
}
