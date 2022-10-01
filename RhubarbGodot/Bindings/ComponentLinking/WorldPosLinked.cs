using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
namespace RhubarbVR.Bindings.ComponentLinking
{

	public abstract class WorldPositionLinked<T, T2> : EngineWorldLinkBase<T> where T : LinkedWorldComponent, new() where T2 : Node3D , new()
	{
		public T2 node;

		public abstract string ObjectName { get; }

		public override void Init() {
			node = new T2 {
				Name = ObjectName
			};
			EngineRunner._.AddChild(node);
			LinkedComp.Entity.GlobalTransformChange += Entity_GlobalTransformChange;
			StartContinueInit();
			UpdatePosThisFrame = true;
		}
		public abstract void StartContinueInit();

		public override void Remove() {
			node?.Dispose();
		}

		public override void Started() {
			node?.SetVisible(true);
		}

		public override void Stopped() {
			node?.SetVisible(false);
		}

		public bool UpdatePosThisFrame { get; private set; } = true;

		private void Entity_GlobalTransformChange(Entity obj, bool data) {
			UpdatePosThisFrame = true;
		}

		public override void Render() {
			if (UpdatePosThisFrame) {
				node.SetPos(LinkedComp.Entity.GlobalTrans);
				UpdatePosThisFrame = false;
			}
		}

	}
}
