using System;
using System.Collections.Generic;
using System.IO;

namespace RNumerics
{
	/// <summary>
	/// TransformSequence stores an ordered list of basic transformations.
	/// This can be useful if you need to construct some modifications and want
	/// to use the same set later. For example, if you have a hierarchy of objects
	/// with relative transformations and want to "save" the nested transform sequence
	/// without having to hold references to the original objects.
	/// 
	/// Use the Append() functions to add different transform types, and the TransformX()
	/// to apply the sequence
	/// </summary>
	public class TransformSequence
	{
		enum XFormType
		{
			Translation = 0,
			QuaterionRotation = 1,
			QuaternionRotateAroundPoint = 2,
			Scale = 3,
			ScaleAroundPoint = 4,
			ToFrame = 5,
			FromFrame = 6
		}

		struct XForm
		{
			public XFormType type;
			public Vector3dTuple3 data;

			// may need to update these to handle other types...
			public Vector3d Translation => data.V0;
			public Vector3d Scale => data.V0;
			public Quaternionf Quaternion => new ((float)data.V0.x, (float)data.V0.y, (float)data.V0.z, (float)data.V1.x);
			public Vector3d RotateOrigin => data.V2;
			public Frame3f Frame => new ((Vector3f)RotateOrigin, Quaternion);
		}

		readonly List<XForm> _operations;



		public TransformSequence()
		{
			_operations = new List<XForm>();
		}

		public TransformSequence(TransformSequence copy)
		{
			_operations = new List<XForm>(copy._operations);
		}



		public void Append(TransformSequence sequence)
		{
			_operations.AddRange(sequence._operations);
		}


		public void AppendTranslation(Vector3d dv)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Translation,
				data = new Vector3dTuple3(dv, Vector3d.Zero, Vector3d.Zero)
			});
		}
		public void AppendTranslation(double dx, double dy, double dz)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Translation,
				data = new Vector3dTuple3(new Vector3d(dx, dy, dz), Vector3d.Zero, Vector3d.Zero)
			});
		}

		public void AppendRotation(Quaternionf q)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.QuaterionRotation,
				data = new Vector3dTuple3(new Vector3d(q.x, q.y, q.z), new Vector3d(q.w, 0, 0), Vector3d.Zero)
			});
		}

		public void AppendRotation(Quaternionf q, Vector3d aroundPt)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.QuaternionRotateAroundPoint,
				data = new Vector3dTuple3(new Vector3d(q.x, q.y, q.z), new Vector3d(q.w, 0, 0), aroundPt)
			});
		}

		public void AppendScale(Vector3d s)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Scale,
				data = new Vector3dTuple3(s, Vector3d.Zero, Vector3d.Zero)
			});
		}

		public void AppendScale(Vector3d s, Vector3d aroundPt)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.ScaleAroundPoint,
				data = new Vector3dTuple3(s, Vector3d.Zero, aroundPt)
			});
		}

		public void AppendToFrame(Frame3f frame)
		{
			var q = frame.Rotation;
			_operations.Add(new XForm()
			{
				type = XFormType.ToFrame,
				data = new Vector3dTuple3(new Vector3d(q.x, q.y, q.z), new Vector3d(q.w, 0, 0), frame.Origin)
			});
		}

		public void AppendFromFrame(Frame3f frame)
		{
			var q = frame.Rotation;
			_operations.Add(new XForm()
			{
				type = XFormType.FromFrame,
				data = new Vector3dTuple3(new Vector3d(q.x, q.y, q.z), new Vector3d(q.w, 0, 0), frame.Origin)
			});
		}


		/// <summary>
		/// Apply transforms to point
		/// </summary>
		public Vector3d TransformP(Vector3d p)
		{
			var N = _operations.Count;
			for (var i = 0; i < N; ++i)
			{
				switch (_operations[i].type)
				{
					case XFormType.Translation:
						p += _operations[i].Translation;
						break;

					case XFormType.QuaterionRotation:
						p = _operations[i].Quaternion * p;
						break;

					case XFormType.QuaternionRotateAroundPoint:
						p -= _operations[i].RotateOrigin;
						p = _operations[i].Quaternion * p;
						p += _operations[i].RotateOrigin;
						break;

					case XFormType.Scale:
						p *= _operations[i].Scale;
						break;

					case XFormType.ScaleAroundPoint:
						p -= _operations[i].RotateOrigin;
						p *= _operations[i].Scale;
						p += _operations[i].RotateOrigin;
						break;

					case XFormType.ToFrame:
						p = _operations[i].Frame.ToFrameP(ref p);
						break;

					case XFormType.FromFrame:
						p = _operations[i].Frame.FromFrameP(ref p);
						break;

					default:
						throw new NotImplementedException("TransformSequence.TransformP: unhandled type!");
				}
			}

			return p;
		}




		/// <summary>
		/// Apply transforms to vector. Includes scaling.
		/// </summary>
		public Vector3d TransformV(Vector3d v)
		{
			var N = _operations.Count;
			for (var i = 0; i < N; ++i)
			{
				switch (_operations[i].type)
				{
					case XFormType.Translation:
						break;

					case XFormType.QuaternionRotateAroundPoint:
					case XFormType.QuaterionRotation:
						v = _operations[i].Quaternion * v;
						break;

					case XFormType.ScaleAroundPoint:
					case XFormType.Scale:
						v *= _operations[i].Scale;
						break;

					case XFormType.ToFrame:
						v = _operations[i].Frame.ToFrameV(ref v);
						break;

					case XFormType.FromFrame:
						v = _operations[i].Frame.FromFrameV(ref v);
						break;

					default:
						throw new NotImplementedException("TransformSequence.TransformV: unhandled type!");
				}
			}

			return v;
		}




		/// <summary>
		/// Apply transforms to point
		/// </summary>
		public Vector3f TransformP(Vector3f p)
		{
			return (Vector3f)TransformP((Vector3d)p);
		}


		/// <summary>
		/// construct inverse transformation sequence
		/// </summary>
		public TransformSequence MakeInverse()
		{
			var reverse = new TransformSequence();
			var N = _operations.Count;
			for (var i = N - 1; i >= 0; --i)
			{
				switch (_operations[i].type)
				{
					case XFormType.Translation:
						reverse.AppendTranslation(-_operations[i].Translation);
						break;

					case XFormType.QuaterionRotation:
						reverse.AppendRotation(_operations[i].Quaternion.Inverse);
						break;

					case XFormType.QuaternionRotateAroundPoint:
						reverse.AppendRotation(_operations[i].Quaternion.Inverse, _operations[i].RotateOrigin);
						break;

					case XFormType.Scale:
						reverse.AppendScale(1.0 / _operations[i].Scale);
						break;

					case XFormType.ScaleAroundPoint:
						reverse.AppendScale(1.0 / _operations[i].Scale, _operations[i].RotateOrigin);
						break;

					case XFormType.ToFrame:
						reverse.AppendFromFrame(_operations[i].Frame);
						break;

					case XFormType.FromFrame:
						reverse.AppendToFrame(_operations[i].Frame);
						break;

					default:
						throw new NotImplementedException("TransformSequence.MakeInverse: unhandled type!");
				}
			}
			return reverse;
		}
	}
}
