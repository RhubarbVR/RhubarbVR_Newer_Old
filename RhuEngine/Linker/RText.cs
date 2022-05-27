using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRText
	{
		public void Add(string id,string v, Matrix p);
		public void Add(string id,char c, Matrix p,Colorf color,RenderFont rFont, Vector2f textCut);

	}


	public static class RText
	{
		public static IRText Instance { get; set; }

		public static void Add(string v, Matrix p, string id = "loading") {
			Instance.Add(id,v, p);
		}
		public static void Add(string id, char c, Matrix p, Colorf color, RenderFont rFont, Vector2f textCut) {
			Instance.Add(id, c, p,color,rFont, textCut);
		}
	}
}
