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

		public float Width => axisAlignedBox3F.Width + 0.05f;

		public float Height => axisAlignedBox3F.Height + 0.05f;

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
			public RenderFont rFont;
			public Vector2f textCut;
			public bool Cull = false;
			public float Leading = 0f;
			public bool NullChar = false;
			public TextChar(string id, char c, Matrix p, Colorf color, RenderFont rFont, Vector2f textCut, Vector2f textsize,float Leading) {
				this.id = id;
				this.c = c;
				this.p = p;
				this.color = color;
				this.rFont = rFont;
				this.textCut = textCut;
				this.textsize = textsize;
				this.Leading = Leading;
			}

			public void Render(Matrix root, Action<Matrix, TextChar, int> action, int index) {
				if (NullChar) {
					action?.Invoke(Offset2 * p * root, this, index);
					return;
				}
				if (!Cull) {
					RText.Add(id, c, Offset2 * p * root, color, rFont, textCut);
				}
				action?.Invoke(Offset2 * p * root, this, index);
			}
		}


		public void Render(Matrix offset,Matrix root,Action<Matrix,TextChar,int> action = null) {
			Chars.SafeOperation((list) => {
				var index = 0;
				foreach (var item in list) {
					item?.Render(offset * item.Offset * root, action, index);
					index++;
				}
			});
		}

#pragma warning disable IDE0060 // Remove unused parameter 
		public void LoadText(string Id ,string Text, RFont Font,float leading, Colorf StartingColor, FontStyle StartingStyle = FontStyle.Regular,float StatingSize = 10f, EVerticalAlien verticalAlien = EVerticalAlien.Center,EHorizontalAlien horizontalAlien= EHorizontalAlien.Middle,bool middleLines = true) {
			if(Font is null) {
				return;
			}
			var bounds = new List<Vector3f>();
			Chars.SafeOperation((list) => list.Clear());
			if (string.IsNullOrEmpty(Text)) {
				Text = " ";
			}
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
			void AddNullText(int first) {
				for (var i = 0; i < first; i++) {
					var textpos = new Vector3f(textXpos, textYpos - textsizeY, 0);
					var chare = new TextChar(Id + "null" + index.ToString(), '\0', Matrix.TRS(textpos, Quaternionf.Yawed180, fontSize.Peek() / 100), color.Peek(), null, Vector2f.Zero, Vector2f.One, leaded.Peek() / 10) { 
						NullChar = true
					};
					Chars.SafeAdd(chare);
				}
			}
			void RenderText(string text) {
				foreach (var item in text) {
					var textsize = FontManager.Size(Font, item,style.Peek());
					if (item == '\n') {
						if(textsizeY == 0) {
							textsizeY = 1 * (fontSize.Peek() / 100);
						}
						textPosZ++;
						textYpos -= textsizeY + (leaded.Peek() / 10);
						textXpos = 0;
						thisrow.Clear();
						var charee = new TextChar(Id + item + index.ToString(), item, Matrix.TRS(new Vector3f(textXpos, textYpos - textsizeY, 0), Quaternionf.Yawed180, fontSize.Peek() / 100), color.Peek(), FontManager.GetFont(Font,item,style.Peek()),  Vector2f.Zero,Vector2f.Zero,leaded.Peek()/10);
						Chars.SafeAdd(charee);
						continue;
					}
					var newsize = Math.Max(textsize.y * (fontSize.Peek() / 100), textsizeY);
					if (newsize != textsizeY) {
						var def = (textsize.y * (fontSize.Peek() / 100)) - textsizeY;
						textsizeY = newsize;
						foreach (var charitem in thisrow) {
							var ewe = charitem.textsize;
							bounds.Remove(charitem.p.Translation - new Vector3f(0, charitem.Leading, 0));
							bounds.Remove(charitem.p.Translation + ewe.X__ - new Vector3f(0, charitem.Leading, 0));
							bounds.Remove(charitem.p.Translation + ewe.XY_);
							bounds.Remove(charitem.p.Translation + ewe._Y_);
							var old = charitem.p.Translation;
							charitem.p.Translation = new Vector3f(old.x, old.y - def, old.z);
							bounds.Add(charitem.p.Translation - new Vector3f(0, charitem.Leading, 0));
							bounds.Add(charitem.p.Translation + ewe.X__ - new Vector3f(0, charitem.Leading, 0));
							bounds.Add(charitem.p.Translation + ewe.XY_);
							bounds.Add(charitem.p.Translation + ewe._Y_);
						}
					}
					var textpos = new Vector3f(textXpos, textYpos - textsizeY, 0);
					var ew = new Vector2f((textsize.x + 0.01f) * (fontSize.Peek() / 100), textsize.y * (fontSize.Peek() / 100));
					var chare = new TextChar(Id + item + index.ToString(), item, Matrix.TRS(textpos, Quaternionf.Yawed180, fontSize.Peek() / 100), color.Peek(), FontManager.GetFont(Font, item, style.Peek()),  Vector2f.Zero,ew, leaded.Peek() / 10);
					Chars.SafeAdd(chare);
					thisrow.Push(chare);
					bounds.Add(textpos - new Vector3f(0,chare.Leading,0));
					bounds.Add(textpos + ew.X__ - new Vector3f(0, chare.Leading, 0));
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
				else if (first != 0) {
					RenderText(segment.Substring(0, first));
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
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(6 + (haseq ? 1 : 0));
					color.Push(Colorf.Parse(data));
				}
				else if(smartCommand.StartsWith("<colour")) {
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(7 + (haseq ? 1 : 0));
					color.Push(Colorf.Parse(data));
				}
				else if (smartCommand.StartsWith("<style")) {
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(6 + (haseq ? 1 : 0));
					if(Enum.TryParse(data,true,out FontStyle fontStyle)) {
						style.Push(fontStyle);
					}
				}
				else if (smartCommand.StartsWith("<size")) {
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(5 + (haseq ? 1 : 0));
					try {
						fontSize.Push(float.Parse(data));
					}
					catch { }
				}
				else if (smartCommand.StartsWith("<leading")) {
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(8 + (haseq ? 1 : 0));
					try {
						leaded.Push(float.Parse(data));
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</color") || smartCommand.StartsWith("<\\color")) {
					AddNullText(command.Length + 1);
					try {
						if (color.Count != 1) {
							color.Pop();
						}
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</colour") || smartCommand.StartsWith("<\\color")) {
					AddNullText(command.Length + 1);
					try {
						if (color.Count != 1) {
							color.Pop();
						}
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</size") || smartCommand.StartsWith("<\\size")) {
					AddNullText(command.Length + 1);
					try {
						if (fontSize.Count != 1) {
							fontSize.Pop();
						}
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</leading") || smartCommand.StartsWith("<\\size")) {
					AddNullText(command.Length + 1);
					try {
						if (leaded.Count != 1) {
							leaded.Pop();
						}
					}
					catch { }
				}
				else if (smartCommand.StartsWith("</style") || smartCommand.StartsWith("<\\style")) {
					AddNullText(command.Length + 1);
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
			AddNullText(1);
			Loop(Text);
			AddNullText(1);
			axisAlignedBox3F = BoundsUtil.Bounds(bounds);
			if (horizontalAlien == EHorizontalAlien.Right || middleLines) {
				Chars.SafeOperation((list) => {
					var thisrow = new Stack<TextChar>();
					if (list.Count == 0) {
						return;
					}
					var offset = 0f;
					foreach (var item in list) {
						if(item is null) {
							continue;
						}
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
#pragma warning restore IDE0060 // Remove unused parameter
	}
}
