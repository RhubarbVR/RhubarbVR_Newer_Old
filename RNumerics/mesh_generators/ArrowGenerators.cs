using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// radially-symmetric 3D arrow
	public class Radial3DArrowGenerator : VerticalGeneralizedCylinderGenerator
	{
		public float StickRadius = 0.5f;
		public float StickLength = 1.0f;
		public float HeadBaseRadius = 1.0f;
		public float TipRadius = 0.0f;
		public float HeadLength = 0.5f;
		public bool DoubleSided = false;

		override public MeshGenerator Generate() {
			if (DoubleSided) {
				Sections = new CircularSection[6];
				Sections[0] = new CircularSection(TipRadius, 0.0f);
				Sections[1] = new CircularSection(HeadBaseRadius, HeadLength);
				Sections[2] = new CircularSection(StickRadius, HeadLength);
				Sections[3] = new CircularSection(StickRadius, StickLength);
				Sections[4] = new CircularSection(HeadBaseRadius, StickLength);
				Sections[5] = new CircularSection(TipRadius, StickLength + HeadLength);
			}
			else {
				Sections = new CircularSection[4];
				Sections[0] = new CircularSection(StickRadius, 0.0f);
				Sections[1] = new CircularSection(StickRadius, StickLength);
				Sections[2] = new CircularSection(HeadBaseRadius, StickLength);
				Sections[3] = new CircularSection(TipRadius, StickLength + HeadLength);
			}

			Capped = true;
			NoSharedVertices = true;
			Clockwise = true;
			base.Generate();

			return this;
		}

	}






}
