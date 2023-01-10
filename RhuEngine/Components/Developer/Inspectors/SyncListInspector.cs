using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	public class SyncListInspector<T> : BaseInspector<T> where T : class, ISyncList
	{

		public bool IsCompList => typeof(T) == typeof(SyncAbstractObjList<IComponent>);

		private T _lastValue;

		public bool IsAbstract => typeof(T).IsAssignableTo(typeof(IAbstractObjList));

		public override void LocalBind() {
			base.LocalBind();
			if (_lastValue is not null) {
				_lastValue.OnReorderList -= LastValue_OnReorderList;
			}
			_lastValue = TargetObject.Target;
			if (_lastValue is not null) {
				_lastValue.OnReorderList += LastValue_OnReorderList;
				LastValue_OnReorderList();
			}
		}

		private void LastValue_OnReorderList() {
			if (LocalUser != MasterUser) {
				return;
			}
			var startingInspectors = Entity.children.Cast<Entity>().SelectMany(x => x.components.Where(x => x is IInspector).Select(x => x as IInspector)).ToList();
			IInspector AnyHas(ISyncObject syncObject) {
				foreach (var item in startingInspectors) {
					if (item.TargetObjectWorld == syncObject) {
						return item;
					}
				}
				return null;
			}
			var currentIndex = 0;
			foreach (var item in _lastValue.Reverse()) {
				var targetIn = AnyHas(item);
				if (targetIn is not null) {
					targetIn.Entity.orderOffset.Value = currentIndex;
					startingInspectors.Remove(targetIn);
				}
				else {
					if(IsCompList) {
						var dat = Entity.AddChild(item.Name).AttachComponent<CompoentInspector>();
						dat.Entity.orderOffset.Value = currentIndex;
						dat.TargetObject.Target = (IComponent)item;
					}
					else {
						var dat = Entity.AddChild(item.Name).AttachComponent<ListElementInspector>();
						dat.Entity.orderOffset.Value = currentIndex;
						dat.TargetObject.Target = item;
					}
				}
				currentIndex++;
			}
			foreach (var item in startingInspectors) {
				item.Entity.Destroy();
			}
		}

		[Exposed]
		public void Add() {
			if (!IsAbstract) {
				if (TargetObject.Target is ISyncObjectList list) {
					list.AddElementVoid();
				}
				return;
			}
		}

		protected override void BuildUI() {
			var mainBox = Entity.AttachComponent<BoxContainer>();
			mainBox.Vertical.Value = true;
			mainBox.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			mainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			if (!IsCompList) {
				var buttonAdd = Entity.AddChild("Add").AttachComponent<Button>();
				buttonAdd.Alignment.Value = RButtonAlignment.Center;
				buttonAdd.Pressed.Target = Add;
				buttonAdd.Text.Value = "Add";
				buttonAdd.Entity.orderOffset.Value = -1;
			}
		}
	}
}