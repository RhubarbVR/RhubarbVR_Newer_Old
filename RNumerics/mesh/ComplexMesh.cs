using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assimp;

using MessagePack;

namespace RNumerics
{
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

		public RVertexWeight(VertexWeight vertexWeight) {
			VertexID = vertexWeight.VertexID;
			Weight = vertexWeight.Weight;
		}
		[IgnoreMember]
		int IVertexWeight.VertexID => VertexID;
		[IgnoreMember]
		float IVertexWeight.Weight => Weight;
	}

	[MessagePackObject]
	public class RBone : IBone
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

		public RBone(Bone asimp) {
			OffsetMatrix = Matrix.CreateFromAssimp(asimp.OffsetMatrix);
			BoneName = asimp.Name;
			VertexWeights = asimp.VertexWeights.Select((x) => new RVertexWeight(x)).ToList();
		}
		public RBone() {

		}
	}
	[MessagePackObject]
	public class RFace : IFace
	{
		[Key(0)]
		public List<int> Indices = new();
		[IgnoreMember]
		List<int> IFace.Indices => Indices;

		public RFace(Face face) {
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
	public class RAnimationAttachment : IAnimationAttachment
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

		public RAnimationAttachment(MeshAnimationAttachment meshAnimationAttachment) {
			Vertices = meshAnimationAttachment.Vertices.Select((x) => (Vector3f)x).ToList();
			Normals = meshAnimationAttachment.Normals.Select((x) => (Vector3f)x).ToList();
			Tangents = meshAnimationAttachment.Tangents.Select((x) => (Vector3f)x).ToList();
			BiTangents = meshAnimationAttachment.BiTangents.Select((x) => (Vector3f)x).ToList();
			Colors = meshAnimationAttachment.VertexColorChannels.Select((x) => x.Select((y) => (Colorf)y).ToList()).ToArray();
			TexCoords = meshAnimationAttachment.TextureCoordinateChannels.Select((x) => x.Select((y) => (Vector3f)y).ToList()).ToArray();
			Weight = meshAnimationAttachment.Weight;
			//Name = meshAnimationAttachment.Name; //Todo:UpdateAssimp
		}
		public RAnimationAttachment() {

		}
	}

	[MessagePackObject]
	public class ComplexMesh : IComplexMesh, IMesh
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

		public void LoadFromAsimp(Mesh mesh) {
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
		[IgnoreMember]
		public bool IsTriangleMesh => PrimitiveType == RPrimitiveType.Triangle;
		[IgnoreMember]
		public int TriangleCount => Faces.Count;
		[IgnoreMember]
		public int MaxTriangleID => Faces.Count;
		[IgnoreMember]
		public bool HasVertexUVs => TexCoords.Length > 0;
		[IgnoreMember]
		public bool HasTriangleGroups => PrimitiveType == RPrimitiveType.Triangle;
		[IgnoreMember]
		public int VertexCount => Vertices.Count;
		[IgnoreMember]
		public int MaxVertexID => Vertices.Count;
		[IgnoreMember]
		public bool HasVertexNormals => Normals.Count > 0;
		[IgnoreMember]
		public bool HasVertexColors => Colors.Length > 0;
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
		public bool HasMeshAttachments => MorphingMethod != RMeshMorphingMethod.None;
		[IgnoreMember]
		public bool IsBasicMesh => !(HasBones || HasMeshAttachments);

		public Vector2f GetVertexUV(int i, int channel = 1) {
			return TexCoords[channel - 1][i].Xy;
		}

		public NewVertexInfo GetVertexAll(int i) {
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
				vi.uv = new Vector2f[] { GetVertexUV(i) };
			}
			else {
				vi.bHaveUV = false;
			}

			return vi;
		}

		public Index3i GetTriangle(int i) {
			return new Index3i(Faces[i].Indices.ToArray());
		}

		public int GetTriangleGroup(int i) {
			throw new NotSupportedException();
		}

		public IEnumerable<int> TriangleIndices() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return i;
			}
		}

		public IEnumerable<int> RenderIndices() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return GetTriangle(i).a;
				yield return GetTriangle(i).b;
				yield return GetTriangle(i).c;
			}
		}

		public Vector3d GetVertex(int i) {
			return new Vector3d(Vertices[i]);
		}

		public Vector3f GetVertexNormal(int i) {
			return Normals[i];
		}

		public Vector3f GetVertexColor(int i) {
			return Colors[0][i].ToRGB();
		}

		public bool IsVertex(int vID) {
			return vID * 3 < Vertices.Count;
		}
		public bool IsTriangle(int tID) {
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

		public ComplexMesh(Mesh asimpMesh) {
			LoadFromAsimp(asimpMesh);
		}
		public ComplexMesh() {

		}
	}
}
