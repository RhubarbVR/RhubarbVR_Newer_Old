using System;
using System.Collections.Generic;
using System.IO;

namespace RNumerics
{

	// partially based on WildMagic5 Box3
	public struct Box3d : ISerlize<Box3d>
	{
		// A box has center C, axis directions U[0], U[1], and U[2] (mutually
		// perpendicular unit-length vectors), and extents e[0], e[1], and e[2]
		// (all nonnegative numbers).  A point X = C+y[0]*U[0]+y[1]*U[1]+y[2]*U[2]
		// is inside or on the box whenever |y[i]| <= e[i] for all i.
		public Vector3d center;
		public Vector3d axisX;
		public Vector3d axisY;
		public Vector3d axisZ;
		public Vector3d extent;

		public void Serlize(BinaryWriter binaryWriter) {
			center.Serlize(binaryWriter);
			axisX.Serlize(binaryWriter);
			axisY.Serlize(binaryWriter);
			axisZ.Serlize(binaryWriter);
			extent.Serlize(binaryWriter);

		}

		public void DeSerlize(BinaryReader binaryReader) {
			center.DeSerlize(binaryReader);
			axisX.DeSerlize(binaryReader);
			axisY.DeSerlize(binaryReader);
			axisZ.DeSerlize(binaryReader);
			extent.DeSerlize(binaryReader);
		}

		[Exposed]
		public Vector3d Center
		{
			get => center;
			set => center = value;
		}
		[Exposed]
		public Vector3d AxisX
		{
			get => axisX;
			set => axisX = value;
		}
		[Exposed]
		public Vector3d AxisY
		{
			get => axisY;
			set => axisY = value;
		}
		[Exposed]
		public Vector3d AxisZ
		{
			get => axisZ;
			set => axisZ = value;
		}
		[Exposed]
		public Vector3d Extent
		{
			get => extent;
			set => extent = value;
		}

		public Box3d() {
			center = Vector3d.Zero;
			axisX = Vector3d.Zero;
			axisY = Vector3d.Zero;
			axisZ = Vector3d.Zero;
			extent = Vector3d.Zero;
		}

		public Box3d(in Vector3d center) {
			this.center = center;
			axisX = Vector3d.AxisX;
			axisY = Vector3d.AxisY;
			axisZ = Vector3d.AxisZ;
			extent = Vector3d.Zero;
		}
		public Box3d(in Vector3d center, in Vector3d x, in Vector3d y, in Vector3d z,
						in Vector3d extent) {
			this.center = center;
			axisX = x;
			axisY = y;
			axisZ = z;
			this.extent = extent;
		}
		public Box3d(in Vector3d center, in Vector3d extent) {
			this.center = center;
			this.extent = extent;
			axisX = Vector3d.AxisX;
			axisY = Vector3d.AxisY;
			axisZ = Vector3d.AxisZ;
		}
		public Box3d(in AxisAlignedBox3d aaBox) {
			// [RMS] this should produce Empty for aaBox.Empty...
			extent = new Vector3f(aaBox.Width * 0.5, aaBox.Height * 0.5, aaBox.Depth * 0.5);
			center = aaBox.Center;
			axisX = Vector3d.AxisX;
			axisY = Vector3d.AxisY;
			axisZ = Vector3d.AxisZ;
		}
		public Box3d(in Frame3f frame, in Vector3d extent) {
			center = frame.Origin;
			axisX = frame.X;
			axisY = frame.Y;
			axisZ = frame.Z;
			this.extent = extent;
		}
		public Box3d(in Segment3d seg) {
			center = seg.center;
			axisZ = seg.direction;
			Vector3d.MakePerpVectors(ref axisZ, out axisX, out axisY);
			extent = new Vector3d(0, 0, seg.extent);
		}

		[Exposed]
		public static readonly Box3d Empty = new(Vector3d.Zero);
		[Exposed]
		public static readonly Box3d UnitZeroCentered = new(Vector3d.Zero, 0.5 * Vector3d.One);
		[Exposed]
		public static readonly Box3d UnitPositive = new(0.5 * Vector3d.One, 0.5 * Vector3d.One);


