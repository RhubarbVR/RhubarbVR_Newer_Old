using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
using RhuEngine.Components;
using static Godot.Control;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class BoxContainerBase<T,T2> : ContainerBase<T,T2> where T : RhuEngine.Components.BoxContainer, new() where T2 : Godot.BoxContainer, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.Alignment.Changed += Alignment_Changed;
			LinkedComp.Vertical.Changed += Vertical_Changed;
			Alignment_Changed(null);
			Vertical_Changed(null);
		}

		private void Vertical_Changed(IChangeable obj) {
			node.Vertical = LinkedComp.Vertical.Value;
		}

		private void Alignment_Changed(IChangeable obj) {
			node.Alignment = LinkedComp.Alignment.Value switch {
				RBoxContainerAlignment.Begin => Godot.BoxContainer.AlignmentMode.Begin,
				RBoxContainerAlignment.End => Godot.BoxContainer.AlignmentMode.End,
				_ => Godot.BoxContainer.AlignmentMode.Center,
			};
		}
	}

	public sealed class BoxContainerLink : BoxContainerBase<RhuEngine.Components.BoxContainer, Godot.BoxContainer>
	{
		public override string ObjectName => "BoxContainer";

		public override void StartContinueInit() {

		}
	}
}
