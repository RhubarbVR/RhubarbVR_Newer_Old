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
		public Matrix Pos { get => Label3D.GetPos(); set => Label3D.SetPos(value); }

		public RFont TargetRhubarbFont;

		public RFont Font
		{
			get => TargetRhubarbFont; set {
				TargetRhubarbFont = value;
				if (TargetRhubarbFont?.Inst is GodotFont font) {
					Label3D.Font = font.FontFile;
				}
			}
		}
		public string Text { get => Label3D.Text; set => Label3D.Text = value; }
		public int FontSize { get => Label3D.FontSize; set => Label3D.FontSize = value; }
		public RHorizontalAlignment HorizontalAlignment { get => (RHorizontalAlignment)Label3D.HorizontalAlignment; set => Label3D.HorizontalAlignment = (HorizontalAlignment)value; }
		public RVerticalAlignment VerticalAlignment { get => (RVerticalAlignment)Label3D.VerticalAlignment; set => Label3D.VerticalAlignment = (VerticalAlignment)value; }

		public void Dispose() {
			Label3D.Free();
		}

		public RText TargetText;
		public Label3D Label3D;
		public void Init(RText text) {
			TargetText = text;
			Label3D = new Label3D();
			EngineRunner._.AddChild(Label3D);
		}
	}
}
