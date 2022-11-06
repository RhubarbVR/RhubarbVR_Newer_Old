using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RhuEngine.WorldObjects.ECS
{
	public interface IComponent : ISyncObject
	{
		Entity Entity { get; }
		int Offset { get; }

		void RunAttach();
	}

	public abstract class Component : SyncObject, IComponent, IOffsetableElement
	{
		[NoSave]
		[NoShow]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity Entity { get; private set; }

		protected override void OnInitialize() {
			base.OnInitialize();
			Entity = (Entity)Parent.Parent;
			var allowedOnWorldRoot = GetType().GetCustomAttribute<AllowedOnWorldRootAttribute>(true);
			if(allowedOnWorldRoot is null && Entity.IsRoot) {
				Destroy();
				throw new Exception("Not Allowed on WorldRoot");
			}
			var locke = GetType().GetHighestAttributeInherit<SingleComponentLockAttribute>();
			if(locke is not null) {
				if(Entity.components.Where(x => x.GetType() == locke).Count() >= 2) {
					Destroy();
					throw new Exception("Single Component Locked");
				}
			}

			if (Entity.IsEnabled) {
				AddListObject();
			}
			else {
				RemoveListObject();
			}
		}

		[Default(true)]
		public readonly Sync<bool> Enabled;

		[OnChanged(nameof(OnOrderOffsetChanged))]
		public readonly Sync<int> OrderOffset;

		protected void OnOrderOffsetChanged() {
			OffsetChanged?.Invoke();
		}

		public int Offset
		{
			get {
				var attras = GetType().GetCustomAttributes(typeof(UpdateLevelAttribute), true);
				return attras.Length == 0 ? OrderOffset.Value : ((UpdateLevelAttribute)attras[0]).offset + OrderOffset.Value;
			}
		}

		internal void ListObjectUpdate(bool add) {
			if (add && !IsDestroying) {
				AddListObject();
			}
			else {
				RemoveListObject();
			}
		}

		protected virtual void AddListObject() {

		}

		protected virtual void RemoveListObject() {

		}

		public void RunAttach() {
			OnAttach();
		}

		protected virtual void OnAttach() {

		}

		internal void RunRenderStep(bool isEnabled) {
			AlwaysRenderStep();
			if (isEnabled) {
				RenderStep();
			}
		}
		internal void RunStep(bool isEnabled) {
			AlwaysStep();
			if (isEnabled) {
				Step();
			}
		}

		protected virtual void RenderStep() {

		}
		protected virtual void AlwaysRenderStep() {

		}

		protected virtual void Step() {

		}
		protected virtual void AlwaysStep() {

		}

		public event Action OffsetChanged;

		public override void Dispose() {
			base.Dispose();
			try {
				RemoveListObject();
			}
			catch { }
		}
	}
}
