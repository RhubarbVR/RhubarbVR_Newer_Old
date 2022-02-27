using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UISurfaceGroup : UIGroup
	{
		public Sync<Pose> Pose;
		public Sync<Vec3> LayoutStart;
		public Sync<Vec2> LayoutDimensions;

		public override void RenderUI() {
			UI.PushSurface(Pose, LayoutStart, LayoutDimensions);
			base.RenderUI();
			UI.PopSurface();
		}
	}
}
