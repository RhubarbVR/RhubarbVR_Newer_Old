using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public interface IRawComplexMeshData
	{
		public List<Vector3f> Vertices { get; }
		public List<Vector3f> Normals { get; }
		public List<Vector3f> Tangents { get; }
		public List<Vector3f> BiTangents { get; }
		public List<Colorf>[] Colors { get; }
		public List<Vector3f>[] TexCoords { get; }
	}

	public interface IAnimationAttachment : IRawComplexMeshData
	{
		public string Name { get; }

		public float Weight { get; }
	}



	public interface IVertexWeight
	{
		/// <summary>
		///  Index of the vertex which is influenced by the bone.
		/// </summary>
		public int VertexID { get; }

		/// <summary>
		/// Strength of the influence in range of (0...1). All influences from all bones
		/// at one vertex amounts to 1.
		/// </summary>
		public float Weight { get; }
	}

	public interface IBone
	{
		public string Name { get; }
		public Matrix OffsetMatrix { get; }
		public IEnumerable<IVertexWeight> VertexWeights { get; }
		public bool HasVertexWeights { get; }
		public int VertexWeightCount { get; }
	}

	public interface IFace
	{
		public List<int> Indices { get; }

	}

	public static class FaceHellper 
	{
		public static RFace CopyAndOffset(this IFace copyData, in int startingVert) {
			var indexs = new List<int>(copyData.Indices);
			for (var i = 0; i < indexs.Count; i++) {
				indexs[i] += startingVert;
			}
			return new RFace { Indices = indexs };
		}

	}

	public interface ISubMesh
	{
		public RPrimitiveType PrimitiveType { get; }

		public int Count { get; }

		public IEnumerable<IFace> Faces { get; }
	}

	public interface IComplexMesh : IRawComplexMeshData, IMesh
	{
		public string MeshName { get; }
		public RPrimitiveType PrimitiveType { get; }
		public IEnumerable<IBone> Bones { get; }
		public int BonesCount { get; }
		public bool HasBones { get; }
		public IEnumerable<IFace> Faces { get; }
		public int[] TexComponentCount { get; }

		public IEnumerable<ISubMesh> SubMeshes { get; }

		public bool HasSubMeshs { get; }

		public IEnumerable<IAnimationAttachment> MeshAttachments { get; }
		public bool HasMeshAttachments { get; }
		public bool IsBasicMesh { get; }

		public RMeshMorphingMethod MorphingMethod { get; }
	}

	public interface IPointSet
	{
		int VertexCount { get; }
		int MaxVertexID { get; }

		bool HasVertexNormals { get; }
		bool HasVertexColors { get; }

		Vector3d GetVertex(in int i);
		Vector3f GetVertexNormal(in int i);
		Vector3f GetVertexColor(in int i);

		bool IsVertex(in int vID);

		// iterators allow us to work with gaps in index space
		System.Collections.Generic.IEnumerable<int> VertexIndices();

		System.Collections.Generic.IEnumerable<Vector3f> VertexPos();


		int Timestamp { get; }
	}



	public interface IMesh : IPointSet
	{
		bool IsTriangleMesh { get; }
		int TriangleCount { get; }
		int MaxTriangleID { get; }

		bool HasVertexUVs { get; }
		Vector2f GetVertexUV(in int i, in int channel = 1);

		NewVertexInfo GetVertexAll(in int i);

		bool HasTriangleGroups { get; }

		Index3i GetTriangle(in int i);
		int GetTriangleGroup(in int i);

		bool IsTriangle(in int tID);

		// iterators allow us to work with gaps in index space
		IEnumerable<int> TriangleIndices();

		IEnumerable<int> RenderIndices();
	}

	public static class MeshExtensions
	{
		public static IEnumerable<int> RenderIndicesClockWizeint(this IMesh mesh) {
			int? first = null;
			int? Next = null;
			foreach (var item in mesh.RenderIndices()) {
				if (first == null) {
					first = item;
				}
				else if (Next == null) {
					Next = item;
				}
				else {
					yield return item;
					yield return Next ?? 0;
					yield return first ?? 0;
					first = null;
					Next = null;
				}
			}
		}

		public static IEnumerable<uint> RenderIndicesUint(this IMesh mesh) {
			foreach (var item in mesh.RenderIndices()) {
				yield return (uint)item;
			}
		}
		public static IEnumerable<uint> TriangleIndicesUint(this IMesh mesh) {
			foreach (var item in mesh.TriangleIndices()) {
				yield return (uint)item;
			}
		}
	}

	public sealed class EnumColl<T> : ICollection<T>
	{
		readonly List<T> _enumer;

		public int Count => _enumer.Count();

		public bool IsReadOnly => false;

		public EnumColl(IEnumerable<T> val) {
			_enumer = new List<T>(val);
		}

		public void Add(in T item) {
			_enumer.Add(item);
		}

		public void Clear() {
			_enumer.Clear();
		}

		public bool Contains(in T item) {
			return _enumer.Contains(item);
		}

		public void CopyTo(in T[] array, in int arrayIndex) {
			_enumer.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator() {
			return _enumer.GetEnumerator();
		}

		public bool Remove(in T item) {
			return _enumer.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _enumer.GetEnumerator();
		}

		void ICollection<T>.Add(T item) {
			Add(item);
		}

		bool ICollection<T>.Contains(T item) {
			return Contains(item);
		}

		 void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
			CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.Remove(T item) {
			return Remove(item);
		}
	}
	public static class Helpers
	{
		public static ICollection<int> ColTriangleIndices(this IMesh mesh) {
			return new EnumColl<int>(mesh.TriangleIndices());
		}
		public static ICollection<int> ColRenderIndices(this IMesh mesh) {
			return new EnumColl<int>(mesh.RenderIndices());
		}
	}

	public interface IDeformableMesh : IMesh
	{
		void SetVertex(in int vID, in Vector3d vNewPos);
		void SetVertexNormal(in int vid, in Vector3f vNewNormal);
	}



	/*
	 * Abstracts construction of meshes, so that we can construct different types, etc
	 */
	public struct NewVertexInfo
	{
		public Vector3d v;
		public Vector3f n, c;
		public Vector2f[] uv;
		public bool bHaveN, bHaveUV, bHaveC;

		public NewVertexInfo(in Vector3d v, in Vector2f nuv, in Colorf color) {
			this.v = v;
			n = c = Vector3f.Zero;
			c = color.ToRGB();
			uv = new Vector2f[] { nuv };
			bHaveN = false;
			bHaveC = bHaveUV = true;
		}

		public NewVertexInfo(in Vector3d v) {
			this.v = v;
			n = c = Vector3f.Zero;
			uv = new Vector2f[] { Vector2f.Zero };
			bHaveN = bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(in Vector3d v, in Vector3f n) {
			this.v = v;
			this.n = n;
			c = Vector3f.Zero;
			uv = new Vector2f[] { Vector2f.Zero };
			bHaveN = true;
			bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(in Vector3d v, in Vector3f n, in Vector3f c) {
			this.v = v;
			this.n = n;
			this.c = c;
			uv = new Vector2f[] { Vector2f.Zero };
			bHaveN = bHaveC = true;
			bHaveUV = false;
		}
		public NewVertexInfo(in Vector3d v, in Vector3f n, in Vector3f c, in Vector2f uv) {
			this.v = v;
			this.n = n;
			this.c = c;
			this.uv = new Vector2f[] { uv };
			bHaveN = bHaveC = bHaveUV = true;
		}

		public NewVertexInfo(in Vector3d v, in Vector3f n, in Vector3f c, in Vector2f[] uv) {
			this.v = v;
			this.n = n;
			this.c = c;
			this.uv = uv.Length == 0 ? new Vector2f[1] : uv;
			bHaveN = bHaveC = bHaveUV = true;
		}
	}

	public interface IMeshBuilder
	{
		// return ID of new mesh
		int AppendNewMesh(in bool bHaveVtxNormals, in bool bHaveVtxColors, in bool bHaveVtxUVs, in bool bHaveFaceGroups);

		void SetActiveMesh(in int id);

		int AppendVertex(in double x, in double y, in double z);
		int AppendVertex(in NewVertexInfo info);

		int AppendTriangle(in int i, in int j, in int k);
		int AppendTriangle(in int i, in int j, in int k, in int g);


		// optional
		bool SupportsMetaData { get; }
		void AppendMetaData(in string identifier, in object data);
	}




}
