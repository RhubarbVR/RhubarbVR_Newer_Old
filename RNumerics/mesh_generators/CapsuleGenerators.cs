using System;

namespace RNumerics
{

	/// <summary>
	/// Source from https://github.com/behreajj/CreateCapsule/blob/master/Unity%20Capsule/Assets/Editor/CapsuleMaker.cs
	/// GNU General Public License v3.0
	/// </summary>
	public class CapsuleGenerator : MeshGenerator
	{
		public enum UvProfile : int
		{
			Fixed = 0,
			Aspect = 1,
			Uniform = 2
		}

		public int Longitudes = 32;
		public int Latitudes = 16;
		public int Rings = 0;
		public float Depth = 1.0f;
		public float Radius = 0.5f;
		public UvProfile Profile = UvProfile.Aspect;

		public override MeshGenerator Generate() {
			var calcMiddle = Rings > 0;
			var halfLats = Latitudes / 2;
			var halfLatsn1 = halfLats - 1;
			var halfLatsn2 = halfLats - 2;
			var ringsp1 = Rings + 1;
			var lonsp1 = Longitudes + 1;
			var halfDepth = Depth * 0.5;
			var summit = halfDepth + Radius;

			// Vertex index offsets.
			var vertOffsetNorthHemi = Longitudes;
			var vertOffsetNorthEquator = vertOffsetNorthHemi + (lonsp1 * halfLatsn1);
			var vertOffsetCylinder = vertOffsetNorthEquator + lonsp1;
			var vertOffsetSouthEquator = calcMiddle ? vertOffsetCylinder + (lonsp1 * Rings) : vertOffsetCylinder;
			var vertOffsetSouthHemi = vertOffsetSouthEquator + lonsp1;
			var vertOffsetSouthPolar = vertOffsetSouthHemi + (lonsp1 * halfLatsn2);
			var vertOffsetSouthCap = vertOffsetSouthPolar + lonsp1;

			// Initialize arrays.
			var vertLen = vertOffsetSouthCap + Longitudes;

			vertices = new VectorArray3d(vertLen);
			uv = new VectorArray2f(vertLen);
			normals = new VectorArray3f(vertLen);

			var toTheta = 2.0 * Math.PI / Longitudes;
			var toPhi = Math.PI / Latitudes;
			var toTexHorizontal = 1.0 / Longitudes;
			var toTexVertical = 1.0 / halfLats;
			var vtAspectRatio = Profile switch {
				UvProfile.Aspect => Radius / (Depth + Radius + Radius),
				UvProfile.Uniform => (double)halfLats / (ringsp1 + Latitudes),
				_ => 1.0 / 3.0,
			};
			var vtAspectNorth = 1.0 - vtAspectRatio;
			var vtAspectSouth = vtAspectRatio;

			var thetaCartesian = new Vector2f[Longitudes];
			var rhoThetaCartesian = new Vector2f[Longitudes];
			var sTextureCache = new double[lonsp1];

			// Polar vertices.
			for (var j = 0; j < Longitudes; ++j) {
				double jf = j;
				var sTexturePolar = 1.0 - ((jf + 0.5) * toTexHorizontal);
				var theta = jf * toTheta;

				var cosTheta = Math.Cos(theta);
				var sinTheta = Math.Sin(theta);

				thetaCartesian[j] = new Vector2f(cosTheta, sinTheta);
				rhoThetaCartesian[j] = new Vector2f(
					Radius * cosTheta,
					Radius * sinTheta);

				// North.
				vertices[j] = new Vector3f(0.0, summit, 0.0);
				uv[j] = new Vector2f(sTexturePolar, 1.0);
				normals[j] = new Vector3f(0.0, 1.0, 0);

				// South.
				var idx = vertOffsetSouthCap + j;
				vertices[idx] = new Vector3f(0.0, -summit, 0.0);
				uv[idx] = new Vector2f(sTexturePolar, 0.0);
				normals[idx] = new Vector3f(0.0, -1.0, 0.0);
			}

			// Equatorial vertices.
			for (var j = 0; j < lonsp1; ++j) {
				var sTexture = 1.0 - (j * toTexHorizontal);
				sTextureCache[j] = sTexture;

				// Wrap to first element upon reaching last.
				var jMod = j % Longitudes;
				var tc = thetaCartesian[jMod];
				var rtc = rhoThetaCartesian[jMod];

				// North equator.
				var idxn = vertOffsetNorthEquator + j;
				vertices[idxn] = new Vector3f(rtc.x, halfDepth, -rtc.y);
				uv[idxn] = new Vector2f(sTexture, vtAspectNorth);
				normals[idxn] = new Vector3f(tc.x, 0.0, -tc.y);

				// South equator.
				var idxs = vertOffsetSouthEquator + j;
				vertices[idxs] = new Vector3f(rtc.x, -halfDepth, -rtc.y);
				uv[idxs] = new Vector2f(sTexture, vtAspectSouth);
				normals[idxs] = new Vector3f(tc.x, 0.0, -tc.y);
			}

			// Hemisphere vertices.
			for (var i = 0; i < halfLatsn1; ++i) {
				var ip1f = i + 1.0;
				var phi = ip1f * toPhi;

				// For coordinates.
				var cosPhiSouth = Math.Cos(phi);
				var sinPhiSouth = Math.Sin(phi);

				// Symmetrical hemispheres mean cosine and sine only need
				// to be calculated once.
				var cosPhiNorth = sinPhiSouth;
				var sinPhiNorth = -cosPhiSouth;

				var rhoCosPhiNorth = Radius * cosPhiNorth;
				var rhoSinPhiNorth = Radius * sinPhiNorth;
				var zOffsetNorth = halfDepth - rhoSinPhiNorth;

				var rhoCosPhiSouth = Radius * cosPhiSouth;
				var rhoSinPhiSouth = Radius * sinPhiSouth;
				var zOffsetSouth = -halfDepth - rhoSinPhiSouth;

				// For texture coordinates.
				var tTexFac = ip1f * toTexVertical;
				var cmplTexFac = 1.0 - tTexFac;
				var tTexNorth = cmplTexFac + (vtAspectNorth * tTexFac);
				var tTexSouth = cmplTexFac * vtAspectSouth;

				var iLonsp1 = i * lonsp1;
				var vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
				var vertCurrLatSouth = vertOffsetSouthHemi + iLonsp1;

				for (var j = 0; j < lonsp1; ++j) {
					var jMod = j % Longitudes;
					var sTexture = sTextureCache[j];
					var tc = thetaCartesian[jMod];

					// North hemisphere.
					var idxn = vertCurrLatNorth + j;
					vertices[idxn] = new Vector3f(
						rhoCosPhiNorth * tc.x,
						zOffsetNorth, // 
						-rhoCosPhiNorth * tc.y);
					uv[idxn] = new Vector2f(sTexture, tTexNorth);
					normals[idxn] = new Vector3f(
						cosPhiNorth * tc.x, //
						-sinPhiNorth, //
						-cosPhiNorth * tc.y);

					// South hemisphere.
					var idxs = vertCurrLatSouth + j;
					vertices[idxs] = new Vector3f(
						rhoCosPhiSouth * tc.x,
						zOffsetSouth, //
						-rhoCosPhiSouth * tc.y);
					uv[idxs] = new Vector2f(sTexture, tTexSouth);
					normals[idxs] = new Vector3f(
						cosPhiSouth * tc.x, //
						-sinPhiSouth, //
						-cosPhiSouth * tc.y);
				}
			}

			// Cylinder vertices.
			if (calcMiddle) {
				// Exclude both origin and destination edges
				// (North and South equators) from the interpolation.
				var toFac = 1.0 / ringsp1;
				var idxCylLat = vertOffsetCylinder;

				for (var h = 1; h < ringsp1; ++h) {
					var fac = h * toFac;
					var cmplFac = 1.0 - fac;
					var tTexture = (cmplFac * vtAspectNorth) + (fac * vtAspectSouth);
					var z = halfDepth - (Depth * fac);

					for (var j = 0; j < lonsp1; ++j) {
						var jMod = j % Longitudes;
						var sTexture = sTextureCache[j];
						var tc = thetaCartesian[jMod];
						var rtc = rhoThetaCartesian[jMod];

						vertices[idxCylLat] = new Vector3f(rtc.x, z, -rtc.y);
						uv[idxCylLat] = new Vector2f(sTexture, tTexture);
						normals[idxCylLat] = new Vector3f(tc.x, 0.0, -tc.y);

						++idxCylLat;
					}
				}
			}

			// Triangle indices.
			// Stride is 3 for polar triangles;
			// stride is 6 for two triangles forming a quad.
			//int longs3 = longitudes ;
			var longs6 = Longitudes * 2;
			var hemiLons = halfLatsn1 * longs6;

			var triOffsetNorthHemi = Longitudes;
			var triOffsetCylinder = triOffsetNorthHemi + hemiLons;
			var triOffsetSouthHemi = triOffsetCylinder + (ringsp1 * longs6);
			var triOffsetSouthCap = triOffsetSouthHemi + hemiLons;

			var fsLen = triOffsetSouthCap + Longitudes;
			triangles = new IndexArray3i(fsLen);

			// Polar caps.
			for (int i = 0, k = 0, m = triOffsetSouthCap; i < Longitudes; ++i, k++, m++) {
				// North.
				triangles.Set(k, i, vertOffsetNorthHemi + i, vertOffsetNorthHemi + i + 1);

				// South.
				triangles.Set(m, vertOffsetSouthCap + i, vertOffsetSouthPolar + i + 1, vertOffsetSouthPolar + i);

			}

			// Hemispheres.
			for (int i = 0, k = triOffsetNorthHemi, m = triOffsetSouthHemi; i < halfLatsn1; ++i) {
				var iLonsp1 = i * lonsp1;

				var vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
				var vertNextLatNorth = vertCurrLatNorth + lonsp1;

				var vertCurrLatSouth = vertOffsetSouthEquator + iLonsp1;
				var vertNextLatSouth = vertCurrLatSouth + lonsp1;

				for (var j = 0; j < Longitudes; ++j, k += 2, m += 2) {
					// North.
					var north00 = vertCurrLatNorth + j;
					var north01 = vertNextLatNorth + j;
					var north11 = vertNextLatNorth + j + 1;
					var north10 = vertCurrLatNorth + j + 1;

					triangles.Set(k, north00, north11, north10);
					triangles.Set(k + 1, north00, north01, north11);

					// South.
					var south00 = vertCurrLatSouth + j;
					var south01 = vertNextLatSouth + j;
					var south11 = vertNextLatSouth + j + 1;
					var south10 = vertCurrLatSouth + j + 1;


					triangles.Set(m, south00, south11, south10);
					triangles.Set(m + 1, south00, south01, south11);

				}
			}

			// Cylinder.
			for (int i = 0, k = triOffsetCylinder; i < ringsp1; ++i) {
				var vertCurrLat = vertOffsetNorthEquator + (i * lonsp1);
				var vertNextLat = vertCurrLat + lonsp1;

				for (var j = 0; j < Longitudes; ++j, k += 2) {
					var cy00 = vertCurrLat + j;
					var cy01 = vertNextLat + j;
					var cy11 = vertNextLat + j + 1;
					var cy10 = vertCurrLat + j + 1;

					triangles.Set(k, cy00, cy11, cy10);
					triangles.Set(k + 1, cy00, cy01, cy11);

				}
			}

			return this;
		}
	}

}
