using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace g3
{
    public class CapsuleGenerator : MeshGenerator
    {
        public enum UvProfile : int
        {
            Fixed = 0,
            Aspect = 1,
            Uniform = 2
        }

        string folderPath = "Assets/Meshes/";
        string meshName = "Capsule";
        bool createInstance = true;

        int longitudes = 32;
        int latitudes = 16;
        int rings = 0;
        float depth = 1.0f;
        float radius = 0.5f;
        UvProfile profile = UvProfile.Aspect;

        public override MeshGenerator Generate()
        {

            bool calcMiddle = rings > 0;
            int halfLats = latitudes / 2;
            int halfLatsn1 = halfLats - 1;
            int halfLatsn2 = halfLats - 2;
            int ringsp1 = rings + 1;
            int lonsp1 = longitudes + 1;
            float halfDepth = depth * 0.5f;
            float summit = halfDepth + radius;

            // Vertex index offsets.
            int vertOffsetNorthHemi = longitudes;
            int vertOffsetNorthEquator = vertOffsetNorthHemi + lonsp1 * halfLatsn1;
            int vertOffsetCylinder = vertOffsetNorthEquator + lonsp1;
            int vertOffsetSouthEquator = calcMiddle ? vertOffsetCylinder + lonsp1 * rings : vertOffsetCylinder;
            int vertOffsetSouthHemi = vertOffsetSouthEquator + lonsp1;
            int vertOffsetSouthPolar = vertOffsetSouthHemi + lonsp1 * halfLatsn2;
            int vertOffsetSouthCap = vertOffsetSouthPolar + lonsp1;

            // Initialize arrays.
            int vertLen = vertOffsetSouthCap + longitudes;
            Vector3d[] vs = new Vector3d[vertLen];
            Vector2f[] vts = new Vector2f[vertLen];
            Vector3f[] vns = new Vector3f[vertLen];

            double toTheta = 2.0f * Math.PI / longitudes;
            double toPhi = Math.PI / latitudes;
            float toTexHorizontal = 1.0f / longitudes;
            float toTexVertical = 1.0f / halfLats;

            // Calculate positions for texture coordinates vertical.
            float vtAspectRatio = 1.0f;
            switch (profile)
            {
                case UvProfile.Aspect:
                    vtAspectRatio = radius / (depth + radius + radius);
                    break;

                case UvProfile.Uniform:
                    vtAspectRatio = (float) halfLats / (ringsp1 + latitudes);
                    break;

                case UvProfile.Fixed:
                default:
                    vtAspectRatio = 1.0f / 3.0f;
                    break;
            }

            float vtAspectNorth = 1.0f - vtAspectRatio;
            float vtAspectSouth = vtAspectRatio;

            Vector2f[] thetaCartesian = new Vector2f[longitudes];
            Vector2f[] rhoThetaCartesian = new Vector2f[longitudes];
            float[] sTextureCache = new float[lonsp1];

            // Polar vertices.
            for (int j = 0; j < longitudes; ++j)
            {
                float jf = j;
                float sTexturePolar = 1.0f - ((jf + 0.5f) * toTexHorizontal);
                double theta = jf * toTheta;

                double cosTheta = Math.Cos(theta);
                double sinTheta = Math.Sin(theta);

                thetaCartesian[j] = new Vector2f(cosTheta, sinTheta);
                rhoThetaCartesian[j] = new Vector2f(
                    radius * cosTheta,
                    radius * sinTheta);

                // North.
                vs[j] = new Vector3f(0.0f, summit, 0.0f);
                vts[j] = new Vector2f(sTexturePolar, 1.0f);
                vns[j] = new Vector3f(0.0f, 1.0f, 0f);

                // South.
                int idx = vertOffsetSouthCap + j;
                vs[idx] = new Vector3f(0.0f, -summit, 0.0f);
                vts[idx] = new Vector2f(sTexturePolar, 0.0f);
                vns[idx] = new Vector3f(0.0f, -1.0f, 0.0f);
            }

            // Equatorial vertices.
            for (int j = 0; j < lonsp1; ++j)
            {
                float sTexture = 1.0f - j * toTexHorizontal;
                sTextureCache[j] = sTexture;

                // Wrap to first element upon reaching last.
                int jMod = j % longitudes;
                Vector2f tc = thetaCartesian[jMod];
                Vector2f rtc = rhoThetaCartesian[jMod];

                // North equator.
                int idxn = vertOffsetNorthEquator + j;
                vs[idxn] = new Vector3f(rtc.x, halfDepth, -rtc.y);
                vts[idxn] = new Vector2f(sTexture, vtAspectNorth);
                vns[idxn] = new Vector3f(tc.x, 0.0f, -tc.y);

                // South equator.
                int idxs = vertOffsetSouthEquator + j;
                vs[idxs] = new Vector3f(rtc.x, -halfDepth, -rtc.y);
                vts[idxs] = new Vector2f(sTexture, vtAspectSouth);
                vns[idxs] = new Vector3f(tc.x, 0.0f, -tc.y);
            }

            // Hemisphere vertices.
            for (int i = 0; i < halfLatsn1; ++i)
            {
                float ip1f = i + 1.0f;
                double phi = ip1f * toPhi;

                // For coordinates.
                double cosPhiSouth = Math.Cos(phi);
                double sinPhiSouth = Math.Sin(phi);

                // Symmetrical hemispheres mean cosine and sine only need
                // to be calculated once.
                double cosPhiNorth = sinPhiSouth;
                double sinPhiNorth = -cosPhiSouth;

                double rhoCosPhiNorth = radius * cosPhiNorth;
                double rhoSinPhiNorth = radius * sinPhiNorth;
                double zOffsetNorth = halfDepth - rhoSinPhiNorth;

                double rhoCosPhiSouth = radius * cosPhiSouth;
                double rhoSinPhiSouth = radius * sinPhiSouth;
                double zOffsetSouth = -halfDepth - rhoSinPhiSouth;

                // For texture coordinates.
                float tTexFac = ip1f * toTexVertical;
                float cmplTexFac = 1.0f - tTexFac;
                float tTexNorth = cmplTexFac + vtAspectNorth * tTexFac;
                float tTexSouth = cmplTexFac * vtAspectSouth;

                int iLonsp1 = i * lonsp1;
                int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
                int vertCurrLatSouth = vertOffsetSouthHemi + iLonsp1;

                for (int j = 0; j < lonsp1; ++j)
                {
                    int jMod = j % longitudes;
                    float sTexture = sTextureCache[j];
                    Vector2f tc = thetaCartesian[jMod];

                    // North hemisphere.
                    int idxn = vertCurrLatNorth + j;
                    vs[idxn] = new Vector3f(
                        rhoCosPhiNorth * tc.x,
                        zOffsetNorth, // 
                        -rhoCosPhiNorth * tc.y);
                    vts[idxn] = new Vector2f(sTexture, tTexNorth);
                    vns[idxn] = new Vector3f(
                        cosPhiNorth * tc.x, //
                        -sinPhiNorth, //
                        -cosPhiNorth * tc.y);

                    // South hemisphere.
                    int idxs = vertCurrLatSouth + j;
                    vs[idxs] = new Vector3f(
                        rhoCosPhiSouth * tc.x,
                        zOffsetSouth, //
                        -rhoCosPhiSouth * tc.y);
                    vts[idxs] = new Vector2f(sTexture, tTexSouth);
                    vns[idxs] = new Vector3f(
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
                float toFac = 1.0f / ringsp1;
                int idxCylLat = vertOffsetCylinder;

                for (int h = 1; h < ringsp1; ++h)
                {
                    float fac = h * toFac;
                    float cmplFac = 1.0f - fac;
                    float tTexture = cmplFac * vtAspectNorth + fac * vtAspectSouth;
                    float z = halfDepth - depth * fac;

                    for (int j = 0; j < lonsp1; ++j)
                    {
                        int jMod = j % longitudes;
                        float sTexture = sTextureCache[j];
                        Vector2f tc = thetaCartesian[jMod];
                        Vector2f rtc = rhoThetaCartesian[jMod];

                        vs[idxCylLat] = new Vector3f(rtc.x, z, -rtc.y);
                        vts[idxCylLat] = new Vector2f(sTexture, tTexture);
                        vns[idxCylLat] = new Vector3f(tc.x, 0.0f, -tc.y);

                        ++idxCylLat;
                    }
                }
            }

            // Triangle indices.
            // Stride is 3 for polar triangles;
            // stride is 6 for two triangles forming a quad.
            int lons3 = longitudes * 3;
            int lons6 = longitudes * 6;
            int hemiLons = halfLatsn1 * lons6;

            int triOffsetNorthHemi = lons3;
            int triOffsetCylinder = (triOffsetNorthHemi + hemiLons);
            int triOffsetSouthHemi = (triOffsetCylinder + ringsp1 * lons6);
            int triOffsetSouthCap = (triOffsetSouthHemi + hemiLons);

            int fsLen = triOffsetSouthCap + lons3;
            triangles = new IndexArray3i(fsLen / 3);

            // Polar caps.
            for (int i = 0, k = 0, m = triOffsetSouthCap / 3; i < longitudes; ++i, k++, m++)
            {
                // North.
                triangles.Set(k, i, vertOffsetNorthHemi + i, vertOffsetNorthHemi + i + 1);

                // South.
                triangles.Set(m, vertOffsetSouthCap + i, vertOffsetSouthPolar + i + 1, vertOffsetSouthPolar + i);

            }

            // Hemispheres.
            for (int i = 0, k = triOffsetNorthHemi / 3, m = triOffsetSouthHemi / 3; i < halfLatsn1; ++i)
            {
                int iLonsp1 = i * lonsp1;

                int vertCurrLatNorth = vertOffsetNorthHemi + iLonsp1;
                int vertNextLatNorth = vertCurrLatNorth + lonsp1;

                int vertCurrLatSouth = vertOffsetSouthEquator + iLonsp1;
                int vertNextLatSouth = vertCurrLatSouth + lonsp1;

                for (int j = 0; j < longitudes; ++j, k += 2, m += 2)
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
            for (int i = 0, k = triOffsetCylinder / 3; i < ringsp1; ++i)
            {
                int vertCurrLat = vertOffsetNorthEquator + i * lonsp1;
                int vertNextLat = vertCurrLat + lonsp1;

                for (int j = 0; j < longitudes; ++j, k += 2)
                {
                    int cy00 = vertCurrLat + j;
                    int cy01 = vertNextLat + j;
                    int cy11 = vertNextLat + j + 1;
                    int cy10 = vertCurrLat + j + 1;

                    triangles.Set(k, cy00, cy11, cy10);
                    triangles.Set(k + 1, cy00, cy01, cy11);

                }
            }

            vertices = new VectorArray3d(vs.Length);
            uv = new VectorArray2f(vts.Length);
            normals = new VectorArray3f(vns.Length);


            for (int i = 0; i < vs.Length; i++)
            {
                vertices[i] = vs[i];
            }

            for (int i = 0; i < vts.Length; i++)
            {
                uv[i] = vts[i];
            }

            for (int i = 0; i < vns.Length; i++)
            {
                normals[i] = vns[i];
            }


            return this;
        }
    }

}
