using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.WorldObjects
{
	public partial class ConstArrayThirteen<T> : ConstArrayTwelve<T> where T : SyncObject, new()
	{
		public readonly T Thirteen;

		public override int Length => 13;
		
		public override T this[int index]
		{
			get {
				if(index == 12) {
					return Thirteen;
				}
				return base[index];
			}
		}
	}
}
