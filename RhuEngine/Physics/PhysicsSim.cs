using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Physics
{
	[Flags]
	public enum ECollisionFilterGroups
	{
		AllFilter = -1,
		None = 0,
		DefaultFilter = 1,
		StaticFilter = 2,
		KinematicFilter = 4,
		DebrisFilter = 8,
		SensorTrigger = 16,
		CharacterFilter = 32,
		UI = 64,
		Custom1 = 128,
		Custom2 = 256,
		Custom3 = 512,
		Custom4 = 1024,
		Custom5 = 2048,
	}

	public interface ILinkedPhysicsSim
	{
		public object NewSim();
		public void UpdateSim(object obj,float DeltaSeconds);
	}
	public class PhysicsSim
	{
		public static ILinkedPhysicsSim Manager { get; set; }

		public object obj;

		public void UpdateSim(float DeltaSeconds) {
			Manager?.UpdateSim(obj, DeltaSeconds);
		}

		public PhysicsSim() {
			obj = Manager?.NewSim();
		}
	}
}
