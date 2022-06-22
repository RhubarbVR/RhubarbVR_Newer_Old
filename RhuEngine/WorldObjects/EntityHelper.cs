using RhuEngine.Components;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.WorldObjects
{
	public static class EntityHelper
	{
		public static (T, S) AttachMesh<T, S>(this Entity entity) where T : AssetProvider<RMesh>, new() where S : AssetProvider<RMaterial>, new() {
			var meshRender = entity.AttachComponent<MeshRender>();
			var material = entity.AttachComponent<S>();
			meshRender.materials.Add().Target = material;
			var mesh = entity.AttachComponent<T>();
			meshRender.mesh.Target = mesh;
			return (mesh, material);
		}

		public static (T, S, MeshRender) AttachMeshWithMeshRender<T, S>(this Entity entity) where T : AssetProvider<RMesh>, new() where S : AssetProvider<RMaterial>, new() {
			var meshRender = entity.AttachComponent<MeshRender>();
			var material = entity.AttachComponent<S>();
			meshRender.materials.Add().Target = material;
			var mesh = entity.AttachComponent<T>();
			meshRender.mesh.Target = mesh;
			return (mesh, material, meshRender);
		}
	}
}
