using System;
using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public class VideoImporter : Importer
	{
		public static bool IsValidImport(string path) {
			path = path.ToLower();
			return
				path.EndsWith(".asx") ||
				path.EndsWith(".dts") ||
				path.EndsWith(".gxf") ||
				path.EndsWith(".m2v") ||
				path.EndsWith(".m3u") ||
				path.EndsWith(".m4v") ||
				path.EndsWith(".mpeg1") ||
				path.EndsWith(".mpeg2") ||
				path.EndsWith(".mts") ||
				path.EndsWith(".mxf") ||
				path.EndsWith(".ogm") ||
				path.EndsWith(".pls") ||
				path.EndsWith(".bup") ||
				path.EndsWith(".a52") ||
				path.EndsWith(".aac") ||
				path.EndsWith(".b4s") ||
				path.EndsWith(".cue") ||
				path.EndsWith(".divx") ||
				path.EndsWith(".dv") ||
				path.EndsWith(".flv") ||
				path.EndsWith(".m1v") ||
				path.EndsWith(".m2ts") ||
				path.EndsWith(".mkv") ||
				path.EndsWith(".mov") ||
				path.EndsWith(".mpeg4") ||
				path.EndsWith(".oma") ||
				path.EndsWith(".spx") ||
				path.EndsWith(".ts") ||
				path.EndsWith(".vlc") ||
				path.EndsWith(".vob") ||
				path.EndsWith(".xspf") ||
				path.EndsWith(".dat") ||
				path.EndsWith(".bin") ||
				path.EndsWith(".ifo") ||
				path.EndsWith(".part") ||
				path.EndsWith(".avi") ||
				path.EndsWith(".mpeg") ||
				path.EndsWith(".mpg") ||
				path.EndsWith(".flac") ||
				path.EndsWith(".m4a") ||
				path.EndsWith(".mp1") ||
				path.EndsWith(".ogg") ||
				path.EndsWith(".wav") ||
				path.EndsWith(".xm") ||
				path.EndsWith(".3gp") ||
				path.EndsWith(".srt") ||
				path.EndsWith(".wmv") ||
				path.EndsWith(".ac3") ||
				path.EndsWith(".asf") ||
				path.EndsWith(".mod") ||
				path.EndsWith(".mp2") ||
				path.EndsWith(".mp3") ||
				path.EndsWith(".mp4") ||
				path.EndsWith(".wma") ||
				path.EndsWith(".mka") ||
				path.EndsWith(".m4p") ||
				path.EndsWith(".3g2");
		}
		public static bool IsStreamingProtocol(string scheme) {
			return scheme.ToLower() switch {
				"rtp" or "mms" or "rtsp" or "rtmp" => true,
				_ => false,
			};
		}
		public static bool IsVideoStreaming(Uri url) {
			return IsStreamingProtocol(url.Scheme) || url.Host.Contains("youtube.") || url.Host.Contains("youtu.be")
|| url.Host.Contains("vimeo.") || url.Host.Contains("twitch.tv") || url.Host.Contains("twitter.") || url.Host.Contains("soundcloud.")
|| url.Host.Contains("reddit.") || url.Host.Contains("dropbox.") || url.Host.Contains("mixer.") || url.Host.Contains("dailymotion.")
|| url.Host.Contains("streamable.") || url.Host.Contains("drive.google.") || url.Host.Contains("tiktok.") || url.Host.Contains("niconico.")
|| url.Host.Contains("nicovideo.") || url.Host.Contains("lbry.tv") || url.Host.Contains("nicovideo.jp");
		}

		public override void Import(string data, bool wasUri, byte[] rawdata) {
			if (wasUri) {
				RLog.Info("Building video");
				Entity.AttachComponent<Grabbable>();
				Entity.AttachComponent<BoxShape>();
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<RectangleMesh, UnlitMaterial>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.Dimensions);
				scaler.scaleMultiplier.Value = 0.5f;
				var textur = Entity.AttachComponent<VideoTexture>();
				textur.url.Value = data;
				var left = Entity.AddChild("left");
				var rignt = Entity.AddChild("rignt");
				left.position.Value = Vector3f.Right * -0.5f;
				rignt.position.Value = Vector3f.Right * 0.5f;
				left.position.Value += Vector3f.Forward * 0.2f;
				rignt.position.Value += Vector3f.Forward * 0.2f;
				left.scale.Value = new Vector3f(0.1f);
				rignt.scale.Value = new Vector3f(0.1f);
				textur.AudioChannels.Add();
				left.AttachMesh<Sphere3NormalizedCubeMesh, UnlitMaterial>();
				rignt.AttachMesh<Sphere3NormalizedCubeMesh, UnlitMaterial>();
				left.AttachComponent<SoundSource>().sound.Target = textur.AudioChannels[0];
				rignt.AttachComponent<SoundSource>().sound.Target = textur.AudioChannels[1];
				scaler.texture.Target = textur;
				mit.MainTexture.Target = textur;
				var WinEntit = Entity.AddChild("Window");
				WinEntit.rotation.Value = Quaternionf.Pitched.Inverse;
				WinEntit.position.Value = new Vector3f(1,1, 1);
				var gabable = WinEntit.AttachComponent<Grabbable>();
				var attachComp = WinEntit.AttachComponent<UICanvas>();
				attachComp.scale.Value = new Vector3f(2, 5, 1);
				var uimit = WinEntit.AttachComponent<UnlitMaterial>();
				uimit.DullSided.Value = true;
				var uiBuilder = new UIBuilder(WinEntit, uimit);
				uiBuilder.PushRect();
				uiBuilder.AddRectangle(0,0.9f,true);

				var grabAction = uiBuilder.AttachComponentToStack<UIGrabInteraction>();
				grabAction.AllowOtherZones.Value = true;
				grabAction.Grabeded.Target = gabable.RemoteGrab;

				var list = uiBuilder.AttachChildRect<VerticalList>();
				list.Fit.Value = true;

				uiBuilder.PushRect();
				uiBuilder.SetOffsetMinMax(new Vector2f(0.1f), new Vector2f(-0.1f));
				var button = uiBuilder.AddButton(false,0.2f,0.9f);
				uiBuilder.PushRect();
				uiBuilder.AddText("Play",null,1.8f);
				var buttonevent = uiBuilder.AttachComponentToStack<ButtonEventManager>();
				button.ButtonEvent.Target = buttonevent.Call;
				buttonevent.Click.Target = textur.Playback.Play;
				uiBuilder.PopRect();
				uiBuilder.PopRect();
				uiBuilder.PopRect();


				uiBuilder.PushRect();
				uiBuilder.SetOffsetMinMax(new Vector2f(0.1f), new Vector2f(-0.1f));
				button = uiBuilder.AddButton(false, 0.2f, 0.9f);
				uiBuilder.PushRect();
				uiBuilder.AddText("Stop", null, 1.8f);
				buttonevent = uiBuilder.AttachComponentToStack<ButtonEventManager>();
				button.ButtonEvent.Target = buttonevent.Call;
				buttonevent.Click.Target = textur.Playback.Stop;
				uiBuilder.PopRect();
				uiBuilder.PopRect();
				uiBuilder.PopRect();

				uiBuilder.PushRect();
				uiBuilder.SetOffsetMinMax(new Vector2f(0.1f), new Vector2f(-0.1f));
				button = uiBuilder.AddButton(false, 0.2f, 0.9f);
				uiBuilder.PushRect();
				uiBuilder.AddText("Pause", null, 1.8f);
				buttonevent = uiBuilder.AttachComponentToStack<ButtonEventManager>();
				button.ButtonEvent.Target = buttonevent.Call;
				buttonevent.Click.Target = textur.Playback.Pause;
				uiBuilder.PopRect();
				uiBuilder.PopRect();
				uiBuilder.PopRect();

				uiBuilder.PushRect();
				uiBuilder.SetOffsetMinMax(new Vector2f(0.1f), new Vector2f(-0.1f));
				button = uiBuilder.AddButton(false, 0.2f, 0.9f);
				uiBuilder.PushRect();
				uiBuilder.AddText("Resume", null, 1.8f);
				buttonevent = uiBuilder.AttachComponentToStack<ButtonEventManager>();
				button.ButtonEvent.Target = buttonevent.Call;
				buttonevent.Click.Target = textur.Playback.Resume;
				uiBuilder.PopRect();
				uiBuilder.PopRect();
				uiBuilder.PopRect();

				// TODO
				//var window = WinEntit.AttachComponent<UIWindow>();
				//WinEntit.rotation.Value *= Quaternionf.CreateFromYawPitchRoll(90, 0, 180);
				//WinEntit.position.Value = Vector3f.Forward * 0.1f;
				//window.Text.Value = "Media Controls";
				//window.WindowType.Value = UIWin.Normal;
				//var UIE = WinEntit.AddChild("UI");
				//var Play = UIE.AttachComponent<UIButton>();
				//Play.Text.Value = "Play";
				//Play.onClick.Target = textur.Playback.Play;
				//var Stop = UIE.AttachComponent<UIButton>();
				//Stop.Text.Value = "Stop";
				//Stop.onClick.Target = textur.Playback.Stop;
				//var Pause = UIE.AttachComponent<UIButton>();
				//Pause.Text.Value = "Pause";
				//Pause.onClick.Target = textur.Playback.Pause;
				//var Resume = UIE.AttachComponent<UIButton>();
				//Resume.Text.Value = "Resume";
				//Resume.onClick.Target = textur.Playback.Resume;
				Destroy();
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = World.LoadLocalAsset(File.ReadAllBytes(data), data);
						Import(newuri.ToString(), true,null);
					}
					else {
						RLog.Err("Video Load Uknown" + data);
					}
				}
				else {
					var newuri = World.LoadLocalAsset(rawdata, data);
					Import(newuri.ToString(), true,null);
				}
			}
		}
	}
}
