using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace RhuEngine.WorldObjects.ECS
{
	public class NotLinkedRenderingComponentAttribute : Attribute {

	}

	public abstract class RenderingComponent : Component
	{
		public IRenderLink RenderLink { get; set; }

		public static Dictionary<Type, Type> loadedCasts = new();

		private void BuildRenderLink() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if(GetType().GetCustomAttribute<NotLinkedRenderingComponentAttribute>() is not null) {
				return;
			}
			if (!loadedCasts.TryGetValue(GetType(), out var linker)) {
				var generic = typeof(IRenderLink<>).MakeGenericType(GetType());
				var types = from a in AppDomain.CurrentDomain.GetAssemblies()
							from t in a.GetTypes()
							where !t.IsAbstract && t.IsClass
							where generic.IsAssignableFrom(t)
							select t;
				if (types.Count() != 1) {
					RLog.Err("No linker found or to many found");
					throw new Exception("No linker found or to many found");
				}
				linker = types.First();
				loadedCasts.Add(GetType(), linker);
			}
			RenderLink = (IRenderLink)Activator.CreateInstance(linker);
			RenderLink.RenderingComponentGen = this;
			RenderLink.Init();
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			BuildRenderLink();
			World.FoucusChanged += World_FoucusChanged;
		}

		private void World_FoucusChanged() {
			if(World.Focus == World.FocusLevel.Background) {
				RenderLink?.Stopped();
			}
			else {
				RenderLink?.Started();
			}
		}

		protected override void AddListObject() {
			World.RegisterRenderObject(this);
			if (World.Focus == World.FocusLevel.Background) {
				RenderLink?.Stopped();
			}
			else {
				RenderLink?.Started();
			}
		}
		protected override void RemoveListObject() {
			World.UnregisterRenderObject(this);
			RenderLink?.Stopped();
		}

		public override void Dispose() {
			World.FoucusChanged -= World_FoucusChanged;
			World.UnregisterRenderObject(this);
			base.Dispose();
		}

		internal void RunRender() {
			Render();
		}

		protected virtual void Render() {
			RenderLink?.Render();
		}
	}
}
