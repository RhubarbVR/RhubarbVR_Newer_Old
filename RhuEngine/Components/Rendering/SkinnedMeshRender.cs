using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{
	[Category(new string[] { "Rendering" })]
	public class Armature : RenderingComponent
	{
		public readonly SyncObjList<SyncRef<Entity>> ArmatureEntitys;
	}

	[Category(new string[] { "Rendering" })]
	public class SkinnedMeshRender : MeshRender
	{
		public class BlendShape : SyncObject
		{
			[Default("Unknown")]
			public readonly Sync<string> BlendName;

			public readonly Sync<float> Weight;
		}
		public readonly Sync<AxisAlignedBox3f> Bounds;

		public readonly SyncRef<Armature> Armature;

		public readonly SyncObjList<BlendShape> BlendShapes;

		public override void OnAttach() {
			base.OnAttach();
			Bounds.Value = AxisAlignedBox3f.CenterZero;
		}

	}
}
