using GDExtension;
using Nodes;

public static class RhubarbVRApplication
{

    public static void LoadProjectSettings(ProjectSettings projectSettings)
    {
        ProjectSettings.SetSetting("application/config/name", "RhubarbVR");
    }


    public static void LoadScene(SceneTree scene) 
    {
        var rootNode = new EngineRunner();
		var openxrRoot = new XROrigin3D();
		var openxrCamera = new XRCamera3D();
		var audioListener = new AudioListener3D();
		var throwAway = new SubViewport {
			Size = new Vector2i(2, 2),
			RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled,
			OwnWorld3D = true,
		};
		rootNode.AddChild(openxrRoot);
		openxrRoot.AddChild(openxrCamera);
		openxrCamera.AddChild(audioListener);
		rootNode.AddChild(throwAway);
		rootNode.Rigin = openxrRoot;
		rootNode.Camera= openxrCamera;
		rootNode.ThowAway= throwAway;
		rootNode.AudioListener = audioListener;
		scene.Root.AddChild(rootNode);
    }
}