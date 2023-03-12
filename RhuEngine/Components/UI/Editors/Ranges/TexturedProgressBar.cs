using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RTexturedProgressBarFillMode {
		Left_To_Right,
		Right_To_Left,
		Top_To_Bottom,
		Bottom_To_Top,
		ClockWise,
		Counter_ClockWise,
		Bilinear_Left_And_Right,
		Bilinear_Top_And_Bottom,
		Counter_ClockWise_And_Counter_ClockWise,
	}

	[Category("UI/Editors/Ranges")]
	public partial class TexturedProgressBar : Range
	{
		public readonly Sync<RTexturedProgressBarFillMode> FillMode;
		public readonly Sync<bool> NinePatchStrech;
		public readonly Sync<Vector2i> MaxStrech;
		public readonly Sync<Vector2i> MinStrech;
		public readonly AssetRef<RTexture2D> Under;
		public readonly Sync<Colorf> Under_Tint;
		public readonly AssetRef<RTexture2D> Over;
		public readonly Sync<Colorf> Over_Tint;
		public readonly AssetRef<RTexture2D> Progress;
		public readonly Sync<Colorf> Progress_Tint;
		public readonly Sync<Vector2i> ProgressOffset;
		public readonly Sync<float> InitialAngle;
		[Default(360.0f)]public readonly Sync<float> FillDegrees;
		public readonly Sync<Vector2f> CenterOffset;

	}
}
