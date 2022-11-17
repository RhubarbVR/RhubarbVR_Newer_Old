using System;

using MessagePack;
namespace RNumerics
{

	// partially based on WildMagic5 Box2
	[MessagePackObject]
	public struct Box2d
	{
		// A box has center C, axis directions U[0] and U[1] (perpendicular and
		// unit-length vectors), and extents e[0] and e[1] (nonnegative numbers).
		// A/ point X = C+y[0]*U[0]+y[1]*U[1] is inside or on the box whenever
		// |y[i]| <= e[i] for all i.
		[Key(0)]
		public Vector2d Center;
		[Key(1)]
		public Vector2d AxisX;
		[Key(2)]
		public Vector2d AxisY;
		[Key(3)]
		public Vector2d Extent;

		public Box2d() {
			Center = Vector2d.Zero;
			AxisX = Vector2d.Zero;
			AxisY = Vector2d.Zero;
			Extent = Vector2d.Zero;
		}

		public Box2d(in Vector2d center) {
			Center = center;
			AxisX = Vector2d.AxisX;
			AxisY = Vector2d.AxisY;
			Extent = Vector2d.Zero;
		}
		public Box2d(in Vector2d center, in Vector2d x, in Vector2d y, in Vector2d extent) {
			Center = center;
			AxisX = x;
			AxisY = y;
			Extent = extent;
		}
		public Box2d(in Vector2d center, in Vector2d extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector2d.AxisX;
			AxisY = Vector2d.AxisY;
		}
		public Box2d(in AxisAlignedBox2d aaBox) {
			Extent = 0.5 * aaBox.Diagonal;
			Center = aaBox.Min + Extent;
			AxisX = Vector2d.AxisX;
			AxisY = Vector2d.AxisY;
		}
		public Box2d(in Segment2d seg) {
			Center = seg.Center;
			AxisX = seg.Direction;
			AxisY = seg.Direction.Perp;
			Extent = new Vector2d(seg.Extent, 0);
		}

		[IgnoreMember]
		public static readonly Box2d Empty = new(Vector2d.Zero);


		public Vector2d Axis(in int i) {
			return (i == 0) ? AxisX : AxisY;
		}


		public Vector2d[] ComputeVertices() {
			var v = new Vector2d[4];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector2d[] vertex) {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			vertex[0] = Center - extAxis0 - extAxis1;
			vertex[1] = Center + extAxis0 - extAxis1;
			vertex[2] = Center + extAxis0 + extAxis1;
			vertex[3] = Center - extAxis0 + extAxis1;
		}
		public void ComputeVertices(ref Vector2dTuple4 vertex) {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			vertex[0] = Center - extAxis0 - extAxis1;
			vertex[1] = Center + extAxis0 - extAxis1;
			vertex[2] = Center + extAxis0 + extAxis1;
			vertex[3] = Center - extAxis0 + extAxis1;
		}


		// RNumerics extensions
		[IgnoreMember]
		public double MaxExtent => Math.Max(Extent.x, Extent.y);
		[IgnoreMember]
		public double MinExtent => Math.Min(Extent.x, Extent.y);
		[IgnoreMember]
		public Vector2d Diagonal
		{
			get {
				return (Extent.x * AxisX) + (Extent.y * AxisY) -
			  ((-Extent.x * AxisX) - (Extent.y * AxisY));
			}
		}
		[IgnoreMember]
		public double Area => 2 * Extent.x * 2 * Extent.y;

