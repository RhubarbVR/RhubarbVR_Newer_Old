using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;

using LibVLCSharp.Shared;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using SharedModels.GameSpecific;

namespace RhuEngine.AssetSystem
{
	public static class RhubarbFileManager
	{
		public static RhubarbFile CreateFile(Guid owner, string name, FileType fileType, byte[] data, FileCompressionType fileCompressionType = FileCompressionType.DeflateStream) {
			return new RhubarbFile {
				Data = data,
				CreationData = DateTimeOffset.Now,
				Creator = owner,
				FileType = fileType,
				Name = name,
				CompressionType = fileCompressionType
			};
		}

		public static RhubarbFile ReadFile(byte[] data) {
			RhubarbFile file = null;
			try {
				using var s = new MemoryStream(data);
				using var e = new BinaryReader(s);
				file = new RhubarbFile {
					Name = e.ReadString(),
					CompressionType = (FileCompressionType)e.ReadByte(),
					Creator = new Guid(e.ReadBytes(16)),
					CreationData = new DateTimeOffset(e.ReadInt32(), e.ReadInt32(), e.ReadInt32(), e.ReadInt32(), e.ReadInt32(), e.ReadInt32(), e.ReadInt32(), new TimeSpan(e.ReadInt32(), e.ReadInt32(), e.ReadInt32())),
					FileType = (FileType)e.ReadByte(),
				};
				switch (file.CompressionType) {
					case FileCompressionType.DeflateStream:
						file.Data = DeflateStreamDecompress(e.ReadBytes(e.ReadInt32()));
						break;
					case FileCompressionType.DeflateStreamFast:
						file.Data = DeflateStreamDecompress(e.ReadBytes(e.ReadInt32()));
						break;
					default:
						file.Data = e.ReadBytes(e.ReadInt32());
						break;
				}
			}
			catch { }
			return file;
		}

		public static (T, RhubarbFile) ReadFile<T>(byte[] data) {
			var file = ReadFile(data);
			return (Serializer.Read<T>(file.Data), file);
		}

		public static byte[] DeflateStreamCompress(byte[] data, CompressionLevel compressionLevel) {
			var output = new MemoryStream();
			using (var dstream = new DeflateStream(output, compressionLevel)) {
				dstream.Write(data, 0, data.Length);
			}
			return output.ToArray();
		}

		public static byte[] DeflateStreamDecompress(byte[] data) {
			var input = new MemoryStream(data);
			var output = new MemoryStream();
			using (var dstream = new DeflateStream(input, CompressionMode.Decompress)) {
				dstream.CopyTo(output);
			}
			return output.ToArray();
		}

		public static byte[] SaveFile(RhubarbFile rhubarbFile) {
			var buffer = Array.Empty<byte>();
			using (var s = new MemoryStream()) {
				using (var e = new BinaryWriter(s)) {
					var compressType = rhubarbFile.CompressionType;
					e.Write(rhubarbFile.Name);
					if ((rhubarbFile.Data?.Length ?? 0) == 0) {
						e.Write((byte)FileCompressionType.Uncompressed);
						compressType = FileCompressionType.Uncompressed;
					}
					else {
						e.Write((byte)rhubarbFile.CompressionType);
					}
					e.Write(rhubarbFile.Creator.ToByteArray());
					var timeOffset = rhubarbFile.CreationData;
					e.Write(timeOffset.Year);
					e.Write(timeOffset.Month);
					e.Write(timeOffset.Day);
					e.Write(timeOffset.Hour);
					e.Write(timeOffset.Minute);
					e.Write(timeOffset.Second);
					e.Write(timeOffset.Millisecond);
					e.Write(timeOffset.Offset.Hours);
					e.Write(timeOffset.Offset.Minutes);
					e.Write(timeOffset.Offset.Seconds);
					e.Write((byte)rhubarbFile.FileType);
					switch (compressType) {
						case FileCompressionType.DeflateStream:
							var compressed = DeflateStreamCompress(rhubarbFile.Data, CompressionLevel.Optimal);
							e.Write(compressed.Length);
							e.Write(compressed);
							break;
						case FileCompressionType.DeflateStreamFast:
							var compressedFast = DeflateStreamCompress(rhubarbFile.Data, CompressionLevel.Fastest);
							e.Write(compressedFast.Length);
							e.Write(compressedFast);
							break;
						default:
							e.Write(rhubarbFile.Data?.Length ?? 0);
							e.Write(rhubarbFile.Data ?? Array.Empty<byte>());
							break;
					}
				}

				buffer = s.GetBuffer();
			}

			return buffer;
		}
		public static byte[] SaveFile(Guid owner, string name, FileType fileType, byte[] data) {
			return SaveFile(CreateFile(owner, name, fileType, data));
		}
		public static (ComplexMesh, RhubarbFile) ReadComplexMesh(byte[] data) {
			var file = ReadFile(data);
			var messh = new ComplexMesh();
			using (var s = new MemoryStream(file.Data)) {
				using var e = new BinaryReader(s);
				messh.ReadData(e);
			}
			return (messh, file);
		}
		public static byte[] SaveFile(Guid owner, ComplexMesh amesh) {
			var buffer = Array.Empty<byte>();
			using (var s = new MemoryStream()) {
				using (var e = new BinaryWriter(s)) {
					amesh.WriteData(e);
				}
				buffer = s.GetBuffer();
			}
			return SaveFile(CreateFile(owner, amesh.MeshName, FileType.Mesh, buffer));
		}

		public static byte[] SaveFile(Entity saveEntity, bool enbedAssets = false, bool preserveAssets = true) {
			//Todo add avatar detection
			throw new NotImplementedException();
		}
		public static byte[] SaveFile(World saveWorld, bool enbedAssets = false) {
			throw new NotImplementedException();
		}


	}
}
