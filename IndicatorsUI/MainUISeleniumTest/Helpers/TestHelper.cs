﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace IndicatorsUI.MainUISeleniumTest.Helpers
{
    public class TestHelper
    {
        public static void AssertTextContains(string text, string contained)
        {
            AssertTextContains(text, contained, string.Empty);
        }

        public static void AssertTextContains(string text, string contained, string messageOnTestFailure)
        {
            Assert.IsNotNull(text, "Text is null but was expected to contain: " + contained);
            Assert.IsTrue(text.ToLower().Contains(contained.ToLower()), messageOnTestFailure);
        }

        public static void AssertElementTextIsEqual(string expectedText, IWebElement webElement)
        {
            Assert.AreEqual(expectedText, webElement.Text);
        }
    }
}