using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RNumerics
{
	public sealed class PolyLine3d : IEnumerable<Vector3d>
	{
		private readonly List<Vector3d> _vertices;
		public int Timestamp;

		public PolyLine3d() {
			_vertices = new List<Vector3d>();
			Timestamp = 0;
		}

		public PolyLine3d(in PolyLine3d copy) {
			_vertices = new List<Vector3d>(copy._vertices);
			Timestamp = 0;
		}

		public PolyLine3d(in Vector3d[] v) {
			_vertices = new List<Vector3d>(v);
			Timestamp = 0;
		}
		public PolyLine3d(in VectorArray3d v) {
			_vertices = new List<Vector3d>(v.AsVector3d());
			Timestamp = 0;
		}


		public Vector3d this[in int key]
		{
			get => _vertices[key];
			set { _vertices[key] = value; Timestamp++; }
		}

		public Vector3d Start => _vertices[0];
		public Vector3d End => _vertices[_vertices.Count - 1];


		public ReadOnlyCollection<Vector3d> Vertices => _vertices.AsReadOnly();

		public int VertexCount => _vertices.Count;

		public void AppendVertex(in Vector3d v) {
			_vertices.Add(v);
			Timestamp++;
		}


		public Vector3d GetTangent(in int i) {
			return i == 0
				? (_vertices[1] - _vertices[0]).Normalized
				: i == _vertices.Count - 1
				? (_vertices[_vertices.Count - 1] - _vertices[_vertices.Count - 2]).Normalized
				: (_vertices[i + 1] - _vertices[i - 1]).Normalized;
		}


		public AxisAlignedBox3d GetBounds() {
			if (_vertices.Count == 0) {
				return AxisAlignedBox3d.Empty;
			}

			var box = new AxisAlignedBox3d(_vertices[0]);
			for (var i = 1; i < _vertices.Count; ++i) {
				box.Contain(_vertices[i]);
			}

			return box;
		}


		public IEnumerable<Segment3d> SegmentItr() {
			for (var i = 0; i < _vertices.Count - 1; ++i) {
				yield return new Segment3d(_vertices[i], _vertices[i + 1]);
			}
		}

		public IEnumerator<Vector3d> GetEnumerator() {
			return _vertices.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return _vertices.GetEnumerator();
		}
	}
}
