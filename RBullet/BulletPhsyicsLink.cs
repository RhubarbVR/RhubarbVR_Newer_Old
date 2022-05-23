using System;
using System.Runtime.InteropServices;

using RhuEngine.Physics;
using BulletSharp;
using System.IO;
using System.Linq;
using RhuEngine.Linker;

namespace RBullet
{  
	public class BulletPhsyicsLink: PhysicsHelper.Physics<BulletRigidBodyCollider, BulletPhysicsSim, BulletColliderShape>
    {
		public BulletPhsyicsLink(bool ByPass) { }

		public BulletPhsyicsLink() {
			if (!Native.Load()) {
				if (Environment.OSVersion.Platform == PlatformID.Unix) {
					RLog.Info("Did not load lib at first");
					var files = Directory.GetFiles("./../../", "*/*/linux-x64/native/libbulletc.so");
					var filesarray = files.ToArray();
					if (filesarray.Length == 0) {
						throw new Exception("Failed to Find lib");
					}
					else {
						File.Copy(filesarray[0], "./libbulletc.so");
						RLog.Info("Did copy to try and get arround lib no loading");
					}
				}
				else {
						throw new Exception("Failed to load lib");
				}
			}
		}

	}
}
