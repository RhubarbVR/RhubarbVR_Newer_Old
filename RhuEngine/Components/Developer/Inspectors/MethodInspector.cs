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
	public interface IMethodInspector : IComponent
	{
		public void InitField(MethodInfo methodInfo, IWorldObject holder);
	}

	[Category(new string[] { "Developer/Inspectors" })]
	public sealed partial class MethodInspector<T> : Component, IMethodInspector where T : Delegate
	{
		public readonly SyncDelegate<T> TargetMethod;

		public void InitField(MethodInfo methodInfo, IWorldObject holder) {
			var box = Entity.AttachComponent<BoxContainer>();
			box.Vertical.Value = false;
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			var text = Entity.AddChild("FiledName").AttachComponent<Button>();
			text.ButtonMask.Value = RButtonMask.Secondary;
			text.FocusMode.Value = RFocusMode.None;
			text.TextOverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;
			text.MinSize.Value = new Vector2i(18, 18);
			text.Text.Value = methodInfo.ReflectedType.GetFormattedName() + " ";

			text.Text.Value += methodInfo.Name;
			if (methodInfo.IsGenericMethod) {
				text.Text.Value += "<";
				var args = methodInfo.GetGenericMethodDefinition().GetGenericArguments();
				for (var i = 0; i < args.Length; i++) {
					text.Text.Value += args[i].GetFormattedName() + ",";
				}
				if (args.Length != 0) {
					text.Text.Value = text.Text.Value.Remove(text.Text.Value.Length - 1);
				}
				text.Text.Value += ">";
			}
			text.Text.Value += "(";
			var prams = methodInfo.GetParameters();
			for (var i = 0; i < prams.Length; i++) {
				text.Text.Value += prams[i].ParameterType.GetFormattedName() + " ";
				text.Text.Value += prams[i].Name + " ";
				if (prams[i].HasDefaultValue) {
					var def = prams[i].DefaultValue;
					var injectString = "null";
					if (def != null) {
						injectString = "\"" + def?.ToString() + "\"";
					}
					text.Text.Value += " = " + injectString + " ";
				}
				text.Text.Value += ",";
			}
			if (prams.Length != 0) {
				text.Text.Value = text.Text.Value.Remove(text.Text.Value.Length - 1);
			}
			text.Text.Value += ");";
			if (!methodInfo.IsGenericMethod) {
				try {
					TargetMethod.Target = methodInfo.CreateDelegate<T>(holder);
				}
				catch { }
			}
			text.Alignment.Value = RButtonAlignment.Center;
			text.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
		}
	}
}