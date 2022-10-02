using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;

using RNumerics;
using Godot;
using System.Xml.Linq;
using Godot.Collections;
using Array = Godot.Collections.Array;
using SArray = System.Array;
using NAudio.Wave;
using RhuEngine;

namespace RhubarbVR.Bindings
{
	public class GodotMesh : IRMesh
	{
		public void Draw(RMaterial loadingLogo, Matrix p, Colorf tint, int zDepth, RenderLayer layer, int submesh) {
			if (loadingLogo.Target is GodotMaterial material) {
				if (TempMeshDraw.Visible) {
					var temperDraw = new MeshInstance3D();
					EngineRunner._.AddChild(temperDraw);
					temperDraw.Visible = true;
					if (submesh <= -1) {
						temperDraw.MaterialOverride = material.GetMatarial(tint, zDepth);
						for (var i = 0; i < temperDraw.GetSurfaceOverrideMaterialCount(); i++) {
							temperDraw.SetSurfaceOverrideMaterial(i, null);
						}
					}
					else {
						temperDraw.SetSurfaceOverrideMaterial(submesh, material.GetMatarial(tint, zDepth));
						temperDraw.MaterialOverride = null;
					}
					temperDraw.Layers = (uint)(int)layer;
					temperDraw.Mesh = LoadedMesh;
					temperDraw.SetPos(p);
					RenderThread.ExecuteOnStartOfFrame(() => temperDraw.Free());
				}
				else {
					TempMeshDraw.Visible = true;
					if (submesh <= -1) {
						TempMeshDraw.MaterialOverride = material.GetMatarial(tint, zDepth);
						for (var i = 0; i < TempMeshDraw.GetSurfaceOverrideMaterialCount(); i++) {
							TempMeshDraw.SetSurfaceOverrideMaterial(i, null);
						}
					}
					else {
						TempMeshDraw.SetSurfaceOverrideMaterial(submesh, material.GetMatarial(tint, zDepth));
						TempMeshDraw.MaterialOverride = null;
					}
					TempMeshDraw.Layers = (uint)(int)layer;
					TempMeshDraw.Mesh = LoadedMesh;
					TempMeshDraw.SetPos(p);
				}
			}
		}

		public RMesh RMesh { get; private set; }

		public ArrayMesh LoadedMesh { get; private set; }
		public MeshInstance3D TempMeshDraw { get; private set; }

		public Mesh.BlendShapeMode shapeMode = Mesh.BlendShapeMode.Relative;

		public string Name;

		public string[] BlendShapeNames = SArray.Empty<string>();

		public (Mesh.PrimitiveType, Array)[] subMeshes = SArray.Empty<(Mesh.PrimitiveType, Array)>();

		private static (Mesh.PrimitiveType, Array) CreateSubmesh(RPrimitiveType rPrimitiveType, Vector3f[][] uvs, int[] indexs, Vector3[] vectors, Vector3[] normals, Color[] color, float[] tangents, int[] bones, float[] wights) {
			if(vectors.Length == 0) {
				return (Mesh.PrimitiveType.Points, null);
			}
			var arrays = new Array();
			arrays.Resize((int)Mesh.ArrayType.Max);
			arrays[(int)Mesh.ArrayType.Vertex] = vectors.AsSpan();
			if (normals is not null) {
				arrays[(int)Mesh.ArrayType.Normal] = normals.AsSpan();
			}
			if (tangents is not null) {
				arrays[(int)Mesh.ArrayType.Tangent] = tangents.AsSpan();
			}
			if (color is not null) {
				arrays[(int)Mesh.ArrayType.Color] = color.AsSpan();
			}
			if (bones is not null) {
				arrays[(int)Mesh.ArrayType.Bones] = bones.AsSpan();
			}
			if (wights is not null) {
				arrays[(int)Mesh.ArrayType.Weights] = wights.AsSpan();
			}
			arrays[(int)Mesh.ArrayType.Index] = indexs.AsSpan();
			for (var i = 0; i < uvs.Length; i++) {
				if (i <= 1) {
					var array = new Vector2[uvs[i].Length];
					for (var e = 0; e < uvs[i].Length; e++) {
						array[e] = new Vector2(uvs[i][e].x, uvs[i][e].y);
					}
					if (i == 0) {
						arrays[(int)Mesh.ArrayType.TexUv] = array.AsSpan();
					}
					else {
						arrays[(int)Mesh.ArrayType.TexUv2] = array.AsSpan();
					}

				}
				else {
					var curentIndex = ((int)Mesh.ArrayType.Custom0) + i - 2;
					if (curentIndex > ((int)Mesh.ArrayType.Custom3)) {
						break;
					}
					var array = new float[uvs[i].Length * 3];
					for (var e = 0; e < uvs[i].Length; e++) {
						var currentLoop = e * 3;
						array[currentLoop] = uvs[i][e].x;
						array[currentLoop + 1] = uvs[i][e].y;
						array[currentLoop + 2] = uvs[i][e].z;
					}
					arrays[curentIndex] = array.AsSpan();
				}
			}
			return (ToGodot(rPrimitiveType), arrays);
		}

