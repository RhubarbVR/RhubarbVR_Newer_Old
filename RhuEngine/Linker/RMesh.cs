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

		public void Draw(string id,object mesh, RMaterial loadingLogo, Matrix p);

		public RMesh Quad { get; }

	}

	public class RMesh
	{
		public static IRMesh Instance { get; set; }

		public static RMesh Quad => Instance.Quad;

		public object mesh;

		public RMesh(object e) {
			mesh = e;
		}

		public RMesh(IMesh mesh) {
			LoadMesh(mesh);
		}

		public void LoadMesh(IMesh mesh) {
			Instance.LoadMesh(this,mesh);
		}

		public void Draw(string id,RMaterial loadingLogo, Matrix p) {
			Instance.Draw(id,mesh, loadingLogo, p);
		}
	}
}