		public Vector3d Axis(in int i) {
			return (i == 0) ? axisX : (i == 1) ? axisY : axisZ;
		}


		public Vector3d[] ComputeVertices() {
			var v = new Vector3d[8];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector3d[] vertex) {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			var extAxis2 = extent.z * axisZ;
			vertex[0] = center - extAxis0 - extAxis1 - extAxis2;
			vertex[1] = center + extAxis0 - extAxis1 - extAxis2;
			vertex[2] = center + extAxis0 + extAxis1 - extAxis2;
			vertex[3] = center - extAxis0 + extAxis1 - extAxis2;
			vertex[4] = center - extAxis0 - extAxis1 + extAxis2;
			vertex[5] = center + extAxis0 - extAxis1 + extAxis2;
			vertex[6] = center + extAxis0 + extAxis1 + extAxis2;
			vertex[7] = center - extAxis0 + extAxis1 + extAxis2;
		}


		public IEnumerable<Vector3d> VerticesItr() {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			var extAxis2 = extent.z * axisZ;
			yield return center - extAxis0 - extAxis1 - extAxis2;
			yield return center + extAxis0 - extAxis1 - extAxis2;
			yield return center + extAxis0 + extAxis1 - extAxis2;
			yield return center - extAxis0 + extAxis1 - extAxis2;
			yield return center - extAxis0 - extAxis1 + extAxis2;
			yield return center + extAxis0 - extAxis1 + extAxis2;
			yield return center + extAxis0 + extAxis1 + extAxis2;
			yield return center - extAxis0 + extAxis1 + extAxis2;
		}


		public AxisAlignedBox3d ToAABB() {
			// [TODO] probably more efficient way to do this...at minimum can move center-shift
			// to after the containments...
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			var extAxis2 = extent.z * axisZ;
			var result = new AxisAlignedBox3d(center - extAxis0 - extAxis1 - extAxis2);
			result.Contain(center + extAxis0 - extAxis1 - extAxis2);
			result.Contain(center + extAxis0 + extAxis1 - extAxis2);
			result.Contain(center - extAxis0 + extAxis1 - extAxis2);
			result.Contain(center - extAxis0 - extAxis1 + extAxis2);
			result.Contain(center + extAxis0 - extAxis1 + extAxis2);
			result.Contain(center + extAxis0 + extAxis1 + extAxis2);
			result.Contain(center - extAxis0 + extAxis1 + extAxis2);
			return result;
		}



		// corners [ (-x,-y), (x,-y), (x,y), (-x,y) ], -z, then +z
		//
		//   7---6     +z       or        3---2     -z
		//   |\  |\                       |\  |\
		//   4-\-5 \                      0-\-1 \
		//    \ 3---2                      \ 7---6   
		//     \|   |                       \|   |
		//      0---1  -z                    4---5  +z
		//
		// Note that in RHS system (which is our default), +z is "forward" so -z in this diagram 
		// is actually the back of the box (!) This is odd but want to keep consistency w/ ComputeVertices(),
		// and the implementation there needs to stay consistent w/ C++ Wildmagic5
		public Vector3d Corner(in int i) {
			var c = center;
			c += (((i & 1) != 0) ^ ((i & 2) != 0)) ? (extent.x * axisX) : (-extent.x * axisX);
			c += (i / 2 % 2 == 0) ? (-extent.y * axisY) : (extent.y * axisY);
			c += (i < 4) ? (-extent.z * axisZ) : (extent.z * axisZ);
			return c;
		}


		// RNumerics extensions
		
		public double MaxExtent => Math.Max(extent.x, Math.Max(extent.y, extent.z));
		
		public double MinExtent => Math.Min(extent.x, Math.Max(extent.y, extent.z));
		
		public Vector3d Diagonal
		{
			get {
				return (extent.x * axisX) + (extent.y * axisY) + (extent.z * axisZ) -
			  ((-extent.x * axisX) - (extent.y * axisY) - (extent.z * axisZ));
			}
		}
		
		public double Volume => 2 * extent.x * 2 * extent.y * 2 * extent.z;

