using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuScript
{
	public interface IScriptPartData<T> : IScriptPartDataSet<T> , IScriptPartDataGet<T> {

	}

	public interface IScriptPartDataSet<T> : IScriptPartDataSet
	{
		public T Value { set; }
	}


	public interface IScriptPartDataSet : IScriptPart
	{
		public object Data { set; }
	}

	public interface IScriptPartDataGet<T> : IScriptPartDataGet
	{
		public T Value { get; }
	}


	public interface IScriptPartDataGet : IScriptPart
	{
		public object Data { get; }

		public void VoidGetData();
	}
}
