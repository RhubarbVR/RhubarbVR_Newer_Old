using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayEleven<T> : ConstArrayTen<T> where T : SyncObject, new()
	{
		public readonly T Eleven;

		public override int Length => 11;

		public override T this[int index]
		{
			get {
				if(index == 10) {
					return Eleven;
				}
				return base[index];
			}
		}
	}
}
