using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RNumerics
{
	/// <summary>
	/// This class represents an outline font, where the outline is composed of polygons.
	/// Each font is a list of GeneralPolygon2D objects, so each outline may have 1 or more holes.
	/// (In fact, the mapping is [string,list_of_gpolygons], so you can actually keep entire strings together if desired)
	/// </summary>
	public class PolygonFont2d
	{
		public class CharacterInfo
		{
			public GeneralPolygon2d[] Polygons;
			public AxisAlignedBox2d Bounds;
		}

		public Dictionary<string, CharacterInfo> Characters;
		public AxisAlignedBox2d MaxBounds;


		public PolygonFont2d() {
			Characters = new Dictionary<string, CharacterInfo>();
			MaxBounds = AxisAlignedBox2d.Empty;
		}


		public void AddCharacter(string s, GeneralPolygon2d[] polygons) {
			var info = new CharacterInfo {
				Polygons = polygons,
				Bounds = polygons[0].Bounds
			};
			for (var i = 1; i < polygons.Length; ++i) {
				info.Bounds.Contain(polygons[i].Bounds);
			}

			Characters.Add(s, info);

			MaxBounds.Contain(info.Bounds);
		}


		public List<GeneralPolygon2d> GetCharacter(char c) {
			var s = c.ToString();
			return !Characters.ContainsKey(s)
				? throw new Exception("PolygonFont2d.GetCharacterBounds: character " + c + " not available!")
				: new List<GeneralPolygon2d>(Characters[s].Polygons);
		}
		public List<GeneralPolygon2d> GetCharacter(string s) {
			return !Characters.ContainsKey(s)
				? throw new Exception("PolygonFont2d.GetCharacterBounds: character " + s + " not available!")
				: new List<GeneralPolygon2d>(Characters[s].Polygons);
		}

		public AxisAlignedBox2d GetCharacterBounds(char c) {
			var s = c.ToString();
			return !Characters.ContainsKey(s)
				? throw new Exception("PolygonFont2d.GetCharacterBounds: character " + c + " not available!")
				: Characters[s].Bounds;
		}

		public bool HasCharacter(char c) {
			var s = c.ToString();
			return Characters.ContainsKey(s);
		}
	}
}
