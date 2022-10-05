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
	public struct BoneWeight
	{
		private float m_Weight0;

		private float m_Weight1;

		private float m_Weight2;

		private float m_Weight3;

		private int m_BoneIndex0;

		private int m_BoneIndex1;

		private int m_BoneIndex2;

		private int m_BoneIndex3;

		//
		// Summary:
		//     Skinning weight for first bone.
		public float weight0
		{
			get {
				return m_Weight0;
			}
			set {
				m_Weight0 = value;
			}
		}

		//
		// Summary:
		//     Skinning weight for second bone.
		public float weight1
		{
			get {
				return m_Weight1;
			}
			set {
				m_Weight1 = value;
			}
		}

		//
		// Summary:
		//     Skinning weight for third bone.
		public float weight2
		{
			get {
				return m_Weight2;
			}
			set {
				m_Weight2 = value;
			}
		}

		//
		// Summary:
		//     Skinning weight for fourth bone.
		public float weight3
		{
			get {
				return m_Weight3;
			}
			set {
				m_Weight3 = value;
			}
		}

		//
		// Summary:
		//     Index of first bone.
		public int boneIndex0
		{
			get {
				return m_BoneIndex0;
			}
			set {
				m_BoneIndex0 = value;
			}
		}

		//
		// Summary:
		//     Index of second bone.
		public int boneIndex1
		{
			get {
				return m_BoneIndex1;
			}
			set {
				m_BoneIndex1 = value;
			}
		}

		//
		// Summary:
		//     Index of third bone.
		public int boneIndex2
		{
			get {
				return m_BoneIndex2;
			}
			set {
				m_BoneIndex2 = value;
			}
		}

		//
		// Summary:
		//     Index of fourth bone.
		public int boneIndex3
		{
			get {
				return m_BoneIndex3;
			}
			set {
				m_BoneIndex3 = value;
			}
		}

		public void Normalize() {
			float totalWeight = this.weight0 + this.weight1 + this.weight2 + this.weight3;
			if (totalWeight > 0f) {
				float num = 1f / totalWeight;
				this.weight0 *= num;
				this.weight1 *= num;
				this.weight2 *= num;
				this.weight3 *= num;
			}
		}

		public void AddBone(int boneIndex, float boneWeight) {
			var targetLayer = -1;
			var smallestValue = float.MaxValue;
			for (int i = 0; i < 4; i++) {
				var currentLayerWeight = i switch {
					0 => this.weight0,
					1 => this.weight1,
					2 => this.weight2,
					_ => this.weight3,
				};
				if (currentLayerWeight < smallestValue) {
					smallestValue = currentLayerWeight;
					targetLayer = i;
				}
			}
			if (boneWeight > smallestValue) {
				switch (targetLayer) {
					case 0:
						this.weight0 = boneWeight;
						this.boneIndex0 = boneIndex;
						break;
					case 1:
						this.weight1 = boneWeight;
						this.boneIndex1 = boneIndex;
						break;
					case 2:
						this.weight2 = boneWeight;
						this.boneIndex2 = boneIndex;
						break;
					case 3:
						this.weight3 = boneWeight;
						this.boneIndex3 = boneIndex;
						break;
					default:
						break;
				}
			}
		}

		public override int GetHashCode() {
			return boneIndex0.GetHashCode() ^ (boneIndex1.GetHashCode() << 2) ^ (boneIndex2.GetHashCode() >> 2) ^ (boneIndex3.GetHashCode() >> 1) ^ (weight0.GetHashCode() << 5) ^ (weight1.GetHashCode() << 4) ^ (weight2.GetHashCode() >> 4) ^ (weight3.GetHashCode() >> 3);
		}

		public override bool Equals(object other) {
			return other is BoneWeight && Equals((BoneWeight)other);
		}

		public bool Equals(BoneWeight other) {
			return boneIndex0.Equals(other.boneIndex0) && boneIndex1.Equals(other.boneIndex1) && boneIndex2.Equals(other.boneIndex2) && boneIndex3.Equals(other.boneIndex3) && new Vector4(weight0, weight1, weight2, weight3).Equals(new Vector4(other.weight0, other.weight1, other.weight2, other.weight3));
		}

		public static bool operator ==(BoneWeight lhs, BoneWeight rhs) {
			return lhs.boneIndex0 == rhs.boneIndex0 && lhs.boneIndex1 == rhs.boneIndex1 && lhs.boneIndex2 == rhs.boneIndex2 && lhs.boneIndex3 == rhs.boneIndex3 && new Vector4(lhs.weight0, lhs.weight1, lhs.weight2, lhs.weight3) == new Vector4(rhs.weight0, rhs.weight1, rhs.weight2, rhs.weight3);
		}

		public static bool operator !=(BoneWeight lhs, BoneWeight rhs) {
			return !(lhs == rhs);
		}
	}


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

		private static IEnumerable<float> GetData(Vector4 data) {
			yield return data.x;
			yield return data.y;
			yield return data.z;
			yield return data.w;
		}

		private static (Mesh.PrimitiveType, Array) CreateSubmesh(RPrimitiveType rPrimitiveType, Vector3f[][] uvs, int[] indexs, Vector3[] vectors, Vector3[] normals, Color[] color, Vector4[] tangents, int[] bones, float[] wights) {
			if (vectors.Length == 0) {
				return (Mesh.PrimitiveType.Points, null);
			}
			var arrays = new Array();
			arrays.Resize((int)Mesh.ArrayType.Max);
			arrays[(int)Mesh.ArrayType.Vertex] = vectors.AsSpan();
			if (normals is not null) {
				arrays[(int)Mesh.ArrayType.Normal] = normals.AsSpan();
			}
			if (tangents is not null) {
				arrays[(int)Mesh.ArrayType.Tangent] = tangents.SelectMany(GetData).ToArray().AsSpan();
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
			if (LoadedMesh is not null) {
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
				var cvertices = new Vector3[complexMesh.Vertices.Count];
				for (var i = 0; i < complexMesh.Vertices.Count; i++) {
					cvertices[i] = new Vector3(complexMesh.Vertices[i].x, complexMesh.Vertices[i].y, complexMesh.Vertices[i].z);
				}
				var cnormals = new Vector3[complexMesh.Normals.Count];
				for (var i = 0; i < complexMesh.Normals.Count; i++) {
					cnormals[i] = new Vector3(complexMesh.Normals[i].x, complexMesh.Normals[i].y, complexMesh.Normals[i].z);
				}
				var ctangents = new Vector4[complexMesh.Tangents.Count];
				for (var i = 0; i < complexMesh.Tangents.Count; i++) {
					var tangent = complexMesh.Tangents[i];
					var crossnt = complexMesh.Normals[i].Cross(tangent);
					ctangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, (crossnt.Dot(complexMesh.BiTangents[i]) <= 0f) ? 1 : (-1));
				}
				var cuve = new Vector3f[complexMesh.TexCoords.Length][];
				for (var i = 0; i < complexMesh.TexCoords.Length; i++) {
					if (complexMesh.TexCoords[i].Count == 0) {
						System.Array.Resize(ref cuve, i);
						break;
					}
					cuve[i] = new Vector3f[complexMesh.Vertices.Count];
					for (var x = 0; x < complexMesh.Vertices.Count; x++) {
						if (complexMesh.TexCoords[i].Count > i) {
							cuve[i][x] = (new Vector3f(complexMesh.TexCoords[i][x].x, complexMesh.TexCoords[i][x].y, complexMesh.TexCoords[i][x].z));
						}
					}
				}
				var colorAmount = 0;
				if (complexMesh.Colors.Length > 0) {
					colorAmount = complexMesh.Colors[0].Count;
				}
				var ccolors = new Color[colorAmount];
				if (complexMesh.Colors.Length > 0) {
					Parallel.For(0, complexMesh.Colors[0].Count, (i) => ccolors[i] = new Color(complexMesh.Colors[0][i].r, complexMesh.Colors[0][i].g, complexMesh.Colors[0][i].b, complexMesh.Colors[0][i].a));
				}
				if (cnormals.Length == 0) {
					cnormals = null;
				}
				if (ccolors.Length == 0) {
					ccolors = null;
				}
				if (ctangents.Length == 0) {
					ctangents = null;
				}
				BoneWeight[] BoneVertexWights = null;
				if (complexMesh.HasBones) {
					BoneVertexWights = new BoneWeight[complexMesh.VertexCount];
					var BoneIndex = 0;
					foreach (var Bone in complexMesh.Bones) {
						foreach (var vertexWe in Bone.VertexWeights) {
							BoneVertexWights[vertexWe.VertexID].AddBone(BoneIndex, vertexWe.Weight);
						}
						BoneIndex++;
					}
				}
				int[] bones = null;
				float[] wights = null;
				if (BoneVertexWights is not null) {
					bones = new int[BoneVertexWights.Length * 4];
					wights = new float[BoneVertexWights.Length * 4];
					for (int i = 0; i < BoneVertexWights.Length; i++) {
						var currentondex = i * 4;
						var currentBone = BoneVertexWights[i];
						currentBone.Normalize();
						bones[currentondex] = currentBone.boneIndex0;
						bones[currentondex + 1] = currentBone.boneIndex1;
						bones[currentondex + 2] = currentBone.boneIndex2;
						bones[currentondex + 3] = currentBone.boneIndex3;
						wights[currentondex] = currentBone.weight0;
						wights[currentondex + 1] = currentBone.weight1;
						wights[currentondex + 2] = currentBone.weight2;
						wights[currentondex + 3] = currentBone.weight3;
					}
				}



				//if (complexMesh.HasMeshAttachments) {
				//	blendShapeFrames = new BlendShapeFrame[complexMesh.MeshAttachments.Count()];
				//	var current = 0;
				//	foreach (var item in complexMesh.MeshAttachments) {
				//		try {
				//			blendShapeFrames[current].vertices = new Vector3[complexMesh.VertexCount];
				//			var smallist = Math.Min(item.Vertices.Count, complexMesh.VertexCount);
				//			for (int i = 0; i < smallist; i++) {
				//				blendShapeFrames[current].vertices[i] = new Vector3(item.Vertices[i].x, item.Vertices[i].y, item.Vertices[i].z) - cvertices[i];
				//			}
				//			blendShapeFrames[current].normals = new Vector3[complexMesh.VertexCount];
				//			var smallistnorm = Math.Min(item.Normals.Count, complexMesh.VertexCount);
				//			for (int i = 0; i < smallistnorm; i++) {
				//				blendShapeFrames[current].normals[i] = new Vector3(item.Normals[i].x, item.Normals[i].y, item.Normals[i].z) - cnormals[i];
				//			}
				//			blendShapeFrames[current].tangents = new Vector3[complexMesh.VertexCount];
				//			var smallitsTang = Math.Min(complexMesh.Tangents.Count, complexMesh.VertexCount);
				//			for (int i = 0; i < smallitsTang; i++) {
				//				var tangent = item.Tangents[i];
				//				blendShapeFrames[current].tangents[i] = new Vector3(tangent.x - ctangents[i].x, tangent.y - ctangents[i].y, tangent.z - ctangents[i].z);
				//			}
				//			blendShapeFrames[current].name = item.Name;
				//			blendShapeFrames[current].wight = item.Weight;
				//			current++;
				//		}
				//		catch { }
				//	}
				//}

				Name = "Complex Mesh:" + complexMesh.MeshName;
				subMeshes = new (Mesh.PrimitiveType, Array)[complexMesh.SubMeshes.Count() + 1];
				var indes = LoadIndexs(complexMesh.PrimitiveType, complexMesh.Faces).ToArray();
				subMeshes[0] = CreateSubmesh(complexMesh.PrimitiveType, cuve, indes, cvertices, cnormals, ccolors, ctangents, bones, wights);
				var currentIndex = 0;
				foreach (var item in complexMesh.SubMeshes) {
					currentIndex++;
					indes = LoadIndexs(item.PrimitiveType, item.Faces).ToArray();
					subMeshes[currentIndex] = CreateSubmesh(complexMesh.PrimitiveType, cuve, indes, cvertices, cnormals, ccolors, ctangents, bones, wights);
				}
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
