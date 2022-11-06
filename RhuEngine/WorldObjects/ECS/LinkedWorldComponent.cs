using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace RhuEngine.WorldObjects.ECS
{
	public class NotLinkedRenderingComponentAttribute : Attribute
	{

	}

	public abstract class LinkedWorldComponent : Component
	{
		protected virtual bool AddToUpdateList => true;
		public IWorldLink WorldLink { get; set; }

		public static Dictionary<Type, Type> loadedCasts = new();

		private void BuildRenderLink() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (GetType().GetCustomAttribute<NotLinkedRenderingComponentAttribute>() is not null) {
					return;
				}
				if (!loadedCasts.TryGetValue(GetType(), out var linker)) {
					var generic = typeof(IWorldLink<>).MakeGenericType(GetType());
					var types = from a in AppDomain.CurrentDomain.GetAssemblies()
								from t in a.GetTypes()
								where !t.IsAbstract && t.IsClass
								where generic.IsAssignableFrom(t)
								select t;
					if (types.Count() != 1) {
						RLog.Err($"No linker found or to many found Amount:{types.Count()} Type {GetType().GetFormattedName()}");
						throw new Exception("No linker found or to many found");
					}
					linker = types.First();
					loadedCasts.Add(GetType(), linker);
				}
				WorldLink = (IWorldLink)Activator.CreateInstance(linker);
				WorldLink.LinkCompGen = this;
				WorldLink.Init();
				World_FoucusChanged();
			});
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			BuildRenderLink();
			World.FoucusChanged += World_FoucusChanged;
			Enabled.Changed += Enabled_Changed;
		}

		private void Enabled_Changed(IChangeable obj) {
			World_FoucusChanged();
		}

		private void World_FoucusChanged() {
			if (World.Focus == World.FocusLevel.Background) {
				WorldLink?.Stopped();
			}
			else {
				if (Entity.IsEnabled && Enabled.Value) {
					WorldLink?.Started();
				}
				else {
					WorldLink?.Stopped();
				}
			}
		}

		protected override void AddListObject() {
			if (AddToUpdateList) {
				World.RegisterWorldLinkObject(this);
			}
			World_FoucusChanged();
		}
		protected override void RemoveListObject() {
			if (AddToUpdateList) {
				World.UnregisterWorldLinkObject(this);
			}
			WorldLink?.Stopped();
		}

		public override void Dispose() {
			World.FoucusChanged -= World_FoucusChanged;
			if (AddToUpdateList) {
				World.UnregisterWorldLinkObject(this);
			}
			WorldLink?.Remove();
			base.Dispose();
		}

		internal void RunRender() {
			Render();
		}

		protected virtual void Render() {
			WorldLink?.Render();
		}
	}
}
