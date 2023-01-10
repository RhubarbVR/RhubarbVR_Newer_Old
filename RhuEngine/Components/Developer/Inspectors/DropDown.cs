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


	[Category(new string[] { "Developer/Inspectors" })]
	public sealed class DropDown : Component
	{
		public readonly SyncRef<Entity> DropDownData;

		public readonly SyncRef<Entity> DropDownHeader;
		public readonly SyncRef<Button> DropDownButton;

		protected override void OnAttach() {
			base.OnAttach();
			var box = Entity.AttachComponent<BoxContainer>();
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			box.Vertical.Value = true;
			var top = Entity.AddChild().AttachComponent<BoxContainer>();
			DropDownHeader.Target = top.Entity;
			top.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			var but = DropDownButton.Target = top.Entity.AddChild().AttachComponent<Button>();
			but.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			but.ToggleMode.Value = true;
			but.Alignment.Value = RButtonAlignment.Center;
			DropDownData.Target = Entity.AddChild();

			var copyEr = Entity.AttachComponent<ValueCopy<bool>>();
			copyEr.Target.Target = DropDownData.Target?.enabled;
			copyEr.Source.Target = but.ButtonPressed;
		}
	}
}