using System;
using System.Collections.Generic;
using System.Text;
using RNumerics;

namespace RhuEngine.Linker
{
	public interface IRRenderer
	{
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
		public static Matrix CameraRoot
		{
			get => Instance.GetCameraRootMatrix();
			set => Instance.SetCameraRootMatrix(value);
		}
	}
}
