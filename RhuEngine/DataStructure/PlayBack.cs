using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RNumerics;

namespace RhuEngine.DataStructure
{
	public struct Playback : ISerlize<Playback>
	{
		public bool Playing { get; set; }
		public bool Looping { get; set; }
		public float Speed { get; set; }
		public double Offset { get; set; }
		public double Position { get; set; }

		public static TypeCode GetTypeCode() {
			return TypeCode.Object;
		}

		public void DeSerlize(BinaryReader binaryReader) {
			Playing = binaryReader.ReadBoolean();
			Looping = binaryReader.ReadBoolean();
			Speed = binaryReader.ReadSingle();
			Offset = binaryReader.ReadDouble();
			Position = binaryReader.ReadDouble();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(Playing);
			binaryWriter.Write(Looping);
			binaryWriter.Write(Speed);
			binaryWriter.Write(Offset);
			binaryWriter.Write(Position);
		}
	}
}