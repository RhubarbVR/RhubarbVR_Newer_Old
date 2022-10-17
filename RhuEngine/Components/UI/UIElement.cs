using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	public enum RLayoutDir
	{
		Inherited,
		Locale,
		Left_to_Right,
		Right_to_Left,
	}
	public enum RGrowHorizontal
	{
		Left,
		Right,
		Both,
	}
	public enum RGrowVertical
	{
		Top,
		Bottom,
		Both,
	}
	[Flags]
	public enum RFilling
	{
		None = 0,
		Fill = 1,
		ShrinkCenter = 2,
		ShrinkEnd = 4,
		Expand = 8,
	}

	public enum RInputFilter
	{
		Stop,
		Pass,
		Ignore
	}

	public enum RCursorShape : long
	{
		//
		// Summary:
		//     Show the system's arrow mouse cursor when the user hovers the node. Use with
		//     Godot.Control.MouseDefaultCursorShape.
		Arrow,
		//
		// Summary:
		//     Show the system's I-beam mouse cursor when the user hovers the node. The I-beam
		//     pointer has a shape similar to "I". It tells the user they can highlight or insert
		//     text.
		Ibeam,
		//
		// Summary:
		//     Show the system's pointing hand mouse cursor when the user hovers the node.
		PointingHand,
		//
		// Summary:
		//     Show the system's cross mouse cursor when the user hovers the node.
		Cross,
		//
		// Summary:
		//     Show the system's wait mouse cursor when the user hovers the node. Often an hourglass.
		Wait,
		//
		// Summary:
		//     Show the system's busy mouse cursor when the user hovers the node. Often an arrow
		//     with a small hourglass.
		Busy,
		//
		// Summary:
		//     Show the system's drag mouse cursor, often a closed fist or a cross symbol, when
		//     the user hovers the node. It tells the user they're currently dragging an item,
		//     like a node in the Scene dock.
		Drag,
		//
		// Summary:
		//     Show the system's drop mouse cursor when the user hovers the node. It can be
		//     an open hand. It tells the user they can drop an item they're currently grabbing,
		//     like a node in the Scene dock.
		CanDrop,
		//
		// Summary:
		//     Show the system's forbidden mouse cursor when the user hovers the node. Often
		//     a crossed circle.
		Forbidden,
		//
		// Summary:
		//     Show the system's vertical resize mouse cursor when the user hovers the node.
		//     A double-headed vertical arrow. It tells the user they can resize the window
		//     or the panel vertically.
		Vsize,
		//
		// Summary:
		//     Show the system's horizontal resize mouse cursor when the user hovers the node.
		//     A double-headed horizontal arrow. It tells the user they can resize the window
		//     or the panel horizontally.
		Hsize,
		//
		// Summary:
		//     Show the system's window resize mouse cursor when the user hovers the node. The
		//     cursor is a double-headed arrow that goes from the bottom left to the top right.
		//     It tells the user they can resize the window or the panel both horizontally and
		//     vertically.
		Bdiagsize,
		//
		// Summary:
		//     Show the system's window resize mouse cursor when the user hovers the node. The
		//     cursor is a double-headed arrow that goes from the top left to the bottom right,
		//     the opposite of Godot.Control.CursorShape.Bdiagsize. It tells the user they can
		//     resize the window or the panel both horizontally and vertically.
		Fdiagsize,
		//
		// Summary:
		//     Show the system's move mouse cursor when the user hovers the node. It shows 2
		//     double-headed arrows at a 90 degree angle. It tells the user they can move a
		//     UI element freely.
		Move,
		//
		// Summary:
		//     Show the system's vertical split mouse cursor when the user hovers the node.
		//     On Windows, it's the same as Godot.Control.CursorShape.Vsize.
		Vsplit,
		//
		// Summary:
		//     Show the system's horizontal split mouse cursor when the user hovers the node.
		//     On Windows, it's the same as Godot.Control.CursorShape.Hsize.
		Hsplit,
		//
		// Summary:
		//     Show the system's help mouse cursor when the user hovers the node, a question
		//     mark.
		Help
	}

	public enum RFocusMode
	{
		None,
		Click,
		All
	}

	public interface IKeyboardInteraction
	{
		public void KeyboardBind();
		public void KeyboardUnBind();
		public Matrix WorldPos { get; }

		public string EditString { get; }
	}

	[Category("UI")]
	public class UIElement : CanvasItem, IKeyboardInteraction
	{
		public Matrix WorldPos => Entity.GlobalTrans;

		public virtual string EditString => null;

		public Action KeyboardBindAction;
		public Action KeyboardUnBindAction;

		public void KeyboardBind() {
			KeyboardBindAction?.Invoke();
		}

		public void KeyboardUnBind() {
			KeyboardUnBindAction?.Invoke();
		}


		public readonly Sync<bool> ClipContents;
		public readonly Sync<Vector2i> MinSize;
		public readonly Sync<RLayoutDir> LayoutDir;


		public readonly Sync<Vector2f> Min;
		public readonly Sync<Vector2f> Max;
		public readonly Sync<Vector2f> MinOffset;
		public readonly Sync<Vector2f> MaxOffset;

		[Default(RGrowHorizontal.Both)]
		public readonly Sync<RGrowHorizontal> GrowHorizontal;
		[Default(RGrowVertical.Both)]
		public readonly Sync<RGrowVertical> GrowVertical;

		public readonly Sync<float> Rotation;
		public readonly Sync<Vector2f> Scale;
		public readonly Sync<Vector2f> PivotOffset;
		[Default(RFilling.Fill)]
		public readonly Sync<RFilling> HorizontalFilling;
		[Default(RFilling.Fill)]
		public readonly Sync<RFilling> VerticalFilling;
		[Default(1f)]
		public readonly Sync<float> StretchRatio;
		[Default(true)]
		public readonly Sync<bool> AutoTranslate;
		[Default(RFocusMode.None)]
		public readonly Sync<RFocusMode> FocusMode;

		[Default(RInputFilter.Stop)]
		public readonly Sync<RInputFilter> InputFilter;
		[Default(true)]
		public readonly Sync<bool> ForceScrollEventPassing;
		[Default(RCursorShape.Arrow)]
		public readonly Sync<RCursorShape> CursorShape;

		protected override void OnAttach() {
			base.OnAttach();
			Scale.Value = Vector2f.One;
			Max.Value = Vector2f.One;
			Min.Value = Vector2f.Zero;
		}

	}
}
