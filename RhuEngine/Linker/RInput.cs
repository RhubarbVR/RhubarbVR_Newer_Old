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
	[Flags]
	public enum KnownControllers: uint
	{
		Unknown,
		Vive = 1,
		Index = 2,
		Touch = 4,
		Cosmos = 8,
		HPReverb = 16,
		WindowsMR = 32,
		Etee = 64,
		Khronos = 128,
		MicrosoftHand = 256,
		GenericXR = 512,
	}
	public interface IRController
	{
		public string Model { get; }

		public KnownControllers ModelEnum { get; }

		public float BatteryPercentage { get; }

		public float Trigger { get; }
		public float Grip { get; }
		public IKeyPress StickClick { get; }
		public IKeyPress X1 { get; }
		public IKeyPress X2 { get; }
		public IRStick Stick { get; }
	}
	public interface IRMouse
	{
		public bool HideMouse { get; set; }
		public bool CenterMouse { get; set; }
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

		public class ScreenHead : IRHead
		{

			public Vector3f Position => HeadMatrix.Translation;

			public Matrix HeadMatrix { get; set;}
		}

		public static ScreenHead screenhd = new();

		public static IRHead Head => RWorld.IsInVR? (Instance?.Head): screenhd;


		public static IRMouse Mouse => Instance?.Mouse;

		public static string InjectedTypeDelta = "";

		public static string TypeDelta => Instance.TypeDelta + InjectedTypeDelta;

		public class InjectKey : IKeyPress
		{
			public InjectKey(bool isActive, bool isJustActive) {
				IsActive1 = isActive;
				IsJustActive1 = isJustActive;
			}

			public bool IsActive1 { get; }
			public bool IsJustActive1 { get; }

			public bool IsActive() {
				return IsActive1;
			}

			public bool IsJustActive() {
				return IsJustActive1;
			}
		}

		public static Dictionary<Key, InjectKey> ingectedkeys = new();

		public static IKeyPress Key(Key secondKey) {
			return ingectedkeys.ContainsKey(secondKey) ? ingectedkeys[secondKey] : (Instance?.Key(secondKey));
		}

		public static IRHand Hand(Handed value) {
			return Instance?.Hand(value);
		}

		public static IRController Controller(Handed handed) {
			return Instance?.Controller(handed);
		}
	}
}
