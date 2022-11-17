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
		public Vector2d center;
		[Key(1)]
		public Vector2d axisX;
		[Key(2)]
		public Vector2d axisY;
		[Key(3)]
		public Vector2d extent;

		[Exposed, IgnoreMember]
		public Vector2d Center
		{
			get => center;
			set => center = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d AxisX
		{
			get => axisX;
			set => axisX = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d AxisY
		{
			get => axisY;
			set => axisY = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d Extent
		{
			get => extent;
			set => extent = value;
		}

		public Box2d() {
			center = Vector2d.Zero;
			axisX = Vector2d.Zero;
			axisY = Vector2d.Zero;
			extent = Vector2d.Zero;
		}

		public Box2d(in Vector2d center) {
			this.center = center;
			axisX = Vector2d.AxisX;
			axisY = Vector2d.AxisY;
			extent = Vector2d.Zero;
		}
		public Box2d(in Vector2d center, in Vector2d x, in Vector2d y, in Vector2d extent) {
			this.center = center;
			axisX = x;
			axisY = y;
			this.extent = extent;
		}
		public Box2d(in Vector2d center, in Vector2d extent) {
			this.center = center;
			this.extent = extent;
			axisX = Vector2d.AxisX;
			axisY = Vector2d.AxisY;
		}
		public Box2d(in AxisAlignedBox2d aaBox) {
			extent = 0.5 * aaBox.Diagonal;
			center = aaBox.min + extent;
			axisX = Vector2d.AxisX;
			axisY = Vector2d.AxisY;
		}
		public Box2d(in Segment2d seg) {
			center = seg.Center;
			axisX = seg.Direction;
			axisY = seg.Direction.Perp;
			extent = new Vector2d(seg.Extent, 0);
		}

		[Exposed,IgnoreMember]
		public static readonly Box2d Empty = new(Vector2d.Zero);


		public Vector2d Axis(in int i) {
			return (i == 0) ? axisX : axisY;
		}


		public Vector2d[] ComputeVertices() {
			var v = new Vector2d[4];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector2d[] vertex) {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			vertex[0] = center - extAxis0 - extAxis1;
			vertex[1] = center + extAxis0 - extAxis1;
			vertex[2] = center + extAxis0 + extAxis1;
			vertex[3] = center - extAxis0 + extAxis1;
		}
		public void ComputeVertices(ref Vector2dTuple4 vertex) {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			vertex[0] = center - extAxis0 - extAxis1;
			vertex[1] = center + extAxis0 - extAxis1;
			vertex[2] = center + extAxis0 + extAxis1;
			vertex[3] = center - extAxis0 + extAxis1;
		}


		// RNumerics extensions
		[IgnoreMember]
		public double MaxExtent => Math.Max(extent.x, extent.y);
		[IgnoreMember]
		public double MinExtent => Math.Min(extent.x, extent.y);
		[IgnoreMember]
		public Vector2d Diagonal
		{
			get {
				return (extent.x * axisX) + (extent.y * axisY) -
			  ((-extent.x * axisX) - (extent.y * axisY));
			}
		}
		[IgnoreMember]
		public double Area => 2 * extent.x * 2 * extent.y;

		public void Contain(in Vector2d v) {
			var lv = v - center;
			for (var k = 0; k < 2; ++k) {
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

		// I think this can be more efficient...no? At least could combine
		// all the axis-interval updates before updating Center...
		public void Contain(in Box2d o) {
			var v = o.ComputeVertices();
			for (var k = 0; k < 4; ++k) {
				Contain(v[k]);
			}
		}

		public bool Contains(in Vector2d v) {
			var lv = v - center;
			return (Math.Abs(lv.Dot(axisX)) <= extent.x) &&
				(Math.Abs(lv.Dot(axisY)) <= extent.y);
		}

		public void Expand(in double f) {
			extent += f;
		}

		public void Translate(in Vector2d v) {
			center += v;
		}

		public void RotateAxes(in Matrix2d m) {
			axisX = m * axisX;
			axisY = m * axisY;
		}





		/// <summary>
		/// Returns distance to box, or 0 if point is inside box.
		/// Ported from WildMagic5 Wm5DistPoint2Box2.cpp
		/// </summary>
		public double DistanceSquared(Vector2d v) {
			// Work in the box's coordinate system.
			v -= center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta, c, extent;
			for (var i = 0; i < 2; ++i) {
				if (i == 0) {
					c = v.Dot(axisX);
					extent = this.extent.x;
				}
				else {
					c = v.Dot(axisY);
					extent = this.extent.y;
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
			var diff = v - center;

			// Compute squared distance and closest point on box.
			double sqrDistance = 0;
			double delta;
			var closest = new Vector2d();
			for (var i = 0; i < 2; ++i) {
				closest[i] = diff.Dot((i == 0) ? axisX : axisY);
				var extent = (i == 0) ? this.extent.x : this.extent.y;
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

			return center + (closest.x * axisX) + (closest.y * axisY);
		}



		// ported from WildMagic5 Wm5ContBox2.cpp::MergeBoxes
		public static Box2d Merge(in Box2d box0, in Box2d box1) {
			// Construct a box that contains the input boxes.
			var box = new Box2d {

				// The first guess at the box center.  This value will be updated later
				// after the input box vertices are projected onto axes determined by an
				// average of box axes.
				center = 0.5 * (box0.center + box1.center)
			};

			// The merged box axes are the averages of the input box axes.  The
			// axes of the second box are negated, if necessary, so they form acute
			// angles with the axes of the first box.
			if (box0.axisX.Dot(box1.axisX) >= 0) {
				box.axisX = 0.5 * (box0.axisX + box1.axisX);
				box.axisX.Normalize();
			}
			else {
				box.axisX = 0.5 * (box0.axisX - box1.axisX);
				box.axisX.Normalize();
			}
			box.axisY = -box.axisX.Perp;

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
				diff = vertex[i] - box.center;
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
				diff = vertex[i] - box.center;
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
			box.extent[0] = 0.5 * (pmax[0] - pmin[0]);
			box.extent[1] = 0.5 * (pmax[1] - pmin[1]);
			box.center += box.axisX * (0.5 * (pmax[0] + pmin[0]));
			box.center += box.axisY * (0.5 * (pmax[1] + pmin[1]));

			return box;
		}










		public static implicit operator Box2d(in Box2f v) => new(v.center, v.axisX, v.axisY, v.extent);
		public static explicit operator Box2f(in Box2d v) => new((Vector2f)v.center, (Vector2f)v.axisX, (Vector2f)v.axisY, (Vector2f)v.extent);


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
		public Vector2f center;
		[Key(1)]
		public Vector2f axisX;
		[Key(2)]
		public Vector2f axisY;
		[Key(3)]
		public Vector2f extent;

		[Exposed, IgnoreMember]
		public Vector2f Center
		{
			get => center;
			set => center = value;
		}
		[Exposed, IgnoreMember]
		public Vector2f AxisX
		{
			get => axisX;
			set => axisX = value;
		}
		[Exposed, IgnoreMember]
		public Vector2f AxisY
		{
			get => axisY;
			set => axisY = value;
		}
		[Exposed, IgnoreMember]
		public Vector2f Extent
		{
			get => extent;
			set => extent = value;
		}
		public Box2f(in Vector2f center) {
			this.center = center;
			axisX = Vector2f.AxisX;
			axisY = Vector2f.AxisY;
			extent = Vector2f.Zero;
		}
		public Box2f(in Vector2f center, in Vector2f x, in Vector2f y, in Vector2f extent) {
			this.center = center;
			axisX = x;
			axisY = y;
			this.extent = extent;
		}
		public Box2f(in Vector2f center, in Vector2f extent) {
			this.center = center;
			this.extent = extent;
			axisX = Vector2f.AxisX;
			axisY = Vector2f.AxisY;
		}
		public Box2f(in AxisAlignedBox2f aaBox) {
			extent = 0.5f * aaBox.Diagonal;
			center = aaBox.min + extent;
			axisX = Vector2f.AxisX;
			axisY = Vector2f.AxisY;
		}

		[Exposed,IgnoreMember]
		public static readonly Box2f Empty = new(Vector2f.Zero);


		public Vector2f Axis(in int i) {
			return (i == 0) ? axisX : axisY;
		}


		public Vector2f[] ComputeVertices() {
			var v = new Vector2f[4];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices(in Vector2f[] vertex) {
			var extAxis0 = extent.x * axisX;
			var extAxis1 = extent.y * axisY;
			vertex[0] = center - extAxis0 - extAxis1;
			vertex[1] = center + extAxis0 - extAxis1;
			vertex[2] = center + extAxis0 + extAxis1;
			vertex[3] = center - extAxis0 + extAxis1;
		}


		// RNumerics extensions
		[IgnoreMember]
		public double MaxExtent => Math.Max(extent.x, extent.y);
		[IgnoreMember]
		public double MinExtent => Math.Min(extent.x, extent.y);
		[IgnoreMember]
		public Vector2f Diagonal
		{
			get {
				return (extent.x * axisX) + (extent.y * axisY) -
			  ((-extent.x * axisX) - (extent.y * axisY));
			}
		}
		[IgnoreMember]
		public double Area => 2 * extent.x * 2 * extent.y;

		public void Contain(in Vector2f v) {
			var lv = v - center;
			for (var k = 0; k < 2; ++k) {
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
		public void Contain(in Box2f o) {
			var v = o.ComputeVertices();
			for (var k = 0; k < 4; ++k) {
				Contain(v[k]);
			}
		}

		public bool Contains(in Vector2f v) {
			var lv = v - center;
			return (Math.Abs(lv.Dot(axisX)) <= extent.x) &&
				(Math.Abs(lv.Dot(axisY)) <= extent.y);
		}


		public void Expand(in float f) {
			extent += f;
		}

		public void Translate(in Vector2f v) {
			center += v;
		}

	}




}
