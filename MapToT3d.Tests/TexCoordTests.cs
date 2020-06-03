using map2t3d;
using map2t3d.Data.ObjData;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
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
        private static readonly Face poly1 = new Face
        {
            Material = new MaterialInfo
            {
                UTiling = 2,
                VTiling = 8,
                TextureSizeU = 256,
                TextureSizeV = 256
            },
            FaceVertices = new List<FaceVertex> {
                new FaceVertex {
                    Vertex =  new double[] { 90.0, 26.0, 0.0 },
                    Uv = new double[] { 0, 0 }
                },
                new FaceVertex {
                    Vertex =  new double[] { 94.0, 30.0, 0.0 },
                    Uv = new double[] { 0.024500000000000001, 0 }
                },
                new FaceVertex {
                    Vertex =  new double[] { 92.381399999999999, 30.0, 16.885700000000000 },
                    Uv = new double[] { 0.024500000000000001, 0.057500000000000002 }
                }
            }
        };
        private static readonly double[] origin1 = { 129.05697870452533, 66.816326530612301, 18.354021739130442 };
        private static readonly double[] textureU1 = { 1.5608292212076709, 1.5751707787923253, 0.14961524706981269 };
        private static readonly double[] textureV1 = { 0.33271970934524098, -0.33271970934524076, -6.9420551045235772 };
        #endregion
        #region poly2
        private static readonly Face poly2 = new Face
        {
            Material = new MaterialInfo {
                UTiling = 1,
                VTiling = 1,
                TextureSizeU = 256,
                TextureSizeV = 256
            },
            FaceVertices = new List<FaceVertex> {
                new FaceVertex {
                    Vertex =  new double[] { 272, 32, 64 },
                    Uv = new double[] { -0.12500000000000000, 4.9882999999999997 }
                },
                new FaceVertex {
                    Vertex =  new double[] { 272, -16, 64 },
                    Uv = new double[] { -0.87500000000000000, 4.9882999999999997 }
                },
                new FaceVertex {
                    Vertex =  new double[] { 208, -16, 64 },
                    Uv = new double[] { -0.87500000000000000, 3.9883000000000002 }
                }
            }
        };
        private static readonly double[] origin2 = { 208.74879999999999, 40.000000000000007, 64.000000000000000 };
        private static readonly double[] textureU2 = { -0.00000000000000000, 4.0000000000000000, 0.00000000000000000 };
        private static readonly double[] textureV2 = { -3.9999999999999982, 0.00000000000000000, 0.00000000000000000 };
        #endregion

        public class UvTestCasesFactory
        {
            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(poly1, origin1, textureU1, textureV1).SetName("poly1");
                    yield return new TestCaseData(poly2, origin2, textureU2, textureV2).SetName("poly2");
                }
            }
        }

        //[Test]
        //[TestCaseSource("AllCases")]
        [TestCaseSource(typeof(UvTestCasesFactory), "TestCases")]
        public void UvTest(Face poly, double[] origin, double[] textureU, double[] textureV)
        {
            // Arrange

            // Act
            var result = UvConverter.CalculateTextureUV(poly: poly);

            // Assert
            Assert.IsTrue(NearlyEqual(result.origin, origin));
            Assert.IsTrue(NearlyEqual(result.textureU, textureU));
            Assert.IsTrue(NearlyEqual(result.textureV, textureV));
        }

        public static bool NearlyEqual(double f1, double f2)
        {
            return Math.Abs(f1 - f2) < 0.0000001;
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