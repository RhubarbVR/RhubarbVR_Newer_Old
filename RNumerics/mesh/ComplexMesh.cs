using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

using Assimp;

using Newtonsoft.Json;

namespace RNumerics
{
	public struct RSubMesh : ISubMesh
	{
		public RPrimitiveType rPrimitiveType;
		public RPrimitiveType PrimitiveType => rPrimitiveType;
		public List<RFace> rFaces;
		public int rCount;
		[JsonIgnore]
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
		[JsonIgnore]
		public IEnumerable<IFace> Faces
		{
			get {
				foreach (var item in rFaces) {
					yield return item;
				}
			}
		}
	}


	public struct RVertexWeight : IVertexWeight
	{
		/// <summary>
		///  Index of the vertex which is influenced by the bone.
		/// </summary>
		public int VertexID;

		/// <summary>
		/// Strength of the influence in range of (0...1). All influences from all bones
		/// at one vertex amounts to 1.
		/// </summary>
		public float Weight;

		public RVertexWeight(in VertexWeight vertexWeight) {
			VertexID = vertexWeight.VertexID;
			Weight = vertexWeight.Weight;
		}
		public RVertexWeight(in float weight, in int vertexID) {
			VertexID = vertexID;
			Weight = weight;
		}

		public RVertexWeight() {
			VertexID = 0;
			Weight = 0;
		}

		[JsonIgnore]
		int IVertexWeight.VertexID => VertexID;
		[JsonIgnore]
		float IVertexWeight.Weight => Weight;
	}


	public sealed class RBone : IBone
	{

		public Matrix OffsetMatrix = Matrix.Identity;

		public List<RVertexWeight> VertexWeights = new();

		public string BoneName;

		[JsonIgnore]
		public bool HasVertexWeights => VertexWeights.Count > 0;

		[JsonIgnore]
		public int VertexWeightCount => VertexWeights.Count;
		[JsonIgnore]
		public string Name => BoneName;
		[JsonIgnore]
		Matrix IBone.OffsetMatrix => OffsetMatrix;
		[JsonIgnore]
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

	public sealed class RFace : IFace
	{
		public List<int> Indices = new();
		[JsonIgnore]
		List<int> IFace.Indices => Indices;

		public RFace(params int[] face) {
			Indices = face.ToList();
		}

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
		None = 0,
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
		Polygon = 0x8,
		All = Point | Line | Triangle | Polygon,
	}


	public sealed class RAnimationAttachment : IAnimationAttachment
	{

		public string Name = "Unknown";
		public List<Vector3f> Vertices = new();
		public List<Vector3f> Normals = new();
		public List<Vector3f> Tangents = new();
		public List<Vector3f> BiTangents = new();
		public List<Colorf>[] Colors = Array.Empty<List<Colorf>>();
		public List<Vector3f>[] TexCoords = Array.Empty<List<Vector3f>>();
		public float Weight;
		[JsonIgnore]
		float IAnimationAttachment.Weight => Weight;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.Vertices => Vertices;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.Normals => Normals;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.Tangents => Tangents;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.BiTangents => BiTangents;
		[JsonIgnore]
		List<Colorf>[] IRawComplexMeshData.Colors => Colors;
		[JsonIgnore]
		List<Vector3f>[] IRawComplexMeshData.TexCoords => TexCoords;
		[JsonIgnore]
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

	[Flags]
	public enum SaveFlags : ushort
	{
		None = 0,
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
		Polygon = 0x8,
		Normals = 0x10,
		Tangents = 0x20,
		Colors = 0x40,
		UVs = 0x80,
		Bones = 0x100,
		ShapeKeys = 0x200,
		/// <summary>
		/// Interpolation between morph targets.
		/// </summary>
		VertexBlend = 0x400,
		/// <summary>
		/// Normalized morphing between morph targets.
		/// </summary>
		MorphNormalized = 0x600,
		/// <summary>
		/// Relative morphing between morph targets.
		/// </summary>
		MorphRelative = 0x800,
		HasSubMeshes = 0x1000,
		NOTSetTwo = 0x2000,
		NOTSetThree = 0x4000,
		NOTSetFour = 0x8000,
	}


	public sealed class ComplexMesh : IComplexMesh, IMesh, ISerlize<ComplexMesh>
	{
		public const byte MESH_VERSION = 0;

