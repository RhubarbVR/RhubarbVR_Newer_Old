using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using RhuEngine.WorldObjects;
using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using RhuEngine.WorldObjects.ECS;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RhuEngine.Components;

namespace RhuEngine
{
	public static class ArrayHelper
	{
		public static T[] EnsureSize<T>(this T[] array, int length, bool keepData = false) {
			if (array == null || array.Length < length) {
				var array2 = new T[length];
				if (keepData && array != null) {
					Array.Copy(array, array2, Math.Min(array.Length, array2.Length));
				}
				return array2;
			}
			return array;
		}


	}
}
