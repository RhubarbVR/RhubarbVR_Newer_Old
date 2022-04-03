using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRText
	{
		public Vector2f Size(string text);

		public void Add(string v, Matrix p);
	}

	public static class RText
	{
		public static IRText Instance { get; set; }

		public static Vector2f Size(string text) {
			return Instance.Size(text);
		}

		public static void Add(string v, Matrix p) {
			Instance.Add(v, p);
		}
	}
}
