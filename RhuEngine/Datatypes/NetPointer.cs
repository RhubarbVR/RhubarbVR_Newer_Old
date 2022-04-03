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

				while (temp.Substring(0, 1) == "0") {
					temp = temp.Substring(1);
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
			return id.Equals(obj);
		}

		public override int GetHashCode() {
			return id.GetHashCode();
		}

		public override string ToString() {
			return HexString();
		}

		public static bool operator ==(NetPointer a, NetPointer b) => a.Equals(b);
		public static bool operator !=(NetPointer a, NetPointer b) => !a.Equals(b);
	}
}
