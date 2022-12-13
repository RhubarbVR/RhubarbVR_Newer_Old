using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers" })]
	public sealed class ObserverListBase<T> : ObserverBase<SyncListBase<T>> where T : class, ISyncObject
	{
		public readonly SyncObjList<SyncRef<BoxContainer>> Elements;

		protected override async Task LoadObservedUI(UIBuilder2D ui) {
			var isSyncObjectList = TargetElement?.GetType().IsAssignableTo(typeof(ISyncObjectList)) ?? false;
			for (var i = 0; i < Elements.Count; i++) {
				Elements[i].Target?.Destroy();
			}
			Elements?.Clear();
			foreach (var item in TargetElement) {
				var element = ui.Entity.AddChild(item.Name);
				var boxXo = element.AttachComponent<BoxContainer>();
				Elements.Add().Target = boxXo;
				boxXo.Vertical.Value = true;
				boxXo.InputFilter.Value = RInputFilter.Pass;
				var newOBserver = element.AttachComponent<IObserver>(item.GetObserver());
				await newOBserver.SetObserverd(item);
			}
		}

		protected override void LoadValueIn() {

		}
	}
}