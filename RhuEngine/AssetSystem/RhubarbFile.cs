using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RNumerics;

namespace RhuEngine.AssetSystem
{
	public enum FileType : byte
	{
		Unknown,
		Mesh,
		Entity,
		World,
		Avatar,
	}

	public enum FileCompressionType : byte
	{
		Uncompressed,
		DeflateStream,
		DeflateStreamFast,
	}

	public sealed class RhubarbFile : ISerlize<RhubarbFile>
	{
		public FileType FileType { get; set; }
		public FileCompressionType CompressionType { get; set; }
		public DateTimeOffset CreationData { get; set; }
		public Guid Creator { get; set; }
		public string Name { get; set; }
		public byte[] Data { get; set; }

		public void DeSerlize(BinaryReader binaryReader) {
			Name = binaryReader.ReadString();
			Creator = new Guid(binaryReader.ReadBytes(16));
			FileType = (FileType)binaryReader.ReadByte();
			CompressionType = (FileCompressionType)binaryReader.ReadByte();
			CreationData = DateTimeOffset.FromUnixTimeMilliseconds(binaryReader.ReadInt64());
			var length = binaryReader.ReadInt32();
			Data = binaryReader.ReadBytes(length);
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(Name);
			binaryWriter.Write(Creator.ToByteArray());
			binaryWriter.Write((byte)FileType);
			binaryWriter.Write((byte)CompressionType);
			binaryWriter.Write(CreationData.ToUnixTimeMilliseconds());
			binaryWriter.Write(Data.Length);
			binaryWriter.Write(Data);
		}
	}
}
