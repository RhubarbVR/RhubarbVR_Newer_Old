using System;

namespace RhuEngine.WorldObjects.ECS
{
	public abstract class Component : SyncObject, IOffsetableElement
	{
		[NoSave]
		[NoShow]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity Entity { get; private set; }

		public override void OnInitialize() {
			base.OnInitialize();
			Entity = (Entity)Parent.Parent;
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

		public void OnOrderOffsetChanged() {
			OffsetChanged?.Invoke();
		}

		public int Offset
		{
			get {
				var attras = GetType().GetCustomAttributes(typeof(UpdateLevelAttribute), true);
				return attras.Length == 0 ? OrderOffset.Value : ((UpdateLevelAttribute)attras[0]).offset + OrderOffset.Value;
			}
		}

		public virtual void AddListObject() {

		}

		public virtual void RemoveListObject() {

		}

		public virtual void OnAttach() {

		}

		public virtual void Step() {

		}
		public virtual void AlwaysStep() {

		}

		public event Action OffsetChanged;

		public void AddWorldCoroutine(Action action) {
			World.AddCoroutine(action);
		}
	}
}
