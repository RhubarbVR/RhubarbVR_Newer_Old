using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayNine<T> : ConstArrayEight<T> where T : SyncObject, new()
	{
		public readonly T Nine;

		public override int Length => 9;

		public override T this[int index]
		{
			get {
				if(index == 8) {
					return Nine;
				}
				return base[index];
			}
		}
	}
}
