using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArraySixteen<T> : ConstArrayFifteen<T> where T : SyncObject, new()
	{
		public readonly T Sixteen;

		public override int Length => 16;

		public override T this[int index]
		{
			get {
				if(index == 15) {
					return Sixteen;
				}
				return base[index];
			}
		}
	}
}
