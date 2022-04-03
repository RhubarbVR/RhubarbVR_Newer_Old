using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Normal)]
	[Category(new string[] { "User" })]
	public class SimpleSpawn : Component
	{
		public override void Step() {
			if (!Engine.EngineLink.SpawnPlayer) {
				return;
			}
			var user = World.GetLocalUser();
			if (user is null) {
				return;
			}
			if (user.userRoot.Target is null) {
				var userEntity = Entity.AddChild("User");
				var userRoot = userEntity.AttachComponent<UserRoot>();
				userEntity.AttachComponent<LocomotionManager>().user.Target = World.GetLocalUser();
				var leftHand = userEntity.AddChild("LeftHand");
				var leftComp = leftHand.AttachComponent<Hand>();
				leftComp.hand.Value = Handed.Left;
				leftComp.user.Target = World.GetLocalUser();
				userRoot.leftHand.Target = leftHand;
				if (!World.IsPersonalSpace) {
					var r = leftHand.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitShader>();
					r.Item1.Diameter.Value = 0.06f;
					r.Item3.colorLinear.Value = Colorf.RhubarbRed;
				}
				var rightHand = userEntity.AddChild("RightHand");
				var rightComp = rightHand.AttachComponent<Hand>();
				rightComp.hand.Value = Handed.Right;
				rightComp.user.Target = World.GetLocalUser();
				if (!World.IsPersonalSpace) {
					var l = rightHand.AttachMeshWithMeshRender<Sphere3NormalizedCubeMesh, UnlitShader>();
					l.Item1.Diameter.Value = 0.06f;
					l.Item3.colorLinear.Value = Colorf.RhubarbGreen;
				}
				userRoot.rightHand.Target = rightHand;
				var head = userEntity.AddChild("Head");
				head.AttachComponent<Head>().user.Target = World.GetLocalUser();
				if (!World.IsPersonalSpace) {
					var thing = head.AddChild("Render").AttachMeshWithMeshRender<CappedCylinderMesh, UnlitShader>();
					thing.Item3.colorLinear.Value = user.UserName.GetHashHue();
					var mesh = thing.Item1;
					mesh.Entity.rotation.Value = Quaternionf.Pitched;
					mesh.Height.Value = 0.1f;
					mesh.BaseRadius.Value = 0.05f;
					mesh.TopRadius.Value = 0.05f;
					var opus = user.FindOrCreateSyncStream<OpusStream>("MainOpusStream");
					var audioPlayer = head.AttachComponent<SoundSource>();
					audioPlayer.volume.Value = 0f;
					audioPlayer.sound.Target = opus;
					head.AttachComponent<UserAudioManager>().audioVolume.SetLinkerTarget(audioPlayer.volume);
					opus.LoadInput();
				}
				userRoot.head.Target = head;
				userRoot.user.Target = World.GetLocalUser();
				user.userRoot.Target = userRoot;
				RLog.Info("User Made");
			}
		}
	}
}
