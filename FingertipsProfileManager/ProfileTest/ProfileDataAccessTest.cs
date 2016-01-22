﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fpm.ProfileData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProfileDataTest
{
    [TestClass]
    public class ProfileDataAccessTest
    {

        [TestInitialize]
        public void RunBeforeEachTest()
        {
        }
        

        [TestMethod]
        public void TestGetIndicatorAudit_MostRecentFirst()
        {
            var audits = ReaderFactory.GetProfilesReader()
                .GetIndicatorAudit(new List<int> {IndicatorIds.ObesityYear6})
                .ToList();
            MostRecentIsFirst(audits.First().Timestamp, audits.Last().Timestamp);
        }

        [TestMethod]
        public void TestGetContentAuditList_MostRecentFirst()
        {
            var audits = ReaderFactory.GetProfilesReader()
                .GetContentAuditList(new List<int> {17 /*PHOF introduction*/})
                .ToList();
            MostRecentIsFirst(audits.First().Timestamp, audits.Last().Timestamp);
        }

      

        public static void MostRecentIsFirst(DateTime first, DateTime last)
        {
            Assert.AreEqual(1, first.CompareTo(last));
        }

        
     }
}