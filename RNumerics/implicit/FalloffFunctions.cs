// Copyright (c) Ryan Schmidt (rms@gradientspace.com) - All Rights Reserved
// Distributed under the Boost Software License, Version 1.0. http://www.boost.org/LICENSE_1_0.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RNumerics;

namespace RNumerics
{
	public interface IFalloffFunction
	{
		/// <summary>
		/// t is value in range [0,1], returns value in range [0,1]
		/// </summary>
		double FalloffT(double t);

		/// <summary>
		/// In most cases, users of IFalloffFunction will make a local copy
		/// </summary>
		IFalloffFunction Duplicate();
	}



	/// <summary>
	/// returns 1 in range [0,ConstantRange], and then falls off to 0 in range [ConstantRange,1]
	/// </summary>
	public sealed class LinearFalloff : IFalloffFunction
	{
		public double ConstantRange = 0;

		public double FalloffT(double t) {
			t = MathUtil.Clamp(t, 0.0, 1.0);
			return ConstantRange <= 0 ? 1.0 - t : (t < ConstantRange) ? 1.0 : 1.0 - ((t - ConstantRange) / (1 - ConstantRange));
		}


		public IFalloffFunction Duplicate() {
			return new WyvillFalloff() {
				ConstantRange = ConstantRange
			};
		}
	}



	/// <summary>
	/// returns 1 in range [0,ConstantRange], and then falls off to 0 in range [ConstantRange,1]
	/// </summary>
	public sealed class WyvillFalloff : IFalloffFunction
	{
		public double ConstantRange = 0;

		public double FalloffT(double t) {
			t = MathUtil.Clamp(t, 0.0, 1.0);
			return ConstantRange <= 0 ? MathUtil.WyvillFalloff01(t) : MathUtil.WyvillFalloff(t, ConstantRange, 1.0);
		}


		public IFalloffFunction Duplicate() {
			return new WyvillFalloff() {
				ConstantRange = ConstantRange
			};
		}

	}



}
