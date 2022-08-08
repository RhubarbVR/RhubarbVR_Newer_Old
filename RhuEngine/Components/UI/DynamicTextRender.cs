using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;
using SixLabors.Fonts;

namespace RhuEngine.Components
{
	public class DynamicTextRender
	{
		public Action UpdatedMeshses;
		public DynamicTextRender(bool uiText = false, Action update = null) {
			UpdatedMeshses = update;
			UIText = uiText;
		}

		public SafeList<TextChar> Chars = new();

		public Vector2f MinClamp = Vector2f.MinValue;

		public Vector2f MaxClamp = Vector2f.MaxValue;
		public float Width => MathUtil.Clamp(axisAlignedBox3F.Width + 0.05f, MinClamp.x, MaxClamp.x);

		public float Height => MathUtil.Clamp(axisAlignedBox3F.Height + 0.05f, MinClamp.y, MaxClamp.y);

		public AxisAlignedBox3f axisAlignedBox3F = AxisAlignedBox3f.Zero;

		public class TextChar
		{
			public float fontSize = 0;

			public Vector2f textsize = Vector2f.Zero;
			public Rune c;
			public Matrix p;
			public Colorf color;
			public float Leading = 0f;
			public bool NullChar = false;
			public int mitIndex = 0;
			public TextChar(Rune c, Matrix p, Colorf color, Vector2f textsize, float fontSize, float Leading, int mitIndex) {
				this.fontSize = fontSize;
				this.c = c;
				this.p = p;
				this.color = color;
				this.textsize = textsize;
				this.Leading = Leading;
				this.mitIndex = mitIndex;
			}
			public TextChar(Rune c, Matrix p, Colorf color, Vector2f textsize, float Leading, float fontSize, int mitIndex, Vector2f bottomleft, Vector2f topright) {
				this.fontSize = fontSize;
				this.c = c;
				this.p = Matrix.T(new Vector3f(0, -0.25, 0)) * p;
				this.color = color;
				this.textsize = textsize;
				this.Leading = Leading;
				this.mitIndex = mitIndex;
				Bottomleft = bottomleft;
				Topright = topright;
			}

			public Vector2f Bottomleft { get; }
			public Vector2f Topright { get; }

			public void Render(Matrix root, Action<Matrix, TextChar, int> action, int index) {
				action?.Invoke(Matrix.S(fontSize) * p * root, this, index);
			}
		}

		public List<RMaterial> renderMits = new();
		public List<RMesh> rendermeshes = new();
		public List<SimpleMesh> simprendermeshes = new();

		private void LoadRenderMeshes() {
			for (var i = 0; i < rendermeshes.Count - renderMits.Count; i++) {
				rendermeshes.RemoveAt(0);
				simprendermeshes.RemoveAt(0);
			}
			for (var i = 0; i < renderMits.Count - rendermeshes.Count; i++) {
				rendermeshes.Add(null);
				simprendermeshes.Add(null);
			}
			Chars.SafeOperation((list) => {
				var meshses = new Dictionary<int, SimpleMesh>();
				foreach (var item in list) {
					if (rendermeshes.Count > item.mitIndex) {
						if (item.mitIndex == -1) {
							continue;
						}
						var rmesh = rendermeshes[item.mitIndex];
						if (!meshses.ContainsKey(item.mitIndex)) {
							meshses.Add(item.mitIndex, new SimpleMesh());
						}
						var smesh = meshses[item.mitIndex];
						smesh.AppendUVRectangle(item.p, item.Bottomleft, item.Topright, item.textsize * new Vector2f(-1, 1), item.fontSize, item.color);
					}
				}
				if (!UIText) {
					foreach (var item in meshses) {
						var rmesh = rendermeshes[item.Key];
						if (rmesh is null) {
							rendermeshes[item.Key] = new RMesh(item.Value, true);
						}
						else {
							rmesh.LoadMesh(item.Value);
						}
					}
				}
				else {
					foreach (var item in meshses) {
						simprendermeshes[item.Key] = item.Value;
					}
				}
			});
			UpdatedMeshses?.Invoke();
		}

		public bool UIText = false;

		public void Render(Matrix offset, Matrix root, RenderLayer tar, Action<Matrix, TextChar, int> action = null) {
			Chars.SafeOperation((list) => {
				var index = 0;
				foreach (var item in list) {
					item?.Render(offset * root, action, index);
					index++;
				}
			});
			if (!UIText) {
				for (var i = 0; i < renderMits.Count; i++) {
					rendermeshes[i]?.Draw(renderMits[i], offset * root, null, 0, tar);
				}
			}
		}

