using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RhuEngine.Linker;
using System;

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

		public void LoadMesh() 
		{
			// TODO Add the funny UI start/end methods here. -dfg
			try {
				ComputeMesh();
			}
			catch (Exception e) {
				#if DEBUG
				RLog.Err(e.ToString());
				#endif
				Load(null);
			}
		}

		public abstract void ComputeMesh();
	}
}
