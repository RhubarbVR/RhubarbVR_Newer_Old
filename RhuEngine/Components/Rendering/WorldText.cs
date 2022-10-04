using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using System;

namespace RhuEngine.Components
{

	[NotLinkedRenderingComponent]
	[Category(new string[] { "Rendering" })]
	public sealed class WorldText : LinkedWorldComponent
	{
		[Default(1f)]
		public readonly Sync<float> Size;
		[Default("Text Here")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> Text;
		[OnAssetLoaded(nameof(UpdateFont))]
		public readonly AssetRef<RFont> Font;

		public readonly Sync<Colorf> StartingColor;

		[Default(RenderLayer.Text)]
		public readonly Sync<RenderLayer> TargetRenderLayer;

		private ITextMaterial _textMaterial;

		private RText _rText;


		private void UpdateFont() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(() => {
				_rText?.Dispose();
				_rText = null;
				if (Font.Asset is null) {
					return;
				}
				_rText = new RText(Font.Asset);
				if (_textMaterial is null) {
					return;
				}
				_textMaterial.Texture = _rText.texture2D;
				_rText.Text = Text.Value;
			});
		}
		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (_rText is null) {
				return;
			}
			_rText.Text = Text.Value;
		}

		public override void Dispose() {
			base.Dispose();
			_rText?.Dispose();
			_textMaterial?.Dispose();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_textMaterial = StaticMaterialManager.GetMaterial<ITextMaterial>();
			if (_rText is not null) {
				_textMaterial.Texture = _rText.texture2D;
			}
			UpdateFont();
		}

		protected override void Render() {
			if (_rText is null) {
				return;
			}
			if (_textMaterial is null) {
				return;
			}
			RMesh.Quad.Draw(_textMaterial.Material, Matrix.R(Quaternionf.Yawed180) * Matrix.S(new Vector3f(Size * _rText.AspectRatio, Size, Size)) * Entity.GlobalTrans, null, 0, RenderLayer.Text);
		}

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
		}
	}
}