		public void Contain(in Vector3d v) {
			var lv = v - center;
			for (var k = 0; k < 3; ++k) {
				var t = lv.Dot(Axis(k));
				if (Math.Abs(t) > extent[k]) {
					double min = -extent[k], max = extent[k];
					if (t < min) {
						min = t;
					}
					else if (t > max) {
						max = t;
					}

					extent[k] = (max - min) * 0.5;
					center += (max + min) * 0.5 * Axis(k);
				}
			}
		}


		/// <summary>
		/// update the box to contain set of input points. More efficient tha ncalling Contain() many times
		/// code ported from GTEngine GteContOrientedBox3.h 
		/// </summary>
		public void Contain(in IEnumerable<Vector3d> points) {
			// Let C be the box center and let U0, U1, and U2 be the box axes.
			// Each input point is of the form X = C + y0*U0 + y1*U1 + y2*U2.
			// The following code computes min(y0), max(y0), min(y1), max(y1),
			// min(y2), and max(y2).  The box center is then adjusted to be
			//   C' = C + 0.5*(min(y0)+max(y0))*U0 + 0.5*(min(y1)+max(y1))*U1 + 0.5*(min(y2)+max(y2))*U2
			var points_itr = points.GetEnumerator();
			points_itr.MoveNext();

			var diff = points_itr.Current - center;
			var pmin = new Vector3d(diff.Dot(axisX), diff.Dot(axisY), diff.Dot(axisZ));
			var pmax = pmin;
			while (points_itr.MoveNext()) {
				diff = points_itr.Current - center;

				var dotx = diff.Dot(axisX);
				if (dotx < pmin[0]) {
					pmin[0] = dotx;
				}
				else if (dotx > pmax[0]) {
					pmax[0] = dotx;
				}

				var doty = diff.Dot(axisY);
				if (doty < pmin[1]) {
					pmin[1] = doty;
				}
				else if (doty > pmax[1]) {
					pmax[1] = doty;
				}

				var dotz = diff.Dot(axisZ);
				if (dotz < pmin[2]) {
					pmin[2] = dotz;
				}
				else if (dotz > pmax[2]) {
					pmax[2] = dotz;
				}
			}
			for (var j = 0; j < 3; ++j) {
				center += ((double)0.5) * (pmin[j] + pmax[j]) * Axis(j);
				extent[j] = ((double)0.5) * (pmax[j] - pmin[j]);
			}
		}



		// I think this can be more efficient...no? At least could combine
		// all the axis-interval updates before updating Center...
		public void Contain(in Box3d o) {
			var v = o.ComputeVertices();
			for (var k = 0; k < 8; ++k) {
				Contain(v[k]);
			}
		}

		public bool Contains(in Vector3d v) {
			var lv = v - center;
			return (Math.Abs(lv.Dot(axisX)) <= extent.x) &&
				(Math.Abs(lv.Dot(axisY)) <= extent.y) &&
				(Math.Abs(lv.Dot(axisZ)) <= extent.z);
		}

		public void Expand(in double f) {
			extent += f;
		}

		public void Translate(in Vector3d v) {
			center += v;
		}

		public void Scale(in Vector3d s) {
			center *= s;
			extent *= s;
			axisX *= s;
			axisX.Normalize();
			axisY *= s;
			axisY.Normalize();
			axisZ *= s;
			axisZ.Normalize();
		}

		public void ScaleExtents(in Vector3d s) {
			extent *= s;
		}




		/// <summary>
		/// Returns distance to box, or 0 if point is inside box.
		/// Ported from WildMagic5 Wm5DistPoint3Box3.cpp
		/// </summary>
		public double DistanceSquared(Vector3d v) {
			// Work in the box's coordinate system.
			v -= center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta;
			var closest = new Vector3d();
			int i;
			for (i = 0; i < 3; ++i) {
				closest[i] = Axis(i).Dot(v);
				if (closest[i] < -extent[i]) {
					delta = closest[i] + extent[i];
					sqrDistance += delta * delta;
					closest[i] = -extent[i];
				}
				else if (closest[i] > extent[i]) {
					delta = closest[i] - extent[i];
					sqrDistance += delta * delta;
					closest[i] = extent[i];
				}
			}

			return sqrDistance;
		}



