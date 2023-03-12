using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{
	[Category(new string[] { "Rendering3D" })]
	public sealed partial class Sprite3D : Sprite3DBase
	{
		public readonly AssetRef<RTexture2D> texture;
		[Default(1)]
		public readonly Sync<int> HFrames;
		[Default(1)]
		public readonly Sync<int> VFrames;
		[Default(0)]
		public readonly Sync<int> Frame;
		public readonly Sync<bool> RegionEnabled;
		public readonly Sync<Vector2f> MinRect;
		public readonly Sync<Vector2f> MaxRect;

		public override Vector2f SizeOfElement
		{
			get {
				if(texture.Asset is null) {
					return Vector2f.One;
				}
				var size = new Vector2f(texture.Asset.Width, texture.Asset.Height);
				if (RegionEnabled.Value) {
					size *= MaxRect.Value - MinRect.Value;
				}
				size /= new Vector2f(HFrames, VFrames);
				return size;
			}
		}
	}

	public enum RSprite3DDir
	{
		X,
		Y,
		Z,
	}
	public enum RSprite3DAlphaCut
	{
		Disabled,
		Discard,
		OpaquePrePass,
	}
	[Category(new string[] { "Rendering3D" })]
	public abstract partial class Sprite3DBase : GeometryInstance3D, IWorldBoundingBox
	{
		public abstract Vector2f SizeOfElement { get; }

		[Default(true)]
		public readonly Sync<bool> Centered;
		public readonly Sync<Vector2f> OffsetPos;
		public readonly Sync<bool> FlipH;
		public readonly Sync<bool> FlipV;
		public readonly Sync<Colorf> Moduluate;
		[Default(0.01f)]
		public readonly Sync<float> PixelSize;
		[Default(RSprite3DDir.Z)]
		public readonly Sync<RSprite3DDir> Axis;
		[Default(RBillboardOptions.Disabled)]
		public readonly Sync<RBillboardOptions> Billboard;
		[Default(true)]
		public readonly Sync<bool> Transparent;
		public readonly Sync<bool> Shaded;
		[Default(true)]
		public readonly Sync<bool> DoubleSided;
		public readonly Sync<bool> NoDepthTest;
		public readonly Sync<bool> FixedSize;
		[Default(RSprite3DAlphaCut.Disabled)]
		public readonly Sync<RSprite3DAlphaCut> AlphaMode;
		public readonly Sync<int> RenderPriority;

		public AxisAlignedBox3f Bounds
		{
			get {
				var min = Vector2f.Zero;
				var max = Vector2f.Zero;
				min += OffsetPos.Value;
				max += SizeOfElement;
				max += OffsetPos.Value;
				min *= PixelSize.Value;
				max *= PixelSize.Value;
				if (Centered.Value) {
					max /= 2;
					min -= max;
				}
				return Axis.Value switch {
					RSprite3DDir.X => new(new Vector3f(0, min.x, min.y), new Vector3f(0, max.x, max.y)),
					RSprite3DDir.Y => new(new Vector3f(min.x, 0, min.y), new Vector3f(max.x, 0, max.y)),
					_ => new(new Vector3f(min.x, min.y, 0), new Vector3f(max.x, max.y, 0)),
				};
			}

		}
		protected override void FirstCreation() {
			base.FirstCreation();
			Moduluate.Value = Colorf.White;
		}
	}
}
