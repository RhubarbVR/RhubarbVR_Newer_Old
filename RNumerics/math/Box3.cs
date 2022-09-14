﻿using System;
using System.Collections.Generic;

using MessagePack;

namespace RNumerics
{

	// partially based on WildMagic5 Box3
	[MessagePackObject]
	public struct Box3d
	{
		// A box has center C, axis directions U[0], U[1], and U[2] (mutually
		// perpendicular unit-length vectors), and extents e[0], e[1], and e[2]
		// (all nonnegative numbers).  A point X = C+y[0]*U[0]+y[1]*U[1]+y[2]*U[2]
		// is inside or on the box whenever |y[i]| <= e[i] for all i.
		[Key(0)]
		public Vector3d Center;
		[Key(1)]
		public Vector3d AxisX;
		[Key(2)]
		public Vector3d AxisY;
		[Key(3)]
		public Vector3d AxisZ;
		[Key(4)]
		public Vector3d Extent;
		public Box3d() {
			Center = Vector3d.Zero;
			AxisX = Vector3d.Zero;
			AxisY = Vector3d.Zero;
			AxisZ = Vector3d.Zero;
			Extent = Vector3d.Zero;
		}

		public Box3d(in Vector3d center) {
			Center = center;
			AxisX = Vector3d.AxisX;
			AxisY = Vector3d.AxisY;
			AxisZ = Vector3d.AxisZ;
			Extent = Vector3d.Zero;
		}
		public Box3d(in Vector3d center, in Vector3d x, in Vector3d y, in Vector3d z,
						in Vector3d extent) {
			Center = center;
			AxisX = x;
			AxisY = y;
			AxisZ = z;
			Extent = extent;
		}
		public Box3d(in Vector3d center, in Vector3d extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector3d.AxisX;
			AxisY = Vector3d.AxisY;
			AxisZ = Vector3d.AxisZ;
		}
		public Box3d(in AxisAlignedBox3d aaBox) {
			// [RMS] this should produce Empty for aaBox.Empty...
			Extent = new Vector3f(aaBox.Width * 0.5, aaBox.Height * 0.5, aaBox.Depth * 0.5);
			Center = aaBox.Center;
			AxisX = Vector3d.AxisX;
			AxisY = Vector3d.AxisY;
			AxisZ = Vector3d.AxisZ;
		}
		public Box3d(in Frame3f frame, in Vector3d extent) {
			Center = frame.Origin;
			AxisX = frame.X;
			AxisY = frame.Y;
			AxisZ = frame.Z;
			Extent = extent;
		}
		public Box3d(in Segment3d seg) {
			Center = seg.Center;
			AxisZ = seg.Direction;
			Vector3d.MakePerpVectors(ref AxisZ, out AxisX, out AxisY);
			Extent = new Vector3d(0, 0, seg.Extent);
		}

		[IgnoreMember]
		public static readonly Box3d Empty = new(Vector3d.Zero);
		[IgnoreMember]
		public static readonly Box3d UnitZeroCentered = new(Vector3d.Zero, 0.5 * Vector3d.One);
		[IgnoreMember]
		public static readonly Box3d UnitPositive = new(0.5 * Vector3d.One, 0.5 * Vector3d.One);


		public Vector3d Axis(in int i) {
			return (i == 0) ? AxisX : (i == 1) ? AxisY : AxisZ;
		}


		public Vector3d[] ComputeVertices() {
			var v = new Vector3d[8];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector3d[] vertex) {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			var extAxis2 = Extent.z * AxisZ;
			vertex[0] = Center - extAxis0 - extAxis1 - extAxis2;
			vertex[1] = Center + extAxis0 - extAxis1 - extAxis2;
			vertex[2] = Center + extAxis0 + extAxis1 - extAxis2;
			vertex[3] = Center - extAxis0 + extAxis1 - extAxis2;
			vertex[4] = Center - extAxis0 - extAxis1 + extAxis2;
			vertex[5] = Center + extAxis0 - extAxis1 + extAxis2;
			vertex[6] = Center + extAxis0 + extAxis1 + extAxis2;
			vertex[7] = Center - extAxis0 + extAxis1 + extAxis2;
		}


