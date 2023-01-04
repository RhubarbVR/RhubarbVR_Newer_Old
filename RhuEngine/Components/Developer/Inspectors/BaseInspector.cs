using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RhuEngine.Components
{
	public interface IInspector : IComponent
	{
		public IWorldObject TargetObjectWorld { get; set; }
	}

	[Category(new string[] { "Developer/Inspectors" })]
	public abstract class BaseInspector<T> : Component, IInspector where T : class, IWorldObject
	{
		[OnChanged(nameof(TargetObjectRebuild))]
		public readonly SyncRef<T> TargetObject;

		public IWorldObject TargetObjectWorld { get => TargetObject.TargetIWorldObject; set => TargetObject.TargetIWorldObject = value; }

		public static Type GetFiled(Type type) {
			if (type.IsAssignableTo(typeof(ISync))) {
				if (type.IsGenericType) {
					return typeof(PrimitiveEditorBuilder<>).MakeGenericType(type.GetGenericArguments()[0]);
				}
				return typeof(PrimitiveEditor);
			}
			return typeof(WorldObjectInspector);
		}

		protected void WorldObjectUIBuild() {
			var vert = Entity.AttachComponent<BoxContainer>();
			vert.Vertical.Value = true;
			vert.HorizontalFilling.Value = RFilling.Expand | RFilling.Expand;
			if (TargetObject.Target is null) {
				return;
			}
			var members = TargetObject.Target.GetType().GetMembers().OrderBy(x=>x is MethodInfo);
			foreach (var member in members) {
				if (member.GetCustomAttribute<NoShowAttribute>() != null) {
					continue;
				}
				if (member is MethodInfo method) {
					if (member.GetCustomAttribute<ExposedAttribute>() == null) {
						continue;
					}
					try {
						var newField = Entity.AddChild(method.Name);
						var newType = Helper.CreateDelegateType(method);
						newField.AttachComponent<IMethodInspector>(typeof(MethodInspector<>).MakeGenericType(newType)).InitField(method, TargetObject.Target);
					}
					catch {

					}
					//Show method target
				}
				if (member is FieldInfo field) {
					if (field.FieldType.IsAssignableTo(typeof(IWorldObject))) {
						var newField = Entity.AddChild(field.Name);
						var e = typeof(FieldInspector<>).MakeGenericType(GetFiled(field.FieldType));
						newField.AttachComponent<IFiledInit>(e).InitField(member, (IWorldObject)field.GetValue(TargetObject.Target));
					}
				}
			}
		}

		protected abstract void BuildUI();

		public virtual void CleanUpUI() {
			Entity.DestroyChildren();
		}

		public virtual void LocalBind() {

		}

		public void TargetObjectRebuild() {
			LocalBind();
			if (LocalUser != MasterUser) {
				return;
			}

			CleanUpUI();
			if (TargetObject.Target is null) { return; }
			BuildUI();
		}
	}
}