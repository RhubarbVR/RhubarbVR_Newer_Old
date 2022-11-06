using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface IRRenderer
	{
		public float Fov { get; set; }

		public float MinClip { get; set; }

		public float FarClip { get; set; }

		public bool PassthroughSupport { get; }
		public bool PassthroughMode { get; set; }

		public bool GetEnableSky();
		public void SetEnableSky(bool e);

		public Matrix GetCameraRootMatrix();

		public void SetCameraRootMatrix(Matrix m);
	}

	public static class RRenderer
	{
		public static IRRenderer Instance { get; set; }
		public static bool EnableSky
		{
			get => Instance.GetEnableSky();
			set => Instance.SetEnableSky(value);
		}
		public static Matrix LocalCam { get; set; } = Matrix.Identity;

		public static Matrix GetMainViewMatrix => LocalCam * CameraRoot;

		public static Matrix CameraRoot
		{
			get => Instance.GetCameraRootMatrix();
			set => Instance.SetCameraRootMatrix(value);
		}

		public static float MinClip
		{
			get => Instance.MinClip;
			set => Instance.MinClip = value;
		}
		public static float FarClip
		{
			get => Instance.FarClip;
			set => Instance.FarClip = value;
		}
		public static bool PassthroughMode
		{
			get => Instance.PassthroughMode;
			set => Instance.PassthroughMode = value;
		}

		public static bool PassthroughSupport => Instance.PassthroughSupport;

		public static float Fov
		{
			get => Instance.Fov;
			set => Instance.Fov = value;
		}
	}
}
