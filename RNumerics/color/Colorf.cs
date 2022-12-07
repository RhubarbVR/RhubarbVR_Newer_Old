using Assimp;

using MessagePack;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{

	[MessagePackObject]
	public struct Colorf : IComparable<Colorf>, IEquatable<Colorf>
	{
		[Key(0)]
		public float r = 1;
		[Key(1)]
		public float g = 1;
		[Key(2)]
		public float b = 1;
		[Key(3)]
		public float a = 1;
		public Colorf() {
			r = 0f;
			g = 0f;
			b = 0f;
			a = 0f;
		}
		public Colorf(in float greylevel, in float a = 1) { r = g = b = greylevel; this.a = a; }
		public Colorf(in float r, in float g, in float b, in float a = 1) { this.r = r; this.g = g; this.b = b; this.a = a; }
		public Colorf(in int r, in int g, in int b, in int a = 255) {
			this.r = MathUtil.Clamp((float)r, 0.0f, 255.0f) / 255.0f;
			this.g = MathUtil.Clamp((float)g, 0.0f, 255.0f) / 255.0f;
			this.b = MathUtil.Clamp((float)b, 0.0f, 255.0f) / 255.0f;
			this.a = MathUtil.Clamp((float)a, 0.0f, 255.0f) / 255.0f;
		}
		public Colorf(in float[] v2) { r = v2[0]; g = v2[1]; b = v2[2]; a = v2[3]; }
		public Colorf(in Colorf copy) { r = copy.r; g = copy.g; b = copy.b; a = copy.a; }
		public Colorf(in Colorf copy, in float newAlpha) { r = copy.r; g = copy.g; b = copy.b; a = newAlpha; }


		public static Colorf Parse(in string colorString) {
			if (colorString.Length == 0) {
				return White;
			}
			try {
				if (colorString[0] == '#') {
					if (colorString.Length == 7) {
						var color = Convert.ToInt32(colorString.Substring(1), 16);
						var r = (color & 0xff0000) >> 16;
						var g = (color & 0xff00) >> 8;
						var b = color & 0xff;
						return new Colorf(r, g, b);
					}
					else if (colorString.Length == 9) {
						var color = Convert.ToInt32(colorString.Substring(1), 16);
						var r = (color & 0xff000000) >> 24;
						var g = (color & 0xff0000) >> 16;
						var b = (color & 0xff00) >> 8;
						var a = color & 0xff00;
						return new Colorf(r, g, b, a);
					}
				}
				if (colorString.Contains("(") && colorString.Contains(")")) {
					var lowerText = colorString.ToLower();
					var waStrings = lowerText.Substring(lowerText.IndexOf('(')).GetArgStrings().GetEnumerator();
					if (lowerText.Contains("rgba")) {
						waStrings.MoveNext();
						var fr = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fb = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fg = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fa = float.Parse(waStrings.Current);
						return new Colorf(fr, fb, fg, fa);
					}
					else if (lowerText.Contains("rgb")) {
						waStrings.MoveNext();
						Console.WriteLine(waStrings.Current);
						var fr = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fb = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fg = float.Parse(waStrings.Current);
						return new Colorf(fr, fb, fg);
					}
					else if (lowerText.Contains("hsv")) {
						waStrings.MoveNext();
						var fr = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fb = float.Parse(waStrings.Current);
						waStrings.MoveNext();
						var fg = float.Parse(waStrings.Current);
						return new ColorHSV(fr, fb, fg);
					}
				}
				return GetStaticColor((Colors)Enum.Parse(typeof(Colors), colorString, true));
			}
			catch {
				return White;
			}
		}
		[IgnoreMember, JsonIgnore]
		public static Random random = new();

		public static Colorf RandomHue() {
			var color = new ColorHSV((float)random.NextDouble() * 360, 1, 1);
			return color.ConvertToRGB();
		}

		public Colorf Clone(in float fAlphaMultiply = 1.0f) {
			return new Colorf(r, g, b, a * fAlphaMultiply);
		}

		[IgnoreMember, JsonIgnore]
		public float this[in int key]
		{
			get => key == 0 ? r : key == 1 ? g : key == 2 ? b : a;
			set {
				if (key == 0) { r = value; }
				else if (key == 1) { g = value; }
				else if (key == 2) { b = value; }
				else {
					a = value;
				}
			}
		}

		public float SqrDistance(in Colorf v2) {
			float a = r - v2.r, b = g - v2.g, c = b - v2.b, d = a - v2.a;
			return (a * a) + (b * b) + (c * c) + (d * d);
		}

		public Vector3f ToRGB() {
			return new Vector3f(r, g, b);
		}
		public Vector4f ToRGBA() {
			return new Vector4f(r, g, b, a);
		}
		public Colorb ToBytes() {
			return new Colorb(r, g, b, a);
		}

		public void Set(in Colorf o) {
			r = o.r;
			g = o.g;
			b = o.b;
			a = o.a;
		}
		public void Set(in float fR, in float fG, in float fB, in float fA) {
			r = fR;
			g = fG;
			b = fB;
			a = fA;
		}


		public Colorf SetAlpha(in float a) {
			this.a = a;
			return this;
		}
		public void Add(in Colorf o) {
			r += o.r;
			g += o.g;
			b += o.b;
			a += o.a;
		}
		public void Subtract(in Colorf o) {
			r -= o.r;
			g -= o.g;
			b -= o.b;
			a -= o.a;
		}
		public Colorf WithAlpha(in float newAlpha) {
			return new Colorf(r, g, b, newAlpha);
		}


		public static Colorf operator -(in Colorf v) => new(-v.r, -v.g, -v.b, -v.a);

		public static Colorf operator *(in float f, in Colorf v) => new(f * v.r, f * v.g, f * v.b, f * v.a);
		public static Colorf operator *(in Colorf v, in float f) => new(f * v.r, f * v.g, f * v.b, f * v.a);
		public static Colorf operator *(in Colorf v, in Colorf f) => new(f.r * v.r, f.g * v.g, f.b * v.b, f.a * v.a);

		public static Colorf operator +(in Colorf v0, in Colorf v1) => new(v0.r + v1.r, v0.g + v1.g, v0.b + v1.b, v0.a + v1.a);
		public static Colorf operator +(in Colorf v0, in float f) => new(v0.r + f, v0.g + f, v0.b + f, v0.a + f);

		public static Colorf operator -(in Colorf v0, in Colorf v1) => new(v0.r - v1.r, v0.g - v1.g, v0.b - v1.b, v0.a - v1.a);
		public static Colorf operator -(Colorf v0, in float f) => new(v0.r - f, v0.g - f, v0.b - f, v0.a = f);


		public static bool operator ==(in Colorf a, in Colorf b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
		public static bool operator !=(in Colorf a, in Colorf b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;

		public unsafe Color4D ToAssimp() {
			fixed (Colorf* vector3f = &this) {
				return *(Color4D*)vector3f;
			}
		}
		public static unsafe Colorf ToRhuNumricsFromAssimp(ref Color4D value) {
			fixed (Color4D* vector3f = &value) {
				return *(Colorf*)vector3f;
			}
		}
		public static implicit operator Color4D(in Colorf b) => b.ToAssimp();

		public static implicit operator Colorf(Color4D b) => ToRhuNumricsFromAssimp(ref b);


		public override bool Equals(object obj) {
			return this == (Colorf)obj;
		}
		public override int GetHashCode() {
			return (r + g + b + a).GetHashCode();
		}
		public int CompareTo(Colorf other) {
			if (r != other.r) {
				return r < other.r ? -1 : 1;
			}
			else if (g != other.g) {
				return g < other.g ? -1 : 1;
			}
			else if (b != other.b) {
				return b < other.b ? -1 : 1;
			}
			else if (a != other.a) {
				return a < other.a ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Colorf other) {
			return r == other.r && g == other.g && b == other.b && a == other.a;
		}


		public static Colorf Lerp(in Colorf a, in Colorf b, in float t) {
			var s = 1 - t;
			return new Colorf((s * a.r) + (t * b.r), (s * a.g) + (t * b.g), (s * a.b) + (t * b.b), (s * a.a) + (t * b.a));
		}



		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", r, g, b, a);
		}
		public string ToString(in string fmt) {
			return string.Format("{0} {1} {2} {3}", r.ToString(fmt), g.ToString(fmt), b.ToString(fmt), a.ToString(fmt));
		}

		// allow conversion to/from Vector3f
		public static implicit operator Vector3f(in Colorf c) => new(c.r, c.g, c.b);
		public static implicit operator Colorf(in Vector3f c) => new(c.x, c.y, c.z, 1);


		public static implicit operator Colorf(ColorHSV c) => c.ConvertToRGB();


		public static implicit operator ColorHSV(in Colorf color) {
			var outval = new ColorHSV(0, 0, 0, color.a);
			outval.ConvertFromRGB(color);
			return outval;
		}

		public enum Colors
		{
			// CSS Specification Colors
			Black, Silver, Gray, White, Maroon, Red, Purple, Fuchsia,
			Green, Lime, Olive, Yellow, Navy, Blue, Teal, Aqua, Orange,
			AliceBlue, AntiqueWhite, Aquamarine, Azure, Beige, Bisque,
			BlanchedAlmond, BlueBiolet, Brown, BurlyWood, CadetBlue,
			Chartreuse, Chocolate, Coral, CornflowerBlue, Cornsilk,
			Crimson, Cyan, DarkBlue, DarkCyan, DarkGoldenrod, DarkGray,
			DarkGreen, DarkGrey, DarkKhaki, DarkMagenta, DarkOliveGreen,
			DarkOrange, DarkOrchid, DarkRed, DarkSalmon, DarkSeaGreen,
			DarkSlateBlue, DarkSlateGray, DarkSlateGrey, DarkTurquoise,
			DarkViolet, DeepPink, DeepSkyBlue, DimGray, DimGrey,
			DodgerBlue, FireBrick, FloralWhite, ForestGreen, Gainsboro,
			GhostWhite, Gold, Goldenrod, GreenYellow, Grey, Honeydew,
			HotPink, Indianred, Indigo, Ivory, Khaki, Lavender,
			LavenderBlush, LawnGreen, LemonChiffon, LightBlue,
			LightCoral, LightCyan, LightGoldenrodYellow, LightGray,
			LightGreen, LightGrey, LightPink, LightSalmon, LightSeaGreen,
			LightSkyBlue, LightSlateGray, LightSlateGrey, LightSteelBlue,
			LightYellow, LimeGreen, Linen, Magenta, MediumAquamarine,
			MediumBlue, MediumOrchid, MediumPurple, MediumSeaGreen,
			MediumSlateBlue, MediumSpringGreen, MediumTurquoise,
			MediumVioletRed, MidnightBlue, MintCream, MistyRose,
			Moccasin, NavajoWhite, OldLace, OliveDrab, OrangeRed, Orchid,
			PaleGoldenrod, PaleGreen, PaleTurquoise, PaleVioletRed,
			PapayaWhip, PeachPuff, Peru, Pink, Plum, PowderBlue,
			RosyBrown, RoyalBlue, SaddleBrown, Salmon, SandyBrown,
			SeaGreen, SeaShell, Sienna, SkyBlue, SlateBlue, SlateGray,
			SlateGrey, Snow, SpringGreen, SteelBlue, Tan, Thistle,
			Tomato, Turquoise, Violet, Wheat, WhiteSmoke, YellowGreen,
			RebeccaPurple,
			Transparent,

			// Colors Defined by Rhubarb
			BlueMetal, DarkYellow,
			RhubarbGreen, RhubarbRed, SiennaBrown, TransparentBlack,
			TransparentWhite, VideoBlack, VideoBlue, VideoCyan,
			VideoGreen, VideoMagenta, VideoRed, VideoWhite, VideoYellow,

			// default colors
			StandardBeige, SelectionGold,
			PivotYellow
		}

		public static Colorf GetStaticColor(Colors colors) {
			return colors switch {
				// CSS Specification Colors
				Colors.Black => Black,
				Colors.Silver => Silver,
				Colors.Gray => Gray,
				Colors.White => White,
				Colors.Maroon => Maroon,
				Colors.Red => Red,
				Colors.Purple => Purple,
				Colors.Fuchsia => Fuchsia,
				Colors.Green => Green,
				Colors.Lime => Lime,
				Colors.Olive => Olive,
				Colors.Yellow => Yellow,
				Colors.Navy => Navy,
				Colors.Blue => Blue,
				Colors.Teal => Teal,
				Colors.Aqua => Aqua,
				Colors.Orange => Orange,
				Colors.AliceBlue => AliceBlue,
				Colors.AntiqueWhite => AntiqueWhite,
				Colors.Aquamarine => Aquamarine,
				Colors.Azure => Azure,
				Colors.Beige => Beige,
				Colors.Bisque => Bisque,
				Colors.BlanchedAlmond => BlanchedAlmond,
				Colors.BlueBiolet => BlueBiolet,
				Colors.Brown => Brown,
				Colors.BurlyWood => BurlyWood,
				Colors.CadetBlue => CadetBlue,
				Colors.Chartreuse => Chartreuse,
				Colors.Chocolate => Chocolate,
				Colors.Coral => Coral,
				Colors.CornflowerBlue => CornflowerBlue,
				Colors.Cornsilk => Cornsilk,
				Colors.Crimson => Crimson,
				Colors.Cyan => Cyan,
				Colors.DarkBlue => DarkBlue,
				Colors.DarkCyan => DarkCyan,
				Colors.DarkGoldenrod => DarkGoldenrod,
				Colors.DarkGray => DarkGray,
				Colors.DarkGreen => DarkGreen,
				Colors.DarkGrey => DarkGrey,
				Colors.DarkKhaki => DarkKhaki,
				Colors.DarkMagenta => DarkMagenta,
				Colors.DarkOliveGreen => DarkOliveGreen,
				Colors.DarkOrange => DarkOrange,
				Colors.DarkOrchid => DarkOrchid,
				Colors.DarkRed => DarkRed,
				Colors.DarkSalmon => DarkSalmon,
				Colors.DarkSeaGreen => DarkSeaGreen,
				Colors.DarkSlateBlue => DarkSlateBlue,
				Colors.DarkSlateGray => DarkSlateGray,
				Colors.DarkSlateGrey => DarkSlateGrey,
				Colors.DarkTurquoise => DarkTurquoise,
				Colors.DarkViolet => DarkViolet,
				Colors.DeepPink => DeepPink,
				Colors.DeepSkyBlue => DeepSkyBlue,
				Colors.DimGray => DimGray,
				Colors.DimGrey => DimGrey,
				Colors.DodgerBlue => DodgerBlue,
				Colors.FireBrick => FireBrick,
				Colors.FloralWhite => FloralWhite,
				Colors.ForestGreen => ForestGreen,
				Colors.Gainsboro => Gainsboro,
				Colors.GhostWhite => GhostWhite,
				Colors.Gold => Gold,
				Colors.Goldenrod => Goldenrod,
				Colors.GreenYellow => GreenYellow,
				Colors.Grey => Grey,
				Colors.Honeydew => Honeydew,
				Colors.HotPink => HotPink,
				Colors.Indianred => Indianred,
				Colors.Indigo => Indigo,
				Colors.Ivory => Ivory,
				Colors.Khaki => Khaki,
				Colors.Lavender => Lavender,
				Colors.LavenderBlush => LavenderBlush,
				Colors.LawnGreen => LawnGreen,
				Colors.LemonChiffon => LemonChiffon,
				Colors.LightBlue => LightBlue,
				Colors.LightCoral => LightCoral,
				Colors.LightCyan => LightCyan,
				Colors.LightGoldenrodYellow => LightGoldenrodYellow,
				Colors.LightGray => LightGray,
				Colors.LightGreen => LightGreen,
				Colors.LightGrey => LightGrey,
				Colors.LightPink => LightPink,
				Colors.LightSalmon => LightSalmon,
				Colors.LightSeaGreen => LightSeaGreen,
				Colors.LightSkyBlue => LightSkyBlue,
				Colors.LightSlateGray => LightSlateGray,
				Colors.LightSlateGrey => LightSlateGrey,
				Colors.LightSteelBlue => LightSteelBlue,
				Colors.LightYellow => LightYellow,
				Colors.LimeGreen => LimeGreen,
				Colors.Linen => Linen,
				Colors.Magenta => Magenta,
				Colors.MediumAquamarine => MediumAquamarine,
				Colors.MediumBlue => MediumBlue,
				Colors.MediumOrchid => MediumOrchid,
				Colors.MediumPurple => MediumPurple,
				Colors.MediumSeaGreen => MediumSeaGreen,
				Colors.MediumSlateBlue => MediumSlateBlue,
				Colors.MediumSpringGreen => MediumSpringGreen,
				Colors.MediumTurquoise => MediumTurquoise,
				Colors.MediumVioletRed => MediumVioletRed,
				Colors.MidnightBlue => MidnightBlue,
				Colors.MintCream => MintCream,
				Colors.MistyRose => MistyRose,
				Colors.Moccasin => Moccasin,
				Colors.NavajoWhite => NavajoWhite,
				Colors.OldLace => OldLace,
				Colors.OliveDrab => OliveDrab,
				Colors.OrangeRed => OrangeRed,
				Colors.Orchid => Orchid,
				Colors.PaleGoldenrod => PaleGoldenrod,
				Colors.PaleGreen => PaleGreen,
				Colors.PaleTurquoise => PaleTurquoise,
				Colors.PaleVioletRed => PaleVioletRed,
				Colors.PapayaWhip => PapayaWhip,
				Colors.PeachPuff => PeachPuff,
				Colors.Peru => Peru,
				Colors.Pink => Pink,
				Colors.Plum => Plum,
				Colors.PowderBlue => PowderBlue,
				Colors.RosyBrown => RosyBrown,
				Colors.RoyalBlue => RoyalBlue,
				Colors.SaddleBrown => SaddleBrown,
				Colors.Salmon => Salmon,
				Colors.SandyBrown => SandyBrown,
				Colors.SeaGreen => SeaGreen,
				Colors.SeaShell => SeaShell,
				Colors.Sienna => Sienna,
				Colors.SkyBlue => SkyBlue,
				Colors.SlateBlue => SlateBlue,
				Colors.SlateGray => SlateGray,
				Colors.SlateGrey => SlateGrey,
				Colors.Snow => Snow,
				Colors.SpringGreen => SpringGreen,
				Colors.SteelBlue => SteelBlue,
				Colors.Tan => Tan,
				Colors.Thistle => Thistle,
				Colors.Tomato => Tomato,
				Colors.Turquoise => Turquoise,
				Colors.Violet => Violet,
				Colors.Wheat => Wheat,
				Colors.WhiteSmoke => WhiteSmoke,
				Colors.YellowGreen => YellowGreen,
				Colors.RebeccaPurple => RebeccaPurple,
				Colors.Transparent => Transparent,

				// Colors Defined by Rhubarb
				Colors.BlueMetal => BlueMetal,
				Colors.DarkYellow => DarkYellow,
				Colors.RhubarbGreen => RhubarbGreen,
				Colors.RhubarbRed => RhubarbRed,
				Colors.SiennaBrown => SiennaBrown,
				Colors.TransparentBlack => TransparentBlack,
				Colors.TransparentWhite => TransparentWhite,
				Colors.VideoBlack => VideoBlack,
				Colors.VideoBlue => VideoBlue,
				Colors.VideoCyan => VideoCyan,
				Colors.VideoGreen => VideoGreen,
				Colors.VideoMagenta => VideoMagenta,
				Colors.VideoRed => VideoRed,
				Colors.VideoWhite => VideoWhite,
				Colors.VideoYellow => VideoYellow,

				// default colors
				Colors.StandardBeige => StandardBeige,
				Colors.SelectionGold => SelectionGold,
				Colors.PivotYellow => PivotYellow,
				_ => White,
			};
		}

		// CSS Specification Colors
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Black = new(0, 0, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Silver = new(192, 192, 192, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Gray = new(128, 128, 128, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf White = new(255, 255, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Maroon = new(128, 0, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Red = new(255, 0, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Purple = new(128, 0, 128, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Fuchsia = new(255, 0, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Green = new(0, 128, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Lime = new(0, 255, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Olive = new(128, 128, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Yellow = new(255, 255, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Navy = new(0, 0, 128, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Blue = new(0, 0, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Teal = new(0, 128, 128, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Aqua = new(0, 255, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Orange = new(255, 165, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf AliceBlue = new(240, 248, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf AntiqueWhite = new(250, 235, 215, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Aquamarine = new(127, 255, 212, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Azure = new(240, 255, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Beige = new(245, 245, 220, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Bisque = new(255, 228, 196, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf BlanchedAlmond = new(255, 235, 205, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf BlueBiolet = new(138, 43, 226, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Brown = new(165, 42, 42, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf BurlyWood = new(222, 184, 135, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf CadetBlue = new(95, 158, 160, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Chartreuse = new(127, 255, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Chocolate = new(210, 105, 30, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Coral = new(255, 127, 80, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf CornflowerBlue = new(100, 149, 237, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Cornsilk = new(255, 248, 220, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Crimson = new(220, 20, 60, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Cyan = new(0, 255, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkBlue = new(0, 0, 139, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkCyan = new(0, 139, 139, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkGoldenrod = new(184, 134, 11, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkGray = new(169, 169, 169, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkGreen = new(0, 100, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkGrey = new(169, 169, 169, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkKhaki = new(189, 183, 107, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkMagenta = new(139, 0, 139, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkOliveGreen = new(85, 107, 47, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkOrange = new(255, 140, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkOrchid = new(153, 50, 204, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkRed = new(139, 0, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkSalmon = new(233, 150, 122, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkSeaGreen = new(143, 188, 143, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkSlateBlue = new(72, 61, 139, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkSlateGray = new(47, 79, 79, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkSlateGrey = new(47, 79, 79, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkTurquoise = new(0, 206, 209, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkViolet = new(148, 0, 211, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DeepPink = new(255, 20, 147, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DeepSkyBlue = new(0, 191, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DimGray = new(105, 105, 105, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DimGrey = new(105, 105, 105, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DodgerBlue = new(30, 144, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf FireBrick = new(178, 34, 34, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf FloralWhite = new(255, 250, 240, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf ForestGreen = new(34, 139, 34, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Gainsboro = new(220, 220, 220, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf GhostWhite = new(248, 248, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Gold = new(255, 215, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Goldenrod = new(218, 165, 32, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf GreenYellow = new(173, 255, 47, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Grey = new(128, 128, 128, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Honeydew = new(240, 255, 240, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf HotPink = new(255, 105, 180, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Indianred = new(205, 92, 92, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Indigo = new(75, 0, 130, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Ivory = new(255, 255, 240, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Khaki = new(240, 230, 140, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Lavender = new(230, 230, 250, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LavenderBlush = new(255, 240, 245, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LawnGreen = new(124, 252, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LemonChiffon = new(255, 250, 205, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightBlue = new(173, 216, 230, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightCoral = new(240, 128, 128, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightCyan = new(224, 255, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightGoldenrodYellow = new(250, 250, 210, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightGray = new(211, 211, 211, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightGreen = new(144, 238, 144, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightGrey = new(211, 211, 211, 255);
		[IgnoreMember, JsonIgnore]	
		static public readonly Colorf LightPink = new(255, 182, 193, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightSalmon = new(255, 160, 122, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightSeaGreen = new(32, 178, 170, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightSkyBlue = new(135, 206, 250, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightSlateGray = new(119, 136, 153, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightSlateGrey = new(119, 136, 153, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightSteelBlue = new(176, 196, 222, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LightYellow = new(255, 255, 224, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf LimeGreen = new(50, 205, 50, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Linen = new(250, 240, 230, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Magenta = new(255, 0, 255, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumAquamarine = new(102, 205, 170, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumBlue = new(0, 0, 205, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumOrchid = new(186, 85, 211, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumPurple = new(147, 112, 219, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumSeaGreen = new(60, 179, 113, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumSlateBlue = new(123, 104, 238, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumSpringGreen = new(0, 250, 154, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumTurquoise = new(72, 209, 204, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MediumVioletRed = new(199, 21, 133, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MidnightBlue = new(25, 25, 112, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MintCream = new(245, 255, 250, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf MistyRose = new(255, 228, 225, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Moccasin = new(255, 228, 181, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf NavajoWhite = new(255, 222, 173, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf OldLace = new(253, 245, 230, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf OliveDrab = new(107, 142, 35, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf OrangeRed = new(255, 69, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Orchid = new(218, 112, 214, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PaleGoldenrod = new(238, 232, 170, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PaleGreen = new(152, 251, 152, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PaleTurquoise = new(175, 238, 238, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PaleVioletRed = new(219, 112, 147, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PapayaWhip = new(255, 239, 213, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PeachPuff = new(255, 218, 185, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Peru = new(205, 133, 63, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Pink = new(255, 192, 203, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Plum = new(221, 160, 221, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf PowderBlue = new(176, 224, 230, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf RosyBrown = new(188, 143, 143, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf RoyalBlue = new(65, 105, 225, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SaddleBrown = new(139, 69, 19, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Salmon = new(250, 128, 114, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SandyBrown = new(244, 164, 96, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SeaGreen = new(46, 139, 87, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SeaShell = new(255, 245, 238, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Sienna = new(160, 82, 45, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SkyBlue = new(135, 206, 235, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SlateBlue = new(106, 90, 205, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SlateGray = new(112, 128, 144, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SlateGrey = new(112, 128, 144, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Snow = new(255, 250, 250, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SpringGreen = new(0, 255, 127, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SteelBlue = new(70, 130, 180, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Tan = new(210, 180, 140, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Thistle = new(216, 191, 216, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Tomato = new(255, 99, 71, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Turquoise = new(64, 224, 208, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Violet = new(238, 130, 238, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Wheat = new(245, 222, 179, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf WhiteSmoke = new(245, 245, 245, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf YellowGreen = new(154, 205, 50, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf RebeccaPurple = new(102, 51, 153, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf Transparent = new(0, 0, 0, 0);
			
		// Colors Defined by Rhubarb
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf BlueMetal = new(176, 197, 235, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf DarkYellow = new(235, 200, 95, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf RhubarbGreen = new(17, 255, 0, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf RhubarbRed = new(237, 25, 67, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf SiennaBrown = new(160, 82, 45, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf TransparentBlack = new(0, 0, 0, 0);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf TransparentWhite = new(255, 255, 255, 0);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoBlack = new(16, 16, 16, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoBlue = new(16, 16, 235, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoCyan = new(16, 235, 235, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoGreen = new(16, 235, 16, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoMagenta = new(235, 16, 235, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoRed = new(235, 16, 16, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoWhite = new(235, 235, 235, 255);
		[IgnoreMember, JsonIgnore]
		static public readonly Colorf VideoYellow = new(235, 235, 16, 255);
			
		// default colors
		[IgnoreMember, JsonIgnore]
		static readonly public Colorf StandardBeige = new(0.75f, 0.75f, 0.5f);
		[IgnoreMember, JsonIgnore]
		static readonly public Colorf SelectionGold = new(1.0f, 0.6f, 0.05f);
		[IgnoreMember, JsonIgnore]
		static readonly public Colorf PivotYellow = new(1.0f, 1.0f, 0.05f);
	}
}
