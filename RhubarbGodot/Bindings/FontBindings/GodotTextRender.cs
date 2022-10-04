using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Esprima.Ast;

using Godot;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine.Linker;

using static System.Net.Mime.MediaTypeNames;
using static Godot.TextServer;

namespace RhubarbVR.Bindings.FontBindings
{
	public class GodotTextRender : IRText
	{
		public SubViewport subViewport;
		public Label textDraw;
		public Font Loaded;
		public void Dispose() {
			textDraw.Free();
			subViewport.Free();
		}

		public const int SIZE = 96;

		public IRTexture2D Init(RText text, RFont font) {
			if (font.Inst is GodotFont gfont) {
				subViewport = new SubViewport {
					RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
					TransparentBg = true,
					Disable3d = true,
					GuiDisableInput = true,
					Msaa2d = Viewport.MSAA.Disabled,
					Scaling3dMode = Viewport.Scaling3DMode.Bilinear,
					SdfScale = Viewport.SDFScale.Scale100Percent,
					
				};
				textDraw = new Label {
					LabelSettings = new LabelSettings {
						Font = gfont.FontFile,
						FontSize = SIZE,
					},
					HorizontalAlignment = HorizontalAlignment.Center,
				};
				Loaded = gfont.FontFile;
				subViewport.AddChild(textDraw);
				EngineRunner._.AddChild(subViewport);
				SetText("Not set");
				return new GodotTexture2D(subViewport.GetTexture());
			}
			else {
				throw new Exception("Failed to load text");
			}
		}

		public void SetText(string text) {
			var size = Loaded.GetMultilineStringSize(text, HorizontalAlignment.Left, -1, SIZE);
			textDraw.Text = text;
			textDraw.CustomMinimumSize = (Vector2i)(size) + new Vector2i(SIZE, SIZE);
			textDraw.Position = new Vector2i(SIZE / 2, SIZE / 2);
			subViewport.Size = (Vector2i)(size) + new Vector2i(SIZE, SIZE);
		}
	}
}
