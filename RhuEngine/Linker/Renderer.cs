using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface IRRenderer
	{
		public float MinClip { get; set; }

		public float FarClip { get; set; }

		public bool GetEnableSky();
		public void SetEnableSky(bool e);

		public Matrix GetCameraRootMatrix();

		public Matrix GetCameraLocalMatrix();
		public void SetCameraLocalMatrix(Matrix m);

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
		public static Matrix LocalCam
		{
			get => Instance.GetCameraLocalMatrix();
			set => Instance.SetCameraLocalMatrix(value);
		}
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
	}
}
