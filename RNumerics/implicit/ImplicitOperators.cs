using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	abstract public class ImplicitNAryOp2d : IImplicitOperator2d
	{
		protected List<IImplicitField2d> m_vChildren;

		public ImplicitNAryOp2d() {
			m_vChildren = new List<IImplicitField2d>();
		}

		public void AddChild(IImplicitField2d pField) {
			m_vChildren.Add(pField);
		}

		virtual public float Value(in float fX, in float fY) {
			return 0;
		}

		virtual public void Gradient(in float fX, in float fY, ref float fGX, ref float fGY) {
			const float F_DELTA = 0.001f;
			var fValue = Value(fX, fY);
			fGX = (Value(fX + F_DELTA, fY) - fValue) / F_DELTA;
			fGY = (Value(fX, fY + F_DELTA) - fValue) / F_DELTA;
		}

		virtual public AxisAlignedBox2f Bounds
		{
			get {
				var box = new AxisAlignedBox2f();
				for (var i = 0; i < m_vChildren.Count; ++i) {
					box.Contain(m_vChildren[i].Bounds);
				}

				return box;
			}
		}
	}

	public sealed class ImplicitBlend2d : ImplicitNAryOp2d
	{
		public ImplicitBlend2d() : base() {
		}

		override public float Value(in float fX, in float fY) {
			var fSumValue = 0.0f;
			foreach (var child in m_vChildren) {
				fSumValue += child.Value(fX, fY);
			}

			return fSumValue;
		}

		override public void Gradient(in float fX, in float fY, ref float fGX, ref float fGY) {
			fGX = fGY = 0;
			float fTempX = 0, fTempY = 0;
			foreach (var child in m_vChildren) {
				child.Gradient(fX, fY, ref fTempX, ref fTempY);
				fGX += fTempX;
				fGY += fTempY;
			}
		}
	}

	public sealed class ImplicitUnion2d : ImplicitNAryOp2d
	{
		public ImplicitUnion2d() : base() {
		}

		override public float Value(in float fX, in float fY) {
			var fMaxValue = 0.0f;
			foreach (var child in m_vChildren) {
				fMaxValue = Math.Max(fMaxValue, child.Value(fX, fY));
			}

			return fMaxValue;
		}

		override public void Gradient(in float fX, in float fY, ref float fGX, ref float fGY) {
			var fMaxValue = 0.0f;
			var nMax = -1;
			for (var i = 0; i < m_vChildren.Count; ++i) {
				var fValue = m_vChildren[i].Value(fX, fY);
				if (fValue > fMaxValue) {
					nMax = i;
					fMaxValue = fValue;
				}
			}
			if (nMax >= 0) {
				m_vChildren[nMax].Gradient(fX, fY, ref fGX, ref fGY);
			}
			else {
				fGX = fGY = 0;
			}
		}
	}


	public sealed class ImplicitIntersection2d : ImplicitNAryOp2d
	{
		public ImplicitIntersection2d() {
		}

		override public float Value(in float fX, in float fY) {
			var fMinValue = 9999999999.0f;
			foreach (var child in m_vChildren) {
				fMinValue = Math.Min(fMinValue, child.Value(fX, fY));
			}

			return fMinValue;
		}

		override public void Gradient(in float fX, in float fY, ref float fGX, ref float fGY) {
			var fMinValue = 9999999999.0f;
			var nMin = -1;
			for (var i = 0; i < m_vChildren.Count; ++i) {
				var fValue = m_vChildren[i].Value(fX, fY);
				if (fValue < fMinValue) {
					nMin = i;
				}

				fMinValue = fValue;
			}
			if (nMin >= 0) {
				m_vChildren[nMin].Gradient(fX, fY, ref fGX, ref fGY);
			}
			else {
				fGX = fGY = 0;
			}
		}
	}


	public sealed class ImplicitDifference2d : ImplicitNAryOp2d
	{
		public ImplicitDifference2d() {
		}

		override public float Value(in float fX, in float fY) {
			if (m_vChildren.Count <= 0) {
				return 0;
			}

			var fCurValue = m_vChildren[0].Value(fX, fY);

			for (var i = 1; i < m_vChildren.Count; ++i) {
				var fValue = 1.0f - m_vChildren[i].Value(fX, fY);
				if (fValue < fCurValue) {
					fCurValue = fValue;
				}
			}
			return fCurValue;
		}

		override public void Gradient(in float fX, in float fY, ref float fGX, ref float fGY) {
			if (m_vChildren.Count <= 0) {
				fGX = fGY = 0;
				return;
			}

			var nMin = 0;
			var fCurValue = m_vChildren[0].Value(fX, fY);

			for (var i = 1; i < m_vChildren.Count; ++i) {
				var fValue = 1.0f - m_vChildren[i].Value(fX, fY);
				if (fValue < fCurValue) {
					nMin = i;
					fCurValue = fValue;
				}
			}

			m_vChildren[nMin].Gradient(fX, fY, ref fGX, ref fGY);
			if (nMin > 0) {
				fGX = -fGX;
				fGY = -fGY;
			}
		}
	}
}
