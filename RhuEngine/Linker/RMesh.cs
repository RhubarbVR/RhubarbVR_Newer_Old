using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRMesh
	{
		public void LoadMesh(RMesh meshtarget, IMesh mesh);

		public void Draw(string id,object mesh, RMaterial loadingLogo, Matrix p,Colorf tint, int zDepth, RenderLayer layer);

		public RMesh Quad { get; }

	}

	public class RMesh
	{
		public AxisAlignedBox3f BoundingBox { get; private set; }

		public bool Dynamic { get; private set; }

		public static IRMesh Instance { get; set; }

		public static RMesh Quad => Instance.Quad;

		public object mesh;

		public IMesh LoadedMesh { get; private set; }

		public RMesh(object e, bool dynamic) {
			mesh = e;
			Dynamic = dynamic;
		}

		public RMesh(IMesh mesh, bool dynamic) {
			Dynamic = dynamic;
			LoadMesh(mesh);
		}

		public void LoadMesh(IMesh mesh) {
			LoadedMesh = mesh;
			Instance.LoadMesh(this,mesh);
			if (!Dynamic) {
				BoundingBox = BoundsUtil.Bounds(mesh.VertexIndices(), (x) => (Vector3f)mesh.GetVertex(x));
			}
		}

		public void Draw(string id,RMaterial loadingLogo, Matrix p,Colorf? tint = null, int zDepth = 0,RenderLayer layer = RenderLayer.UI) {
			Instance.Draw(id,mesh, loadingLogo, p,tint?? Colorf.White, zDepth, layer);
		}
	}
}
