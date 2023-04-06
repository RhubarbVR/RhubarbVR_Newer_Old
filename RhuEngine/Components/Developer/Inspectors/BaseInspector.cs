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
	public abstract partial class BaseInspector<T> : Component, IInspector where T : class, IWorldObject
	{
		[OnChanged(nameof(TargetObjectRebuild))]
		public readonly SyncRef<T> TargetObject;

		protected virtual bool MultiThreaded => true;

		public IWorldObject TargetObjectWorld { get => TargetObject.TargetIWorldObject; set => TargetObject.TargetIWorldObject = value; }

		public static Type GetFiled(Type type, object targetObject = null) {
			if (type.IsAssignableTo(typeof(ISyncList))) {
				return typeof(SyncListInspector<>).MakeGenericType(type);
			}
			if (type.IsAssignableTo(typeof(ISync))) {
				if (type.IsGenericType) {
					return typeof(PrimitiveEditorBuilder<>).MakeGenericType(type.GetGenericArguments()[0]);
				}
				return typeof(PrimitiveEditor);
			}
			if (type.IsAssignableTo(typeof(ISyncRef))) {
				if (targetObject is ISyncRef @ref) {
					return typeof(SyncRefInspector<,>).MakeGenericType(type, @ref?.GetRefType ?? typeof(IWorldObject));
				}
				else {
					return typeof(SyncRefInspector<,>).MakeGenericType(type, typeof(IWorldObject));
				}
			}
			return typeof(WorldObjectInspector);
		}

		protected void WorldObjectUIBuild() {
			var vert = Entity.GetFirstComponentOrAttach<BoxContainer>();
			vert.Vertical.Value = true;
			vert.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			if (TargetObject.Target is null) {
				return;
			}

			var members = TargetObject.Target.GetType().FastGetMembers().OrderBy(x => x is MethodInfo);
			var amountOfMethods = members.Where(x => {
				if (x.GetCustomAttribute<NoShowAttribute>() != null) {
					return false;
				}
				if (x.GetCustomAttribute<ExposedAttribute>() == null) {
					return false;
				}
				if (x is MethodInfo methodInfo) {
					try {
						var newType = Helper.CreateDelegateType(methodInfo);
						var newTypee = typeof(MethodInspector<>).MakeGenericType(newType);
						return true;
					}
					catch { }
				}
				return false;
			}).Count();
			DropDown delegateMemberDropDown = null;
			foreach (var member in members) {
				if (member.GetCustomAttribute<NoShowAttribute>() != null) {
					continue;
				}
				if (member is MethodInfo method) {
					if (member.GetCustomAttribute<ExposedAttribute>() == null) {
						continue;
					}
					try {
						var newType = Helper.CreateDelegateType(method);
						var newTypee = typeof(MethodInspector<>).MakeGenericType(newType);
						if (amountOfMethods == 1) {
							var newField = Entity.AddChild(method.Name);
							newField.AttachComponent<IMethodInspector>(newTypee).InitField(method, TargetObject.Target);
						}
						else {
							if (delegateMemberDropDown is null) {
								delegateMemberDropDown = Entity.AddChild("DropDown").AttachComponent<DropDown>();
								delegateMemberDropDown.DropDownButton.Target.Text.Value = "Methods";
								var newBox = delegateMemberDropDown.DropDownData.Target.AttachComponent<BoxContainer>();
								newBox.Vertical.Value = true;
								newBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
							}

							var newField = delegateMemberDropDown.DropDownData.Target.AddChild(method.Name);
							newField.AttachComponent<IMethodInspector>(newTypee).InitField(method, TargetObject.Target);
						}
					}
					catch { }
				}
				if (member is FieldInfo field) {
					if (field.FieldType.IsAssignableTo(typeof(IWorldObject))) {
						var newField = Entity.AddChild(field.Name);
						var e = typeof(FieldInspector<>).MakeGenericType(GetFiled(field.FieldType, field.GetValue(TargetObject.Target)));
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

		public override void Dispose() {
			IsDestroying = true;
			CleanUpUI();
			LocalBind();
			base.Dispose();
		}

		private void UIUpdate() {
			try {
				CleanUpUI();
				if (TargetObject.Target is null) { return; }
				BuildUI();
				LocalBind();
			}
			catch(Exception e) {
				RLog.Err($"Error Update UI on Inspector {GetType().GetFormattedName()}  World {World.WorldDebugName} Error:{e}");
			}
		}
		public void TargetObjectRebuild() {
			if (LocalUser != MasterUser) {
				LocalBind();
				return;
			}
			if (Task.CurrentId is null & MultiThreaded) {
				Task.Run(UIUpdate);
			}
			else {
				UIUpdate();
			}
		}
	}
}