using System;

namespace RNumerics
{
	public static class Snapping
	{

		public static double SnapToIncrement(double fValue, in double fIncrement, in double offset = 0) {
			if (!MathUtil.IsFinite(fValue)) {
				return 0;
			}

			fValue -= offset;
			double sign = Math.Sign(fValue);
			fValue = Math.Abs(fValue);
			var nInc = (int)(fValue / fIncrement);
			var fRem = fValue % fIncrement;
			if (fRem > fIncrement / 2) {
				++nInc;
			}

			return (sign * (double)nInc * fIncrement) + offset;
		}




		public static double SnapToNearbyIncrement(in double fValue, in double fIncrement, in double fTolerance) {
			var snapped = SnapToIncrement(fValue, fIncrement);
			return Math.Abs(snapped - fValue) < fTolerance ? snapped : fValue;
		}

		private static double SnapToIncrementSigned(double fValue, in double fIncrement, in bool low) {
			if (!MathUtil.IsFinite(fValue)) {
				return 0;
			}

			double sign = Math.Sign(fValue);
			fValue = Math.Abs(fValue);
			var nInc = (int)(fValue / fIncrement);

			if (low && sign < 0) {
				++nInc;
			}
			else if (!low && sign > 0) {
				++nInc;
			}

			return sign * (double)nInc * fIncrement;

		}

		public static double SnapToIncrementLow(in double fValue, in double fIncrement, in double offset = 0) {
			return SnapToIncrementSigned(fValue - offset, fIncrement, true) + offset;
		}

		public static double SnapToIncrementHigh(in double fValue, in double fIncrement, in double offset = 0) {
			return SnapToIncrementSigned(fValue - offset, fIncrement, false) + offset;
		}
	}
}
