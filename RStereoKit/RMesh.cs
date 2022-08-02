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
using System.Threading.Tasks;

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
		public SKRMesh() { }

		public SKRMesh(Mesh mesh) { Meshs = new Mesh[] { mesh }; }

		public Mesh[] Meshs = Array.Empty<Mesh>();
		public Vertex[] vertices = Array.Empty<Vertex>();
		public uint[][] inds = Array.Empty<uint[]>();

		public void Draw(RMaterial loadingLogo, RNumerics.Matrix p,Colorf colorf,int depth,RhuEngine.Linker.RenderLayer renderLayer,int subMesh) {
			var mit = MitManager.GetMitWithOffset(loadingLogo, depth);
			if(subMesh == -1) {
				foreach (var item in Meshs) {
					item.Draw(mit, new StereoKit.Matrix(p.m), new Color(colorf.r, colorf.g, colorf.b, colorf.a), (StereoKit.RenderLayer)(int)renderLayer);
				}
			}
			else {
				Meshs[subMesh].Draw(mit, new StereoKit.Matrix(p.m), new Color(colorf.r, colorf.g, colorf.b, colorf.a), (StereoKit.RenderLayer)(int)renderLayer);
			}
		}

		public void LoadMeshData(IMesh mesh) {
			if (mesh is null) {
				vertices = new Vertex[1];
				inds = new uint[][] { new uint[3] };
				return;
			}
			if (!mesh.IsTriangleMesh) {
				RLog.Err("StereoKit can only render Triangle Meshes");
				return;
			}
			if (mesh is IComplexMesh complexMesh) {

				var eloadedMesh = new Vertex[mesh.VertexCount];
				Parallel.For(0, mesh.VertexCount, (i) => {
					var vert = mesh.GetVertexAll(i);
					var tuv = Vector2.Zero;
					if (vert.bHaveUV && ((vert.uv?.Length ?? 0) > 0)) {
						tuv = (Vector2)vert.uv[0];
					}
					var color = Color.White;
					if (vert.bHaveC) {
						color = new Color(vert.c.x, vert.c.y, vert.c.z, 1);
					}
					eloadedMesh[i] = new Vertex { col = color, norm = (Vector3)vert.n, uv = tuv, pos = (Vector3)vert.v };
				});
				vertices = eloadedMesh;
				var subMeshCount = complexMesh.SubMeshes.Count() + 1;
				inds = new uint[subMeshCount][];
				LoadSubMesh(complexMesh.PrimitiveType, complexMesh.Faces, 0);
				var currentIndex = 0;
				foreach (var item in complexMesh.SubMeshes) {
					currentIndex++;
					LoadSubMesh(item.PrimitiveType, item.Faces, currentIndex);
				}
				return;
			}
			var loadedMesh = new Vertex[mesh.VertexCount];
			Parallel.For(0, mesh.VertexCount, (i) => {
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
			});
			vertices = loadedMesh;
			inds = new uint[][] { mesh.RenderIndicesUint().ToArray() };
		}

		private void LoadSubMesh(RPrimitiveType primitiveType, IEnumerable<IFace> faces, int index) {
			var indexs = LoadIndexs(primitiveType, faces).ToArray();
			inds[index] = indexs;
			RLog.Info($"Loaded SubMesh PrimitiveType{primitiveType} {index}");

		}

		private IEnumerable<uint> LoadIndexs(RPrimitiveType primitiveType, IEnumerable<IFace> faces) {
			foreach (var item in faces) {
				switch (primitiveType) {
					case RPrimitiveType.Point:
						if (item.Indices.Count > 0) {
							yield return (uint)(item.Indices[0]);
						}
						break;
					case RPrimitiveType.Line:
						int? lastPoint = null;
						foreach (var point in item.Indices) {
							if (!(lastPoint is null)) {
								yield return (uint)lastPoint;
							}
							yield return (uint)point;
							lastPoint = point;
						}
						break;
					case RPrimitiveType.Triangle:
						if (item.Indices.Count == 3) {
							yield return (uint)(item.Indices[0]);
							yield return (uint)(item.Indices[1]);
							yield return (uint)(item.Indices[2]);
						}
						else if (item.Indices.Count == 4) {
							yield return (uint)(item.Indices[0]);
							yield return (uint)(item.Indices[1]);
							yield return (uint)(item.Indices[2]);
							yield return (uint)(item.Indices[0]);
							yield return (uint)(item.Indices[2]);
							yield return (uint)(item.Indices[3]);
						}
						else {
							for (var i = 1; i < (item.Indices.Count - 1); i++) {
								yield return (uint)(item.Indices[i]);
								yield return (uint)(item.Indices[i + 1]);
								yield return (uint)(item.Indices[0]);
							}
						}
						break;
					case RPrimitiveType.Polygon:
						foreach (var point in item.Indices) {
							yield return (uint)point;
						}
						break;
					default:
						break;
				}
			}
		}


		public void LoadMeshToRender() {
			var newMeshes = new Mesh[inds.Length];
			for (var i = 0; i < Meshs.Length; i++) {
				if(i >= newMeshes.Length) {
					break;
				}
				newMeshes[i] = Meshs[i];
			}
			for (var i = Meshs.Length; i < inds.Length; i++) {
				newMeshes[i] = new Mesh() {
					KeepData = false
				};
			}
			for (var i = 0; i < inds.Length; i++) {
				newMeshes[i].SetVerts(vertices);
				newMeshes[i].SetInds(inds[i]);
			}
			Meshs = newMeshes;
		}
	}
}
