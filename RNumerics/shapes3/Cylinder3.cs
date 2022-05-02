using System;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	// ported from GTEngine (WildMagic5 doesn't have cylinder primitive)
	public class Cylinder3d
	{
		// The cylinder axis is a line.  The origin of the cylinder is chosen to be
		// the line origin.  The cylinder wall is at a distance R units from the axis.
		// An infinite cylinder has infinite height.  A finite cylinder has center C
		// at the line origin and has a finite height H.  The segment for the finite
		// cylinder has endpoints C-(H/2)*D and C+(H/2)*D where D is a unit-length
		// direction of the line.
		[Key(0)]
		public Line3d Axis;
		[Key(1)]
		public double Radius;
		[Key(2)]
		public double Height;

		public Cylinder3d(Line3d axis, double radius, double height)
		{
			Axis = axis;
			Radius = radius;
			Height = height;
		}
		public Cylinder3d(Vector3d center, Vector3d axis, double radius, double height)
		{
			Axis = new Line3d(center, axis);
			Radius = radius;
			Height = height;
		}
		public Cylinder3d(Frame3f frame, double radius, double height, int nNormalAxis = 1)
		{
			Axis = new Line3d(frame.Origin, frame.GetAxis(nNormalAxis));
			Radius = radius;
			Height = height;
		}
		public Cylinder3d(double radius, double height)
		{
			Axis = new Line3d(Vector3d.Zero, Vector3d.AxisY);
			Radius = radius;
			Height = height;
		}

		[IgnoreMember]
		public double Circumference => MathUtil.TWO_PI * Radius;
		[IgnoreMember]
		public double Diameter => 2 * Radius;
		[IgnoreMember]
		public double Volume => Math.PI * Radius * Radius * Height;

	}
}