		public void Contain(in Vector2d v) {
			var lv = v - Center;
			for (var k = 0; k < 2; ++k) {
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

		// I think this can be more efficient...no? At least could combine
		// all the axis-interval updates before updating Center...
		public void Contain(in Box2d o) {
			var v = o.ComputeVertices();
			for (var k = 0; k < 4; ++k) {
				Contain(v[k]);
			}
		}

		public bool Contains(in Vector2d v) {
			var lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y);
		}

		public void Expand(in double f) {
			Extent += f;
		}

		public void Translate(in Vector2d v) {
			Center += v;
		}

		public void RotateAxes(in Matrix2d m) {
			AxisX = m * AxisX;
			AxisY = m * AxisY;
		}





		/// <summary>
		/// Returns distance to box, or 0 if point is inside box.
		/// Ported from WildMagic5 Wm5DistPoint2Box2.cpp
		/// </summary>
		public double DistanceSquared(Vector2d v) {
			// Work in the box's coordinate system.
			v -= Center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta, c, extent;
			for (var i = 0; i < 2; ++i) {
				if (i == 0) {
					c = v.Dot(AxisX);
					extent = Extent.x;
				}
				else {
					c = v.Dot(AxisY);
					extent = Extent.y;
				}
				if (c < -extent) {
					delta = c + extent;
					sqrDistance += delta * delta;
				}
				else if (c > extent) {
					delta = c - extent;
					sqrDistance += delta * delta;
				}
			}

			return sqrDistance;
		}




		public Vector2d ClosestPoint(in Vector2d v) {
			// Work in the box's coordinate system.
			var diff = v - Center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta;
			var closest = new Vector2d();
			for (var i = 0; i < 2; ++i) {
				closest[i] = diff.Dot((i == 0) ? AxisX : AxisY);
				var extent = (i == 0) ? Extent.x : Extent.y;
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

			return Center + (closest.x * AxisX) + (closest.y * AxisY);
		}



		// ported from WildMagic5 Wm5ContBox2.cpp::MergeBoxes
		public static Box2d Merge(in Box2d box0, in Box2d box1) {
			// Construct a box that contains the input boxes.
			var box = new Box2d {

				// The first guess at the box center.  This value will be updated later
				// after the input box vertices are projected onto axes determined by an
				// average of box axes.
				Center = 0.5 * (box0.Center + box1.Center)
			};

			// The merged box axes are the averages of the input box axes.  The
			// axes of the second box are negated, if necessary, so they form acute
			// angles with the axes of the first box.
			if (box0.AxisX.Dot(box1.AxisX) >= 0) {
				box.AxisX = 0.5 * (box0.AxisX + box1.AxisX);
				box.AxisX.Normalize();
			}
			else {
				box.AxisX = 0.5 * (box0.AxisX - box1.AxisX);
				box.AxisX.Normalize();
			}
			box.AxisY = -box.AxisX.Perp;

			// Project the input box vertices onto the merged-box axes.  Each axis
			// D[i] containing the current center C has a minimum projected value
			// min[i] and a maximum projected value max[i].  The corresponding end
			// points on the axes are C+min[i]*D[i] and C+max[i]*D[i].  The point C
			// is not necessarily the midpoint for any of the intervals.  The actual
			// box center will be adjusted from C to a point C' that is the midpoint
			// of each interval,
			//   C' = C + sum_{i=0}^1 0.5*(min[i]+max[i])*D[i]
			// The box extents are
			//   e[i] = 0.5*(max[i]-min[i])

			int i, j;
			double dot;
			Vector2d diff;
			var pmin = Vector2d.Zero;
			var pmax = Vector2d.Zero;
			var vertex = new Vector2dTuple4();

			box0.ComputeVertices(ref vertex);
			for (i = 0; i < 4; ++i) {
				diff = vertex[i] - box.Center;
				for (j = 0; j < 2; ++j) {
					dot = diff.Dot(box.Axis(j));
					if (dot > pmax[j]) {
						pmax[j] = dot;
					}
					else if (dot < pmin[j]) {
						pmin[j] = dot;
					}
				}
			}

			box1.ComputeVertices(ref vertex);
			for (i = 0; i < 4; ++i) {
				diff = vertex[i] - box.Center;
				for (j = 0; j < 2; ++j) {
					dot = diff.Dot(box.Axis(j));
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
			box.Extent[0] = 0.5 * (pmax[0] - pmin[0]);
			box.Extent[1] = 0.5 * (pmax[1] - pmin[1]);
			box.Center += box.AxisX * (0.5 * (pmax[0] + pmin[0]));
			box.Center += box.AxisY * (0.5 * (pmax[1] + pmin[1]));

			return box;
		}










		public static implicit operator Box2d(in Box2f v) => new(v.Center, v.AxisX, v.AxisY, v.Extent);
		public static explicit operator Box2f(in Box2d v) => new((Vector2f)v.Center, (Vector2f)v.AxisX, (Vector2f)v.AxisY, (Vector2f)v.Extent);


	}











	[MessagePackObject]
	// partially based on WildMagic5 Box3
	public struct Box2f
	{
		// A box has center C, axis directions U[0] and U[1] (perpendicular and
		// unit-length vectors), and extents e[0] and e[1] (nonnegative numbers).
		// A/ point X = C+y[0]*U[0]+y[1]*U[1] is inside or on the box whenever
		// |y[i]| <= e[i] for all i.
		[Key(0)]
		public Vector2f Center;
		[Key(1)]
		public Vector2f AxisX;
		[Key(2)]
		public Vector2f AxisY;
		[Key(3)]
		public Vector2f Extent;

		public Box2f(in Vector2f center) {
			Center = center;
			AxisX = Vector2f.AxisX;
			AxisY = Vector2f.AxisY;
			Extent = Vector2f.Zero;
		}
		public Box2f(in Vector2f center, in Vector2f x, in Vector2f y, in Vector2f extent) {
			Center = center;
			AxisX = x;
			AxisY = y;
			Extent = extent;
		}
		public Box2f(in Vector2f center, in Vector2f extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector2f.AxisX;
			AxisY = Vector2f.AxisY;
		}
		public Box2f(in AxisAlignedBox2f aaBox) {
			Extent = 0.5f * aaBox.Diagonal;
			Center = aaBox.Min + Extent;
			AxisX = Vector2f.AxisX;
			AxisY = Vector2f.AxisY;
		}

		[IgnoreMember]
		public static readonly Box2f Empty = new(Vector2f.Zero);


		public Vector2f Axis(in int i) {
			return (i == 0) ? AxisX : AxisY;
		}


		public Vector2f[] ComputeVertices() {
			var v = new Vector2f[4];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector2f[] vertex) {
			var extAxis0 = Extent.x * AxisX;
			var extAxis1 = Extent.y * AxisY;
			vertex[0] = Center - extAxis0 - extAxis1;
			vertex[1] = Center + extAxis0 - extAxis1;
			vertex[2] = Center + extAxis0 + extAxis1;
			vertex[3] = Center - extAxis0 + extAxis1;
		}


		// RNumerics extensions
		[IgnoreMember]
		public double MaxExtent => Math.Max(Extent.x, Extent.y);
		[IgnoreMember]
		public double MinExtent => Math.Min(Extent.x, Extent.y);
		[IgnoreMember]
		public Vector2f Diagonal
		{
			get {
				return (Extent.x * AxisX) + (Extent.y * AxisY) -
			  ((-Extent.x * AxisX) - (Extent.y * AxisY));
			}
		}
		[IgnoreMember]
		public double Area => 2 * Extent.x * 2 * Extent.y;

		public void Contain(in Vector2f v) {
			var lv = v - Center;
			for (var k = 0; k < 2; ++k) {
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
		public void Contain(in Box2f o) {
			var v = o.ComputeVertices();
			for (var k = 0; k < 4; ++k) {
				Contain(v[k]);
			}
		}

		public bool Contains(in Vector2f v) {
			var lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y);
		}


		public void Expand(in float f) {
			//Extent += f;
		}

		public void Translate(in Vector2f v) {
			Center += v;
		}

	}




}
