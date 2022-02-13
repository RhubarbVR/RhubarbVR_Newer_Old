using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Normal)]
	[Category(new string[] { "User" })]
	public class SimpleSpawn : Component
	{
		public override void Step() {
			var user = World.GetLocalUser();
			if (user is null) {
				return;
			}
			if (user.userRoot.Target is null) {
				var userEntity = Entity.AddChild("User");
				userEntity.persistence.Value = false;
				var userRoot = userEntity.AttachComponent<UserRoot>();
				userEntity.AttachComponent<LocomotionManager>().user.Target = World.GetLocalUser();
				if (!World.IsPersonalSpace) {
					userEntity.AttachMesh<BoxMesh, UIShader>().Item1.dimensions.Value = new Vec3(0.1f, 0.1f, 0.1f);
				}
				var leftHand = userEntity.AddChild("LeftHand");
				var leftComp = leftHand.AttachComponent<Hand>();
				leftComp.hand.Value = Handed.Left;
				leftComp.user.Target = World.GetLocalUser();
				userRoot.leftHand.Target = leftHand;
				if (!World.IsPersonalSpace) {
					leftHand.AttachMesh<BoxMesh, UIShader>().Item1.dimensions.Value = new Vec3(0.1f, 0.1f, 0.1f);
				}
				var rightHand = userEntity.AddChild("RightHand");
				var rightComp = rightHand.AttachComponent<Hand>();
				rightComp.hand.Value = Handed.Right;
				rightComp.user.Target = World.GetLocalUser();
				if (!World.IsPersonalSpace) {
					rightHand.AttachMesh<BoxMesh, UIShader>().Item1.dimensions.Value = new Vec3(0.1f, 0.1f, 0.1f);
				}
				userRoot.rightHand.Target = rightHand;
				var head = userEntity.AddChild("Head");
				head.AttachComponent<Head>().user.Target = World.GetLocalUser();
				if (!World.IsPersonalSpace) {
					var mesh = head.AttachMesh<CylinderMesh, UIShader>().Item1;
					mesh.depth.Value = 0.1f;
					mesh.diameter.Value = 0.1f;
					var opus = user.FindOrCreateSyncStream<RawAudioStream>("MainOpusStream");
					var audioPlayer = head.AttachComponent<SoundSource>();
					audioPlayer.volume.Value = 0f; // so you do not here yourself for a sec
					audioPlayer.sound.Target = opus;
					head.AttachComponent<UserAudioManager>().audioVolume.SetLinkerTarget(audioPlayer.volume);
					opus.LoadInput();
				}
				userRoot.head.Target = head;
				userRoot.user.Target = World.GetLocalUser();
				user.userRoot.Target = userRoot;
				Log.Info("User Made");
			}
		}
	}
}
