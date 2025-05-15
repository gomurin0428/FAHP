using Microsoft.VisualStudio.TestTools.UnitTesting;
using FAHP.Shared.Models;

namespace FAHPWebApp3.Tests
{
    [TestClass]
    public class TriangularFuzzyNumberTests
    {
        [TestMethod]
        public void Constructor_WithValidValues_SetsPropertiesCorrectly()
        {
            var tfn = new TriangularFuzzyNumber(1.0, 2.0, 3.0);
            
            Assert.AreEqual(1.0, tfn.L);
            Assert.AreEqual(2.0, tfn.M);
            Assert.AreEqual(3.0, tfn.U);
        }

        [TestMethod]
        public void Defuzzify_WithValidValues_ReturnsCorrectResult()
        {
            var tfn = new TriangularFuzzyNumber(1.0, 2.0, 3.0);
            
            double result = tfn.Defuzzify();
            
            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void One_StaticProperty_ReturnsCorrectValue()
        {
            var one = TriangularFuzzyNumber.One;
            
            Assert.AreEqual(1.0, one.L);
            Assert.AreEqual(1.0, one.M);
            Assert.AreEqual(1.0, one.U);
        }
    }
}
