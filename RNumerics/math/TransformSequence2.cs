using System;
using System.Collections.Generic;
using System.IO;

namespace RNumerics
{
	public interface ITransform2
	{
		Vector2d TransformP(Vector2d p);
		Vector2d TransformN(Vector2d n);
		double TransformScalar(double s);
	}



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
	public class TransformSequence2 : ITransform2
	{
		enum XFormType
		{
			Translation = 0,
			Rotation = 1,
			RotateAroundPoint = 2,
			Scale = 3,
			ScaleAroundPoint = 4,

			NestedITransform2 = 10
		}

		struct XForm
		{
			public XFormType type;
			public Vector2dTuple2 data;
			public object xform;

			// may need to update these to handle other types...
			public Vector2d Translation => data.V0;
			public Vector2d Scale => data.V0;
			public Matrix2d Rotation => new (data.V0.x);
			public Vector2d RotateOrigin => data.V1;

			public bool ScaleIsUniform => data.V0.EpsilonEqual(data.V1, MathUtil.EPSILONF);

			public ITransform2 NestedITransform2 => xform as ITransform2;
		}

		readonly List<XForm> _operations;



		public TransformSequence2()
		{
			_operations = new List<XForm>();
		}



		public TransformSequence2 Translation(Vector2d dv)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Translation,
				data = new Vector2dTuple2(dv, Vector2d.Zero)
			});
			return this;
		}
		public TransformSequence2 Translation(double dx, double dy)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Translation,
				data = new Vector2dTuple2(new Vector2d(dx, dy), Vector2d.Zero)
			});
			return this;
		}

		public TransformSequence2 RotationRad(double angle)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Rotation,
				data = new Vector2dTuple2(new Vector2d(angle, 0), Vector2d.Zero)
			});
			return this;
		}
		public TransformSequence2 RotationDeg(double angle)
		{
			return RotationRad(angle * MathUtil.DEG_2_RAD);
		}


		public TransformSequence2 RotationRad(double angle, Vector2d aroundPt)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.RotateAroundPoint,
				data = new Vector2dTuple2(new Vector2d(angle, 0), aroundPt)
			});
			return this;
		}
		public TransformSequence2 RotationDeg(double angle, Vector2d aroundPt)
		{
			return RotationRad(angle * MathUtil.DEG_2_RAD, aroundPt);
		}

		public TransformSequence2 Scale(Vector2d s)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.Scale,
				data = new Vector2dTuple2(s, Vector2d.Zero)
			});
			return this;
		}

		public TransformSequence2 Scale(Vector2d s, Vector2d aroundPt)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.ScaleAroundPoint,
				data = new Vector2dTuple2(s, aroundPt)
			});
			return this;
		}

		public TransformSequence2 Append(ITransform2 t2)
		{
			_operations.Add(new XForm()
			{
				type = XFormType.NestedITransform2,
				xform = t2
			});
			return this;
		}



		/// <summary>
		/// Apply transforms to point
		/// </summary>
		public Vector2d TransformP(Vector2d p)
		{
			var N = _operations.Count;
			for (var i = 0; i < N; ++i)
			{
				switch (_operations[i].type)
				{
					case XFormType.Translation:
						p += _operations[i].Translation;
						break;

					case XFormType.Rotation:
						p = _operations[i].Rotation * p;
						break;

					case XFormType.RotateAroundPoint:
						p -= _operations[i].RotateOrigin;
						p = _operations[i].Rotation * p;
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

					case XFormType.NestedITransform2:
						p = _operations[i].NestedITransform2.TransformP(p);
						break;

					default:
						throw new NotImplementedException("TransformSequence.TransformP: unhandled type!");
				}
			}

			return p;
		}



		/// <summary>
		/// Apply transforms to normalized vector
		/// </summary>
		public Vector2d TransformN(Vector2d n)
		{
			var N = _operations.Count;
			for (var i = 0; i < N; ++i)
			{
				switch (_operations[i].type)
				{
					case XFormType.Translation:
						break;

					case XFormType.Rotation:
						n = _operations[i].Rotation * n;
						break;

					case XFormType.RotateAroundPoint:
						n = _operations[i].Rotation * n;
						break;

					case XFormType.Scale:
						n *= _operations[i].Scale;
						break;

					case XFormType.ScaleAroundPoint:
						n *= _operations[i].Scale;
						break;

					case XFormType.NestedITransform2:
						n = _operations[i].NestedITransform2.TransformN(n);
						break;

					default:
						throw new NotImplementedException("TransformSequence.TransformN: unhandled type!");
				}
			}

			return n;
		}





		/// <summary>
		/// Apply transforms to scalar dimension
		/// </summary>
		public double TransformScalar(double s)
		{
			var N = _operations.Count;
			for (var i = 0; i < N; ++i)
			{
				switch (_operations[i].type)
				{
					case XFormType.Translation:
						break;

					case XFormType.Rotation:
						break;

					case XFormType.RotateAroundPoint:
						break;

					case XFormType.Scale:
						s *= _operations[i].Scale.x;
						break;

					case XFormType.ScaleAroundPoint:
						s *= _operations[i].Scale.x;
						break;

					case XFormType.NestedITransform2:
						s = _operations[i].NestedITransform2.TransformScalar(s);
						break;

					default:
						throw new NotImplementedException("TransformSequence.TransformScalar: unhandled type!");
				}
			}

			return s;
		}
	}
}
