using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class DynamicMesh : ProceduralMesh
	{
		[OnChanged(nameof(UpdateMeshData))]
		public SyncValueList<Vertex> verts;

		[OnChanged(nameof(UpdateMeshData))]
		public SyncValueList<uint> inds;

		private void UpdateMeshData() {
			if(_mesh is not null) {
				_mesh.SetVerts(verts);
				_mesh.SetInds(inds);
			}
		}

		Mesh _mesh;
		private void LoadMesh() {
			_mesh = new Mesh();
			Load(_mesh);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
