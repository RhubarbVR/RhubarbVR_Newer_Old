using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayTen<T> : ConstArrayNine<T> where T : SyncObject
	{
		public readonly T Ten;

		public override int Length => 10;

		public override T this[int index]
		{
			get {
				if(index == 9) {
					return Ten;
				}
				return base[index];
			}
		}
	}
}
