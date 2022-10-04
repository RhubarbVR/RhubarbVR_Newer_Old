using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{

	[NotLinkedRenderingComponent]
	[Category(new string[] { "Rendering" })]
	public sealed class WorldText : LinkedWorldComponent
	{
		[Default(1f)]
		public readonly Sync<float> Size;
		public readonly Sync<Vector2i> SizeVector;
		[Default("Text Here")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> Text;
		[OnAssetLoaded(nameof(UpdatePrams))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<Colorf> FontColor;
		[Default(true)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<bool> AutoScale;
		[Default(3f)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<float> LineSpacing;
		[Default(96)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<int> FontSize;
		[Default(0)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<int> OutlineSize;
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<Colorf> OutlineColor;
		[Default(1)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<int> ShadowSize;
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<Colorf> ShadowColor;
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<Vector2f> ShadowOffset;
		[Default(RHorizontalAlignment.Center)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<RHorizontalAlignment> HorizontalAlignment;
		[Default(RVerticalAlignment.Center)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<RVerticalAlignment> VerticalAlignment;
		[Default(RAutowrapMode.Off)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<RAutowrapMode> AutowrapMode;
		[Default(false)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<bool> ClipText;
		[Default(ROverrunBehavior.NoTrimming)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<ROverrunBehavior> TextOverrunBehavior;
		[Default(false)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<bool> Uppercase;
		[Default(0)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<int> LinesSkipped;
		[Default(-1)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<int> MaxLinesVisible;
		[Default(-1)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<int> VisibleCharacters;
		[Default(RVisibleCharactersBehavior.CharsBeforeShaping)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<RVisibleCharactersBehavior> VisibleCharactersBehavior;
		[Default(1f)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<float> VisibleRatio;
		[Default(RTextDirection.Auto)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<RTextDirection> TextDirection;
		[Default("")]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<string> Language;
		[Default(RStructuredTextParser.Default)]
		[OnChanged(nameof(UpdatePrams))]
		public readonly Sync<RStructuredTextParser> StructuredTextBidiOverride;
		[Default(RenderLayer.Text)]
		public readonly Sync<RenderLayer> TargetRenderLayer;

		private ITextMaterial _textMaterial;

		private RText _rText;

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			ShadowOffset.Value = Vector2f.Zero;
			ShadowColor.Value = Colorf.Black;
			SizeVector.Value = Vector2i.One;
			OutlineColor.Value = Colorf.White;
			FontColor.Value = Colorf.White;
		}

		private void UpdateAll() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			UpdatePramsRaw();
			UpdateText();
		}

		private void UpdatePramsRaw() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (_rText is null) {
				return;
			}
			_rText.Font = Font.Asset;
			_rText.AutoScale = AutoScale;
			_rText.LineSpacing = LineSpacing;
			_rText.FontSize = FontSize;
			_rText.FontColor = FontColor;
			_rText.OutlineSize = OutlineSize;
			_rText.OutlineColor = OutlineColor;
			_rText.ShadowSize = ShadowSize;
			_rText.ShadowColor = ShadowColor;
			_rText.ShadowOffset = ShadowOffset;
			_rText.Size = SizeVector;
			_rText.HorizontalAlignment = HorizontalAlignment;
			_rText.VerticalAlignment = VerticalAlignment;
			_rText.AutowrapMode = AutowrapMode;
			_rText.ClipText = ClipText;
			_rText.TextOverrunBehavior = TextOverrunBehavior;
			_rText.Uppercase = Uppercase;
			_rText.LinesSkipped = LinesSkipped;
			_rText.MaxLinesVisible = MaxLinesVisible;
			_rText.VisibleCharacters = VisibleCharacters;
			_rText.VisibleCharactersBehavior = VisibleCharactersBehavior;
			_rText.VisibleRatio = VisibleRatio;
			_rText.TextDirection = TextDirection;
			_rText.Language = Language;
			_rText.StructuredTextBidiOverride = StructuredTextBidiOverride;
		}

		private void UpdatePrams() {
			RenderThread.ExecuteOnStartOfFrame(this, UpdatePramsRaw);
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
			RenderThread.ExecuteOnStartOfFrame(() => {
				_textMaterial = StaticMaterialManager.GetMaterial<ITextMaterial>();
				_rText = new RText(null);
				_textMaterial.Texture = _rText.texture2D;
				UpdateAll();
			});
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


	}
}
