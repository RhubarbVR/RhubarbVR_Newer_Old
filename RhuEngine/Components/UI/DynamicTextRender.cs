using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	public class DynamicTextRender
	{

		public SafeList<TextChar> Chars = new();

		public AxisAlignedBox3f axisAlignedBox3F = AxisAlignedBox3f.Zero;

		public class TextChar
		{
			public Vector2f textsize = Vector2f.Zero;
			public string id;
			public char c;
			public Matrix Offset = Matrix.Identity;
			public Matrix Offset2 = Matrix.Identity;
			public Matrix p;
			public Colorf color;
			public RFont rFont;
			public FontStyle fontStyle;
			public Vector2f textCut;
			public bool Cull = false;
			public TextChar(string id, char c, Matrix p, Colorf color, RFont rFont, FontStyle fontStyle, Vector2f textCut, Vector2f textsize) {
				this.id = id;
				this.c = c;
				this.p = p;
				this.color = color;
				this.rFont = rFont;
				this.fontStyle = fontStyle;
				this.textCut = textCut;
				this.textsize = textsize;
			}

			public void Render(Matrix root) {
				if (!Cull) {
					RText.Add(id, c, Offset2 * p * root, color, rFont, fontStyle, textCut);
				}
			}
		}

		public void Render(Matrix offset,Matrix root) {
			Chars.SafeOperation((list) => {
				foreach (var item in list) {
					item.Render(offset * item.Offset * root);
				}
			});
		}
	
		public void LoadText(string Id ,string Text, RFont Font,float leading, Colorf StartingColor, FontStyle StartingStyle = FontStyle.Regular,float StatingSize = 10f, EVerticalAlien verticalAlien = EVerticalAlien.Center,EHorizontalAlien horizontalAlien= EHorizontalAlien.Middle,bool middleLines = true) {
			if(Font is null) {
				return;
			}
			if (string.IsNullOrEmpty(Text)) {
				Chars.SafeOperation((list) => list.Clear());
				axisAlignedBox3F = AxisAlignedBox3f.Zero;
				return;
			}
			var bounds = new List<Vector3f>();
			Chars.SafeOperation((list) => list.Clear());
			var textXpos = 0f;
			var textsizeY = 0f;
			var textYpos = 0f;
			var textPosZ = 0;
			var style = new Stack<FontStyle>();
			style.Push(StartingStyle);
			var leaded = new Stack<float>();
			leaded.Push(leading);
			var fontSize = new Stack<float>();
			fontSize.Push(StatingSize);
			var color = new Stack<Colorf>();
			var thisrow = new Stack<TextChar>();
			color.Push(StartingColor);
			var index = 0;
			void RenderText(string text) {
				foreach (var item in text) {
					var textsize = RText.Size(Font, item,style.Peek());
					if (item == '\n') {
						if(textsizeY == 0) {
							textsizeY = 1 * (fontSize.Peek() / 100);
						}
						textPosZ++;
						textYpos -= textsizeY + (leaded.Peek() / 10);
						textXpos = 0;
						thisrow.Clear();
						var charee = new TextChar(Id + item + index.ToString(), item, Matrix.TRS(new Vector3f(textXpos, textYpos - textsizeY, 0), Quaternionf.Yawed180, fontSize.Peek() / 100), color.Peek(), Font, style.Peek(), Vector2f.Zero,Vector2f.Zero);
						Chars.SafeAdd(charee);
						continue;
					}
					var newsize = Math.Max(textsize.y * (fontSize.Peek() / 100), textsizeY);
					if (newsize != textsizeY) {
						var def = (textsize.y * (fontSize.Peek() / 100)) - textsizeY;
						textsizeY = newsize;
						foreach (var charitem in thisrow) {
							var ewe = charitem.textsize;
							bounds.Remove(charitem.p.Translation);
							bounds.Remove(charitem.p.Translation + ewe.X__);
							bounds.Remove(charitem.p.Translation + ewe.XY_);
							bounds.Remove(charitem.p.Translation + ewe._Y_);
							var old = charitem.p.Translation;
							charitem.p.Translation = new Vector3f(old.x, old.y - def, old.z);
							bounds.Add(charitem.p.Translation);
							bounds.Add(charitem.p.Translation + ewe.X__);
							bounds.Add(charitem.p.Translation + ewe.XY_);
							bounds.Add(charitem.p.Translation + ewe._Y_);
						}
					}
					var textpos = new Vector3f(textXpos, textYpos - textsizeY, textPosZ);
					var ew = new Vector2f((textsize.x + 0.01f) * (fontSize.Peek() / 100), textsize.y * (fontSize.Peek() / 100));
					var chare = new TextChar(Id + item + index.ToString(), item, Matrix.TRS(textpos, Quaternionf.Yawed180, fontSize.Peek() / 100), color.Peek(), Font, style.Peek(), Vector2f.Zero,ew);
					Chars.SafeAdd(chare);
					thisrow.Push(chare);
					bounds.Add(textpos);
					bounds.Add(textpos + ew.X__);
					bounds.Add(textpos + ew.XY_);
					bounds.Add(textpos + ew._Y_);
					textXpos += (textsize.x + 0.05f) * (fontSize.Peek() / 100);
					index++;
				}
			}
			void Loop(string segment) {
				var first = segment.IndexOf('<');
				if(first <= -1) {
					RenderText(segment);
					return;
				}
				var check = segment.IndexOf('<',first + 1);
				var last = segment.IndexOf('>',first );
				if (last <= -1) {
					RenderText(segment);
					return;
				}
				if (check == -1) {
					check = int.MaxValue;
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
				else if (smartCommand.StartsWith("<style")) {
					var data = smartCommand.Substring(6 + (haseq ? 1 : 0));
					if(Enum.TryParse(data,true,out FontStyle fontStyle)) {
						style.Push(fontStyle);
					}
				}
				else if (smartCommand.StartsWith("<size")) {
					var data = smartCommand.Substring(5 + (haseq ? 1 : 0));
					try {
						fontSize.Push(float.Parse(data));
					}
					catch { }
				}
				else if (smartCommand.StartsWith("<leading")) {
					var data = smartCommand.Substring(8 + (haseq ? 1 : 0));
					try {
						leaded.Push(float.Parse(data));
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
				else if (smartCommand.StartsWith("</leading") || smartCommand.StartsWith("<\\size")) {
					try {
						if (leaded.Count != 1) {
							leaded.Pop();
						}
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</style") || smartCommand.StartsWith("<\\style")) {
					try {
						if (style.Count != 1) {
							style.Pop();
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
			Loop(Text);
			axisAlignedBox3F = BoundsUtil.Bounds(bounds);
			if (horizontalAlien == EHorizontalAlien.Right || middleLines) {
				Chars.SafeOperation((list) => {
					var thisrow = new Stack<TextChar>();
					if (list.Count == 0) {
						return;
					}
					var offset = 0f;
					foreach (var item in list) {
						if (item.c == '\n') {
							if (thisrow.Count != 0) {
								foreach (var element in thisrow) {
									var old = element.p.Translation;
									element.p.Translation = !middleLines ? new Vector3f(old.x + offset, old.y, old.z) : new Vector3f(old.x + (offset / 2), old.y, old.z);
								}
							}
							thisrow.Clear();
							offset = 0f;
							continue;
						}
						offset = axisAlignedBox3F.Width - item.p.Translation.x;
						thisrow.Push(item);
					}
					if (thisrow.Count != 0) {
						foreach (var element in thisrow) {
							var old = element.p.Translation;
							element.p.Translation = !middleLines ? new Vector3f(old.x + offset, old.y, old.z) : new Vector3f(old.x + (offset / 2), old.y, old.z);
						}
					}
					thisrow.Clear();
					offset = 0f;
				});
			}
		}
	}
}
