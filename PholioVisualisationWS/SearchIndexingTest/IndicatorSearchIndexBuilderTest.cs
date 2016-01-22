﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.DataAccess;
using PholioVisualisation.SearchIndexing;

namespace SearchIndexingTest
{
    [TestClass]
    public class IndicatorSearchIndexBuilderTest
    {
        [ClassInitialize]
        public static void ExecuteOnceBeforeAnyTestMethods(TestContext testContext)
        {
            SearchIndexingTestHelper.GivenNoExistingIndexFiles(IndicatorSearchIndexBuilder.DirectoryIndicators);
        }

        [TestMethod]
        public void TestBuildPlacePostcodes()
        {
            var builder = new IndicatorSearchIndexBuilder(ApplicationConfiguration.SearchIndexDirectory);
            SearchIndexingTestHelper.BuildIndexesThenCheckIndexFolderExists(builder,
                IndicatorSearchIndexBuilder.DirectoryIndicators);
        }
    }
}