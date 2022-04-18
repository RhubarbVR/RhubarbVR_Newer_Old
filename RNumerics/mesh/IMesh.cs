using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public interface IPointSet
	{
		int VertexCount { get; }
		int MaxVertexID { get; }

		bool HasVertexNormals { get; }
		bool HasVertexColors { get; }

		Vector3d GetVertex(int i);
		Vector3f GetVertexNormal(int i);
		Vector3f GetVertexColor(int i);

		bool IsVertex(int vID);

		// iterators allow us to work with gaps in index space
		System.Collections.Generic.IEnumerable<int> VertexIndices();

		System.Collections.Generic.IEnumerable<Vector3f> VertexPos();


		int Timestamp { get; }
	}



	public interface IMesh : IPointSet
	{
		int TriangleCount { get; }
		int MaxTriangleID { get; }

		bool HasVertexUVs { get; }
		Vector2f GetVertexUV(int i, int channel = 1);

		NewVertexInfo GetVertexAll(int i);

		bool HasTriangleGroups { get; }

		Index3i GetTriangle(int i);
		int GetTriangleGroup(int i);

		bool IsTriangle(int tID);

		// iterators allow us to work with gaps in index space
		IEnumerable<int> TriangleIndices();

		IEnumerable<int> RenderIndices();
	}

	public static class MeshExtensions
	{
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

	public class EnumColl<T> : ICollection<T>
	{
		readonly List<T> _enumer;

		public int Count => _enumer.Count();

		public bool IsReadOnly => false;

		public EnumColl(IEnumerable<T> val) {
			_enumer = new List<T>(val);
		}

		public void Add(T item) {
			_enumer.Add(item);
		}

		public void Clear() {
			_enumer.Clear();
		}

		public bool Contains(T item) {
			return _enumer.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			_enumer.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator() {
			return _enumer.GetEnumerator();
		}

		public bool Remove(T item) {
			return _enumer.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _enumer.GetEnumerator();
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
		void SetVertex(int vID, Vector3d vNewPos);
		void SetVertexNormal(int vid, Vector3f vNewNormal);
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

		public NewVertexInfo(Vector3d v) {
			this.v = v;
			n = c = Vector3f.Zero;
			uv = new Vector2f[] { Vector2f.Zero };
			bHaveN = bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(Vector3d v, Vector3f n) {
			this.v = v;
			this.n = n;
			c = Vector3f.Zero;
			uv = new Vector2f[] { Vector2f.Zero };
			bHaveN = true;
			bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c) {
			this.v = v;
			this.n = n;
			this.c = c;
			uv = new Vector2f[] { Vector2f.Zero };
			bHaveN = bHaveC = true;
			bHaveUV = false;
		}
		public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c, Vector2f uv) {
			this.v = v;
			this.n = n;
			this.c = c;
			this.uv = new Vector2f[] { uv };
			bHaveN = bHaveC = bHaveUV = true;
		}

		public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c, Vector2f [] uv) {
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
		int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups);

		void SetActiveMesh(int id);

		int AppendVertex(double x, double y, double z);
		int AppendVertex(NewVertexInfo info);

		int AppendTriangle(int i, int j, int k);
		int AppendTriangle(int i, int j, int k, int g);


		// optional
		bool SupportsMetaData { get; }
		void AppendMetaData(string identifier, object data);
	}




}
