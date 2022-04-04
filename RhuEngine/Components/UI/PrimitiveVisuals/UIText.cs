using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/PrimitiveVisuals" })]
	public class UIText : UIComponent
	{
		[Default("<color=red>Hell>o<size5><color=blue> W<>orld<size10><color=red>!!<!</color></color></color></color></color>")]
		public Sync<string> Text;

		public AssetRef<RFont> Font;

		public Sync<Colorf> StartingColor;

		[Default(10f)]
		public Sync<float> StatingSize;

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<DefaultFont>();
		}

		public override void Render(Matrix matrix) {
			if(Font.Asset is null) {
				return;
			}
			var rootmat = Matrix.T(new Vector3f(0,0,Rect.StartPoint+0.01f)) * matrix;
			var size = 0f;
			var fontSize = new Stack<float>();
			fontSize.Push(StatingSize.Value);
			var color = new Stack<Colorf>();
			color.Push(StartingColor.Value);
			void RenderText(string text) {
				foreach (var item in text) {
					var textsize = RText.Size(Font.Asset, item);
					RText.Add(Pointer.ToString(), item, Matrix.TRS(new Vector3f(size, 0, 0), Quaternionf.Yawed180, fontSize.Peek()/100) * rootmat, color.Peek(), Font.Asset, textsize);
					size += (textsize.x + 0.01f) * (fontSize.Peek() / 100);
				}
			}
			void Loop(string segment) {
				var first = segment.IndexOf('<');
				if(first <= -1) {
					RenderText(segment);
					return;
				}
				var check = segment.IndexOf('<',first + 1);
				var last = segment.IndexOf('>',first);
				if (last <= -1) {
					RenderText(segment);
					return;
				}
				if(last > check) {
					RenderText(segment.Substring(first,check - first));
					return;
				}
				var command = segment.Substring(first, last - first).ToLower();
				var smartCommand = new string(command.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
				var haseq = smartCommand.Contains('=');
				if (smartCommand.StartsWith("<color")) {
					var data = smartCommand.Substring(6 + (haseq ? 1 : 0));
					color.Push(Colorf.Parse(data));
				}
				else if (smartCommand.StartsWith("<size")) {
					var data = smartCommand.Substring(5 + (haseq ? 1 : 0));
					try {
						fontSize.Push(float.Parse(data));
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</color") || smartCommand.StartsWith("<\\color")) {
					try {
						if (color.Count != 1) {
							color.Pop();
						}
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</size") || smartCommand.StartsWith("<\\size")) {
					try {
						if (fontSize.Count != 1) {
							fontSize.Pop();
						}
					}
					catch { }
				}
				else {
					RenderText(command + segment.Substring(last,1));
				}
				var end = segment.IndexOf('<',last);
				if (end <= -1) {
					RenderText(segment.Substring(last+1));
					return;
				}
				RenderText(segment.Substring(last + 1, end - last - 1));
				Loop(segment.Substring(end));
			}
			Loop(Text.Value);
		}
	}
}
