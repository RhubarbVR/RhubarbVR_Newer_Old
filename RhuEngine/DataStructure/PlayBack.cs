using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;
namespace RhuEngine.DataStructure
{
	[MessagePackObject]
	public struct Playback
	{
		[Key(0)]
		public bool Playing { get; set; }
		[Key(1)]
		public bool Looping { get; set; }
		[Key(2)]
		public float Speed { get; set; }
		[Key(3)]
		public double Offset { get; set; }
		[Key(4)]
		public double Position { get; set; }

		public static TypeCode GetTypeCode() {
			return TypeCode.Object;
		}
	}
}