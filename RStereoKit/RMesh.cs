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
	public static class MitManager 
	{

		public static Dictionary<(RMaterial, int), Material> mits = new Dictionary<(RMaterial, int), Material>();

		public static Material GetMitWithOffset(RMaterial loadingLogo, int depth) {
			var mit = (Material)loadingLogo?.Target;
			if (depth == 0) {
				return mit;
			}
			if (mits.ContainsKey((loadingLogo, depth))) {
				return mits[(loadingLogo, depth)];
			}
			else {
				return AddMit(loadingLogo,depth);
			}
		}

		public static Material AddMit(RMaterial loadingLogo, int depth) {
			Material CreateNewMit() {
				var mit = (Material)loadingLogo?.Target;
				var e = mit.Copy();
				e.QueueOffset += depth;
				mits.Add((loadingLogo, depth), e);
				return mit;
			}

			loadingLogo.PramChanged += (mit) => {
				mits.Remove((loadingLogo, depth));
				CreateNewMit();
			};
			loadingLogo.OnDispose += (mit) => mits.Remove((mit, depth));
			return CreateNewMit();
		}
	}


	public class SKRMesh : IRMesh
	{
		public RMesh Quad => new RMesh(Mesh.Quad);

		public void Draw(string id, object mesh, RMaterial loadingLogo, RNumerics.Matrix p,Colorf colorf,int depth) {
			var mit = MitManager.GetMitWithOffset(loadingLogo, depth);
			((Mesh)mesh).Draw(mit, new StereoKit.Matrix(p.m),new Color(colorf.r, colorf.g, colorf.b, colorf.a));
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
