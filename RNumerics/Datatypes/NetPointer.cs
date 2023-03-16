using System;
using System.IO;
using System.Linq;

using RNumerics;

namespace RhuEngine.Datatypes
{
	public struct NetPointer : IEquatable<NetPointer>, ISerlize<NetPointer>
	{
		public ulong _id;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(_id);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			_id = binaryReader.ReadUInt64();
		}

		public NetPointer(ulong _id) {
			this._id = _id;
		}

		public ulong GetID() {
			return _id;
		}
		public ushort GetOwnerID() {
			return (ushort)(_id & 0xFFFFuL);
		}
		public ulong ItemIndex() {
			return _id << 16;
		}
		public bool IsLocal => GetID() == 0;

		public static NetPointer BuildID(ulong position, ushort user) {
			return new NetPointer((position << 16) | (user & 0xFFFFuL));
		}
		public static readonly NetPointer Blank = new();
		public string HexString() {
			try {
				var temp = BitConverter.ToString(BitConverter.GetBytes(_id).Reverse().ToArray()).Replace("-", "");

				if (!string.IsNullOrEmpty(temp)) {
					while (!string.IsNullOrEmpty(temp) && temp.Substring(0, 1) == "0") {
						temp = temp.Substring(1);
					}
				}
				if (string.IsNullOrEmpty(temp)) {
					temp = "0";
				}
				return temp;
			}
			catch {
				return $"0";
			}
		}

		public bool Equals(NetPointer other) {
			return other._id == _id;
		}

		public override bool Equals(object obj) {
			return obj is NetPointer pointer ? Equals(pointer) : _id.Equals(obj);
		}

		public override int GetHashCode() {
			return _id.GetHashCode();
		}

		public override string ToString() {
			return HexString();
		}

		public static explicit operator NetPointer(string data) => new(Convert.ToUInt64(data, 16));

		public static bool operator ==(NetPointer a, NetPointer b) => a.Equals(b);
		public static bool operator !=(NetPointer a, NetPointer b) => !a.Equals(b);
	}
}
