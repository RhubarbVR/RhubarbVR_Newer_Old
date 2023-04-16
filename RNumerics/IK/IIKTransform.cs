using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNumerics.IK
{
	public interface ITransform
	{
		Vector3f position { get; set; }
		Vector3f right { get; }
		Vector3f up { get; }
		Vector3f forward { get; }
		Vector3f localPosition { get; }
		Vector3f localScale { get; set; }
		Quaternionf rotation { get; set; }
		Quaternionf localRotation { get; set; }
		ITransform AddChild(string v);

	}
	public interface IIKBoneTransform : ITransform
	{
		public IIKBoneTransform parent { get; }

	}
}
