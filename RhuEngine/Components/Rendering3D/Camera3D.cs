using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{
	public enum RProjectionMode
	{
		Perspective,
		Orthongonal,
		Frustum,
	}
	public enum RDopplerEffect {
		Disabled,
		Idle,
		Physics,
	}
	[Category(new string[] { "Rendering3D" })]
	public class Camera3D : LinkedWorldComponent
	{
		public readonly Sync<bool> KeepWidth;
		[Default(RenderLayer.MainCam)]
		public readonly Sync<RenderLayer> RenderMask;
		public readonly Sync<float> HOffset;
		public readonly Sync<float> VOffset;
		public readonly Sync<RDopplerEffect> DopplerTracking;
		[Default(true)]
		public readonly Sync<bool> Current;
		[Default(0.05f)]
		public readonly Sync<float> Near;
		[Default(4000.0f)]
		public readonly Sync<float> Far;
		[Default(RProjectionMode.Perspective)]
		public readonly Sync<RProjectionMode> Projection;
		[Default(75.0f)]
		public readonly Sync<float> Perspective_Fov;
		[Default(1.0f)]
		public readonly Sync<float> Orthongonal_Frustum_Size;
		public readonly Sync<Vector2f> Frustum_Offset;
	}
}
