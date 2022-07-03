using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using RhuEngine.Physics;
using BEPUik;

namespace RhuEngine.Components
{
	//Todo: Finish adding ik constraints and add the ik controllers to do movement
	[Category("Transform/IK")]
	public class IKManager : Component
	{
		public IKSolver iKSolver;

		public override void OnLoaded() {
			base.OnLoaded();
			iKSolver = new IKSolver();
			iKSolver.ActiveSet.UseAutomass = true;
			iKSolver.AutoscaleControlImpulses = true;
			iKSolver.AutoscaleControlMaximumForce = float.MaxValue;
			iKSolver.TimeStepDuration = .1f;
			iKSolver.ControlIterationCount = 100;
			iKSolver.FixerIterationCount = 10;
			iKSolver.VelocitySubiterationCount = 3;
		}

		public override void Dispose() {
			base.Dispose();
			iKSolver?.Dispose();
			iKSolver = null;
		}

	}
}
