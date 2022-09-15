using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRMesh
	{
		public void LoadMeshData(IMesh mesh);
		public void LoadMeshToRender();
		public void Init(RMesh rMesh);
		public void Draw(RMaterial loadingLogo, Matrix p, Colorf tint, int zDepth, RenderLayer layer, int submesh);
	}

	public class RMesh
	{
		public AxisAlignedBox3f BoundingBox { get; private set; }

		public bool Dynamic { get; private set; }

		public static Type Instance { get; set; }

		public static RMesh Quad { get; set; }

		public IMesh LoadedMesh { get; private set; }

		public IRMesh Inst { get; private set; }

		public RMesh(IRMesh rMesh, bool dynamic) {
			Dynamic = dynamic;
			Inst = rMesh ?? (IRMesh)Activator.CreateInstance(Instance);
			Inst.Init(this);
		}

		public RMesh(IMesh mesh, bool dynamic) : this((IRMesh)null, dynamic) {
			LoadMesh(mesh);
		}

		public void LoadMesh(IMesh mesh) {
			LoadedMesh = mesh;
			Inst.LoadMeshData(mesh);
			if (!Dynamic) {
				BoundingBox = BoundsUtil.Bounds(mesh.VertexIndices(), (x) => (Vector3f)mesh.GetVertex(x));
			}
			RenderThread.ExecuteOnStartOfFrame(Inst.LoadMeshToRender);
		}

		public void Draw(RMaterial loadingLogo, Matrix p, Colorf? tint = null, int zDepth = 0, RenderLayer layer = RenderLayer.UI,int subMesh = -1) {
			Inst.Draw(loadingLogo, p, tint ?? Colorf.White, zDepth, layer, subMesh);
		}
	}
}
