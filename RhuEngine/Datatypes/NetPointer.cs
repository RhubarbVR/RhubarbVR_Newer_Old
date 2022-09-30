using System;
using System.Linq;
using MessagePack;

namespace RhuEngine.Datatypes
{
	[MessagePackObject]
	public struct NetPointer : IEquatable<NetPointer>
	{
		[Key(0)]
		public ulong id;

		public NetPointer(ulong _id) {
			id = _id;
		}

		public ulong GetID() {
			return id;
		}
		public ushort GetOwnerID() {
			return (ushort)(id & 0xFFFFuL);
		}
		public ulong ItemIndex() {
			return id << 16;
		}
		[IgnoreMember]
		public bool IsLocal => GetID() == 0;

		public static NetPointer BuildID(ulong position, ushort user) {
			return new NetPointer((position << 16) | (user & 0xFFFFuL));
		}
		[IgnoreMember]
		public static NetPointer Blank = new();
		public string HexString() {
			try {
				var temp = BitConverter.ToString(BitConverter.GetBytes(id).Reverse().ToArray()).Replace("-", "");

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
			return other.id == id;
		}

		public override bool Equals(object obj) {
			return obj is NetPointer pointer ? Equals(pointer) : id.Equals(obj);
		}

		public override int GetHashCode() {
			return id.GetHashCode();
		}

		public override string ToString() {
			return HexString();
		}

		public static explicit operator NetPointer(string data) => new NetPointer(Convert.ToUInt64(data, 16));

		public static bool operator ==(NetPointer a, NetPointer b) => a.Equals(b);
		public static bool operator !=(NetPointer a, NetPointer b) => !a.Equals(b);
	}
}
