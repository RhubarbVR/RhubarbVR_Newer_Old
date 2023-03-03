using LibGodotSharp;
using Org.Godotengine.Godot;

[Activity(
    Label = "@string/app_name",
    Theme = "@style/LibGodotAppSplashTheme",
    LaunchMode = Android.Content.PM.LaunchMode.SingleTask,
    ExcludeFromRecents = false,
    Exported = true,
    ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
    ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Density | Android.Content.PM.ConfigChanges.Keyboard | Android.Content.PM.ConfigChanges.Navigation | Android.Content.PM.ConfigChanges.ScreenLayout | Android.Content.PM.ConfigChanges.UiMode,
    ResizeableActivity = false,
    MainLauncher = true)]
public class GodotApp : FullScreenGodotApp
{
    public unsafe override void OnCreate(Bundle? savedInstanceState)
    {
        Java.Lang.JavaSystem.LoadLibrary("godot_android");

        var runVerbose = false;
#if DEBUG
        runVerbose = true;
#endif
        //Arguments do nothing on android
        LibGodotManager.RunGodot(Array.Empty<string>(), RhubarbVRApplicationExtensionEntry.EntryPoint, RhubarbVRApplication.LoadScene, RhubarbVRApplication.LoadProjectSettings, runVerbose);
        SetTheme(RhubarbVR_Android.Resource.Style.LibGodotAppMainTheme);
        base.OnCreate(savedInstanceState);
    }
}