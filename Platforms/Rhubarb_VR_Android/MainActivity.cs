using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Views;
using Android.Content;
using StereoKit;
using System;
using Android.Graphics;
using Java.Lang;
using System.Threading.Tasks;
using RhuEngine;
using Android;
using Android.Support.V4.Content;
using Android.Support.V4.App;

namespace RhuEngine
{
	// NativeActivity seems to work fine, so here's a link to that code
	// https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/app/NativeActivity.java
	// Which also calls loadNativeCode_native over here:
	// https://android.googlesource.com/platform/frameworks/base.git/+/android-4.3_r3.1/core/jni/android_app_NativeActivity.cpp
	// Additional ref here:
	// https://github.com/spurious/SDL-mirror/blob/6fe5bd1536beb197de493c9b55d16e516219c58f/android-project/app/src/main/java/org/libsdl/app/SDLActivity.java#L1663
	// https://github.com/MonoGame/MonoGame/blob/31dca640482bc0c27aec8e51d6369612ce8577a2/MonoGame.Framework/Platform/Android/MonoGameAndroidGameView.cs
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
	public class MainActivity : AppCompatActivity, ISurfaceHolderCallback2
	{
		Engine _app;
		View _surface;

		protected override void OnCreate(Bundle savedInstanceState) {
			JavaSystem.LoadLibrary("openxr_loader");
			JavaSystem.LoadLibrary("StereoKitC");
			JavaSystem.LoadLibrary("opus");

			// Set up a surface for StereoKit to draw on
			Window.TakeSurface(this);
			Window.SetFormat(Format.Unknown);
			_surface = new View(this);
			SetContentView(_surface);
			_surface.RequestFocus();

			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			Run(Handle);
		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		static bool _running = false;
		void Run(IntPtr activityHandle) {
				if (_running) {
					return;
				}

				_running = true;
			var runningtask = Task.Run(() => {
				if ((ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != Android.Content.PM.Permission.Granted) || (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted) || (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted)) {
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.RecordAudio, Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage }, 1);
				}
				var cap = new OutputCapture();
				_app = new Engine(new string[] { "" }, cap, System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
				if (_app == null) {
					throw new System.Exception("StereoKit loader couldn't construct an instance of the App!");
				}

				// Initialize StereoKit, and the app
				var settings = _app.Settings;
				settings.androidActivity = activityHandle;
				settings.assetsFolder = "";

				if (!SK.Initialize(settings)) {
					return;
				}

				_app.Init();

				// Now loop until finished, and then shut down
				while (SK.Step(_app.Step)) { }
				cap.DisableSingleString = true;
				_app.IsCloseing = true;
				_app.Dispose();
				SK.Shutdown();

				Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
			});
		}

		// Events related to surface state changes
		public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height) {
			SK.SetWindow(holder.Surface.Handle);
		}

		public void SurfaceCreated(ISurfaceHolder holder) {
			SK.SetWindow(holder.Surface.Handle);
		}

		public void SurfaceDestroyed(ISurfaceHolder holder) {
			SK.SetWindow(IntPtr.Zero);
		}

		public void SurfaceRedrawNeeded(ISurfaceHolder holder) { }
	}
}