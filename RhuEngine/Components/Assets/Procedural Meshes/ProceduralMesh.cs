using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[AllowedOnWorldRoot]
	public abstract class ProceduralMesh : AssetProvider<RMesh>
	{
		public RMesh loadedMesh = null;
		public SimpleMesh LoadedSimpleMesh { get; private set; }

		public void GenMesh(IMesh mesh) {
			LoadedSimpleMesh = mesh is SimpleMesh simple ? simple : null;
			if (loadedMesh == null) {
				loadedMesh = new RMesh(mesh, false);
				Load(loadedMesh);
			}
			else {
				loadedMesh.LoadMesh(mesh);
			}
		}

		public void LoadMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RUpdateManager.ExecuteOnEndOfFrame(this, () => {
				try {
					ComputeMesh();
				}
				catch (Exception e) {
#if DEBUG
					RLog.Err(e.ToString());
#endif
				}
			});
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}

		public abstract void ComputeMesh();
	}
}
