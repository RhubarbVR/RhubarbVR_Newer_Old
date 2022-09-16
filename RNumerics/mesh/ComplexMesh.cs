using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assimp;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct RSubMesh : ISubMesh
	{
		[Key(0)]
		public RPrimitiveType rPrimitiveType;
		[IgnoreMember]
		public RPrimitiveType PrimitiveType => rPrimitiveType;
		[Key(1)]
		public List<RFace> rFaces;
		[Key(2)]
		public int rCount;
		[IgnoreMember]
		public int Count => rFaces.Count;

		public RSubMesh() {
			rPrimitiveType = RPrimitiveType.Triangle;
			rFaces = new List<RFace>();
			rCount = 0;
		}
		public RSubMesh(in RPrimitiveType type) {
			rPrimitiveType = type;
			rFaces = new List<RFace>();
			rCount = 0;
		}
		public RSubMesh(in RPrimitiveType type, in List<RFace> faces, in int count) {
			rPrimitiveType = type;
			rFaces = faces;
			rCount = count;
		}
		[IgnoreMember]
		public IEnumerable<IFace> Faces
		{
			get {
				foreach (var item in rFaces) {
					yield return item;
				}
			}
		}
	}

	[MessagePackObject]
	public struct RVertexWeight : IVertexWeight
	{
		/// <summary>
		///  Index of the vertex which is influenced by the bone.
		/// </summary>
		[Key(0)]
		public int VertexID;

		/// <summary>
		/// Strength of the influence in range of (0...1). All influences from all bones
		/// at one vertex amounts to 1.
		/// </summary>
		[Key(1)]
		public float Weight;

		public RVertexWeight(in VertexWeight vertexWeight) {
			VertexID = vertexWeight.VertexID;
			Weight = vertexWeight.Weight;
		}
		public RVertexWeight() {
			VertexID = 0;
			Weight = 0;
		}

		[IgnoreMember]
		int IVertexWeight.VertexID => VertexID;
		[IgnoreMember]
		float IVertexWeight.Weight => Weight;
	}

	[MessagePackObject]
	public sealed class RBone : IBone
	{

		[Key(0)]
		public Matrix OffsetMatrix = Matrix.Identity;

		[Key(1)]
		public List<RVertexWeight> VertexWeights = new();

		[Key(2)]
		public string BoneName;

		[IgnoreMember]
		public bool HasVertexWeights => VertexWeights.Count > 0;

		[IgnoreMember]
		public int VertexWeightCount => VertexWeights.Count;
		[IgnoreMember]
		public string Name => BoneName;
		[IgnoreMember]
		Matrix IBone.OffsetMatrix => OffsetMatrix;
		[IgnoreMember]
		IEnumerable<IVertexWeight> IBone.VertexWeights
		{
			get {
				foreach (var item in VertexWeights) {
					yield return item;
				}
			}
		}

		public RBone(in Bone asimp) {
			var baseM = asimp.OffsetMatrix;
			OffsetMatrix = new System.Numerics.Matrix4x4 {
				M11 = baseM.A1,
				M12 = baseM.A2,
				M13 = baseM.A3,
				M14 = baseM.A4,
				M21 = baseM.B1,
				M22 = baseM.B2,
				M23 = baseM.B3,
				M24 = baseM.B4,
				M31 = baseM.C1,
				M32 = baseM.C2,
				M33 = baseM.C3,
				M34 = baseM.C4,
				M41 = baseM.D1,
				M42 = baseM.D2,
				M43 = baseM.D3,
				M44 = baseM.D4,
			};
			BoneName = asimp.Name;
			VertexWeights = asimp.VertexWeights.Select((x) => new RVertexWeight(x)).ToList();
		}
		public RBone() {

		}
	}
	[MessagePackObject]
	public sealed class RFace : IFace
	{
		[Key(0)]
		public List<int> Indices = new();
		[IgnoreMember]
		List<int> IFace.Indices => Indices;

		public RFace(in Face face) {
			Indices = face.Indices;
		}
		public RFace() { }
	}
	public enum RMeshMorphingMethod : byte
	{
		/// <summary>
		/// No morphing.
		/// </summary>
		None,
		/// <summary>
		/// Interpolation between morph targets.
		/// </summary>
		VertexBlend,
		/// <summary>
		/// Normalized morphing between morph targets.
		/// </summary>
		MorphNormalized,
		/// <summary>
		/// Relative morphing between morph targets.
		/// </summary>
		MorphRelative
	}
	[Flags]
	public enum RPrimitiveType : byte
	{
		/// <summary>
		/// Point primitive. This is just a single vertex in the virtual world. A face has
		/// one index for such a primitive.</summary>
		Point = 0x1,
		/// <summary>
		/// Line primitive. This is a line defined through a start and an end position. A
		/// face contains exactly two indices for such a primitive.</summary>
		Line = 0x2,
		/// <summary>
		/// Triangle primitive, consisting of three indices.
		/// </summary>
		Triangle = 0x4,
		/// <summary>
		/// A n-Gon that has more than three edges (thus is not a triangle).
		/// </summary>
		Polygon = 0x8
	}

	[MessagePackObject]
	public sealed class RAnimationAttachment : IAnimationAttachment
	{
		[Key(0)]
		public string Name = "Unknown";
		[Key(1)]
		public List<Vector3f> Vertices = new();
		[Key(2)]
		public List<Vector3f> Normals = new();
		[Key(3)]
		public List<Vector3f> Tangents = new();
		[Key(4)]
		public List<Vector3f> BiTangents = new();
		[Key(5)]
		public List<Colorf>[] Colors = Array.Empty<List<Colorf>>();
		[Key(6)]
		public List<Vector3f>[] TexCoords = Array.Empty<List<Vector3f>>();
		[Key(7)]
		public float Weight;
		[IgnoreMember]
		float IAnimationAttachment.Weight => Weight;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.Vertices => Vertices;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.Normals => Normals;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.Tangents => Tangents;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.BiTangents => BiTangents;
		[IgnoreMember]
		List<Colorf>[] IRawComplexMeshData.Colors => Colors;
		[IgnoreMember]
		List<Vector3f>[] IRawComplexMeshData.TexCoords => TexCoords;
		[IgnoreMember]
		string IAnimationAttachment.Name => Name;

		public RAnimationAttachment(in MeshAnimationAttachment meshAnimationAttachment) {
			Vertices = meshAnimationAttachment.Vertices.Select((x) => (Vector3f)x).ToList();
			Normals = meshAnimationAttachment.Normals.Select((x) => (Vector3f)x).ToList();
			Tangents = meshAnimationAttachment.Tangents.Select((x) => (Vector3f)x).ToList();
			BiTangents = meshAnimationAttachment.BiTangents.Select((x) => (Vector3f)x).ToList();
			Colors = meshAnimationAttachment.VertexColorChannels.Select((x) => x.Select((y) => (Colorf)y).ToList()).ToArray();
			TexCoords = meshAnimationAttachment.TextureCoordinateChannels.Select((x) => x.Select((y) => (Vector3f)y).ToList()).ToArray();
			Weight = meshAnimationAttachment.Weight;
			Name = meshAnimationAttachment.Name;
		}
		public RAnimationAttachment() {

		}
	}

	[MessagePackObject]
	public sealed class ComplexMesh : IComplexMesh, IMesh
	{
		[Key(0)]
		public string MeshName;
		[Key(1)]
		public RPrimitiveType PrimitiveType;
		[Key(2)]
		public List<Vector3f> Vertices = new();
		[Key(3)]
		public List<Vector3f> Normals = new();
		[Key(4)]
		public List<Vector3f> Tangents = new();
		[Key(5)]
		public List<Vector3f> BiTangents = new();
		[Key(6)]
		public List<RFace> Faces = new();
		[Key(7)]
		public List<Colorf>[] Colors = Array.Empty<List<Colorf>>();
		[Key(8)]
		public List<Vector3f>[] TexCoords = Array.Empty<List<Vector3f>>();
		[Key(9)]
		public int[] TexComponentCount = Array.Empty<int>();
		[Key(10)]
		public List<RBone> Bones = new();
		[IgnoreMember]
		public int BonesCount => Bones.Count;
		[Key(11)]
		public List<RAnimationAttachment> MeshAttachments = new();
		[Key(12)]
		public RMeshMorphingMethod MorphingMethod = RMeshMorphingMethod.None;

		[Key(13)]
		public List<RSubMesh> SubMeshes = new();

		[IgnoreMember]
		bool IComplexMesh.HasSubMeshs => SubMeshes.Count > 0;

		[IgnoreMember]
		IEnumerable<ISubMesh> IComplexMesh.SubMeshes
		{
			get {
				foreach (var item in SubMeshes) {
					yield return item;
				}
			}
		}

		public void LoadFromAsimp(in Mesh mesh) {
			MeshName = mesh.Name;
			PrimitiveType = (RPrimitiveType)(byte)(int)mesh.PrimitiveType;
			Vertices = mesh.Vertices.Select((x) => (Vector3f)x).ToList();
			Normals = mesh.Normals.Select((x) => (Vector3f)x).ToList();
			Tangents = mesh.Tangents.Select((x) => (Vector3f)x).ToList();
			BiTangents = mesh.BiTangents.Select((x) => (Vector3f)x).ToList();
			Faces = mesh.Faces.Select((x) => new RFace(x)).ToList();
			Colors = mesh.VertexColorChannels.Select((x) => x.Select((y) => (Colorf)y).ToList()).ToArray();
			TexCoords = mesh.TextureCoordinateChannels.Select((x) => x.Select((y) => (Vector3f)y).ToList()).ToArray();
			TexComponentCount = mesh.UVComponentCount;
			Bones = mesh.Bones.Select((x) => new RBone(x)).ToList();
			MeshAttachments = mesh.MeshAnimationAttachments.Select((x) => new RAnimationAttachment(x)).ToList();
			MorphingMethod = (RMeshMorphingMethod)(byte)(int)mesh.MorphMethod;
		}


		public int AddSubMesh(in IComplexMesh complexMesh) {
			if (complexMesh.HasSubMeshs) {
				throw new Exception("Adding Mesh Already Has a submesh");
			}
			var startingVert = Vertices.Count;
			Vertices.AddRange(complexMesh.Vertices);
			if (Normals.Count > 0) {
				Normals.AddRange(complexMesh.Normals);
			}
			if (Tangents.Count > 0) {
				Tangents.AddRange(complexMesh.Tangents);
			}
			if (BiTangents.Count > 0) {
				BiTangents.AddRange(complexMesh.BiTangents);
			}
			for (var i = 0; i < Colors.Length; i++) {
				if (Colors[i].Count > 0) {
					if (complexMesh.Colors.Length > i) {
						Colors[i].AddRange(complexMesh.Colors[i]);
					}
				}
			}
			for (var i = 0; i < TexCoords.Length; i++) {
				if (TexCoords[i].Count > 0) {
					if (complexMesh.TexCoords.Length > i) {
						TexCoords[i].AddRange(complexMesh.TexCoords[i]);
					}
				}
			}
			foreach (var item in complexMesh.Bones) {
				for (var i = 0; i < Bones.Count; i++) {
					if(item.Name == Bones[i].Name) {
						Bones[i].VertexWeights.AddRange(item.VertexWeights.Select((x) => new RVertexWeight { VertexID = x.VertexID + startingVert, Weight = x.Weight }));
					}
				}
			}
			foreach (var item in complexMesh.MeshAttachments) {
				for (var i = 0; i < MeshAttachments.Count; i++) {
					if (item.Name == MeshAttachments[i].Name) {
						var selected = MeshAttachments[i];
						if (selected is null) {
							break;
						}
						selected.Vertices.AddRange(item.Vertices);
						if (selected.Normals.Count > 0) {
							selected.Normals.AddRange(item.Normals);
						}
						if (selected.Tangents.Count > 0) {
							selected.Tangents.AddRange(item.Tangents);
						}
						if (selected.BiTangents.Count > 0) {
							selected.BiTangents.AddRange(item.BiTangents);
						}
						for (var x = 0; x < selected.Colors.Length; x++) {
							if (selected.Colors[x].Count > 0) {
								if (item.Colors.Length > x) {
									selected.Colors[x].AddRange(item.Colors[x]);
								}
							}
						}
						for (var x = 0; x < selected.TexCoords.Length; x++) {
							if (selected.TexCoords[x].Count > 0) {
								if (item.TexCoords.Length > x) {
									selected.TexCoords[x].AddRange(complexMesh.TexCoords[x]);
								}
							}
						}
					}
				}
			}
			var addedFaces = new List<RFace>();
			foreach (var item in complexMesh.Faces) {
				addedFaces.Add(item.CopyAndOffset(startingVert));
			}
			SubMeshes.Add(new RSubMesh(complexMesh.PrimitiveType, addedFaces, complexMesh.VertexCount));
			return SubMeshes.Count - 1;
		}

		[IgnoreMember]
		public bool IsTriangleMesh => PrimitiveType == RPrimitiveType.Triangle;
		[IgnoreMember]
		public int TriangleCount => Faces.Count;
		[IgnoreMember]
		public int MaxTriangleID => Faces.Count;
		[IgnoreMember]
		public bool HasVertexUVs => (TexCoords.Length > 0) & TexCoords[0].Count > 0;
		[IgnoreMember]
		public bool HasTriangleGroups => PrimitiveType == RPrimitiveType.Triangle;
		[IgnoreMember]
		public int VertexCount => Vertices.Count;
		[IgnoreMember]
		public int MaxVertexID => Vertices.Count;
		[IgnoreMember]
		public bool HasVertexNormals => Normals.Count > 0;
		[IgnoreMember]
		public bool HasVertexColors => (Colors.Length > 0) & Colors[0].Count > 0;
		[IgnoreMember]
		public int Timestamp => int.MaxValue;
		[IgnoreMember]
		string IComplexMesh.MeshName => MeshName;
		[IgnoreMember]
		RPrimitiveType IComplexMesh.PrimitiveType => PrimitiveType;
		[IgnoreMember]
		IEnumerable<IBone> IComplexMesh.Bones
		{
			get {
				foreach (var item in Bones) {
					yield return item;
				}
			}
		}
		[IgnoreMember]
		IEnumerable<IFace> IComplexMesh.Faces
		{
			get {
				foreach (var item in Faces) {
					yield return item;
				}
			}
		}
		[IgnoreMember]
		int[] IComplexMesh.TexComponentCount => TexComponentCount;
		[IgnoreMember]
		IEnumerable<IAnimationAttachment> IComplexMesh.MeshAttachments
		{
			get {
				foreach (var item in MeshAttachments) {
					yield return item;
				}
			}
		}

		[IgnoreMember]
		RMeshMorphingMethod IComplexMesh.MorphingMethod => MorphingMethod;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.Vertices => Vertices;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.Normals => Normals;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.Tangents => Tangents;
		[IgnoreMember]
		List<Vector3f> IRawComplexMeshData.BiTangents => BiTangents;
		[IgnoreMember]
		List<Colorf>[] IRawComplexMeshData.Colors => Colors;
		[IgnoreMember]
		List<Vector3f>[] IRawComplexMeshData.TexCoords => TexCoords;
		[IgnoreMember]
		public bool HasBones => Bones.Count > 0;
		[IgnoreMember]
		public bool HasMeshAttachments => MeshAttachments.Count > 0;
		[IgnoreMember]
		public bool IsBasicMesh => !(HasBones || HasMeshAttachments);

		public Vector2f GetVertexUV(in int i, in int channel = 1) {
			return TexCoords[channel - 1][i].Xy;
		}

		public NewVertexInfo GetVertexAll(in int i) {
			var vi = new NewVertexInfo {
				v = GetVertex(i)
			};
			if (HasVertexNormals) {
				vi.bHaveN = true;
				vi.n = GetVertexNormal(i);
			}
			else {
				vi.bHaveN = false;
			}

			if (HasVertexColors) {
				vi.bHaveC = true;
				vi.c = GetVertexColor(i);
			}
			else {
				vi.bHaveC = false;
			}

			if (HasVertexUVs) {
				vi.bHaveUV = true;
				vi.uv = new Vector2f[TexCoords.Length];
				for (var v = 0; v < TexCoords.Length; v++) {
					if (TexCoords[v].Count > i) {
						var uv = TexCoords[v][i].Xy;
						vi.uv[v] = new Vector2f(uv.x, 1f - uv.y);
					}
				}
			}
			else {
				vi.bHaveUV = false;
			}

			return vi;
		}

		public Index3i GetTriangle(in int i) {
			var face = Faces[i];
			return new Index3i(face.Indices[0], face.Indices[1], face.Indices[2]);
		}

		public int GetTriangleGroup(in int i) {
			throw new NotSupportedException();
		}

		public IEnumerable<int> TriangleIndices() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return i;
			}
		}

		public IEnumerable<int> RenderIndices() {
			foreach (var item in Faces) {
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
			}
			foreach (var submesh in SubMeshes) {
				foreach (var item in submesh.Faces) {
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
				}
			}
		}

		public Vector3d GetVertex(in int i) {
			return new Vector3d(Vertices[i]);
		}

		public Vector3f GetVertexNormal(in int i) {
			return Normals[i];
		}

		public Vector3f GetVertexColor(in int i) {
			return Colors[0][i].ToRGB().IsAnyNan ? new Vector3f(1f) : Colors[0][i].ToRGB();
		}

		public bool IsVertex(in int vID) {
			return vID * 3 < Vertices.Count;
		}
		public bool IsTriangle(in int tID) {
			return tID * 3 < Faces.Count;
		}

		public IEnumerable<int> VertexIndices() {
			var N = VertexCount;
			for (var i = 0; i < N; ++i) {
				yield return i;
			}
		}

		public IEnumerable<Vector3f> VertexPos() {
			foreach (var item in Vertices) {
				yield return item;
			}
		}

		public ComplexMesh(in Mesh asimpMesh) {
			LoadFromAsimp(asimpMesh);
		}
		public ComplexMesh() {

		}
	}
}
