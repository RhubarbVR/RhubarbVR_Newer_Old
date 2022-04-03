using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Physics
{
	public interface IBCollider
	{

	}

	public abstract class Collider
	{
		public static IBCollider Manager { get; set; }

		public object obj;
	}
}
