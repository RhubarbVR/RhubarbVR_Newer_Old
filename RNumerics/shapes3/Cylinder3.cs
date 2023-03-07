using System;
using System.IO;

namespace RNumerics
{
	// ported from GTEngine (WildMagic5 doesn't have cylinder primitive)
	public struct Cylinder3d : ISerlize<Cylinder3d>
	{
		// The cylinder axis is a line.  The origin of the cylinder is chosen to be
		// the line origin.  The cylinder wall is at a distance R units from the axis.
		// An infinite cylinder has infinite height.  A finite cylinder has center C
		// at the line origin and has a finite height H.  The segment for the finite
		// cylinder has endpoints C-(H/2)*D and C+(H/2)*D where D is a unit-length
		// direction of the line.
		public Line3d Axis;
		public double Radius;
		public double Height;


		public void Serlize(BinaryWriter binaryWriter) {
			Axis.Serlize(binaryWriter);
			binaryWriter.Write(Radius);
			binaryWriter.Write(Height);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			Axis.DeSerlize(binaryReader);
			Radius = binaryReader.ReadDouble();
			Height = binaryReader.ReadDouble();
		}

		public Cylinder3d(in Line3d axis, in double radius, in double height)
		{
			Axis = axis;
			Radius = radius;
			Height = height;
		}
		public Cylinder3d(in Vector3d center, in Vector3d axis, in double radius, in double height)
		{
			Axis = new Line3d(center, axis);
			Radius = radius;
			Height = height;
		}
		public Cylinder3d(in Frame3f frame, in double radius, in double height, in int nNormalAxis = 1)
		{
			Axis = new Line3d(frame.Origin, frame.GetAxis(nNormalAxis));
			Radius = radius;
			Height = height;
		}
		public Cylinder3d(in double radius, in double height)
		{
			Axis = new Line3d(Vector3d.Zero, Vector3d.AxisY);
			Radius = radius;
			Height = height;
		}

		public double Circumference => MathUtil.TWO_PI * Radius;
		public double Diameter => 2 * Radius;
		public double Volume => Math.PI * Radius * Radius * Height;

	}
}
