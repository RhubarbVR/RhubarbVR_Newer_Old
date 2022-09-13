using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RNumerics
{

	public interface IParametricCurve3d
	{
		bool IsClosed { get; }

		// can call SampleT in range [0,ParamLength]
		double ParamLength { get; }
		Vector3d SampleT(in double t);
		Vector3d TangentT(in double t);        // returns normalized vector

		bool HasArcLength { get; }
		double ArcLength { get; }
		Vector3d SampleArcLength(in double a);

		void Reverse();

		IParametricCurve3d Clone();
	}




	public interface ISampledCurve3d
	{
		int VertexCount { get; }
		int SegmentCount { get; }
		bool Closed { get; }

		Vector3d GetVertex(in int i);
		Segment3d GetSegment(in int i);

		IEnumerable<Vector3d> Vertices { get; }
	}





	public interface IParametricCurve2d
	{
		bool IsClosed { get; }

		// can call SampleT in range [0,ParamLength]
		double ParamLength { get; }
		Vector2d SampleT(in double t);
		Vector2d TangentT(in double t);        // returns normalized vector

		bool HasArcLength { get; }
		double ArcLength { get; }
		Vector2d SampleArcLength(in double a);

		void Reverse();

		bool IsTransformable { get; }
		void Transform(in ITransform2 xform);

		IParametricCurve2d Clone();
	}


	public interface IMultiCurve2d
	{
		ReadOnlyCollection<IParametricCurve2d> Curves { get; }
	}

}