		/// <summary>
		/// Returns distance to box, or 0 if point is inside box.
		/// Ported from WildMagic5 Wm5DistPoint3Box3.cpp
		/// </summary>
		public Vector3d ClosestPoint(Vector3d v) {
			// Work in the box's coordinate system.
			v -= center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta;
			var closest = new Vector3d();
			for (var i = 0; i < 3; ++i) {
				closest[i] = Axis(i).Dot(v);
				var extent = this.extent[i];
				if (closest[i] < -extent) {
					delta = closest[i] + extent;
					sqrDistance += delta * delta;
					closest[i] = -extent;
				}
				else if (closest[i] > extent) {
					delta = closest[i] - extent;
					sqrDistance += delta * delta;
					closest[i] = extent;
				}
			}

			return center + (closest.x * axisX) + (closest.y * axisY) + (closest.z * axisZ);
		}





		// ported from WildMagic5 Wm5ContBox3.cpp::MergeBoxes
		public static Box3d Merge(in Box3d box0, in Box3d box1) {
			// Construct a box that contains the input boxes.
			var box = new Box3d {

				// The first guess at the box center.  This value will be updated later
				// after the input box vertices are projected onto axes determined by an
				// average of box axes.
				center = 0.5 * (box0.center + box1.center)
			};

			// A box's axes, when viewed as the columns of a matrix, form a rotation
			// matrix.  The input box axes are converted to quaternions.  The average
			// quaternion is computed, then normalized to unit length.  The result is
			// the slerp of the two input quaternions with t-value of 1/2.  The result
			// is converted back to a rotation matrix and its columns are selected as
			// the merged box axes.
			Quaterniond q0 = new(), q1 = new();
			var rot0 = new Matrix3d(box0.axisX, box0.axisY, box0.axisZ, false);
			q0.SetFromRotationMatrix( rot0);
			var rot1 = new Matrix3d(box1.axisX, box1.axisY, box1.axisZ, false);
			q1.SetFromRotationMatrix( rot1);
			if (q0.Dot(q1) < 0) {
				q1 = -q1;
			}

			var q = q0 + q1;
			var invLength = 1.0 / Math.Sqrt(q.Dot(q));
			q *= invLength;
			var q_mat = q.ToRotationMatrix();
			box.axisX = q_mat.Column(0);
			box.axisY = q_mat.Column(1);
			box.axisZ = q_mat.Column(2);  //q.ToRotationMatrix(box.Axis); 

			// Project the input box vertices onto the merged-box axes.  Each axis
			// D[i] containing the current center C has a minimum projected value
			// min[i] and a maximum projected value max[i].  The corresponding end
			// points on the axes are C+min[i]*D[i] and C+max[i]*D[i].  The point C
			// is not necessarily the midpoint for any of the intervals.  The actual
			// box center will be adjusted from C to a point C' that is the midpoint
			// of each interval,
			//   C' = C + sum_{i=0}^2 0.5*(min[i]+max[i])*D[i]
			// The box extents are
			//   e[i] = 0.5*(max[i]-min[i])

			int i, j;
			double dot;
			var vertex = new Vector3d[8];
			var pmin = Vector3d.Zero;
			var pmax = Vector3d.Zero;

			box0.ComputeVertices(vertex);
			for (i = 0; i < 8; ++i) {
				var diff = vertex[i] - box.center;
				for (j = 0; j < 3; ++j) {
					dot = box.Axis(j).Dot(diff);
					if (dot > pmax[j]) {
						pmax[j] = dot;
					}
					else if (dot < pmin[j]) {
						pmin[j] = dot;
					}
				}
			}

			box1.ComputeVertices(vertex);
			for (i = 0; i < 8; ++i) {
				var diff = vertex[i] - box.center;
				for (j = 0; j < 3; ++j) {
					dot = box.Axis(j).Dot(diff);
					if (dot > pmax[j]) {
						pmax[j] = dot;
					}
					else if (dot < pmin[j]) {
						pmin[j] = dot;
					}
				}
			}

			// [min,max] is the axis-aligned box in the coordinate system of the
			// merged box axes.  Update the current box center to be the center of
			// the new box.  Compute the extents based on the new center.
			for (j = 0; j < 3; ++j) {
				box.center += 0.5 * (pmax[j] + pmin[j]) * box.Axis(j);
				box.extent[j] = 0.5 * (pmax[j] - pmin[j]);
			}

			return box;
		}








