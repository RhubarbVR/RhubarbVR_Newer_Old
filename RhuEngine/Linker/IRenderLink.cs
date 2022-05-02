using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects.ECS;
namespace RhuEngine.Linker
{
	public interface IRenderLink
	{
		public RenderingComponent RenderingComponentGen { get; set; }
		public void Started();
		
		public void Stopped();
		
		public void Render();

		public void Remove();

		public void Init();

	}

	public interface IRenderLink<T>:IRenderLink where T : RenderingComponent,new()
	{
		T RenderingComponent { get; set; }
	}

	public abstract class RenderLinkBase<T> : IRenderLink<T> where T : RenderingComponent, new()
	{
		public T RenderingComponent { get; set; }
		public RenderingComponent RenderingComponentGen { get => RenderingComponent; set => RenderingComponent = (T)value; }

		public abstract void Init();

		public abstract void Remove();

		public abstract void Render();

		public abstract void Started();

		public abstract void Stopped();
	}
}
