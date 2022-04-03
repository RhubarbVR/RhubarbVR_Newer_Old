using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public abstract class ProceduralMesh : AssetProvider<RMesh>
	{
		public RMesh loadedMesh = null;
		public void GenMesh(IMesh mesh) {
			if(loadedMesh == null) {
				loadedMesh = new RMesh(mesh);
				Load(loadedMesh);
			}
			else {
				loadedMesh.LoadMesh(mesh);
			}
		}
	}
}
