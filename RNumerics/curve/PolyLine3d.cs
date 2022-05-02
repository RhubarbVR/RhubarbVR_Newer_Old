using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RNumerics
{
	public class PolyLine3d : IEnumerable<Vector3d>
	{
		protected List<Vector3d> vertices;
		public int Timestamp;

		public PolyLine3d() {
			vertices = new List<Vector3d>();
			Timestamp = 0;
		}

		public PolyLine3d(PolyLine3d copy) {
			vertices = new List<Vector3d>(copy.vertices);
			Timestamp = 0;
		}

		public PolyLine3d(Vector3d[] v) {
			vertices = new List<Vector3d>(v);
			Timestamp = 0;
		}
		public PolyLine3d(VectorArray3d v) {
			vertices = new List<Vector3d>(v.AsVector3d());
			Timestamp = 0;
		}


		public Vector3d this[int key]
		{
			get => vertices[key];
			set { vertices[key] = value; Timestamp++; }
		}

		public Vector3d Start => vertices[0];
		public Vector3d End => vertices[vertices.Count - 1];


		public ReadOnlyCollection<Vector3d> Vertices => vertices.AsReadOnly();

		public int VertexCount => vertices.Count;

		public void AppendVertex(Vector3d v) {
			vertices.Add(v);
			Timestamp++;
		}


		public Vector3d GetTangent(int i) {
			return i == 0
				? (vertices[1] - vertices[0]).Normalized
				: i == vertices.Count - 1
				? (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).Normalized
				: (vertices[i + 1] - vertices[i - 1]).Normalized;
		}


		public AxisAlignedBox3d GetBounds() {
			if (vertices.Count == 0) {
				return AxisAlignedBox3d.Empty;
			}

			var box = new AxisAlignedBox3d(vertices[0]);
			for (var i = 1; i < vertices.Count; ++i) {
				box.Contain(vertices[i]);
			}

			return box;
		}


		public IEnumerable<Segment3d> SegmentItr() {
			for (var i = 0; i < vertices.Count - 1; ++i) {
				yield return new Segment3d(vertices[i], vertices[i + 1]);
			}
		}

		public IEnumerator<Vector3d> GetEnumerator() {
			return vertices.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return vertices.GetEnumerator();
		}
	}
}
