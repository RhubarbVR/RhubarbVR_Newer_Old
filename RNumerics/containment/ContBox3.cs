using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{

	// ported from GTEngine GteContOrientedBox3.h
	// (2017) url: https://www.geometrictools.com/GTEngine/Include/Mathematics/GteContOrientedBox3.h
	public sealed class ContOrientedBox3
	{
		public Box3d Box;
		public bool ResultValid = false;

		public ContOrientedBox3(in IEnumerable<Vector3d> points) {
			// Fit the points with a Gaussian distribution.
			var fitter = new GaussPointsFit3(points);
			if (fitter.ResultValid == false) {
				return;
			}

			Box = fitter.Box;
			Box.Contain(points);
		}

		public ContOrientedBox3(in IEnumerable<Vector3d> points, in IEnumerable<double> pointWeights) {
			// Fit the points with a Gaussian distribution.
			var fitter = new GaussPointsFit3(points, pointWeights);
			if (fitter.ResultValid == false) {
				return;
			}

			Box = fitter.Box;
			Box.Contain(points);
		}
	}
}
