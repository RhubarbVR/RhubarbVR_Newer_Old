using System;
using System.Collections.Generic;
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
		public static RhubarbFile CreateFile(Guid owner, string name, FileType fileType, byte[] data) {
			return new RhubarbFile {
				Data = data,
				CreationData = DateTimeOffset.Now,
				Creator = owner.ToString(),
				FileType = fileType,
				Name = name,
			};
		}

		public static RhubarbFile ReadFile(byte[] data) {
			return Serializer.Read<RhubarbFile>(data);
		}

		public static (T, RhubarbFile) ReadFile<T>(byte[] data) {
			var file = Serializer.Read<RhubarbFile>(data);
			return (Serializer.Read<T>(file.Data), file);;
		}

		public static byte[] SaveFile(RhubarbFile rhubarbFile) {
			return Serializer.Save(rhubarbFile);
		}
		public static byte[] SaveFile(Guid owner, string name, FileType fileType, byte[] data) {
			return Serializer.Save(CreateFile(owner, name, fileType, data));
		}

		public static byte[] SaveFile(Guid owner, ComplexMesh amesh) {
			return Serializer.Save(CreateFile(owner, amesh.MeshName, FileType.Mesh, Serializer.Save(amesh)));
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
