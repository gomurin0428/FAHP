using Microsoft.VisualStudio.TestTools.UnitTesting;
using FAHP.Shared.Models;
using System;

namespace FAHPWebApp3.Tests
{
    [TestClass]
    public class CrispTOPSISProcessorTests
    {
        [TestMethod]
        public void CalculateScores_WithSampleMatrix_ReturnsValidScores()
        {
            double[,] decisionMatrix = new double[,]
            {
                { 0.7, 0.3, 0.5 },
                { 0.3, 0.7, 0.5 }
            };
            double[] weights = new double[] { 0.5, 0.3, 0.2 };
            
            double[] scores = CrispTOPSISProcessor.CalculateScores(decisionMatrix, weights);
            
            Assert.AreEqual(2, scores.Length);
            Assert.IsTrue(scores[0] >= 0 && scores[0] <= 1);
            Assert.IsTrue(scores[1] >= 0 && scores[1] <= 1);
            Assert.IsTrue(scores[0] > scores[1]);
        }
    }
}