		public static implicit operator Box3d(in Box3f v) => new(v.center, v.axisX, v.axisY, v.axisZ, v.extent);
		public static explicit operator Box3f(in Box3d v) => new((Vector3f)v.center, (Vector3f)v.axisX, (Vector3f)v.axisY, (Vector3f)v.axisZ, (Vector3f)v.extent);

	}











	// partially based on WildMagic5 Box3
	public struct Box3f: ISerlize<Box3f>
	{
		// A box has center C, axis directions U[0], U[1], and U[2] (mutually
		// perpendicular unit-length vectors), and extents e[0], e[1], and e[2]
		// (all nonnegative numbers).  A point X = C+y[0]*U[0]+y[1]*U[1]+y[2]*U[2]
		// is inside or on the box whenever |y[i]| <= e[i] for all i.
		public Vector3f center;
		public Vector3f axisX;
		public Vector3f axisY;
		public Vector3f axisZ;
		public Vector3f extent;


		public void Serlize(BinaryWriter binaryWriter) {
			center.Serlize(binaryWriter);
			axisX.Serlize(binaryWriter);
			axisY.Serlize(binaryWriter);
			axisZ.Serlize(binaryWriter);
			extent.Serlize(binaryWriter);

		}

		public void DeSerlize(BinaryReader binaryReader) {
			center.DeSerlize(binaryReader);
			axisX.DeSerlize(binaryReader);
			axisY.DeSerlize(binaryReader);
			axisZ.DeSerlize(binaryReader);
			extent.DeSerlize(binaryReader);
		}

		[Exposed]
		public Vector3f Center
		{
			get => center;
			set => center = value;
		}
		[Exposed]
		public Vector3f AxisX
		{
			get => axisX;
			set => axisX = value;
		}
		[Exposed]
		public Vector3f AxisY
		{
			get => axisY;
			set => axisY = value;
		}
		[Exposed]
		public Vector3f AxisZ
		{
			get => axisZ;
			set => axisZ = value;
		}
		[Exposed]
		public Vector3f Extent
		{
			get => extent;
			set => extent = value;
		}

		public Box3f(in Vector3f center) {
			this.center = center;
			axisX = Vector3f.AxisX;
			axisY = Vector3f.AxisY;
			axisZ = Vector3f.AxisZ;
			extent = Vector3f.Zero;
		}
		public Box3f(in Vector3f center, in Vector3f x, in Vector3f y, in Vector3f z,
					 in Vector3f extent) {
			this.center = center;
			axisX = x;
			axisY = y;
			axisZ = z;
			this.extent = extent;
		}
		public Box3f(in Vector3f center, in Vector3f extent) {
			this.center = center;
			this.extent = extent;
			axisX = Vector3f.AxisX;
			axisY = Vector3f.AxisY;
			axisZ = Vector3f.AxisZ;
		}
		public Box3f(in AxisAlignedBox3f aaBox) {
			// [RMS] this should produce Empty for aaBox.Empty...
			extent = new Vector3f(aaBox.Width * 0.5f, aaBox.Height * 0.5f, aaBox.Depth * 0.5f);
			center = aaBox.Center;
			axisX = Vector3f.AxisX;
			axisY = Vector3f.AxisY;
			axisZ = Vector3f.AxisZ;
		}

		[Exposed]
		public static readonly Box3f Empty = new(Vector3f.Zero);


		public Vector3f Axis(in int i) {
			return (i == 0) ? axisX : (i == 1) ? axisY : axisZ;
		}


