﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MainUISeleniumTest
{
    [TestClass]
    public class ProfilesAndIndicatorsTest : BaseUnitTest
    {
        [TestMethod]
        public void PageLoads()
        {
            var driver = Driver;
            new NavigateTo(driver).ProfilesAndIndicatorsPage();
            new WaitFor(driver).PageWithModalPopUpToLoad();
           
            // Is one table of indicators
            var indicatorTable = driver.FindElements(By.ClassName("grid"));
            Assert.AreEqual(1, indicatorTable.Count);
        }

        [TestMethod]
        public void CopyIndicators()
        {
            var driver = Driver;
            new NavigateTo(driver).ProfilesAndIndicatorsPage();
            new WaitFor(driver).PageWithModalPopUpToLoad();

            // Select option in menu
            var profileSelect = driver.FindElement(By.Id("selectedProfile"));
            var selectElement = new SelectElement(profileSelect);
            selectElement.SelectByValue("diabetes");

            // Tick an indicator to copy
            var tickBox = driver.FindElements(By.Name("90690_selected"));
            tickBox.First().Click();

            // Click copy
            var copyButton = driver.FindElement(By.Id("copy-indicators-button"));
            copyButton.Click();
            SeleniumHelper.WaitForExpectedElementToBeVisible(driver, By.Id("copyIndicators"));

            // Check the correct profile is selected in the profile menu
            var copyProfileSelect = driver.FindElement(By.Id("selectedProfileId"));
            var copySelectElement = new SelectElement(copyProfileSelect);
            var selectedText = copySelectElement.SelectedOption.Text;
            Assert.AreEqual("Diabetes", selectedText);
        }
    }
}