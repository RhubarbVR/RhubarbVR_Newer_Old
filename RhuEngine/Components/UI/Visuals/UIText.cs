using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;
using static RhuEngine.Components.DynamicTextRender;
using SixLabors.Fonts;

namespace RhuEngine.Components
{
	public enum EVerticalAlien
	{
		Bottom,
		Center,
		Top,
	}
	public enum EHorizontalAlien
	{
		Left,
		Middle,
		Right,
	}

	[Category(new string[] { "UI/Visuals" })]
	public class UIText : UIComponent,ITextComp
	{
		public event Action<Matrix, TextChar,int> OnCharRender;
		public DynamicTextRender TextRender => textRender;

		[Default("<color=hsv(240,100,100)>Hello<color=blue><size14>World \n <size5>Trains \n are cool man<size10>\nHello ")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> Text;
		[Default("")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> EmptyString;
		[Default("<color=rgb(0.9,0.9,0.9)>null")]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<string> NullString;
		[OnAssetLoaded(nameof(UpdateText))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<float> Leading;
		[OnChanged(nameof(UpdateText))]
		[Default(FontStyle.Regular)]
		public readonly Sync<FontStyle> StartingStyle;

		[OnChanged(nameof(UpdateText))]
		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(false)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<bool> Password;

		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Vector2f> MaxClamp;

		[OnChanged(nameof(UpdateText))]
		public readonly Sync<Vector2f> MinClamp;

		public DynamicTextRender textRender = new(true);

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		[Default(true)]
		[OnChanged(nameof(UpdateText))]
		public readonly Sync<bool> MiddleLines;

		public Matrix textOffset = Matrix.S(1);

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			textRender.MaxClamp = MaxClamp;
			textRender.MinClamp = MinClamp;
			var newtext = Text.Value;
			if (Password.Value) {
				newtext = new string('●', newtext.Length);
			}
			if(newtext is null) {
				newtext = NullString.Value;
			}
			if (string.IsNullOrEmpty(newtext)) {
				newtext = EmptyString.Value;
			}
			textRender.LoadText(Pointer.ToString(), newtext, Font.Asset, Leading, StartingColor, StartingStyle, StatingSize, VerticalAlien, HorizontalAlien, MiddleLines);
			UpdateTextOffset();
		}

		public override void OnLoaded() {
			base.OnLoaded();
			textRender.UpdatedMeshses = MeshUpdate;
			UpdateText();
		}

		public void UpdateTextOffset() {
			if (Rect?.Canvas is null) {
				return;
			}
			var startDepth = new Vector3f(0, 0, Entity.UIRect.StartPoint);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			var upleft = depthStart;
			var max = Rect.Max;
			var min = Rect.Min;
			var boxsize = max - min;
			boxsize /= Math.Max(boxsize.x, boxsize.y);
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			var texture = new Vector2f(textRender.Width, textRender.Height) * 10;
			texture /= canvassize;
			texture /= boxsize;
			texture /= Math.Max(texture.x, texture.y);
			var maxmin = (max - min) * texture;
			var maxoffset = maxmin + min;
			var minoffset = min;
			var offset = (max - min - maxmin) / 2;
			if (HorizontalAlien == EHorizontalAlien.Middle) {
				maxoffset = new Vector2f(maxoffset.x + offset.x, maxoffset.y);
				minoffset = new Vector2f(minoffset.x + offset.x, minoffset.y);
			}
			if (VerticalAlien == EVerticalAlien.Center) {
				maxoffset = new Vector2f(maxoffset.x, maxoffset.y + offset.y);
				minoffset = new Vector2f(minoffset.x, minoffset.y + offset.y);
			}
			if (HorizontalAlien == EHorizontalAlien.Right) {
				maxoffset = new Vector2f(max.x, maxoffset.y);
				minoffset = new Vector2f(max.x - maxmin.x, minoffset.y);
			}
			if (VerticalAlien == EVerticalAlien.Top) {
				maxoffset = new Vector2f(maxoffset.x, max.y);
				minoffset = new Vector2f(minoffset.x, max.y - maxmin.y);
			}
			upleft += new Vector3f(minoffset.x, maxoffset.y);
			var size = (max * canvassize) - (min * canvassize);
			var largestscaleside = Math.Max(size.x, size.y);
			var largestestside = Math.Max(textRender.Width, textRender.Height);
			var small = Math.Min(size.y / textRender.Height, size.x / textRender.Width);


			textOffset = Matrix.TS(new Vector3f(upleft.x, upleft.y, Rect.StartPoint + Entity.UIRect.Depth.Value + 0.01f), new Vector3f(small) / Rect.Canvas.scale.Value) * Matrix.T(Rect.ScrollOffset);
			CutElement(true, true);
		}

		public override void RenderTargetChange() {
			UpdateTextOffset();
		}

		public override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			StartingColor.Value = Colorf.White;
			MinClamp.Value = Vector2f.MinValue;
			MaxClamp.Value = Vector2f.MaxValue;
		}

		public override void Render(Matrix matrix) {
			textRender.Render(textOffset,  Matrix.S((Rect.Canvas?.scale.Value ?? Vector3f.One) / 10) * matrix,RenderLayer.Text, OnCharRender);
			for (var i = 0; i < mainMeshes.Length; i++) {
				mainMeshes[i]?.Draw(textRender.ID + i, textRender.renderMits[i], matrix, null,0,RenderLayer.Text);
			}
		}
		private void MeshUpdate() {
			UpdateTextOffset();
		}

		public RMesh[] mainMeshes = Array.Empty<RMesh>();

		public override void CutElement(bool cut, bool update) {
			var meshes = new SimpleMesh[textRender.simprendermeshes.Count];
			for (var i = 0; i < meshes.Length; i++) {
				meshes[i] = new SimpleMesh(TextRender.simprendermeshes[i]);
				if(TextRender.simprendermeshes[i] is null) {
					continue;
				}
				meshes[i].Translate(textOffset);
				if (cut) {
					meshes[i] = meshes[i].Cut(Rect.CutZonesMax, Rect.CutZonesMin);
				}
				if (Rect.Canvas.TopOffset.Value) {
					meshes[i].OffsetTop(Rect.Canvas.TopOffsetValue.Value);
				}
				if (Rect.Canvas.FrontBind.Value) {
					meshes[i] = meshes[i].UIBind(Rect.Canvas.FrontBindAngle.Value, Rect.Canvas.FrontBindRadus.Value, Rect.Canvas.FrontBindSegments.Value, Rect.Canvas.scale);
				}
				meshes[i].Scale(Rect.Canvas.scale.Value.x / 10, Rect.Canvas.scale.Value.y / 10, Rect.Canvas.scale.Value.z / 10);
			}
			if(mainMeshes.Length != meshes.Length) {
				Array.Resize(ref mainMeshes,meshes.Length);
			}
			for (var i = 0; i < meshes.Length; i++) {
				var mesh = mainMeshes[i];
				if(mesh is not null) {
					mesh.LoadMesh(meshes[i]);
				}
				else {
					if(meshes[i].TriangleCount > 0) {
						mainMeshes[i] = new RMesh(meshes[i]);
					}
				}
			}
		}
	}
}