		public void Serlize(BinaryWriter binaryWriter) {
			WriteData(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			ReadData(binaryReader);
		}

		public void WriteData(BinaryWriter binaryWriter) {
			binaryWriter.Write(MESH_VERSION);
			binaryWriter.Write(MeshName);
			var flags = SaveFlags.None;
			flags |= (SaveFlags)PrimitiveType;
			flags |= MorphingMethod switch {
				RMeshMorphingMethod.None => SaveFlags.None,
				RMeshMorphingMethod.VertexBlend => SaveFlags.VertexBlend,
				RMeshMorphingMethod.MorphNormalized => SaveFlags.MorphNormalized,
				RMeshMorphingMethod.MorphRelative => SaveFlags.MorphRelative,
				_ => SaveFlags.None,
			};
			if (HasVertexNormals) {
				flags |= SaveFlags.Normals;
			}
			if (HasVertexUVs) {
				flags |= SaveFlags.UVs;
			}
			if (Tangents.Count != 0) {
				flags |= SaveFlags.Tangents;
			}
			if (HasVertexColors) {
				flags |= SaveFlags.Colors;
			}
			if (HasBones) {
				flags |= SaveFlags.Bones;
			}
			if (HasMeshAttachments) {
				flags |= SaveFlags.ShapeKeys;
			}
			if (SubMeshes.Count != 0) {
				flags |= SaveFlags.HasSubMeshes;
			}
			binaryWriter.Write((ushort)flags);
			binaryWriter.Write(VertexCount);
			if (HasVertexColors) {
				var ColorChannelCount = Colors.Length;
				for (var i = 0; i < Colors.Length; i++) {
					if (Colors[i].Count != 0) {
						ColorChannelCount = i + 1;
					}
					else {
						break;
					}
				}
				binaryWriter.Write(ColorChannelCount);
			}
			if (HasVertexUVs) {
				var textCordAmount = TexCoords.Length;
				for (var i = 0; i < TexCoords.Length; i++) {
					if (TexCoords[i].Count != 0) {
						textCordAmount = i + 1;
					}
				}
				binaryWriter.Write(textCordAmount);
				for (var i = 0; i < textCordAmount; i++) {
					binaryWriter.Write((byte)TexComponentCount[i]);
				}
			}
			for (var i = 0; i < VertexCount; i++) {
				var vert = Vertices[i];
				binaryWriter.Write(vert.x);
				binaryWriter.Write(vert.y);
				binaryWriter.Write(vert.z);
				if (HasVertexNormals) {
					var norm = Normals[i];
					binaryWriter.Write(norm.x);
					binaryWriter.Write(norm.y);
					binaryWriter.Write(norm.z);
				}
				if (Tangents.Count != 0) {
					var tan = Tangents[i];
					var BiT = BiTangents[i];
					binaryWriter.Write(tan.x);
					binaryWriter.Write(tan.y);
					binaryWriter.Write(tan.z);
					binaryWriter.Write(BiT.x);
					binaryWriter.Write(BiT.y);
					binaryWriter.Write(BiT.z);
				}
				if (HasVertexColors) {
					for (var currentColor = 0; currentColor < Colors.Length; currentColor++) {
						if (Colors[currentColor].Count == 0) {
							break;
						}
						var coler = Colors[currentColor][i];
						binaryWriter.Write(coler.r);
						binaryWriter.Write(coler.g);
						binaryWriter.Write(coler.b);
						binaryWriter.Write(coler.a);
					}
				}
				if (HasVertexUVs) {
					for (var uvChannel = 0; uvChannel < TexCoords.Length; uvChannel++) {
						var amountOfChannel = TexComponentCount[uvChannel];
						if (amountOfChannel == 0) {
							continue;
						}
						var uv = TexCoords[uvChannel][i];
						if (amountOfChannel == 1) {
							binaryWriter.Write(uv.x);
						}
						else if (amountOfChannel == 2) {
							binaryWriter.Write(uv.x);
							binaryWriter.Write(uv.y);
						}
						else if (amountOfChannel == 3) {
							binaryWriter.Write(uv.x);
							binaryWriter.Write(uv.y);
							binaryWriter.Write(uv.z);
						}
						else {
							throw new InvalidDataException($"Amount of Channel was {amountOfChannel}");
						}
					}
				}
			}

			var faces = Faces.GroupBy(x => x.Indices.Count).ToList();
			binaryWriter.Write(faces.Count);
			for (var i = 0; i < faces.Count; i++) {
				var group = faces[i];
				var values = group.ToArray();
				binaryWriter.Write((ushort)group.Key);
				binaryWriter.Write(values.Length);
				foreach (var item in values.SelectMany(x => x.Indices)) {
					binaryWriter.Write(item);
				}
			}

			if (SubMeshes.Count != 0) {
				binaryWriter.Write(SubMeshes.Count);
				for (var subMesh = 0; subMesh < SubMeshes.Count; subMesh++) {
					var currentSubMesh = SubMeshes[subMesh];
					binaryWriter.Write((byte)currentSubMesh.rPrimitiveType);
					var subMeshfaces = currentSubMesh.rFaces.GroupBy(x => x.Indices.Count).ToList();
					binaryWriter.Write(subMeshfaces.Count);
					for (var i = 0; i < subMeshfaces.Count; i++) {
						var group = subMeshfaces[i];
						var values = group.ToArray();
						binaryWriter.Write((ushort)group.Key);
						binaryWriter.Write(values.Length);
						foreach (var item in values.SelectMany(x => x.Indices)) {
							binaryWriter.Write(item);
						}
					}

				}
			}

			if (HasBones) {
				binaryWriter.Write(Bones.Count);
				for (var i = 0; i < Bones.Count; i++) {
					var currentBone = Bones[i];
					binaryWriter.Write(currentBone.BoneName);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M11);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M12);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M13);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M14);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M21);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M22);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M23);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M24);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M31);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M32);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M33);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M34);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M41);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M42);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M43);
					binaryWriter.Write(currentBone.OffsetMatrix.m.M44);
					binaryWriter.Write(currentBone.VertexWeights.Count);
					for (var curentWeights = 0; curentWeights < currentBone.VertexWeights.Count; curentWeights++) {
						binaryWriter.Write(currentBone.VertexWeights[curentWeights].Weight);
						binaryWriter.Write(currentBone.VertexWeights[curentWeights].VertexID);
					}
				}
			}

			if (HasMeshAttachments) {
				binaryWriter.Write(MeshAttachments.Count);
				for (var attach = 0; attach < MeshAttachments.Count; attach++) {
					var attachment = MeshAttachments[attach];
					binaryWriter.Write(attachment.Name);
					binaryWriter.Write(attachment.Weight);
					var meshAttachflags = (byte)0x00;
					var hasVert = false;
					var hasNorm = false;
					var hasTan = false;
					var hasBit = false;
					var hasColor = false;
					var hasUV = false;
					var saneVertsIndex = new HashSet<int>();
					var saneNormsIndex = new HashSet<int>();
					var saneTansIndex = new HashSet<int>();
					var saneBitsIndex = new HashSet<int>();
					var saneColorIndex = new HashSet<int>();
					var saneUVIndex = new HashSet<int>();

					for (var i = 0; i < VertexCount; i++) {
						var vert = attachment.Vertices[i];
						if (vert != Vertices[i]) {
							meshAttachflags |= 0x01;
							hasVert = true;
						}
						else {
							saneVertsIndex.Add(i);
						}
						if (HasVertexNormals) {
							var norm = attachment.Normals[i];
							if (norm != Normals[i]) {
								meshAttachflags |= 0x02;
								hasNorm = true;
							}
							else {
								saneNormsIndex.Add(i);
							}
						}
						if (Tangents.Count != 0) {
							var tan = attachment.Tangents[i];
							if (tan != Tangents[i]) {
								meshAttachflags |= 0x04;
								hasTan = true;
							}
							else {
								saneTansIndex.Add(i);
							}
							var BiT = attachment.BiTangents[i];
							if (BiT != BiTangents[i]) {
								meshAttachflags |= 0x08;
								hasBit = true;
							}
							else {
								saneBitsIndex.Add(i);
							}
						}
						if (HasVertexColors) {
							for (var currentColor = 0; currentColor < Colors.Length; currentColor++) {
								if (Colors[currentColor].Count == 0) {
									continue;
								}
								var coler = attachment.Colors[currentColor][i];
								if (coler != Colors[currentColor][i]) {
									meshAttachflags |= 0x10;
									hasColor = true;
								}
								else {
									saneColorIndex.Add(i);
								}
							}
						}
						if (HasVertexUVs) {
							for (var uvChannel = 0; uvChannel < TexCoords.Length; uvChannel++) {
								var amountOfChannel = TexComponentCount[uvChannel];
								if (amountOfChannel == 0) {
									continue;
								}
								var uv = attachment.TexCoords[uvChannel][i];
								if (uv != TexCoords[uvChannel][i]) {
									meshAttachflags |= 0x20;
									hasUV = true;
								}
								else {
									saneUVIndex.Add(i);
								}
							}
						}
					}

					binaryWriter.Write(meshAttachflags);
					if (hasVert) {
						var change = saneVertsIndex.ToArray();
						binaryWriter.Write(change.Length);
						for (var targetIndex = 0; targetIndex < change.Length; targetIndex++) {
							binaryWriter.Write(change[targetIndex]);
						}
					}
					if (hasNorm) {
						var change = saneNormsIndex.ToArray();
						binaryWriter.Write(change.Length);
						for (var targetIndex = 0; targetIndex < change.Length; targetIndex++) {
							binaryWriter.Write(change[targetIndex]);
						}
					}
					if (hasTan) {
						var change = saneTansIndex.ToArray();
						binaryWriter.Write(change.Length);
						for (var targetIndex = 0; targetIndex < change.Length; targetIndex++) {
							binaryWriter.Write(change[targetIndex]);
						}
					}
					if (hasBit) {
						var change = saneBitsIndex.ToArray();
						binaryWriter.Write(change.Length);
						for (var targetIndex = 0; targetIndex < change.Length; targetIndex++) {
							binaryWriter.Write(change[targetIndex]);
						}
					}
					if (hasColor) {
						var change = saneColorIndex.ToArray();
						binaryWriter.Write(change.Length);
						for (var targetIndex = 0; targetIndex < change.Length; targetIndex++) {
							binaryWriter.Write(change[targetIndex]);
						}
					}
					if (hasUV) {
						var change = saneUVIndex.ToArray();
						binaryWriter.Write(change.Length);
						for (var targetIndex = 0; targetIndex < change.Length; targetIndex++) {
							binaryWriter.Write(change[targetIndex]);
						}
					}

					for (var i = 0; i < VertexCount; i++) {
						var vert = attachment.Vertices[i];
						if (hasVert && !saneVertsIndex.Contains(i)) {
							binaryWriter.Write(vert.x);
							binaryWriter.Write(vert.y);
							binaryWriter.Write(vert.z);
						}
						if (HasVertexNormals && hasNorm && !saneNormsIndex.Contains(i)) {
							var norm = attachment.Normals[i];
							binaryWriter.Write(norm.x);
							binaryWriter.Write(norm.y);
							binaryWriter.Write(norm.z);
						}
						if (Tangents.Count != 0) {
							var tan = attachment.Tangents[i];
							var BiT = attachment.BiTangents[i];
							if (hasTan && !saneTansIndex.Contains(i)) {
								binaryWriter.Write(tan.x);
								binaryWriter.Write(tan.y);
								binaryWriter.Write(tan.z);
							}
							if (hasBit && !saneBitsIndex.Contains(i)) {
								binaryWriter.Write(BiT.x);
								binaryWriter.Write(BiT.y);
								binaryWriter.Write(BiT.z);
							}
						}
						if (HasVertexColors && hasColor && !saneColorIndex.Contains(i)) {
							for (var currentColor = 0; currentColor < Colors.Length; currentColor++) {
								if (Colors[currentColor].Count == 0) {
									continue;
								}
								var coler = attachment.Colors[currentColor][i];
								binaryWriter.Write(coler.r);
								binaryWriter.Write(coler.g);
								binaryWriter.Write(coler.b);
								binaryWriter.Write(coler.a);
							}
						}
						if (HasVertexUVs && hasUV && !saneUVIndex.Contains(i)) {
							for (var uvChannel = 0; uvChannel < TexCoords.Length; uvChannel++) {
								var amountOfChannel = TexComponentCount[uvChannel];
								if (amountOfChannel == 0) {
									continue;
								}
								var uv = attachment.TexCoords[uvChannel][i];
								if (amountOfChannel == 1) {
									binaryWriter.Write(uv.x);
								}
								else if (amountOfChannel == 2) {
									binaryWriter.Write(uv.x);
									binaryWriter.Write(uv.y);
								}
								else if (amountOfChannel == 3) {
									binaryWriter.Write(uv.x);
									binaryWriter.Write(uv.y);
									binaryWriter.Write(uv.z);
								}
								else {
									throw new InvalidDataException($"Amount of Channel was {amountOfChannel}");
								}
							}
						}
					}

				}
			}

		}


		public void ReadData(BinaryReader reader) {
			var version = reader.ReadByte();
			if (version != MESH_VERSION) {
				//Add old Read code here

				throw new Exception("Don't know how to read mesh");
			}
			MeshName = reader.ReadString();
			var saveFlags = (SaveFlags)reader.ReadUInt16();
			PrimitiveType = (RPrimitiveType)saveFlags & RPrimitiveType.All;
			MorphingMethod = saveFlags.HasFlag(SaveFlags.MorphRelative)
				? RMeshMorphingMethod.MorphRelative
				: saveFlags.HasFlag(SaveFlags.VertexBlend)
					? RMeshMorphingMethod.VertexBlend
					: saveFlags.HasFlag(SaveFlags.MorphNormalized) ? RMeshMorphingMethod.MorphNormalized : RMeshMorphingMethod.None;

			var vertCount = reader.ReadInt32();

			if (saveFlags.HasFlag(SaveFlags.Colors)) {
				var colorChannelCount = reader.ReadInt32();
				Colors = new List<Colorf>[colorChannelCount];
				for (var i = 0; i < Colors.Length; i++) {
					Colors[i] ??= new List<Colorf>();
				}
			}
			if (saveFlags.HasFlag(SaveFlags.UVs)) {
				var textCordAmount = reader.ReadInt32();
				TexComponentCount = new int[textCordAmount];
				TexCoords = new List<Vector3f>[textCordAmount];
				for (var i = 0; i < textCordAmount; i++) {
					TexComponentCount[i] = reader.ReadByte();
					TexCoords[i] = new List<Vector3f>();
				}
			}

			for (var i = 0; i < vertCount; i++) {
				Vertices.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				if (saveFlags.HasFlag(SaveFlags.Normals)) {
					Normals.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				}
				if (saveFlags.HasFlag(SaveFlags.Tangents)) {
					Tangents.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
					BiTangents.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				}
				if (saveFlags.HasFlag(SaveFlags.Colors)) {
					for (var currentColor = 0; currentColor < Colors.Length; currentColor++) {
						Colors[currentColor].Add(new Colorf(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
					}
				}
				if (saveFlags.HasFlag(SaveFlags.UVs)) {
					for (var uvChannel = 0; uvChannel < TexCoords.Length; uvChannel++) {
						var amountOfChannel = TexComponentCount[uvChannel];
						if (amountOfChannel == 0) {
							continue;
						}
						if (amountOfChannel == 1) {
							TexCoords[uvChannel].Add(new Vector3f(reader.ReadSingle(), 0, 0));
						}
						else if (amountOfChannel == 2) {
							TexCoords[uvChannel].Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), 0));
						}
						else if (amountOfChannel == 3) {
							TexCoords[uvChannel].Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
						}
						else {
							throw new InvalidDataException($"Amount of Channel was {amountOfChannel}");
						}
					}
				}
			}


			var amountOfFaceGroups = reader.ReadInt32();
			for (var i = 0; i < amountOfFaceGroups; i++) {
				var amounOfInexs = reader.ReadUInt16();
				var ammountOfFaces = reader.ReadInt32();
				for (var faceIndex = 0; faceIndex < ammountOfFaces; faceIndex++) {
					var face = new RFace();
					for (var readVal = 0; readVal < amounOfInexs; readVal++) {
						face.Indices.Add(reader.ReadInt32());
					}
					Faces.Add(face);
				}
			}

			if (saveFlags.HasFlag(SaveFlags.HasSubMeshes)) {
				var subMeshCount = reader.ReadInt32();
				for (var subMesh = 0; subMesh < subMeshCount; subMesh++) {
					var primitiveType = (RPrimitiveType)reader.ReadByte();
					var subMeshFaces = new List<RFace>();
					var amountOfFaceGroupsSub = reader.ReadInt32();
					for (var i = 0; i < amountOfFaceGroupsSub; i++) {
						var amounOfInexs = reader.ReadUInt16();
						var ammountOfFaces = reader.ReadInt32();
						for (var faceIndex = 0; faceIndex < ammountOfFaces; faceIndex++) {
							var face = new RFace();
							for (var readVal = 0; readVal < amounOfInexs; readVal++) {
								face.Indices.Add(reader.ReadInt32());
							}
							subMeshFaces.Add(face);
						}
					}
					SubMeshes.Add(new RSubMesh(primitiveType, subMeshFaces, subMeshFaces.Count));
				}
			}

			if (saveFlags.HasFlag(SaveFlags.Bones)) {
				var boneCount = reader.ReadInt32();
				Bones.Clear();
				for (var i = 0; i < boneCount; i++) {
					var newBone = new RBone {
						BoneName = reader.ReadString(),
						OffsetMatrix = new Matrix(new System.Numerics.Matrix4x4(
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle(),
								reader.ReadSingle()))
					};
					var wateCount = reader.ReadInt32();
					for (var curentWeights = 0; curentWeights < wateCount; curentWeights++) {
						newBone.VertexWeights.Add(new RVertexWeight(reader.ReadSingle(), reader.ReadInt32()));
					}
					Bones.Add(newBone);
				}
			}

			if (saveFlags.HasFlag(SaveFlags.ShapeKeys)) {
				MeshAttachments.Clear();
				var amountOfShapeKeys = reader.ReadInt32();
				for (var attach = 0; attach < amountOfShapeKeys; attach++) {
					var attachment = new RAnimationAttachment {
						Name = reader.ReadString(),
						Weight = reader.ReadSingle()
					};
					var meshAttachflags = reader.ReadByte();
					var hasVert = (meshAttachflags & 0x01) != 0;
					var hasNorm = (meshAttachflags & 0x02) != 0;
					var hasTan = (meshAttachflags & 0x04) != 0;
					var hasBit = (meshAttachflags & 0x08) != 0;
					var hasColor = (meshAttachflags & 0x10) != 0;
					var hasUV = (meshAttachflags & 0x20) != 0;
					var Verts = new HashSet<int>();
					var Norms = new HashSet<int>();
					var Tans = new HashSet<int>();
					var Bits = new HashSet<int>();
					var _Colors = new HashSet<int>();
					var UVs = new HashSet<int>();
					if (hasVert) {
						var length = reader.ReadInt32();
						for (var targetIndex = 0; targetIndex < length; targetIndex++) {
							Verts.Add(reader.ReadInt32());
						}
					}
					if (hasNorm) {
						var length = reader.ReadInt32();
						for (var targetIndex = 0; targetIndex < length; targetIndex++) {
							Norms.Add(reader.ReadInt32());
						}
					}
					if (hasTan) {
						var length = reader.ReadInt32();
						for (var targetIndex = 0; targetIndex < length; targetIndex++) {
							Tans.Add(reader.ReadInt32());
						}
					}
					if (hasBit) {
						var length = reader.ReadInt32();
						for (var targetIndex = 0; targetIndex < length; targetIndex++) {
							Bits.Add(reader.ReadInt32());
						}
					}
					if (hasColor) {
						var length = reader.ReadInt32();
						for (var targetIndex = 0; targetIndex < length; targetIndex++) {
							_Colors.Add(reader.ReadInt32());
						}
					}
					if (hasUV) {
						var length = reader.ReadInt32();
						for (var targetIndex = 0; targetIndex < length; targetIndex++) {
							UVs.Add(reader.ReadInt32());
						}
					}
					for (var i = 0; i < VertexCount; i++) {
						if (hasVert && !Verts.Contains(i)) {
							attachment.Vertices.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
						}
						if (hasVert && Verts.Contains(i)) {
							attachment.Vertices.Add(Vertices[i]);
						}
						if (hasNorm && !Norms.Contains(i)) {
							attachment.Normals.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
						}
						if (hasNorm && Norms.Contains(i)) {
							attachment.Normals.Add(Normals[i]);
						}
						if (hasTan && !Tans.Contains(i)) {
							attachment.Tangents.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
						}
						if (hasTan && Tans.Contains(i)) {
							attachment.Tangents.Add(Tangents[i]);
						}
						if (hasBit && !Bits.Contains(i)) {
							attachment.BiTangents.Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
						}
						if (hasBit && Bits.Contains(i)) {
							attachment.BiTangents.Add(BiTangents[i]);
						}
						if (hasColor) {
							attachment.Colors = new List<Colorf>[Colors.Length];
							for (var c = 0; c < Colors.Length; c++) {
								attachment.Colors[c] = new List<Colorf>();
							}
						}
						if (hasColor && !_Colors.Contains(i)) {
							for (var currentColor = 0; currentColor < Colors.Length; currentColor++) {
								var coler = attachment.Colors[currentColor];
								coler.Add(new Colorf(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
							}
						}
						if (hasColor && _Colors.Contains(i)) {
							for (var currentColor = 0; currentColor < Colors.Length; currentColor++) {
								attachment.Colors[currentColor].Add(Colors[currentColor][i]);
							}
						}
						if (hasUV) {
							attachment.TexCoords = new List<Vector3f>[TexCoords.Length];
							for (var t = 0; t < TexCoords.Length; t++) {
								attachment.TexCoords[t] = new List<Vector3f>();
							}
						}
						if (hasUV && !UVs.Contains(i)) {
							for (var uvChannel = 0; uvChannel < TexCoords.Length; uvChannel++) {
								var amountOfChannel = TexComponentCount[uvChannel];
								if (amountOfChannel == 0) {
									continue;
								}
								if (amountOfChannel == 1) {
									attachment.TexCoords[uvChannel].Add(new Vector3f(reader.ReadSingle(), 0, 0));
								}
								else if (amountOfChannel == 2) {
									attachment.TexCoords[uvChannel].Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), 0));
								}
								else if (amountOfChannel == 3) {
									attachment.TexCoords[uvChannel].Add(new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
								}
								else {
									throw new InvalidDataException($"Amount of Channel was {amountOfChannel}");
								}
							}
						}
						if (hasUV && UVs.Contains(i)) {
							for (var uvChannel = 0; uvChannel < TexCoords.Length; uvChannel++) {
								var amountOfChannel = TexComponentCount[uvChannel];
								if (amountOfChannel == 0) {
									continue;
								}
								attachment.TexCoords[uvChannel].Add(TexCoords[uvChannel][i]);
							}
						}
					}
					MeshAttachments.Add(attachment);
				}
			}
		}




		public string MeshName;

		public RPrimitiveType PrimitiveType;

		public RMeshMorphingMethod MorphingMethod = RMeshMorphingMethod.None;

		public List<Vector3f> Vertices = new();

		public List<Vector3f> Normals = new();

		public List<Vector3f> Tangents = new();

		public List<Vector3f> BiTangents = new();

		public List<RFace> Faces = new();

		public List<Colorf>[] Colors = Array.Empty<List<Colorf>>();

		public List<Vector3f>[] TexCoords = Array.Empty<List<Vector3f>>();

		public int[] TexComponentCount = Array.Empty<int>();

		public List<RBone> Bones = new();
		[JsonIgnore]
		public int BonesCount => Bones.Count;

		public List<RAnimationAttachment> MeshAttachments = new();

		public List<RSubMesh> SubMeshes = new();

		[JsonIgnore]
		bool IComplexMesh.HasSubMeshs => SubMeshes.Count > 0;

		[JsonIgnore]
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
			if (MorphingMethod == RMeshMorphingMethod.None && complexMesh.MorphingMethod != RMeshMorphingMethod.None) {
				MorphingMethod = complexMesh.MorphingMethod;
			}
			if (complexMesh.MorphingMethod != MorphingMethod) {
				throw new Exception("Not able to connvert MorphingMethod to be the same for mesh");
			}
			var startingVert = Vertices.Count;
			Vertices.AddRange(complexMesh.Vertices);
			if (Normals.Count > 0) {
				if (complexMesh.Normals.Count > 0) {
					Normals.AddRange(complexMesh.Normals);
				}
				else {
					for (var i = 0; i < complexMesh.VertexCount; i++) {
						Normals.Add(Vector3f.Zero);
					}
				}
			}
			if (Tangents.Count > 0) {
				if (complexMesh.Tangents.Count > 0) {
					Tangents.AddRange(complexMesh.Tangents);
				}
				else {
					for (var i = 0; i < complexMesh.VertexCount; i++) {
						Tangents.Add(Vector3f.Zero);
					}
				}
			}
			if (BiTangents.Count > 0) {
				if (complexMesh.BiTangents.Count > 0) {
					BiTangents.AddRange(complexMesh.BiTangents);
				}
				else {
					for (var i = 0; i < complexMesh.VertexCount; i++) {
						BiTangents.Add(Vector3f.Zero);
					}
				}
			}
			for (var i = 0; i < Colors.Length; i++) {
				if (Colors[i].Count > 0) {
					if (complexMesh.Colors.Length > i) {
						Colors[i].AddRange(complexMesh.Colors[i]);
					}
					else {
						for (var c = 0; c < complexMesh.VertexCount; c++) {
							Colors[i].Add(Colorf.White);
						}
					}
				}
			}
			for (var i = 0; i < TexCoords.Length; i++) {
				if (TexCoords[i].Count > 0) {
					if (complexMesh.TexCoords.Length > i) {
						TexCoords[i].AddRange(complexMesh.TexCoords[i]);
					}
					else {
						for (var c = 0; c < complexMesh.VertexCount; c++) {
							TexCoords[i].Add(Vector3f.Zero);
						}
					}
				}
			}
			foreach (var item in complexMesh.Bones) {
				for (var i = 0; i < Bones.Count; i++) {
					if (item.Name == Bones[i].Name) {
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

		[JsonIgnore]
		public bool IsTriangleMesh => PrimitiveType == RPrimitiveType.Triangle;
		[JsonIgnore]
		public int TriangleCount => Faces.Count + SubMeshTriangleCount;
		[JsonIgnore]
		public int SubMeshTriangleCount
		{
			get {
				var amount = 0;
				foreach (var item in SubMeshes) {
					amount += item.rFaces.Count;
				}
				return amount;
			}
		}
		[JsonIgnore]
		public int MaxTriangleID => Faces.Count;
		[JsonIgnore]
		public bool HasVertexUVs => (TexCoords.Length > 0) & TexCoords[0].Count > 0;
		[JsonIgnore]
		public bool HasTriangleGroups => PrimitiveType == RPrimitiveType.Triangle;
		[JsonIgnore]
		public int VertexCount => Vertices.Count;
		[JsonIgnore]
		public int MaxVertexID => Vertices.Count;
		[JsonIgnore]
		public bool HasVertexNormals => Normals.Count > 0;
		[JsonIgnore]
		public bool HasVertexColors
		{
			get {
				for (var i = 0; i < Colors.Length; i++) {
					if (Colors[i].Count > 0) {
						return true;
					}
				}
				return false;
			}
		}
		[JsonIgnore]
		public int Timestamp => int.MaxValue;
		[JsonIgnore]
		string IComplexMesh.MeshName => MeshName;
		[JsonIgnore]
		RPrimitiveType IComplexMesh.PrimitiveType => PrimitiveType;
		[JsonIgnore]
		IEnumerable<IBone> IComplexMesh.Bones
		{
			get {
				foreach (var item in Bones) {
					yield return item;
				}
			}
		}
		[JsonIgnore]
		IEnumerable<IFace> IComplexMesh.Faces
		{
			get {
				foreach (var item in Faces) {
					yield return item;
				}
			}
		}
		[JsonIgnore]
		int[] IComplexMesh.TexComponentCount => TexComponentCount;
		[JsonIgnore]
		IEnumerable<IAnimationAttachment> IComplexMesh.MeshAttachments
		{
			get {
				foreach (var item in MeshAttachments) {
					yield return item;
				}
			}
		}

		[JsonIgnore]
		RMeshMorphingMethod IComplexMesh.MorphingMethod => MorphingMethod;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.Vertices => Vertices;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.Normals => Normals;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.Tangents => Tangents;
		[JsonIgnore]
		List<Vector3f> IRawComplexMeshData.BiTangents => BiTangents;
		[JsonIgnore]
		List<Colorf>[] IRawComplexMeshData.Colors => Colors;
		[JsonIgnore]
		List<Vector3f>[] IRawComplexMeshData.TexCoords => TexCoords;
		[JsonIgnore]
		public bool HasBones => Bones.Count > 0;
		[JsonIgnore]
		public bool HasMeshAttachments => MeshAttachments.Count > 0;
		[JsonIgnore]
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
			if (i >= Faces.Count) {
				var faceIndex = i - Faces.Count;
				foreach (var item in SubMeshes) {
					if (faceIndex < item.rFaces.Count) {
						return new Index3i(item.rFaces[faceIndex].Indices[0], item.rFaces[faceIndex].Indices[1], item.rFaces[faceIndex].Indices[2]);
					}
					faceIndex -= item.rFaces.Count;
				}
				return new Index3i();
			}
			else {
				var face = Faces[i];
				return new Index3i(face.Indices[0], face.Indices[1], face.Indices[2]);
			}
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