		private static IEnumerable<int> LoadIndexs(RPrimitiveType primitiveType, IEnumerable<IFace> faces) {
			foreach (var item in faces) {
				switch (primitiveType) {
					case RPrimitiveType.Point:
						if (item.Indices.Count > 0) {
							yield return (item.Indices[0]);
						}
						break;
					case RPrimitiveType.Line:
						int? lastPoint = null;
						foreach (var point in item.Indices) {
							if (lastPoint is not null) {
								yield return (int)lastPoint;
							}
							yield return point;
							lastPoint = point;
						}
						break;
					case RPrimitiveType.Triangle:
						if (item.Indices.Count == 3) {
							yield return (item.Indices[0]);
							yield return (item.Indices[1]);
							yield return (item.Indices[2]);
						}
						else if (item.Indices.Count == 4) {
							yield return (item.Indices[0]);
							yield return (item.Indices[1]);
							yield return (item.Indices[2]);
							yield return (item.Indices[0]);
							yield return (item.Indices[2]);
							yield return (item.Indices[3]);
						}
						else {
							for (var i = 1; i < (item.Indices.Count - 1); i++) {
								yield return (item.Indices[i]);
								yield return (item.Indices[i + 1]);
								yield return (item.Indices[0]);
							}
						}
						break;
					case RPrimitiveType.Polygon:
						foreach (var point in item.Indices) {
							yield return point;
						}
						break;
					default:
						break;
				}
			}
		}

		private static Mesh.PrimitiveType ToGodot(RPrimitiveType primitiveType) {
			return primitiveType switch {
				RPrimitiveType.Point => Mesh.PrimitiveType.Points,
				RPrimitiveType.Line => Mesh.PrimitiveType.Lines,
				_ => Mesh.PrimitiveType.Triangles,
			};
		}

		public static ArrayMesh MakeQuad() {
			ArrayMesh mesh = new();

			var vertices = new Vector3[4]
			{
				new Vector3(-0.5f,-0.5f,0),
				new Vector3(0.5f,-0.5f,0),
				new Vector3(0.5f, 0.5f,0),
				new Vector3(-0.5f, 0.5f,0)
			};

			var tris = new int[6]
			{
				2, 1, 0,
				3, 2, 0
			};

			var normals = new Vector3[4]
			{
				Vector3.Forward,
				Vector3.Forward,
				Vector3.Forward,
				Vector3.Forward
			};

			var uv = new Vector3f[4]
			{
				new Vector3f(1, 1,0),
				new Vector3f(0, 1,0),
				new Vector3f(0, 0,0),
				new Vector3f(1, 0,0)
			};
			var uvs = new Vector3f[][] { uv };
			var subMesh = CreateSubmesh(RPrimitiveType.Triangle, uvs, tris, vertices, normals, null, null, null, null);
			mesh.AddSurfaceFromArrays(subMesh.Item1, subMesh.Item2);
			return mesh;
		}


		public GodotMesh(ArrayMesh loaded) {
			LoadedMesh = loaded;
			TempMeshDraw = new MeshInstance3D {
				Mesh = LoadedMesh
			};
			EngineRunner._.AddMeshInst(TempMeshDraw);
		}

		public void Init(RMesh rMesh) {
			RMesh = rMesh;
			if(LoadedMesh is not null) {
				return;
			}
			Name = Guid.NewGuid().ToString();
			LoadedMesh = new ArrayMesh();
			TempMeshDraw = new MeshInstance3D {
				Mesh = LoadedMesh
			};
			EngineRunner._.AddMeshInst(TempMeshDraw);
		}

		public void LoadMeshData(IMesh mesh) {
			BlendShapeNames = SArray.Empty<string>();
			subMeshes = SArray.Empty<(Mesh.PrimitiveType, Array)>();
			if (mesh is null) {
				return;
			}
			if (mesh is IComplexMesh complexMesh) {
				return;
			}
			if (!mesh.IsTriangleMesh) {
				RLog.Err("Godot can only render Triangle Meshes When basic");
				return;
			}

			var vertices = new Vector3[mesh.VertexCount];
			var normals = new Vector3[mesh.VertexCount];
			var colors = new Color[mesh.VertexCount];
			var cuv = new Vector3f[1][] { new Vector3f[mesh.VertexCount] };
			for (var i = 0; i < mesh.VertexCount; i++) {
				var vert = mesh.GetVertexAll(i);
				vertices[i] = new Vector3((float)vert.v.x, (float)vert.v.y, (float)vert.v.z);
				normals[i] = new Vector3(vert.n.x, vert.n.y, vert.n.z);
				cuv[0][i] = vert.bHaveUV && ((vert.uv?.Length ?? 0) > 0) ? new Vector3f(vert.uv[0].x, vert.uv[0].y, 0) : new Vector3f(0, 0, 0);
				colors[i] = vert.bHaveC ? new Color(vert.c.x, vert.c.y, vert.c.z, 1f) : new Color(1f, 1f, 1f, 1f);
			}
			var indexs = mesh.RenderIndices().ToArray();
			subMeshes = new (Mesh.PrimitiveType, Array)[] { CreateSubmesh(RPrimitiveType.Triangle, cuv, indexs, vertices, normals, colors, null, null, null) };
		}

		public void LoadMeshToRender() {
			LoadedMesh.ClearSurfaces();
			LoadedMesh.ClearBlendShapes();
			LoadedMesh.BlendShapeMode = shapeMode;
			LoadedMesh.ResourceName = Name;
			foreach (var item in BlendShapeNames) {
				LoadedMesh.AddBlendShape(item);
			}
			foreach (var item in subMeshes) {
				if (item.Item2 is not null) {
					LoadedMesh.AddSurfaceFromArrays(item.Item1, item.Item2);
				}
			}
		}

		public void Dispose() {
			try {
				EngineRunner._.RemoveMeshInst(TempMeshDraw);
				TempMeshDraw?.Free();
				LoadedMesh?.Free();
			}
			catch { }
		}

		public GodotMesh() {

		}
	}
}
