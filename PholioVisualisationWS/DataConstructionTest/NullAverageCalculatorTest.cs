﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.DataConstruction;

namespace PholioVisualisation.DataConstructionTest
{
    [TestClass]
    public class NullAverageCalculatorTest
    {
        [TestMethod]
        public void TestAverageIsNull()
        {
            Assert.IsNull(new NullAverageCalculator().Average);
        }
    }
}
