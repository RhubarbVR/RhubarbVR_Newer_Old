using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;
using RhuEngine.Components.Visuals;

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

	[Category(new string[] { "UI3D/Visuals" })]
	public sealed class UI3DText : RenderUI3DComponent
	{
		[Default(true)]
		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<bool> FitText;

		[Default("Text Here")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> Text;
		[Default("")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> EmptyString;
		[Default("<color=rgb(0.9,0.9,0.9)>null")]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<string> NullString;
		[OnAssetLoaded(nameof(UpdateOtherData))]
		public readonly AssetRef<RFont> Font;
		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<Colorf> StartingColor;
		[Default(0.1f)]
		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<float> Leading;
		[OnChanged(nameof(UpdateOtherData))]
		[Default(RFontStyle.None)]
		public readonly Sync<RFontStyle> StartingStyle;

		[OnChanged(nameof(UpdateOtherData))]
		[Default(10f)]
		public readonly Sync<float> StatingSize;

		[Default(false)]
		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<bool> Password;

		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<Vector2f> MaxClamp;

		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<Vector2f> MinClamp;

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(UpdateOtherData))]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		public Matrix textOffset = Matrix.S(1);

		public override Colorf RenderTint => Colorf.White;

		private ITextMaterial _textMaterial;

		private RText _rText;

		public override RMaterial RenderMaterial => _textMaterial?.Material;

		protected override void OnLoaded() {
			base.OnLoaded();
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_rText = new RText(null);
			_textMaterial = StaticMaterialManager.GetMaterial<ITextMaterial>();
			_textMaterial.Texture = _rText.texture2D;
		}

		private void UpdateOtherData() {
			if (_rText is null) {
				return;
			}
			_rText.Font = Font.Asset;
			ForceUpdate();
		}

		private void UpdateText() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if(_rText is null) {
				return;
			}
			_rText.Text = Text.Value;
		}

		protected override void OnAttach() {
			base.OnAttach();
			Font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
			StartingColor.Value = Colorf.White;
			MinClamp.Value = Vector2f.MinValue;
			MaxClamp.Value = Vector2f.MaxValue;
		}

		protected override void UpdateMesh() {
			UpdateText();
			StandaredBaseMesh = new SimpleMesh();
			var startDepth = new Vector3f(0, 0, Entity.UIRect.CachedDepth);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			Vector3f upleft, upright, downleft, downright = upleft = upright = downleft = depthStart;
			var max = Max;
			var min = Min;
			var boxsize = max - min;
			boxsize /= Math.Max(boxsize.x, boxsize.y);
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			var texture = new Vector2f(_rText.texture2D.Width, _rText.texture2D.Height);
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
			upright += maxoffset.XY_;
			downright += new Vector3f(maxoffset.x, minoffset.y);
			downleft += minoffset.XY_;
			StandaredBaseMesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = downleft });
			StandaredBaseMesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.One }, bHaveC = false, v = downright });
			StandaredBaseMesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upleft });
			StandaredBaseMesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisX }, bHaveC = false, v = upright });
			StandaredBaseMesh.AppendTriangle(0, 1, 2);
			StandaredBaseMesh.AppendTriangle(1, 3, 2);
		}
	}
}
