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

	public sealed class GodotTempMeshRender : IRTempQuad
	{
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
			var subMesh = GodotMesh.CreateSubmesh(RPrimitiveType.Triangle, uvs, tris, vertices, normals, null, null, null, null);
			mesh.AddSurfaceFromArrays(subMesh.Item1, subMesh.Item2);
			return mesh;
		}

		public Matrix Pos { get => instance3D.GetPos(); set => instance3D.SetPos(value); }
		public bool Visible { get => instance3D.Visible; set => instance3D.Visible = value; }

		private RMaterial _targetRhubarbMit;
		public RMaterial Material
		{
			get => _targetRhubarbMit; set {
				_targetRhubarbMit = value;
				if (_targetRhubarbMit.Target is GodotMaterial godotMaterial) {
					instance3D.MaterialOverride = godotMaterial.Material;
				}
			}
		}

		public void Dispose() {
			instance3D.Free();
		}

		public MeshInstance3D instance3D;
		public RTempQuad Target;

		public ArrayMesh targetMesh;

		public void Init(RTempQuad text) {
			Target = text;
			instance3D = new MeshInstance3D();
			targetMesh ??= MakeQuad();
			instance3D.Mesh = targetMesh;
			EngineRunnerHelpers._.AddChild(instance3D);
		}
	}



	public struct BoneWeight
	{
		//
		// Summary:
		//     Skinning weight for first bone.
		public float Weight0 { get; set; }

		//
		// Summary:
		//     Skinning weight for second bone.
		public float Weight1 { get; set; }

		//
		// Summary:
		//     Skinning weight for third bone.
		public float Weight2 { get; set; }

		//
		// Summary:
		//     Skinning weight for fourth bone.
		public float Weight3 { get; set; }

		//
		// Summary:
		//     Index of first bone.
		public int BoneIndex0 { get; set; }

		//
		// Summary:
		//     Index of second bone.
		public int BoneIndex1 { get; set; }

		//
		// Summary:
		//     Index of third bone.
		public int BoneIndex2 { get; set; }

		//
		// Summary:
		//     Index of fourth bone.
		public int BoneIndex3 { get; set; }

		public void Normalize() {
			var totalWeight = Weight0 + Weight1 + Weight2 + Weight3;
			if (totalWeight > 0f) {
				var num = 1f / totalWeight;
				Weight0 *= num;
				Weight1 *= num;
				Weight2 *= num;
				Weight3 *= num;
			}
		}

		public void AddBone(int boneIndex, float boneWeight) {
			var targetLayer = -1;
			var smallestValue = float.MaxValue;
			for (var i = 0; i < 4; i++) {
				var currentLayerWeight = i switch {
					0 => Weight0,
					1 => Weight1,
					2 => Weight2,
					_ => Weight3,
				};
				if (currentLayerWeight < smallestValue) {
					smallestValue = currentLayerWeight;
					targetLayer = i;
				}
			}
			if (boneWeight > smallestValue) {
				switch (targetLayer) {
					case 0:
						Weight0 = boneWeight;
						BoneIndex0 = boneIndex;
						break;
					case 1:
						Weight1 = boneWeight;
						BoneIndex1 = boneIndex;
						break;
					case 2:
						Weight2 = boneWeight;
						BoneIndex2 = boneIndex;
						break;
					case 3:
						Weight3 = boneWeight;
						BoneIndex3 = boneIndex;
						break;
					default:
						break;
				}
			}
		}

		public override int GetHashCode() {
			return BoneIndex0.GetHashCode() ^ (BoneIndex1.GetHashCode() << 2) ^ (BoneIndex2.GetHashCode() >> 2) ^ (BoneIndex3.GetHashCode() >> 1) ^ (Weight0.GetHashCode() << 5) ^ (Weight1.GetHashCode() << 4) ^ (Weight2.GetHashCode() >> 4) ^ (Weight3.GetHashCode() >> 3);
		}

		public override bool Equals(object other) {
			return other is BoneWeight weight && Equals(weight);
		}

		public bool Equals(BoneWeight other) {
			return BoneIndex0.Equals(other.BoneIndex0) && BoneIndex1.Equals(other.BoneIndex1) && BoneIndex2.Equals(other.BoneIndex2) && BoneIndex3.Equals(other.BoneIndex3) && new Vector4(Weight0, Weight1, Weight2, Weight3).Equals(new Vector4(other.Weight0, other.Weight1, other.Weight2, other.Weight3));
		}

		public static bool operator ==(BoneWeight lhs, BoneWeight rhs) => lhs.BoneIndex0 == rhs.BoneIndex0 && lhs.BoneIndex1 == rhs.BoneIndex1 && lhs.BoneIndex2 == rhs.BoneIndex2 && lhs.BoneIndex3 == rhs.BoneIndex3 && new Vector4(lhs.Weight0, lhs.Weight1, lhs.Weight2, lhs.Weight3) == new Vector4(rhs.Weight0, rhs.Weight1, rhs.Weight2, rhs.Weight3);

		public static bool operator !=(BoneWeight lhs, BoneWeight rhs) => !(lhs == rhs);
	}


	public sealed class GodotMesh : IRMesh
	{
		public RMesh RMesh { get; private set; }

		public ArrayMesh LoadedMesh { get; private set; }
		public Skin LoadedSkin { get; private set; }

		public Mesh.BlendShapeMode shapeMode = Mesh.BlendShapeMode.Relative;

		public string Name;

		public Array<Godot.Collections.Array> BlendShapes;

		public string[] BlendShapeNames = SArray.Empty<string>();

		public Matrix[] BonePos = SArray.Empty<Matrix>();

		public (Mesh.PrimitiveType, Array)[] subMeshes = SArray.Empty<(Mesh.PrimitiveType, Array)>();


		private static IEnumerable<float> GetData(Vector4 data) {
			yield return data.X;
			yield return data.Y;
			yield return data.Z;
			yield return data.W;
		}

		public static (Mesh.PrimitiveType, Array) CreateSubmesh(RPrimitiveType rPrimitiveType, Vector3f[][] uvs, int[] indexs, Vector3[] vectors, Vector3[] normals, Color[] color, Vector4[] tangents, int[] bones, float[] wights) {
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
						arrays[(int)Mesh.ArrayType.TexUV] = array.AsSpan();
					}
					else {
						arrays[(int)Mesh.ArrayType.TexUV2] = array.AsSpan();
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
							yield return item.Indices[0];
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
							yield return item.Indices[0];
							yield return item.Indices[1];
							yield return item.Indices[2];
						}
						else if (item.Indices.Count == 4) {
							yield return item.Indices[0];
							yield return item.Indices[1];
							yield return item.Indices[2];
							yield return item.Indices[0];
							yield return item.Indices[2];
							yield return item.Indices[3];
						}
						else {
							for (var i = 1; i < (item.Indices.Count - 1); i++) {
								yield return item.Indices[i];
								yield return item.Indices[i + 1];
								yield return item.Indices[0];
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

		public GodotMesh(ArrayMesh loaded) {
			LoadedMesh = loaded;
			LoadedSkin = new Skin();
		}

		public void Init(RMesh rMesh) {
			RMesh = rMesh;
			if (LoadedMesh is not null) {
				return;
			}
			Name = Guid.NewGuid().ToString();
			LoadedMesh = new ArrayMesh();
			LoadedSkin = new Skin();
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
							cuve[i][x] = new Vector3f(complexMesh.TexCoords[i][x].x, complexMesh.TexCoords[i][x].y, complexMesh.TexCoords[i][x].z);
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
					BonePos = complexMesh.Bones.Select(x => x.OffsetMatrix).ToArray();
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
					for (var i = 0; i < BoneVertexWights.Length; i++) {
						var currentondex = i * 4;
						var currentBone = BoneVertexWights[i];
						currentBone.Normalize();
						bones[currentondex] = currentBone.BoneIndex0;
						bones[currentondex + 1] = currentBone.BoneIndex1;
						bones[currentondex + 2] = currentBone.BoneIndex2;
						bones[currentondex + 3] = currentBone.BoneIndex3;
						wights[currentondex] = currentBone.Weight0;
						wights[currentondex + 1] = currentBone.Weight1;
						wights[currentondex + 2] = currentBone.Weight2;
						wights[currentondex + 3] = currentBone.Weight3;
					}
				}

				BlendShapes?.Clear();
				BlendShapes ??= new Array<Array>();
				if (complexMesh.HasMeshAttachments && complexMesh.MorphingMethod != RMeshMorphingMethod.None) {
					if (complexMesh.MorphingMethod == RMeshMorphingMethod.VertexBlend) {
						var current = 0;
						var blendNames = new List<string>();
						foreach (var item in complexMesh.MeshAttachments) {
							var shapedata = new Array();
							shapedata.Resize(3);
							var verts = new Vector3[complexMesh.VertexCount];
							var smallist = Math.Min(item.Vertices.Count, complexMesh.VertexCount);
							for (var i = 0; i < smallist; i++) {
								verts[i] = new Vector3(item.Vertices[i].x, item.Vertices[i].y, item.Vertices[i].z) - cvertices[i];
							}
							shapedata[0] = verts.AsSpan();
							var norms = new Vector3[complexMesh.VertexCount];
							var smallistnorm = Math.Min(item.Normals.Count, complexMesh.VertexCount);
							for (var i = 0; i < smallistnorm; i++) {
								norms[i] = new Vector3(item.Normals[i].x, item.Normals[i].y, item.Normals[i].z) - cnormals[i];
							}
							if (cnormals is not null) {
								shapedata[1] = norms.AsSpan();
							}

							var tangents = new Vector3[complexMesh.VertexCount];
							var smallitsTang = Math.Min(complexMesh.Tangents.Count, complexMesh.VertexCount);
							for (var i = 0; i < smallitsTang; i++) {
								var tangent = item.Tangents[i];
								tangents[i] = new Vector3(tangent.x - ctangents[i].X, tangent.y - ctangents[i].Y, tangent.z - ctangents[i].Z);
							}
							if (ctangents is not null) {
								shapedata[2] = tangents.AsSpan();
							}
							BlendShapes.Add(shapedata);
							blendNames.Add(item.Name);
							current++;
						}
						BlendShapeNames = blendNames.ToArray();
					}
					if (complexMesh.MorphingMethod is RMeshMorphingMethod.MorphRelative or RMeshMorphingMethod.MorphNormalized) {
						var current = 0;
						var blendNames = new List<string>();
						foreach (var item in complexMesh.MeshAttachments) {
							var shapedata = new Array();
							shapedata.Resize(3);
							var verts = new Vector3[complexMesh.VertexCount];
							var smallist = Math.Min(item.Vertices.Count, complexMesh.VertexCount);
							for (var i = 0; i < smallist; i++) {
								verts[i] = new Vector3(item.Vertices[i].x, item.Vertices[i].y, item.Vertices[i].z);
							}
							shapedata[0] = verts.AsSpan();
							var norms = new Vector3[complexMesh.VertexCount];
							var smallistnorm = Math.Min(item.Normals.Count, complexMesh.VertexCount);
							for (var i = 0; i < smallistnorm; i++) {
								norms[i] = new Vector3(item.Normals[i].x, item.Normals[i].y, item.Normals[i].z);
							}
							if (cnormals is not null) {
								shapedata[1] = norms.AsSpan();
							}

							var tangents = new Vector3[complexMesh.VertexCount];
							var smallitsTang = Math.Min(complexMesh.Tangents.Count, complexMesh.VertexCount);
							for (var i = 0; i < smallitsTang; i++) {
								tangents[i] = new Vector3(item.Tangents[i].x, item.Tangents[i].y, item.Tangents[i].z);
							}
							if (ctangents is not null) {
								shapedata[2] = tangents.AsSpan();
							}
							BlendShapes.Add(shapedata);
							blendNames.Add(item.Name);
							current++;
						}
						BlendShapeNames = blendNames.ToArray();
					}
				}
				else {
					BlendShapeNames = SArray.Empty<string>();
				}

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
			if (BonePos.Length != 0) {
				LoadedSkin?.ClearBinds();
				for (var i = 0; i < BonePos.Length; i++) {
					LoadedSkin.AddBind(i, BonePos[i].CastMatrix());
				}
			}
			else {
				LoadedSkin?.ClearBinds();
			}

			foreach (var item in BlendShapeNames) {
				LoadedMesh.AddBlendShape(item);
			}

			foreach (var item in subMeshes) {
				if (item.Item2 is not null) {
					LoadedMesh.AddSurfaceFromArrays(item.Item1, item.Item2, BlendShapes);
				}
			}
		}

		public void Dispose() {
			//Todo make mesh CleanUP better
			LoadedSkin?.ClearBinds();
			LoadedSkin?.Unreference();
			LoadedSkin = null;
			LoadedMesh?.ClearSurfaces();
			LoadedMesh?.ClearBlendShapes();
			LoadedMesh?.Unreference();
			LoadedMesh = null;
			BlendShapeNames = SArray.Empty<string>();
			BonePos = SArray.Empty<Matrix>();
			BlendShapes?.Clear();
			BlendShapes = null;
			foreach (var item in subMeshes) {
				item.Item2?.Clear();
			}
			subMeshes = SArray.Empty<(Mesh.PrimitiveType, Array)>();
		}

		public GodotMesh() {

		}
	}
}
