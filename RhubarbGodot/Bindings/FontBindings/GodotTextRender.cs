using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Esprima.Ast;

using Godot;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Linker;

using RNumerics;

using static System.Net.Mime.MediaTypeNames;
using static Godot.Control;
using static Godot.TextServer;

using Vector2i = Godot.Vector2i;

namespace RhubarbVR.Bindings.FontBindings
{
	public class GodotTextRender : IRText
	{
		public SubViewport subViewport;
		public Label textDraw;

		public void Dispose() {
			textDraw.Free();
			subViewport.Free();
		}

		public bool AutoScale { get; set; }

		public RFont LoadedFont;

		public RFont Font
		{
			get => LoadedFont; set {
				LoadedFont = value;
				textDraw.LabelSettings.Font = ((GodotFont)value?.Inst)?.FontFile;
			}
		}
		public float LineSpacing { get => textDraw.LabelSettings.LineSpacing; set => textDraw.LabelSettings.LineSpacing = value; }
		public int FontSize { get => textDraw.LabelSettings.FontSize; set => textDraw.LabelSettings.FontSize = value; }
		public Colorf FontColor
		{
			get {
				var data = textDraw.LabelSettings.FontColor;
				return new Colorf(data.r, data.g, data.b, data.a);
			}
			set => textDraw.LabelSettings.FontColor = new Color(value.r, value.g, value.b, value.a);
		}
		public int OutlineSize { get => textDraw.LabelSettings.OutlineSize; set => textDraw.LabelSettings.OutlineSize = value; }
		public Colorf OutlineColor
		{
			get {
				var data = textDraw.LabelSettings.OutlineColor;
				return new Colorf(data.r, data.g, data.b, data.a);
			}
			set => textDraw.LabelSettings.OutlineColor = new Color(value.r, value.g, value.b, value.a);
		}
		public int ShadowSize { get => textDraw.LabelSettings.ShadowSize; set => textDraw.LabelSettings.ShadowSize = value; }
		public Colorf ShadowColor
		{
			get {
				var data = textDraw.LabelSettings.ShadowColor;
				return new Colorf(data.r, data.g, data.b, data.a);
			}
			set => textDraw.LabelSettings.ShadowColor = new Color(value.r, value.g, value.b, value.a);
		}
		public Vector2f ShadowOffset
		{
			get {
				var data = textDraw.LabelSettings.ShadowOffset;
				return new Vector2f(data.x, data.y);
			}
			set => textDraw.LabelSettings.ShadowOffset = new Vector2(value.x, value.y);
		}
		public RHorizontalAlignment HorizontalAlignment { get => (RHorizontalAlignment)textDraw.HorizontalAlignment; set { textDraw.HorizontalAlignment = (HorizontalAlignment)value; Update(); } }
		public RVerticalAlignment VerticalAlignment { get => (RVerticalAlignment)textDraw.VerticalAlignment; set { textDraw.VerticalAlignment = (VerticalAlignment)value; Update(); } }
		public RAutowrapMode AutowrapMode
		{
			get => (RAutowrapMode)textDraw.AutowrapMode; set { textDraw.AutowrapMode = (AutowrapMode)value; Update(); }
		}
		public bool ClipText { get => textDraw.ClipText; set { textDraw.ClipText = value; Update(); } }
		public ROverrunBehavior TextOverrunBehavior { get => (ROverrunBehavior)textDraw.TextOverrunBehavior; set { textDraw.TextOverrunBehavior = (OverrunBehavior)value; Update(); } }
		public bool Uppercase
		{
			get => textDraw.Uppercase; set { textDraw.Uppercase = value; Update(); }
		}
		public int LinesSkipped { get => textDraw.LinesSkipped; set { textDraw.LinesSkipped = value; Update(); } }
		public int MaxLinesVisible { get => textDraw.MaxLinesVisible; set { textDraw.MaxLinesVisible = value; Update(); } }
		public int VisibleCharacters { get => textDraw.VisibleCharacters; set { textDraw.VisibleCharacters = value; Update(); } }
		public RVisibleCharactersBehavior VisibleCharactersBehavior { get => (RVisibleCharactersBehavior)textDraw.VisibleCharactersBehavior; set { textDraw.VisibleCharactersBehavior = (VisibleCharactersBehavior)value; Update(); } }
		public float VisibleRatio { get => textDraw.VisibleRatio; set { textDraw.VisibleRatio = value; Update(); } }
		public RTextDirection TextDirection
		{
			get => (RTextDirection)textDraw.TextDirection; set { textDraw.TextDirection = (TextDirection)value; Update(); }
		}
		public string Language
		{
			get => textDraw.Language; set { textDraw.Language = value; Update(); }
		}
		public RStructuredTextParser StructuredTextBidiOverride { get => (RStructuredTextParser)textDraw.StructuredTextBidiOverride; set { textDraw.StructuredTextBidiOverride = (StructuredTextParser)value; Update(); } }



		public string Text
		{
			get => textDraw.Text; set {
				textDraw.Text = value;
				Update();
			}
		}

		private void Update() {
			if(textDraw is null) {
				return;
			}
			if (subViewport is not null) {
				subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
			}
			if (!AutoScale) {
				return;
			}
			if (textDraw.LabelSettings.Font is null) {
				return;
			}
			var SIZE = textDraw.LabelSettings.FontSize;
			var size = textDraw.LabelSettings.Font.GetMultilineStringSize(textDraw.Text, textDraw.HorizontalAlignment, -1, SIZE, textDraw.MaxLinesVisible, TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound, TextServer.JustificationFlag.Kashida | TextServer.JustificationFlag.WordBound);
			textDraw.CustomMinimumSize = (Vector2i)(size) + new Vector2i(SIZE, SIZE);
			textDraw.Position = new Vector2i(SIZE / 2, SIZE / 2);
			subViewport.Size = (Vector2i)(size) + new Vector2i(SIZE, SIZE);
		}

		public RNumerics.Vector2i Size
		{
			get => new RNumerics.Vector2i(subViewport.Size.x, subViewport.Size.y);
			set {
				if (!AutoScale) {
					subViewport.Size = new Godot.Vector2i(value.x, value.y);
					textDraw.CustomMinimumSize = subViewport.Size;
				}
				Update();
			}
		}

		public IRTexture2D Init(RText text) {
			subViewport = new SubViewport {
				RenderTargetUpdateMode = SubViewport.UpdateMode.Once,
				TransparentBg = true,
				Disable3d = true,
				GuiDisableInput = true,
				Msaa2d = Viewport.MSAA.Disabled,
				Scaling3dMode = Viewport.Scaling3DMode.Bilinear,
				SdfScale = Viewport.SDFScale.Scale100Percent,

			};
			textDraw = new Label {
				LabelSettings = new LabelSettings(),
			};
			subViewport.AddChild(textDraw);
			EngineRunner._.AddChild(subViewport);
			return new GodotTexture2D(subViewport.GetTexture());
		}


	}
}
