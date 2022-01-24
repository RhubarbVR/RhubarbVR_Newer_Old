using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Rendering" })]
	public class LineRender : RenderingComponent
	{
		public class LineElement : SyncObject
		{
			public Sync<Vec3> start;
			public Sync<Vec3> end;
			public Sync<Color32> colorStart;
			public Sync<Color32> colorEnd;
			[Default(0.1f)]
			public Sync<float> thickness;

			public override void FirstCreation() {
				base.FirstCreation();
				end.Value = Vec3.One;
				colorStart.Value = Color32.White;
				colorEnd.Value = Color32.White;
			}
		}
		public SyncObjList<LineElement> lineElements;

		public override void Render() {
			Hierarchy.Push(Entity.GlobalTrans);
			foreach (var item in lineElements) {
				Lines.Add(((LineElement)item).start, ((LineElement)item).end, ((LineElement)item).colorStart, ((LineElement)item).colorEnd, ((LineElement)item).thickness);
			}
			Hierarchy.Pop();
		}
	}
}
