using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class TriangleMesh : ProceduralMesh
	{
		public readonly SyncObjList<Triangle> listOfTris;
		public override void OnAttach() {
			base.OnAttach();
			var tri = listOfTris.Add();
			tri.a.ver.Value = new Vector3d(1, 0, 0);
			tri.b.ver.Value = new Vector3d(0.5, 1, 0);
			tri.c.ver.Value = new Vector3d(0, 0, 0);

			tri.a.uv.Add(new Vector2f(0, 0));
			tri.b.uv.Add(new Vector2f(0.5, 1));
			tri.c.uv.Add(new Vector2f(1, 0));

			// Make a UV Triangle by adding a rend.colorLinear.Value = Colorf.Orange -dfg
			tri.a.color.Value = Colorf.Blue;
			tri.b.color.Value = Colorf.Green;
			tri.c.color.Value = Colorf.Red;

			// Make a UV Triangle
			// tri.a.color.Value = Colorf.White;
			// tri.b.color.Value = Colorf.White;
			// tri.c.color.Value = Colorf.White;
		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new SimpleMesh();
			foreach (Triangle tri in listOfTris) {
				mesh.AppendTriangle(tri.a.VertexInfo, tri.b.VertexInfo, tri.c.VertexInfo);
			}
			GenMesh(mesh);
		}

		public class Triangle : SyncObject
		{
			public readonly Vertex a;
			public readonly Vertex b;
			public readonly Vertex c;

			public void UpdateMesh()
			{
				if (Parent.Parent is TriangleMesh mesh) {
					mesh.LoadMesh();
				}
			}
		}

		public class Vertex : SyncObject
		{
			[OnChanged(nameof(UpdateMesh))]
			public readonly Sync<Vector3d> ver;

			[OnChanged(nameof(UpdateMesh))]
			public readonly Sync<Vector3f> norm;

			[OnChanged(nameof(UpdateMesh))]
			public readonly Sync<Colorf> color;

			[OnChanged(nameof(UpdateMesh))]
			public readonly SyncValueList<Vector2f> uv;

			private void UpdateMesh() 
			{
				if (Parent is Triangle mesh) 
				{
					mesh.UpdateMesh();
				}
			}

			public NewVertexInfo VertexInfo => new NewVertexInfo(ver, norm, color.Value.ToRGB(), uv);
		}

	}
}
