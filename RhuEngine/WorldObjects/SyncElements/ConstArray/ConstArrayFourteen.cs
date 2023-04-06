using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayFourteen<T> : ConstArrayThirteen<T> where T : SyncObject, new()
	{
		public readonly T Fourteen;

		public override int Length => 14;

		public override T this[int index]
		{
			get {
				if(index == 13) {
					return Fourteen;
				}
				return base[index];
			}
		}
	}
}
