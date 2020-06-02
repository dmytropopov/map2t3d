using map2t3d;
using map2t3d.Data.Obj;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToT3d.Tests
{
    public class TexCoordTests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region poly1
        private static Face poly1 = new Face
        {
            FaceVertices = new List<FaceVertex> {
                new FaceVertex {
                    Vertex =  new double[] { 90.0, -26.0, 0.0 },
                    Uv = new double[] { 0, 0 }
                },
                new FaceVertex {
                    Vertex =  new double[] { 94.0, -30.0, 0.0 },
                    Uv = new double[] { 0.024500000000000001, 0 }
                },
                new FaceVertex {
                    Vertex =  new double[] { 92.381399999999999, -30.0, 16.885700000000000 },
                    Uv = new double[] { 0.024500000000000001, 0.057500000000000002 }
                }
            }
        };
        private static double[] origin1 = { 129.05697870452533, 66.816326530612301, 18.354021739130442 };
        private static double[] textureU1 = { 1.5608292212076709, 1.5751707787923253, 0.14961524706981269 };
        private static double[] textureV1 = { 0.33271970934524098, -0.33271970934524076, -6.9420551045235772 };
        #endregion

        public static object[] AllCases = {
            new object[] { poly1, origin1, textureU1, textureV1 }
        };

        [Test]
        [TestCaseSource("AllCases")]
        public void Test1(Face poly, double[] origin, double[] textureU, double[] textureV)
        {
            // Arrange

            // Act
            var result = T3dWriter.CalculateTextureUV(poly);

            // Assert
            Assert.IsTrue(NearlyEqual(result.origin, origin));
            Assert.IsTrue(NearlyEqual(result.textureU, textureU));
            Assert.IsTrue(NearlyEqual(result.textureV, textureV));
        }

        public static bool NearlyEqual(double f1, double f2)
        {
            return Math.Abs(f1 - f2) < 0.000000001;
        }

        public static bool NearlyEqual(double[] f1, double[] f2)
        {
            for (int i = 0; i < f1.Length; i++)
            {
                if (!NearlyEqual(f1[i], f2[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}