using System;

namespace RhuEngine.Utils
{
	public static class MathR
	{
		public static float Clamp(float value, float min, float max)
		{
			return Math.Max(Math.Min(value, max), min);
		}
	}
}