using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine;
using RhuEngine.Linker;
using RNumerics;
using RhuEngine.WorldObjects;
using System.Numerics;
using System.Linq;

namespace RStereoKit
{

	public class SKRMesh : IRMesh
	{
		public RMesh Quad => new RMesh(Mesh.Quad);

		public void Draw(string id, object mesh, RMaterial loadingLogo, RNumerics.Matrix p,Colorf colorf) {
			((Mesh)mesh).Draw((Material)loadingLogo?.Target, new StereoKit.Matrix(p.m),new Color(colorf.r, colorf.g, colorf.b, colorf.a));
		}

		public void LoadMesh(RMesh meshtarget, IMesh mesh) {
			if (meshtarget.mesh == null) {
				meshtarget.mesh = new Mesh();
				((Mesh)meshtarget.mesh).KeepData = false;
			}
			if (mesh is null) {
				((Mesh)meshtarget.mesh).SetVerts(new Vertex[1]);
				((Mesh)meshtarget.mesh).SetInds(new uint[3]);
				return;
			}
			var loadedMesh = new Vertex[mesh.VertexCount];
			for (var i = 0; i < mesh.VertexCount; i++) {
				var vert = mesh.GetVertexAll(i);
				var tuv = Vector2.Zero;
				if (vert.bHaveUV && ((vert.uv?.Length ?? 0) > 0)) {
					tuv = (Vector2)vert.uv[0];
				}
				var color = Color.White;
				if (vert.bHaveC) {
					color = new Color(vert.c.x, vert.c.y, vert.c.z, 1);
				}
				loadedMesh[i] = new Vertex { col = color, norm = (Vector3)vert.n, uv = tuv, pos = (Vector3)vert.v };
			}
			((Mesh)meshtarget.mesh).SetVerts(loadedMesh);
			((Mesh)meshtarget.mesh).SetInds(mesh.RenderIndicesUint().ToArray());
		}
	}
}
