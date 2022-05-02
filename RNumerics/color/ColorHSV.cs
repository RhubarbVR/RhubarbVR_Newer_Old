using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public class ColorHSV : IConvertible
	{
		[Key(0)]
		public float h;
		[Key(1)]
		public float s;
		[Key(2)]
		public float v;
		[Key(3)]
		public float a;

		public ColorHSV(float h, float s, float v, float a = 1) { this.h = h; this.s = s; this.v = v; this.a = a; }
		public ColorHSV(Colorf rgb) {
			ConvertFromRGB(rgb);
		}

		public ColorHSV UpdateHue(float val) {
			var newh = (h + val) % 360f;
			return new ColorHSV(newh, s, v, a);
		}
		[IgnoreMember]
		public Colorf RGBA
		{
			get => ConvertToRGB();
			set => ConvertFromRGB(value);
		}



		public Colorf ConvertToRGB() {
			var h = this.h;
			var s = this.s;
			var v = this.v;

			if (h > 360) {
				h -= 360;
			}

			if (h < 0) {
				h += 360;
			}

			h = MathUtil.Clamp(h, 0.0f, 360.0f);
			s = MathUtil.Clamp(s, 0.0f, 1.0f);
			v = MathUtil.Clamp(v, 0.0f, 1.0f);
			var c = v * s;
			var x = c * (1 - Math.Abs((h / 60.0f % 2) - 1));
			var m = v - c;
			float rp, gp, bp;
			var a = (int)(h / 60.0f);

			switch (a) {
				case 0:
					rp = c;
					gp = x;
					bp = 0;
					break;

				case 1:
					rp = x;
					gp = c;
					bp = 0;
					break;

				case 2:
					rp = 0;
					gp = c;
					bp = x;
					break;

				case 3:
					rp = 0;
					gp = x;
					bp = c;
					break;

				case 4:
					rp = x;
					gp = 0;
					bp = c;
					break;

				default: // case 5:
					rp = c;
					gp = 0;
					bp = x;
					break;
			}

			return new Colorf(
				MathUtil.Clamp(rp + m, 0, 1),
				MathUtil.Clamp(gp + m, 0, 1),
				MathUtil.Clamp(bp + m, 0, 1), this.a);
		}


		public void ConvertFromRGB(Colorf rgb) {
			a = rgb.a;
			float rp = rgb.r, gp = rgb.g, bp = rgb.b;

			var cmax = rp;
			var cmaxwhich = 0; /* faster comparison afterwards */
			if (gp > cmax) { cmax = gp; cmaxwhich = 1; }
			if (bp > cmax) { cmax = bp; cmaxwhich = 2; }
			var cmin = rp;
			//int cminwhich = 0;
			if (gp < cmin) { cmin = gp; /*cminwhich = 1;*/ }
			if (bp < cmin) { cmin = bp; /*cminwhich = 2;*/ }

			var delta = cmax - cmin;

			/* HUE */
			if (delta == 0) {
				h = 0;
			}
			else {
				switch (cmaxwhich) {
					case 0: /* cmax == rp */
						h = 60.0f * ((gp - bp) / delta % 6.0f);
						break;

					case 1: /* cmax == gp */
						h = 60.0f * (((bp - rp) / delta) + 2);
						break;

					case 2: /* cmax == bp */
						h = 60.0f * (((rp - gp) / delta) + 4);
						break;
				}
				if (h < 0) {
					h += 360.0f;
				}
			}

			/* LIGHTNESS/VALUE */
			//l = (cmax + cmin) / 2;
			v = cmax;

			/* SATURATION */
			/*if (delta == 0) {
              *r_s = 0;
            } else {
              *r_s = delta / (1 - fabs (1 - (2 * (l - 1))));
            }*/
			s = cmax == 0 ? 0 : delta / cmax;
		}

		public TypeCode GetTypeCode() {
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider) {
			return ToString();
		}

		public object ToType(Type conversionType, IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider) {
			throw new NotImplementedException();
		}
	}
}
