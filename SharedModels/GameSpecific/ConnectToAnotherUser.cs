using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharedModels.GameSpecific
{

	public struct ConnectToAnotherUser : IRelayNetPacked
	{
		public string Key;
		public ConnectToAnotherUser(string key) : this() {
			Key = key;
		}

		public void DeSerlize(BinaryReader binaryReader) {
			Key = binaryReader.ReadString();
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(Key);
		}
	}
}
