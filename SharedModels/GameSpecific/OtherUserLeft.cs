using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharedModels.GameSpecific
{

	public struct OtherUserLeft : IRelayNetPacked
	{
		public ushort id;
		public OtherUserLeft(ushort id) : this() {
			this.id = id;
		}
		public void DeSerlize(BinaryReader binaryReader) {
			id = binaryReader.ReadUInt16();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(id);
		}
	}
}
