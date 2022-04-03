using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic5 MinCircle2. 
	// Compute the minimum area circle containing the input set of points.  The
	// algorithm randomly permutes the input points so that the construction
	// occurs in 'expected' O(N) time.


	/// <summary>
	/// Fit minimal bounding-circle to a set of 2D points 
	/// </summary>
	public class ContMinCircle2
	{
        readonly double _mEpsilon;
        readonly Func<int, int[], Support, Circle>[] _mUpdate = new Func<int, int[], Support, Circle>[4];
        readonly IList<Vector2d> _points;
        readonly Circle[] _circle_buf = new Circle[6];

		public Circle2d Result;

		public ContMinCircle2(IList<Vector2d> pointsIn, double epsilon = 1e-05)
		{
			_mEpsilon = epsilon;

			_mUpdate[0] = null;
			_mUpdate[1] = UpdateSupport1;
			_mUpdate[2] = UpdateSupport2;
			_mUpdate[3] = UpdateSupport3;

			Circle minimal;

			var support = new Support();
			double distDiff = 0;

			_points = pointsIn;
			var numPoints = pointsIn.Count;

			int[] permutation = null;
			var r = new Random();

			if (numPoints >= 1)
			{
				// Create identity permutation (0,1,..,numPoints-1).
				permutation = new int[numPoints];
				for (var i = 0; i < numPoints; ++i)
				{
					permutation[i] = i;
				}

				// Generate random permutation.
				for (var i = numPoints - 1; i > 0; --i)
				{
					var j = r.Next() % (i + 1);
					if (j != i)
					{
						var save = permutation[i];
						permutation[i] = permutation[j];
						permutation[j] = save;
					}
				}

				minimal = new Circle(_points[permutation[0]], 0);
				support.Quantity = 1;
				support.Index[0] = 0;

				// The previous version of the processing loop is
				//  i = 1;
				//  while (i < numPoints)
				//  {
				//      if (!support.Contains(i, permutation, mEpsilon))
				//      {
				//          if (!Contains(*permutation[i], minimal, distDiff))
				//          {
				//              UpdateFunction update = mUpdate[support.Quantity];
				//              Circle circle = (this->*update)(i, permutation,
				//                  support);
				//              if (circle.Radius > minimal.Radius)
				//              {
				//                  minimal = circle;
				//                  i = 0;
				//                  continue;
				//              }
				//          }
				//      }
				//      ++i;
				//  }
				// This loop restarts from the beginning of the point list each time
				// the circle needs updating.  Linus Källberg (Computer Science at
				// Mälardalen University in Sweden) discovered that performance is
				// better when the remaining points in the array are processed before
				// restarting.  The points processed before the point that caused the
				// update are likely to be enclosed by the new circle (or near the
				// circle boundary) because they were enclosed by the previous circle.
				// The chances are better that points after the current one will cause
				// growth of the bounding circle.
				for (int i = 1 % numPoints, n = 0; i != n; i = (i + 1) % numPoints)
				{
					if (!support.Contains(i, _points, permutation, _mEpsilon))
					{
						if (!Contains(_points[permutation[i]], ref minimal, ref distDiff))
						{
							var updateF = _mUpdate[support.Quantity];
							var circle = updateF(i, permutation, support);
							if (circle.Radius > minimal.Radius)
							{
								minimal = circle;
								n = i;
							}
						}
					}
				}


			}
			else
			{
				throw new Exception("ContMinCircle2: Input must contain points\n");
			}

			Result = new Circle2d(minimal.Center, Math.Sqrt(minimal.Radius));
		}

        static bool Contains(Vector2d point, ref Circle circle, ref double distDiff)
		{
			var diff = point - circle.Center;
			var test = diff.LengthSquared;

			// NOTE:  In this algorithm, Circle2 is storing the *squared radius*,
			// so the next line of code is not in error.
			distDiff = test - circle.Radius;

			return distDiff <= 0;
		}

        static Circle ExactCircle2(ref Vector2d P0, ref Vector2d P1)
		{
			return new Circle(
				0.5 * (P0 + P1), 0.25 * P1.DistanceSquared(P0));
		}

        static Circle ExactCircle2(Vector2d P0, ref Vector2d P1)
		{
			return new Circle(
				0.5 * (P0 + P1), 0.25 * P1.DistanceSquared(P0));
		}


		Circle ExactCircle3(ref Vector2d P0, ref Vector2d P1, ref Vector2d P2)
		{
            var E10 = P1 - P0;
            var E20 = P2 - P0;

            var A = new Matrix2d(E10.x, E10.y, E20.x, E20.y);

            var B = new Vector2d(
				((double)0.5) * E10.LengthSquared,
				((double)0.5) * E20.LengthSquared);

            var det = (A.m00 * A.m11) - (A.m01 * A.m10);

			if (Math.Abs(det) > _mEpsilon)
			{
                var invDet = ((double)1) / det;
				Vector2d Q;
				Q.x = ((A.m11 * B.x) - (A.m01 * B.y)) * invDet;
				Q.y = ((A.m00 * B.y) - (A.m10 * B.x)) * invDet;

				return new Circle(P0 + Q, Q.LengthSquared);
			}
			else
			{
				return new Circle(Vector2d.Zero, double.MaxValue);
			}
		}
		Circle ExactCircle3(Vector2d P0, Vector2d P1, ref Vector2d P2)
		{
			return ExactCircle3(ref P0, ref P1, ref P2);
		}
		Circle ExactCircle3(Vector2d P0, ref Vector2d P1, ref Vector2d P2)
		{
			return ExactCircle3(ref P0, ref P1, ref P2);
		}


		Circle UpdateSupport1(int i, int[] permutation, Support support)
		{
            var P0 = _points[permutation[support.Index[0]]];
            var P1 = _points[permutation[i]];

            var minimal = ExactCircle2(ref P0, ref P1);
			support.Quantity = 2;
			support.Index[1] = i;

			return minimal;
		}


		static readonly int[,] _type2_2 = new int[2, 2]
			{ {0, /*2*/ 1}, {1, /*2*/ 0} };

		Circle UpdateSupport2(int i, int[] permutation, Support support)
		{
            var point = new Vector2dTuple2(
				_points[permutation[support.Index[0]]],  // P0
				_points[permutation[support.Index[1]]]   // P1
			);
            var P2 = _points[permutation[i]];

            // Permutations of type 2, used for calling ExactCircle2(..).
            var numType2 = 2;

            // Permutations of type 3, used for calling ExactCircle3(..).
            //int numType3 = 1;  // {0, 1, 2}

            var circle = _circle_buf;
            var indexCircle = 0;
            var minRSqr = double.MaxValue;
            var indexMinRSqr = -1;
			double distDiff = 0, minDistDiff = double.MaxValue;
            var indexMinDistDiff = -1;

			// Permutations of type 2.
			int j;
			for (j = 0; j < numType2; ++j, ++indexCircle)
			{
				circle[indexCircle] = ExactCircle2(point[_type2_2[j, 0]], ref P2);
				if (circle[indexCircle].Radius < minRSqr)
				{
					if (Contains(point[_type2_2[j, 1]], ref circle[indexCircle], ref distDiff))
					{
						minRSqr = circle[indexCircle].Radius;
						indexMinRSqr = indexCircle;
					}
					else if (distDiff < minDistDiff)
					{
						minDistDiff = distDiff;
						indexMinDistDiff = indexCircle;
					}
				}
			}

			// Permutations of type 3.
			circle[indexCircle] = ExactCircle3(point[0], point[1], ref P2);
			if (circle[indexCircle].Radius < minRSqr)
			{
				//minRSqr = circle[indexCircle].Radius;
				indexMinRSqr = indexCircle;
			}

			// Theoreticaly, indexMinRSqr >= 0, but floating-point round-off errors
			// can lead to indexMinRSqr == -1.  When this happens, the minimal sphere
			// is chosen to be the one that has the minimum absolute errors between
			// the sphere and points (barely) outside the sphere.
			if (indexMinRSqr == -1)
			{
				indexMinRSqr = indexMinDistDiff;
			}

            var minimal = circle[indexMinRSqr];
			switch (indexMinRSqr)
			{
				case 0:
					support.Index[1] = i;
					break;
				case 1:
					support.Index[0] = i;
					break;
				case 2:
					support.Quantity = 3;
					support.Index[2] = i;
					break;
			}

			return minimal;
		}


		static readonly int[,] _type2_3 = new int[3, 3] {
				{ 0, /*3*/ 1, 2}, { 1, /*3*/ 0, 2}, { 2, /*3*/ 0, 1} };
		static readonly int[,] _type3_3 = new int[3, 3] {
				{0, 1, /*3*/ 2}, {0, 2, /*3*/ 1}, {1, 2, /*3*/ 0} };

		Circle UpdateSupport3(int i, int[] permutation, Support support)
		{
            var point = new Vector2dTuple3(
				_points[permutation[support.Index[0]]],  // P0
				_points[permutation[support.Index[1]]],  // P1
				_points[permutation[support.Index[2]]]   // P2
			);
            var P3 = _points[permutation[i]];

            // Permutations of type 2, used for calling ExactCircle2(..).
            var numType2 = 3;


            // Permutations of type 2, used for calling ExactCircle3(..).
            var numType3 = 3;

            var circle = _circle_buf;
            var indexCircle = 0;
            var minRSqr = double.MaxValue;
            var indexMinRSqr = -1;
			double distDiff = 0, minDistDiff = double.MaxValue;
            var indexMinDistDiff = -1;

			// Permutations of type 2.
			int j;
			for (j = 0; j < numType2; ++j, ++indexCircle)
			{
				circle[indexCircle] = ExactCircle2(point[_type2_3[j, 0]], ref P3);
				if (circle[indexCircle].Radius < minRSqr)
				{
					if (Contains(point[_type2_3[j, 1]], ref circle[indexCircle], ref distDiff)
						 && Contains(point[_type2_3[j, 2]], ref circle[indexCircle], ref distDiff))
					{
						minRSqr = circle[indexCircle].Radius;
						indexMinRSqr = indexCircle;
					}
					else if (distDiff < minDistDiff)
					{
						minDistDiff = distDiff;
						indexMinDistDiff = indexCircle;
					}
				}
			}

			// Permutations of type 3.
			for (j = 0; j < numType3; ++j, ++indexCircle)
			{
				circle[indexCircle] = ExactCircle3(point[_type3_3[j, 0]], point[_type3_3[j, 1]], ref P3);
				if (circle[indexCircle].Radius < minRSqr)
				{
					if (Contains(point[_type3_3[j, 2]], ref circle[indexCircle], ref distDiff))
					{
						minRSqr = circle[indexCircle].Radius;
						indexMinRSqr = indexCircle;
					}
					else if (distDiff < minDistDiff)
					{
						minDistDiff = distDiff;
						indexMinDistDiff = indexCircle;
					}
				}
			}

			// Theoreticaly, indexMinRSqr >= 0, but floating-point round-off errors
			// can lead to indexMinRSqr == -1.  When this happens, the minimal circle
			// is chosen to be the one that has the minimum absolute errors between
			// the circle and points (barely) outside the circle.
			if (indexMinRSqr == -1)
			{
				indexMinRSqr = indexMinDistDiff;
			}

            var minimal = circle[indexMinRSqr];
			switch (indexMinRSqr)
			{
				case 0:
					support.Quantity = 2;
					support.Index[1] = i;
					break;
				case 1:
					support.Quantity = 2;
					support.Index[0] = i;
					break;
				case 2:
					support.Quantity = 2;
					support.Index[0] = support.Index[2];
					support.Index[1] = i;
					break;
				case 3:
					support.Index[2] = i;
					break;
				case 4:
					support.Index[1] = i;
					break;
				case 5:
					support.Index[0] = i;
					break;
			}

			return minimal;
		}



		struct Circle
		{
			public Vector2d Center;
			public double Radius;
			public Circle(Vector2d c, double radius)
			{
				Center = c;
				Radius = radius;
			}
		}



		// Indices of points that support current minimum area circle.
		protected class Support
		{
			public bool Contains(int index, IList<Vector2d> Points, int[] permutation, double epsilon)
			{
				for (var i = 0; i < Quantity; ++i)
				{
                    var diff = Points[permutation[index]] - Points[permutation[Index[i]]];
					if (diff.LengthSquared < epsilon)
					{
						return true;
					}
				}
				return false;
			}

			public int Quantity;
			public Index3i Index;
		};

	}
}
