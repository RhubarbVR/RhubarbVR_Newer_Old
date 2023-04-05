using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayEight<T> : ConstArraySeven<T> where T : SyncObject
	{
		public readonly T Eight;

		public override int Length => 8;

		public override T this[int index]
		{
			get {
				if(index == 7) {
					return Eight;
				}
				return base[index];
			}
		}
	}
}