		public Vector3f[] ComputeVertices() {
			var v = new Vector3f[8];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector3f[] vertex) {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			var extAxis2 = extent.z * axisZ;
			vertex[0] = center - extAxis0 - extAxis1 - extAxis2;
			vertex[1] = center + extAxis0 - extAxis1 - extAxis2;
			vertex[2] = center + extAxis0 + extAxis1 - extAxis2;
			vertex[3] = center - extAxis0 + extAxis1 - extAxis2;
			vertex[4] = center - extAxis0 - extAxis1 + extAxis2;
			vertex[5] = center + extAxis0 - extAxis1 + extAxis2;
			vertex[6] = center + extAxis0 + extAxis1 + extAxis2;
			vertex[7] = center - extAxis0 + extAxis1 + extAxis2;
		}


		public IEnumerable<Vector3f> VerticesItr() {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			var extAxis2 = extent.z * axisZ;
			yield return center - extAxis0 - extAxis1 - extAxis2;
			yield return center + extAxis0 - extAxis1 - extAxis2;
			yield return center + extAxis0 + extAxis1 - extAxis2;
			yield return center - extAxis0 + extAxis1 - extAxis2;
			yield return center - extAxis0 - extAxis1 + extAxis2;
			yield return center + extAxis0 - extAxis1 + extAxis2;
			yield return center + extAxis0 + extAxis1 + extAxis2;
			yield return center - extAxis0 + extAxis1 + extAxis2;
		}


		public AxisAlignedBox3f ToAABB() {
			// [TODO] probably more efficient way to do this...at minimum can move center-shift
			// to after the containments...
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			var extAxis2 = extent.z * axisZ;
			var result = new AxisAlignedBox3f(center - extAxis0 - extAxis1 - extAxis2);
			result.Contain(center + extAxis0 - extAxis1 - extAxis2);
			result.Contain(center + extAxis0 + extAxis1 - extAxis2);
			result.Contain(center - extAxis0 + extAxis1 - extAxis2);
			result.Contain(center - extAxis0 - extAxis1 + extAxis2);
			result.Contain(center + extAxis0 - extAxis1 + extAxis2);
			result.Contain(center + extAxis0 + extAxis1 + extAxis2);
			result.Contain(center - extAxis0 + extAxis1 + extAxis2);
			return result;
		}



		// RNumerics extensions
		
		public double MaxExtent => Math.Max(extent.x, Math.Max(extent.y, extent.z));
		
		public double MinExtent => Math.Min(extent.x, Math.Max(extent.y, extent.z));
		
		public Vector3f Diagonal
		{
			get {
				return (extent.x * axisX) + (extent.y * axisY) + (extent.z * axisZ) -
			  ((-extent.x * axisX) - (extent.y * axisY) - (extent.z * axisZ));
			}
		}
		
		public double Volume => 2 * extent.x * 2 * extent.y * 2 * extent.z;

		public void Contain(in Vector3f v) {
			var lv = v - center;
			for (var k = 0; k < 3; ++k) {
				double t = lv.Dot(Axis(k));
				if (Math.Abs(t) > extent[k]) {
					double min = -extent[k], max = extent[k];
					if (t < min) {
						min = t;
					}
					else if (t > max) {
						max = t;
					}

					extent[k] = (float)(max - min) * 0.5f;
					center += (float)(max + min) * 0.5f * Axis(k);
				}
			}
		}

		// I think this can be more efficient...no? At least could combine
		// all the axis-interval updates before updating Center...
		public void Contain(in Box3f o) {
			var v = o.ComputeVertices();
			for (var k = 0; k < 8; ++k) {
				Contain(v[k]);
			}
		}

		public bool Contains(in Vector3f v) {
			var lv = v - center;
			return (Math.Abs(lv.Dot(axisX)) <= extent.x) &&
				(Math.Abs(lv.Dot(axisY)) <= extent.y) &&
				(Math.Abs(lv.Dot(axisZ)) <= extent.z);
		}

		public void Expand(in float f) {
			extent += f;
		}

		public void Translate(in Vector3f v) {
			center += v;
		}

		public void Scale(in Vector3f s) {
			center *= s;
			extent *= s;
			axisX *= s;
			axisX.Normalize();
			axisY *= s;
			axisY.Normalize();
			axisZ *= s;
			axisZ.Normalize();
		}

		public void ScaleExtents(in Vector3f s) {
			extent *= s;
		}

	}




}
