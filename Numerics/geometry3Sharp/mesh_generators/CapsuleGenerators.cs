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

		public override MeshGenerator Generate()
		{
			bool calcMiddle = Rings > 0;
			int halfLats = Latitudes / 2;
			int halfLatsn1 = halfLats - 1;
			int halfLatsn2 = halfLats - 2;
			int ringsp1 = Rings + 1;
			int lonsp1 = Longitudes + 1;
			double halfDepth = Depth * 0.5;
			double summit = halfDepth + Radius;

			// Vertex index offsets.
			int vertOffsetNorthHemi = Longitudes;
			int vertOffsetNorthEquator = vertOffsetNorthHemi + lonsp1 * halfLatsn1;
			int vertOffsetCylinder = vertOffsetNorthEquator + lonsp1;
			int vertOffsetSouthEquator = calcMiddle ? vertOffsetCylinder + lonsp1 * Rings : vertOffsetCylinder;
			int vertOffsetSouthHemi = vertOffsetSouthEquator + lonsp1;
			int vertOffsetSouthPolar = vertOffsetSouthHemi + lonsp1 * halfLatsn2;
			int vertOffsetSouthCap = vertOffsetSouthPolar + lonsp1;

			// Initialize arrays.
			int vertLen = vertOffsetSouthCap + Longitudes;

			vertices = new VectorArray3d(vertLen);
			uv = new VectorArray2f(vertLen);
			normals = new VectorArray3f(vertLen);

			double toTheta = 2.0 * Math.PI / Longitudes;
			double toPhi = Math.PI / Latitudes;
			double toTexHorizontal = 1.0 / Longitudes;
			double toTexVertical = 1.0 / halfLats;

			// Calculate positions for texture coordinates vertical.
			double vtAspectRatio;
			switch (Profile)
			{
				case UvProfile.Aspect:
					vtAspectRatio = Radius / (Depth + Radius + Radius);
					break;

				case UvProfile.Uniform:
					vtAspectRatio = (double)halfLats / (ringsp1 + Latitudes);
					break;

				case UvProfile.Fixed:
				default:
					vtAspectRatio = 1.0 / 3.0;
					break;
			}

			double vtAspectNorth = 1.0 - vtAspectRatio;
			double vtAspectSouth = vtAspectRatio;

			Vector2f[] thetaCartesian = new Vector2f[Longitudes];
			Vector2f[] rhoThetaCartesian = new Vector2f[Longitudes];
			double[] sTextureCache = new double[lonsp1];

			// Polar vertices.
			for (int j = 0; j < Longitudes; ++j)
			{
				double jf = j;
				double sTexturePolar = 1.0 - ((jf + 0.5) * toTexHorizontal);
				double theta = jf * toTheta;

				double cosTheta = Math.Cos(theta);
				double sinTheta = Math.Sin(theta);

				thetaCartesian[j] = new Vector2f(cosTheta, sinTheta);
				rhoThetaCartesian[j] = new Vector2f(
					Radius * cosTheta,
					Radius * sinTheta);

				// North.
				vertices[j] = new Vector3f(0.0, summit, 0.0);
				uv[j] = new Vector2f(sTexturePolar, 1.0);
				normals[j] = new Vector3f(0.0, 1.0, 0);

				// South.
				int idx = vertOffsetSouthCap + j;
				vertices[idx] = new Vector3f(0.0, -summit, 0.0);
				uv[idx] = new Vector2f(sTexturePolar, 0.0);
				normals[idx] = new Vector3f(0.0, -1.0, 0.0);
			}

			// Equatorial vertices.
			for (int j = 0; j < lonsp1; ++j)
			{
				double sTexture = 1.0 - j * toTexHorizontal;
				sTextureCache[j] = sTexture;

				// Wrap to first element upon reaching last.
				int jMod = j % Longitudes;
				Vector2f tc = thetaCartesian[jMod];
				Vector2f rtc = rhoThetaCartesian[jMod];

				// North equator.
				int idxn = vertOffsetNorthEquator + j;
				vertices[idxn] = new Vector3f(rtc.x, halfDepth, -rtc.y);
				uv[idxn] = new Vector2f(sTexture, vtAspectNorth);
				normals[idxn] = new Vector3f(tc.x, 0.0, -tc.y);

				// South equator.
				int idxs = vertOffsetSouthEquator + j;
				vertices[idxs] = new Vector3f(rtc.x, -halfDepth, -rtc.y);
				uv[idxs] = new Vector2f(sTexture, vtAspectSouth);
				normals[idxs] = new Vector3f(tc.x, 0.0, -tc.y);
			}

			// Hemisphere vertices.
			for (int i = 0; i < halfLatsn1; ++i)
			{
				double ip1f = i + 1.0;
				double phi = ip1f * toPhi;

				// For coordinates.
				double cosPhiSouth = Math.Cos(phi);
				double sinPhiSouth = Math.Sin(phi);

				// Symmetrical hemispheres mean cosine and sine only need
				// to be calculated once.
				double cosPhiNorth = sinPhiSouth;
				double sinPhiNorth = -cosPhiSouth;

				double rhoCosPhiNorth = Radius * cosPhiNorth;
				double rhoSinPhiNorth = Radius * sinPhiNorth;
				double zOffsetNorth = halfDepth - rhoSinPhiNorth;

				double rhoCosPhiSouth = Radius * cosPhiSouth;
				double rhoSinPhiSouth = Radius * sinPhiSouth;
				double zOffsetSouth = -halfDepth - rhoSinPhiSouth;

				// For texture coordinates.
				double tTexFac = ip1f * toTexVertical;
				double cmplTexFac = 1.0 - tTexFac;
				double tTexNorth = cmplTexFac + vtAspectNorth * tTexFac;
				double tTexSouth = cmplTexFac * vtAspectSouth;

				int iLonsp1 = i * lonsp1;
				int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
				int vertCurrLatSouth = vertOffsetSouthHemi + iLonsp1;

				for (int j = 0; j < lonsp1; ++j)
				{
					int jMod = j % Longitudes;
					double sTexture = sTextureCache[j];
					Vector2f tc = thetaCartesian[jMod];

					// North hemisphere.
					int idxn = vertCurrLatNorth + j;
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
					int idxs = vertCurrLatSouth + j;
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
			if (calcMiddle)
			{
				// Exclude both origin and destination edges
				// (North and South equators) from the interpolation.
				double toFac = 1.0 / ringsp1;
				int idxCylLat = vertOffsetCylinder;

				for (int h = 1; h < ringsp1; ++h)
				{
					double fac = h * toFac;
					double cmplFac = 1.0 - fac;
					double tTexture = cmplFac * vtAspectNorth + fac * vtAspectSouth;
					double z = halfDepth - Depth * fac;

					for (int j = 0; j < lonsp1; ++j)
					{
						int jMod = j % Longitudes;
						double sTexture = sTextureCache[j];
						Vector2f tc = thetaCartesian[jMod];
						Vector2f rtc = rhoThetaCartesian[jMod];

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
			int longs6 = Longitudes * 2;
			int hemiLons = halfLatsn1 * longs6;

			int triOffsetNorthHemi = Longitudes;
			int triOffsetCylinder = (triOffsetNorthHemi + hemiLons);
			int triOffsetSouthHemi = (triOffsetCylinder + ringsp1 * longs6);
			int triOffsetSouthCap = (triOffsetSouthHemi + hemiLons);

			int fsLen = triOffsetSouthCap + Longitudes;
			triangles = new IndexArray3i(fsLen);

			// Polar caps.
			for (int i = 0, k = 0, m = triOffsetSouthCap; i < Longitudes; ++i, k++, m++)
			{
				// North.
				triangles.Set(k, i, vertOffsetNorthHemi + i, vertOffsetNorthHemi + i + 1);

				// South.
				triangles.Set(m, vertOffsetSouthCap + i, vertOffsetSouthPolar + i + 1, vertOffsetSouthPolar + i);

			}

			// Hemispheres.
			for (int i = 0, k = triOffsetNorthHemi, m = triOffsetSouthHemi; i < halfLatsn1; ++i)
			{
				int iLonsp1 = i * lonsp1;

				int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
				int vertNextLatNorth = vertCurrLatNorth + lonsp1;

				int vertCurrLatSouth = vertOffsetSouthEquator + iLonsp1;
				int vertNextLatSouth = vertCurrLatSouth + lonsp1;

				for (int j = 0; j < Longitudes; ++j, k += 2, m += 2)
				{
					// North.
					int north00 = vertCurrLatNorth + j;
					int north01 = vertNextLatNorth + j;
					int north11 = vertNextLatNorth + j + 1;
					int north10 = vertCurrLatNorth + j + 1;

					triangles.Set(k, north00, north11, north10);
					triangles.Set(k + 1, north00, north01, north11);

					// South.
					int south00 = vertCurrLatSouth + j;
					int south01 = vertNextLatSouth + j;
					int south11 = vertNextLatSouth + j + 1;
					int south10 = vertCurrLatSouth + j + 1;


					triangles.Set(m, south00, south11, south10);
					triangles.Set(m + 1, south00, south01, south11);

				}
			}

			// Cylinder.
			for (int i = 0, k = triOffsetCylinder; i < ringsp1; ++i)
			{
				int vertCurrLat = vertOffsetNorthEquator + i * lonsp1;
				int vertNextLat = vertCurrLat + lonsp1;

				for (int j = 0; j < Longitudes; ++j, k += 2)
				{
					int cy00 = vertCurrLat + j;
					int cy01 = vertNextLat + j;
					int cy11 = vertNextLat + j + 1;
					int cy10 = vertCurrLat + j + 1;

					triangles.Set(k, cy00, cy11, cy10);
					triangles.Set(k + 1, cy00, cy01, cy11);

				}
			}

			return this;
		}
	}

}
