using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[AllowedOnWorldRoot]
	public abstract partial class ProceduralMesh : AssetProvider<RMesh>
	{
		public SimpleMesh LoadedSimpleMesh { get; private set; }

		public void GenMesh(IMesh mesh) {
			LoadedSimpleMesh = mesh is SimpleMesh simple ? simple : null;
			if (Value == null) {
				Load(new RMesh(mesh, true));
			}
			else {
				Value.LoadMesh(mesh);
			}
		}

		public void LoadMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(this, () => {
				try {
					if (IsDestroying || IsRemoved) {
						return;
					}
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
