﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IndicatorsUI.AngularAppBuildTest
{
    [TestClass]
    public class VerifyAngularApp
    {
        /// <summary>
        /// To check that the Angular app has been built by Jenkins
        /// </summary>
        [TestMethod]
        public void CheckAngularAppDistFolderHasBeenCreated()
        {
            // C:\<root>
            var path = string.Join(@"\", AssemblyDirectory.Split('\\').Take(2));
            path = Path.Combine(path, "IndicatorsUI/MainUI/angular-app-dist");
            Assert.IsTrue(Directory.Exists(path), path);
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}