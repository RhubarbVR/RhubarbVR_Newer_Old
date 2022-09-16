﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	static public class Units
	{
		public enum Angular
		{
			Degrees,
			Radians
		}

		public enum Linear
		{
			UnknownUnits = 0,

			// numbers are 20 + power-of-ten  
			Nanometers = 11,        // 10^-9
			Microns = 14,           // 10^-6
			Millimeters = 17,       // 10^-3
			Centimeters = 18,       // 10^-2
			Meters = 20,            // 10^0
			Kilometers = 23,        // 10^3

			// metric values must be < 50

			Inches = 105,
			Feet = 109,
			Yards = 110,
			Miles = 115
		}



		public static bool IsMetric(in Linear t) {
			return t > 0 && (int)t < 50;     // based on enum above
		}


		public static double GetMetricPower(in Linear t) {
			if (t > 0 && (int)t < 50) {
				return (double)t - 20.0;        // based on enum values above
			}

			throw new Exception("Units.GetMetricPower: input unit is not metric!");
		}


		public static double ToMeters(in Linear t) {
			if (t > 0 && (int)t < 50) {
				var d = GetMetricPower(t);
				return Math.Pow(10, d);
			}
			switch (t) {
				case Linear.Inches:
					return 0.0254;
				case Linear.Feet:
					return 0.3048;
				case Linear.Yards:
					return 0.9144;
				case Linear.Miles:
					return 1609.34;
				case Linear.UnknownUnits:
					break;
				case Linear.Nanometers:
					break;
				case Linear.Microns:
					break;
				case Linear.Millimeters:
					break;
				case Linear.Centimeters:
					break;
				case Linear.Meters:
					break;
				case Linear.Kilometers:
					break;
			}
			throw new Exception("Units.ToMeters: input unit is not handled!");
		}


		public static double MetersTo(in Linear t) {
			if (t > 0 && (int)t < 50) {
				var d = GetMetricPower(t);
				return Math.Pow(10, -d);
			}
			switch (t) {
				case Linear.Inches:
					return 1.0 / 0.0254;
				case Linear.Feet:
					return 1.0 / 0.3048;
				case Linear.Yards:
					return 1.0 / 0.9144;
				case Linear.Miles:
					return 1.0 / 1609.34;
				case Linear.UnknownUnits:
					break;
				case Linear.Nanometers:
					break;
				case Linear.Microns:
					break;
				case Linear.Millimeters:
					break;
				case Linear.Centimeters:
					break;
				case Linear.Meters:
					break;
				case Linear.Kilometers:
					break;
			}
			throw new Exception("Units.ToMeters: input unit is not handled!");
		}


		public static double Convert(in Linear from, in Linear to) {
			if (from == to) {
				return 1.0;
			}

			if (IsMetric(from) && IsMetric(to)) {
				var pfrom = GetMetricPower(from);
				var pto = GetMetricPower(to);
				var d = pfrom - pto;
				return Math.Pow(10, d);
			}

			return ToMeters(from) * MetersTo(to);
		}



		public static Linear ParseLinear(in string units) {
			// [TODO] could switch on first character? would be more efficient
			if (units == "nm") {
				return Linear.Nanometers;
			}
			else if (units == "um") {
				return Linear.Microns;
			}
			else if (units == "mm") {
				return Linear.Millimeters;
			}
			else if (units == "cm") {
				return Linear.Centimeters;
			}
			else if (units == "m") {
				return Linear.Meters;
			}
			else if (units == "km") {
				return Linear.Kilometers;
			}
			else if (units == "in") {
				return Linear.Inches;
			}
			else if (units == "ft") {
				return Linear.Feet;
			}
			else if (units == "yd") {
				return Linear.Yards;
			}
			else if (units == "mi") {
				return Linear.Miles;
			}
			return Linear.UnknownUnits;
		}



		public static string GetShortString(in Linear unit) {
			switch (unit) {
				case Linear.UnknownUnits:
					return "??";
				case Linear.Nanometers:
					return "nm";
				case Linear.Microns:
					return "um";
				case Linear.Millimeters:
					return "mm";
				case Linear.Centimeters:
					return "cm";
				case Linear.Meters:
					return "m";
				case Linear.Kilometers:
					return "km";

				case Linear.Inches:
					return "in";
				case Linear.Feet:
					return "ft";
				case Linear.Yards:
					return "yd";
				case Linear.Miles:
					return "mi";
				default:
					break;
			}
			throw new Exception("Units.GetShortString: unhandled unit type!");
		}



	}
}
