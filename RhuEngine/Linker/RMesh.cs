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

		public void Draw(string id,object mesh, RMaterial loadingLogo, Matrix p,Colorf tint);

		public RMesh Quad { get; }

	}

	public class RMesh
	{
		public static IRMesh Instance { get; set; }

		public static RMesh Quad => Instance.Quad;

		public object mesh;

		public IMesh LoadedMesh { get; private set; }

		public RMesh(object e) {
			mesh = e;
		}

		public RMesh(IMesh mesh) {
			LoadMesh(mesh);
		}

		public void LoadMesh(IMesh mesh) {
			LoadedMesh = mesh;
			Instance.LoadMesh(this,mesh);
		}

		public void Draw(string id,RMaterial loadingLogo, Matrix p,Colorf? tint = null) {
			Instance.Draw(id,mesh, loadingLogo, p,tint?? Colorf.White);
		}
	}
}
