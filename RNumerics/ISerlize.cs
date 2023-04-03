using System;
using System.IO;

using Assimp;

using K4os.Compression.LZ4;

namespace RNumerics
{
	public static class LZCompress
	{
		public static byte[] Compress(byte[] data, LZ4Level level) {
			var targetOutPut = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
			var e = LZ4Codec.Encode(data.AsSpan(), targetOutPut.AsSpan(), level);
			Array.Resize(ref targetOutPut, e);
			using var memStream = new MemoryStream();
			using var writer = new BinaryWriter(memStream);
			writer.Write(data.Length);
			writer.Write(targetOutPut);
			return memStream.ToArray();
		}

		public static byte[] DeCompress(byte[] data) {
			using var memStream = new MemoryStream(data);
			using var reader = new BinaryReader(memStream);
			var targetOutPut = new byte[reader.ReadInt32()];
			var compressedData = reader.ReadBytes((int)(memStream.Length - memStream.Position));
			LZ4Codec.Decode(compressedData, targetOutPut);
			return targetOutPut;
		}

	}


	public interface ISerlize
	{
		public void Serlize(BinaryWriter binaryWriter);
		public void DeSerlize(BinaryReader binaryReader);
	}

	public interface ISerlize<T> : ISerlize
	{

	}
}
