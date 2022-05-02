using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRText
	{
		public Vector2f Size(RFont rFont,char c, FontStyle fontStyle);

		public void Add(string id,string v, Matrix p);
		public void Add(string id,char c, Matrix p,Colorf color,RFont rFont,FontStyle fontStyle, Vector2f textCut);

	}
	public interface IRFont
	{
		public RFont Default { get; }

	}
	public class RFont
	{
		public static IRFont Instance { get; set; }

		public object Instances;
		public RFont(object inst) {
			Instances = inst;
		}

		public static RFont Default => Instance.Default;
	}

	public static class RText
	{
		public static IRText Instance { get; set; }

		public static Vector2f Size(RFont rFont,char c, FontStyle fontStyle) {
			return Instance.Size(rFont,c, fontStyle);
		}

		public static void Add(string v, Matrix p, string id = "loading") {
			Instance.Add(id,v, p);
		}
		public static void Add(string id, char c, Matrix p, Colorf color, RFont rFont, FontStyle fontStyle, Vector2f textCut) {
			Instance.Add(id, c, p,color,rFont, fontStyle, textCut);
		}
	}
}
