using Microsoft.VisualStudio.TestTools.UnitTesting;
using FAHP.Shared.Models;
using System;

namespace FAHPWebApp3.Tests
{
    [TestClass]
    public class FuzzyAHPProcessorTests
    {
        [TestMethod]
        public void ToTriangular_WithValidScale_ReturnsCorrectTFN()
        {
            var tfn1 = FuzzyAHPProcessor.ToTriangular(1);
            var tfn3 = FuzzyAHPProcessor.ToTriangular(3);
            var tfn5 = FuzzyAHPProcessor.ToTriangular(5);
            var tfn7 = FuzzyAHPProcessor.ToTriangular(7);
            var tfn9 = FuzzyAHPProcessor.ToTriangular(9);
            
            Assert.AreEqual(1.0, tfn1.M);
            Assert.AreEqual(3.0, tfn3.M);
            Assert.AreEqual(5.0, tfn5.M);
            Assert.AreEqual(7.0, tfn7.M);
            Assert.AreEqual(9.0, tfn9.M);
        }

        [TestMethod]
        public void Reciprocal_WithValidTFN_ReturnsCorrectReciprocal()
        {
            var tfn = new TriangularFuzzyNumber(2.0, 4.0, 6.0);
            
            var reciprocal = FuzzyAHPProcessor.Reciprocal(tfn);
            
            Assert.AreEqual(1.0/6.0, reciprocal.L);
            Assert.AreEqual(1.0/4.0, reciprocal.M);
            Assert.AreEqual(1.0/2.0, reciprocal.U);
        }

        [TestMethod]
        public void CalculateConsistencyRatio_WithIdentityMatrix_ReturnsZero()
        {
            int n = 3;
            var matrix = new TriangularFuzzyNumber[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = i == j ? TriangularFuzzyNumber.One : TriangularFuzzyNumber.One;
                }
            }
            
            double cr = FuzzyAHPProcessor.CalculateConsistencyRatio(matrix);
            
            Assert.AreEqual(0.0, cr, 0.001);
        }

        [TestMethod]
        public void CalculateWeights_WithIdentityMatrix_ReturnsEqualWeights()
        {
            int n = 3;
            var matrix = new TriangularFuzzyNumber[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = i == j ? TriangularFuzzyNumber.One : TriangularFuzzyNumber.One;
                }
            }
            
            double[] weights = FuzzyAHPProcessor.CalculateWeights(matrix);
            
            Assert.AreEqual(n, weights.Length);
            double expectedWeight = 1.0 / n;
            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(expectedWeight, weights[i], 0.001);
            }
        }
    }
}