		public IEnumerable<Vector3d> VerticesItr() {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			var extAxis2 = Extent.z * AxisZ;
			yield return Center - extAxis0 - extAxis1 - extAxis2;
			yield return Center + extAxis0 - extAxis1 - extAxis2;
			yield return Center + extAxis0 + extAxis1 - extAxis2;
			yield return Center - extAxis0 + extAxis1 - extAxis2;
			yield return Center - extAxis0 - extAxis1 + extAxis2;
			yield return Center + extAxis0 - extAxis1 + extAxis2;
			yield return Center + extAxis0 + extAxis1 + extAxis2;
			yield return Center - extAxis0 + extAxis1 + extAxis2;
		}


		public AxisAlignedBox3d ToAABB() {
			// [TODO] probably more efficient way to do this...at minimum can move center-shift
			// to after the containments...
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			var extAxis2 = Extent.z * AxisZ;
			var result = new AxisAlignedBox3d(Center - extAxis0 - extAxis1 - extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 - extAxis2);
			result.Contain(Center + extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 + extAxis1 + extAxis2);
			result.Contain(Center - extAxis0 + extAxis1 + extAxis2);
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
			var c = Center;
			c += (((i & 1) != 0) ^ ((i & 2) != 0)) ? (Extent.x * AxisX) : (-Extent.x * AxisX);
			c += (i / 2 % 2 == 0) ? (-Extent.y * AxisY) : (Extent.y * AxisY);
			c += (i < 4) ? (-Extent.z * AxisZ) : (Extent.z * AxisZ);
			return c;
		}


		// RNumerics extensions
		[IgnoreMember]
		public double MaxExtent => Math.Max(Extent.x, Math.Max(Extent.y, Extent.z));
		[IgnoreMember]
		public double MinExtent => Math.Min(Extent.x, Math.Max(Extent.y, Extent.z));
		[IgnoreMember]
		public Vector3d Diagonal
		{
			get {
				return (Extent.x * AxisX) + (Extent.y * AxisY) + (Extent.z * AxisZ) -
			  ((-Extent.x * AxisX) - (Extent.y * AxisY) - (Extent.z * AxisZ));
			}
		}
		[IgnoreMember]
		public double Volume => 2 * Extent.x * 2 * Extent.y * 2 * Extent.z;

