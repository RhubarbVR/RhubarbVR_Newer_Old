using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public static class UIntColorHelper
	{

		public static uint MakeRandomColor(byte alpha = 255) {
			var value = new Random();
			var byrs = new byte[3];
			value.NextBytes(byrs);
			return MakeColor(byrs[0], byrs[1], byrs[2], alpha);
		}
		public static uint MakeColor(byte r, byte g, byte b, byte a) {
			return (uint)r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24);
		}
		public static byte R(this uint value) {
			return (byte)(value & 0xFF);
		}
		public static byte G(this uint value) {
			return (byte)((value >> 8) & 0xFF);
		}
		public static byte B(this uint value) {
			return (byte)((value >> 16) & 0xFF);
		}
		public static byte A(this uint value) {
			return (byte)((value >> 24) & 0xFF);
		}

		public static (byte r, byte g, byte b, byte a) GetColor(this uint value) {
			return (value.R(), value.G(), value.B(), value.A());
		}
	}
}
