using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;
namespace RhuEngine.Linker
{
	public interface IRStick
	{
		public Vector2f YX { get; }
	}

	public interface IRController
	{
		public float Trigger { get; }
		public float Grip { get; }
		public IKeyPress StickClick { get; }
		public IKeyPress X1 { get; }
		public IKeyPress X2 { get; }
		public IRStick Stick { get; }
	}
	public interface IRMouse
	{
		public Vector2f ScrollChange { get; }
		public Vector2f PosChange { get; }
	}

	public interface IRHead
	{
		public Vector3f Position { get; }

		public Matrix HeadMatrix { get; }
	}

	public interface IRHand
	{
		public Matrix Wrist { get; }
	}


	public interface IKeyPress
	{
		public bool IsActive();

		public bool IsJustActive();
	}
	public interface IRInput
	{
		public string TypeDelta { get; }
		public IRHead Head { get; }


		public IRMouse Mouse { get; }

		public IKeyPress Key(Key secondKey);

		public IRHand Hand(Handed value);

		public IRController Controller(Handed handed);
	}

	public static class RInput
	{
		public static IRInput Instance { get; set; }

		public static IRHead Head => Instance?.Head;


		public static IRMouse Mouse => Instance?.Mouse;

		public static string InjectedTypeDelta = "";

		public static string TypeDelta => Instance.TypeDelta + InjectedTypeDelta;

		public static IKeyPress Key(Key secondKey) {
			return Instance?.Key(secondKey);
		}

		public static IRHand Hand(Handed value) {
			return Instance?.Hand(value);
		}

		public static IRController Controller(Handed handed) {
			return Instance?.Controller(handed);
		}
	}
}
