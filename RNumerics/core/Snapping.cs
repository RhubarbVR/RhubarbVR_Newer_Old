using System;

namespace RNumerics
{
	public class Snapping
	{

		public static double SnapToIncrement(double fValue, double fIncrement, double offset = 0) {
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




		public static double SnapToNearbyIncrement(double fValue, double fIncrement, double fTolerance) {
			var snapped = SnapToIncrement(fValue, fIncrement);
			return Math.Abs(snapped - fValue) < fTolerance ? snapped : fValue;
		}

		private static double SnapToIncrementSigned(double fValue, double fIncrement, bool low) {
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

		public static double SnapToIncrementLow(double fValue, double fIncrement, double offset = 0) {
			return SnapToIncrementSigned(fValue - offset, fIncrement, true) + offset;
		}

		public static double SnapToIncrementHigh(double fValue, double fIncrement, double offset = 0) {
			return SnapToIncrementSigned(fValue - offset, fIncrement, false) + offset;
		}
	}
}
