using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects.ECS;
namespace RhuEngine.Linker
{
	public interface IWorldLink
	{
		public LinkedWorldComponent LinkCompGen { get; set; }
		public void Started();

		public void Stopped();

		public void Render();

		public void Remove();

		public void Init();

	}

	public interface IWorldLink<T> : IWorldLink where T : LinkedWorldComponent, new()
	{
		T LinkedComp { get; set; }
	}

	public abstract class EngineWorldLinkBase<T> : IWorldLink<T> where T : LinkedWorldComponent, new()
	{
		public T LinkedComp { get; set; }
		public LinkedWorldComponent LinkCompGen { get => LinkedComp; set => LinkedComp = (T)value; }

		public abstract void Init();

		public abstract void Remove();

		public abstract void Render();

		public abstract void Started();

		public abstract void Stopped();
	}
}