		public void Contain(in Vector3d v) {
			var lv = v - Center;
			for (var k = 0; k < 3; ++k) {
				var t = lv.Dot(Axis(k));
				if (Math.Abs(t) > Extent[k]) {
					double min = -Extent[k], max = Extent[k];
					if (t < min) {
						min = t;
					}
					else if (t > max) {
						max = t;
					}

					Extent[k] = (max - min) * 0.5;
					Center += (max + min) * 0.5 * Axis(k);
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

			var diff = points_itr.Current - Center;
			var pmin = new Vector3d(diff.Dot(AxisX), diff.Dot(AxisY), diff.Dot(AxisZ));
			var pmax = pmin;
			while (points_itr.MoveNext()) {
				diff = points_itr.Current - Center;

				var dotx = diff.Dot(AxisX);
				if (dotx < pmin[0]) {
					pmin[0] = dotx;
				}
				else if (dotx > pmax[0]) {
					pmax[0] = dotx;
				}

				var doty = diff.Dot(AxisY);
				if (doty < pmin[1]) {
					pmin[1] = doty;
				}
				else if (doty > pmax[1]) {
					pmax[1] = doty;
				}

				var dotz = diff.Dot(AxisZ);
				if (dotz < pmin[2]) {
					pmin[2] = dotz;
				}
				else if (dotz > pmax[2]) {
					pmax[2] = dotz;
				}
			}
			for (var j = 0; j < 3; ++j) {
				Center += ((double)0.5) * (pmin[j] + pmax[j]) * Axis(j);
				Extent[j] = ((double)0.5) * (pmax[j] - pmin[j]);
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
			var lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y) &&
				(Math.Abs(lv.Dot(AxisZ)) <= Extent.z);
		}

		public void Expand(in double f) {
			Extent += f;
		}

		public void Translate(in Vector3d v) {
			Center += v;
		}

		public void Scale(in Vector3d s) {
			Center *= s;
			Extent *= s;
			AxisX *= s;
			AxisX.Normalize();
			AxisY *= s;
			AxisY.Normalize();
			AxisZ *= s;
			AxisZ.Normalize();
		}

		public void ScaleExtents(in Vector3d s) {
			Extent *= s;
		}




		/// <summary>
		/// Returns distance to box, or 0 if point is inside box.
		/// Ported from WildMagic5 Wm5DistPoint3Box3.cpp
		/// </summary>
		public double DistanceSquared(Vector3d v) {
			// Work in the box's coordinate system.
			v -= Center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta;
			var closest = new Vector3d();
			int i;
			for (i = 0; i < 3; ++i) {
				closest[i] = Axis(i).Dot(v);
				if (closest[i] < -Extent[i]) {
					delta = closest[i] + Extent[i];
					sqrDistance += delta * delta;
					closest[i] = -Extent[i];
				}
				else if (closest[i] > Extent[i]) {
					delta = closest[i] - Extent[i];
					sqrDistance += delta * delta;
					closest[i] = Extent[i];
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
			v -= Center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta;
			var closest = new Vector3d();
			for (var i = 0; i < 3; ++i) {
				closest[i] = Axis(i).Dot(v);
				var extent = Extent[i];
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

			return Center + (closest.x * AxisX) + (closest.y * AxisY) + (closest.z * AxisZ);
		}





		// ported from WildMagic5 Wm5ContBox3.cpp::MergeBoxes
		public static Box3d Merge(in Box3d box0, in Box3d box1) {
			// Construct a box that contains the input boxes.
			var box = new Box3d {

				// The first guess at the box center.  This value will be updated later
				// after the input box vertices are projected onto axes determined by an
				// average of box axes.
				Center = 0.5 * (box0.Center + box1.Center)
			};

			// A box's axes, when viewed as the columns of a matrix, form a rotation
			// matrix.  The input box axes are converted to quaternions.  The average
			// quaternion is computed, then normalized to unit length.  The result is
			// the slerp of the two input quaternions with t-value of 1/2.  The result
			// is converted back to a rotation matrix and its columns are selected as
			// the merged box axes.
			Quaterniond q0 = new(), q1 = new();
			var rot0 = new Matrix3d(box0.AxisX, box0.AxisY, box0.AxisZ, false);
			q0.SetFromRotationMatrix( rot0);
			var rot1 = new Matrix3d(box1.AxisX, box1.AxisY, box1.AxisZ, false);
			q1.SetFromRotationMatrix( rot1);
			if (q0.Dot(q1) < 0) {
				q1 = -q1;
			}

			var q = q0 + q1;
			var invLength = 1.0 / Math.Sqrt(q.Dot(q));
			q *= invLength;
			var q_mat = q.ToRotationMatrix();
			box.AxisX = q_mat.Column(0);
			box.AxisY = q_mat.Column(1);
			box.AxisZ = q_mat.Column(2);  //q.ToRotationMatrix(box.Axis); 

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
				var diff = vertex[i] - box.Center;
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
				var diff = vertex[i] - box.Center;
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
				box.Center += 0.5 * (pmax[j] + pmin[j]) * box.Axis(j);
				box.Extent[j] = 0.5 * (pmax[j] - pmin[j]);
			}

			return box;
		}








		public static implicit operator Box3d(in Box3f v) => new(v.Center, v.AxisX, v.AxisY, v.AxisZ, v.Extent);
		public static explicit operator Box3f(in Box3d v) => new((Vector3f)v.Center, (Vector3f)v.AxisX, (Vector3f)v.AxisY, (Vector3f)v.AxisZ, (Vector3f)v.Extent);

	}











	[MessagePackObject]
	// partially based on WildMagic5 Box3
	public struct Box3f
	{
		// A box has center C, axis directions U[0], U[1], and U[2] (mutually
		// perpendicular unit-length vectors), and extents e[0], e[1], and e[2]
		// (all nonnegative numbers).  A point X = C+y[0]*U[0]+y[1]*U[1]+y[2]*U[2]
		// is inside or on the box whenever |y[i]| <= e[i] for all i.
		[Key(0)]
		public Vector3f Center;
		[Key(1)]
		public Vector3f AxisX;
		[Key(2)]
		public Vector3f AxisY;
		[Key(3)]
		public Vector3f AxisZ;
		[Key(4)]
		public Vector3f Extent;

		public Box3f(in Vector3f center) {
			Center = center;
			AxisX = Vector3f.AxisX;
			AxisY = Vector3f.AxisY;
			AxisZ = Vector3f.AxisZ;
			Extent = Vector3f.Zero;
		}
		public Box3f(in Vector3f center, in Vector3f x, in Vector3f y, in Vector3f z,
					 in Vector3f extent) {
			Center = center;
			AxisX = x;
			AxisY = y;
			AxisZ = z;
			Extent = extent;
		}
		public Box3f(in Vector3f center, in Vector3f extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector3f.AxisX;
			AxisY = Vector3f.AxisY;
			AxisZ = Vector3f.AxisZ;
		}
		public Box3f(in AxisAlignedBox3f aaBox) {
			// [RMS] this should produce Empty for aaBox.Empty...
			Extent = new Vector3f(aaBox.Width * 0.5f, aaBox.Height * 0.5f, aaBox.Depth * 0.5f);
			Center = aaBox.Center;
			AxisX = Vector3f.AxisX;
			AxisY = Vector3f.AxisY;
			AxisZ = Vector3f.AxisZ;
		}

		[IgnoreMember]
		public static readonly Box3f Empty = new(Vector3f.Zero);


		public Vector3f Axis(in int i) {
			return (i == 0) ? AxisX : (i == 1) ? AxisY : AxisZ;
		}


		public Vector3f[] ComputeVertices() {
			var v = new Vector3f[8];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector3f[] vertex) {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			var extAxis2 = Extent.z * AxisZ;
			vertex[0] = Center - extAxis0 - extAxis1 - extAxis2;
			vertex[1] = Center + extAxis0 - extAxis1 - extAxis2;
			vertex[2] = Center + extAxis0 + extAxis1 - extAxis2;
			vertex[3] = Center - extAxis0 + extAxis1 - extAxis2;
			vertex[4] = Center - extAxis0 - extAxis1 + extAxis2;
			vertex[5] = Center + extAxis0 - extAxis1 + extAxis2;
			vertex[6] = Center + extAxis0 + extAxis1 + extAxis2;
			vertex[7] = Center - extAxis0 + extAxis1 + extAxis2;
		}


		public IEnumerable<Vector3f> VerticesItr() {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			var extAxis2 = Extent.z * AxisZ;
			yield return Center - extAxis0 - extAxis1 - extAxis2;
			yield return Center + extAxis0 - extAxis1 - extAxis2;
			yield return Center + extAxis0 + extAxis1 - extAxis2;
			yield return Center - extAxis0 + extAxis1 - extAxis2;
			yield return Center - extAxis0 - extAxis1 + extAxis2;
			yield return Center + extAxis0 - extAxis1 + extAxis2;
			yield return Center + extAxis0 + extAxis1 + extAxis2;
			yield return Center - extAxis0 + extAxis1 + extAxis2;
		}


		public AxisAlignedBox3f ToAABB() {
			// [TODO] probably more efficient way to do this...at minimum can move center-shift
			// to after the containments...
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			var extAxis2 = Extent.z * AxisZ;
			var result = new AxisAlignedBox3f(Center - extAxis0 - extAxis1 - extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 - extAxis2);
			result.Contain(Center + extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 + extAxis1 + extAxis2);
			result.Contain(Center - extAxis0 + extAxis1 + extAxis2);
			return result;
		}



		// RNumerics extensions
		[IgnoreMember]
		public double MaxExtent => Math.Max(Extent.x, Math.Max(Extent.y, Extent.z));
		[IgnoreMember]
		public double MinExtent => Math.Min(Extent.x, Math.Max(Extent.y, Extent.z));
		[IgnoreMember]
		public Vector3f Diagonal
		{
			get {
				return (Extent.x * AxisX) + (Extent.y * AxisY) + (Extent.z * AxisZ) -
			  ((-Extent.x * AxisX) - (Extent.y * AxisY) - (Extent.z * AxisZ));
			}
		}
		[IgnoreMember]
		public double Volume => 2 * Extent.x * 2 * Extent.y * 2 * Extent.z;

		public void Contain(in Vector3f v) {
			var lv = v - Center;
			for (var k = 0; k < 3; ++k) {
				double t = lv.Dot(Axis(k));
				if (Math.Abs(t) > Extent[k]) {
					double min = -Extent[k], max = Extent[k];
					if (t < min) {
						min = t;
					}
					else if (t > max) {
						max = t;
					}

					Extent[k] = (float)(max - min) * 0.5f;
					Center += (float)(max + min) * 0.5f * Axis(k);
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
			var lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y) &&
				(Math.Abs(lv.Dot(AxisZ)) <= Extent.z);
		}

		public void Expand(in float f) {
			Extent += f;
		}

		public void Translate(in Vector3f v) {
			Center += v;
		}

		public void Scale(in Vector3f s) {
			Center *= s;
			Extent *= s;
			AxisX *= s;
			AxisX.Normalize();
			AxisY *= s;
			AxisY.Normalize();
			AxisZ *= s;
			AxisZ.Normalize();
		}

		public void ScaleExtents(in Vector3f s) {
			Extent *= s;
		}

	}




}