		public void LoadText(string Text, RFont Font, float leading, Colorf StartingColor, FontStyle StartingStyle = FontStyle.Regular, float StatingSize = 10f, EHorizontalAlien horizontalAlien = EHorizontalAlien.Middle) {
			if (Font is null) {
				return;
			}
			var middleLines = true;
			if (horizontalAlien == EHorizontalAlien.Middle) {
				middleLines = true;
			}
			renderMits.Clear();
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
			Rune? lastRune = null;
			void AddNullText(int first) {
				for (var i = 0; i < first; i++) {
					var textpos = new Vector3f(textXpos, textYpos - textsizeY, 0);
					var chare = new TextChar(Rune.GetRuneAt("\0", 0), Matrix.TRS(textpos, Quaternionf.Yawed180, 0.01f), color.Peek(), Vector2f.One, fontSize.Peek() / 10, leaded.Peek() / 10, -1) {
						NullChar = true
					};
					Chars.SafeAdd(chare);
				}
			}
			void RenderText(string text) {
				foreach (var item in text.EnumerateRunes()) {
					var textsize = FontManager.Size(Font, item);
					if (item == Rune.GetRuneAt("\n", 0)) {
						if (textsizeY == 0) {
							textsizeY = fontSize.Peek() / 10;
						}
						textPosZ++;
						textYpos -= textsizeY + (leaded.Peek() / 10);
						textXpos = 0;
						thisrow.Clear();
						var charee = new TextChar(item, Matrix.TR(new Vector3f(textXpos, textYpos - textsizeY, 0), Quaternionf.Yawed180), color.Peek(), Vector2f.One, fontSize.Peek() / 10, leaded.Peek() / 10, -1);
						Chars.SafeAdd(charee);
						lastRune = null;
						continue;
					}
					if (lastRune is not null) {
						textXpos += Font.GetXAdvances(lastRune ?? throw new Exception(), item) * (fontSize.Peek() / 10);
					}
					lastRune = item;
					var newsize = Math.Max(textsize.y * (fontSize.Peek() / 10), textsizeY);
					if (newsize > textsizeY) {
						var def = (textsize.y * (fontSize.Peek() / 10)) - textsizeY;
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
					var ew = new Vector2f((textsize.x + 0.01f) * (fontSize.Peek() / 10), textsize.y * (fontSize.Peek() / 10));
					var data = Font.GetGlygh(item);
					if (!renderMits.Contains(data.mit)) {
						renderMits.Add(data.mit);
					}
					var chare = new TextChar(item, Matrix.TR(textpos, Quaternionf.Yawed180), color.Peek(), ew, fontSize.Peek() / 10, leaded.Peek() / 10, renderMits.IndexOf(data.mit), data.bottomleft, data.topright);
					Chars.SafeAdd(chare);
					thisrow.Push(chare);
					bounds.Add(textpos - new Vector3f(0, chare.Leading, 0));
					bounds.Add(textpos + ew.X__ - new Vector3f(0, chare.Leading, 0));
					bounds.Add(textpos + ew.XY_);
					bounds.Add(textpos + ew._Y_);
					index++;
				}
			}
			void Loop(string segment) {
				var first = segment.IndexOf('<');
				if (first <= -1) {
					RenderText(segment);
					return;
				}
				var check = segment.IndexOf('<', first + 1);
				var last = segment.IndexOf('>', first);
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
				if (last > check) {
					RenderText(segment.Substring(first, check - first));
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
				else if (smartCommand.StartsWith("<colour")) {
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(7 + (haseq ? 1 : 0));
					color.Push(Colorf.Parse(data));
				}
				else if (smartCommand.StartsWith("<style")) {
					AddNullText(command.Length + 1);
					var data = smartCommand.Substring(6 + (haseq ? 1 : 0));
					if (Enum.TryParse(data, true, out FontStyle fontStyle)) {
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
				else if (smartCommand.StartsWith("</clearstyle") || smartCommand.StartsWith("<\\clearstyle")) {
					AddNullText(command.Length + 1);
					try {
						for (var i = 0; i < style.Count - 1; i++) {
							style.Pop();
						}
						for (var i = 0; i < fontSize.Count - 1; i++) {
							fontSize.Pop();
						}
						for (var i = 0; i < color.Count - 1; i++) {
							color.Pop();
						}
						for (var i = 0; i < leaded.Count - 1; i++) {
							leaded.Pop();
						}
					}
					catch { }
				}
				else {
					RenderText(command + segment.Substring(last, 1));
				}
				var end = segment.IndexOf('<', last);
				if (end <= -1) {
					RenderText(segment.Substring(last + 1));
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
						if (item is null) {
							continue;
						}
						if (item.c == Rune.GetRuneAt("\n", 0)) {
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
			LoadRenderMeshes();
		}
	}
}
