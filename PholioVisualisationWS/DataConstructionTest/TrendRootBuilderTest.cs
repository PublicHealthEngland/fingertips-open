﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.DataAccess;
using PholioVisualisation.DataConstruction;
using PholioVisualisation.PholioObjects;

namespace DataConstructionTest
{
    [TestClass]
    public class TrendRootBuilderTest
    {

        [TestMethod]
        public void TestBuild()
        {
            var areaTypeId = AreaTypeIds.CountyAndUnitaryAuthority;
            var profileId = ProfileIds.Phof;
            var profile = ReaderFactory.GetProfileReader().GetProfile(profileId);

            var parentArea = new ParentArea(AreaCodes.Gor_NorthEast, areaTypeId);
            ComparatorMap comparatorMap = new ComparatorMapBuilder(parentArea).ComparatorMap;

            GroupData data = new GroupDataBuilderByGroupings
            {
                GroupId = profile.GroupIds[0],
                ChildAreaTypeId = areaTypeId,
                ProfileId = profile.Id,
                ComparatorMap = comparatorMap,
                AssignData = true
            }.Build();

            IList<TrendRoot> trendRoots = new TrendRootBuilder().Build(data.GroupRoots, comparatorMap, areaTypeId, profileId,
                data.IndicatorMetadata, false);
            Assert.IsTrue(trendRoots.Count > 0);
            Assert.IsTrue(trendRoots[0].Periods.Count > 0);
            Assert.IsNotNull(trendRoots[0].Limits);
            Assert.IsTrue(trendRoots[0].ComparatorValueFs.Count > 0);

            string s = trendRoots[0].ComparatorValueFs[0][ComparatorIds.Subnational];
            Assert.AreNotEqual("-", s);
        }

        [TestMethod]
        public void TestDiabetesPrevAndRiskHighestQuintileExists()
        {
            var areaTypeId = AreaTypeIds.Ccg;
            var profileId = ProfileIds.Diabetes;

            var parentArea = new ParentArea(AreaCodes.CommissioningRegionLondon, areaTypeId);
            ComparatorMap comparatorMap = new ComparatorMapBuilder(parentArea).ComparatorMap;

            GroupData data = new GroupDataBuilderByGroupings
            {
                GroupId = GroupIds.Diabetes_PrevalenceAndRisk,
                ChildAreaTypeId = areaTypeId,
                ProfileId = ProfileIds.Diabetes,
                ComparatorMap = comparatorMap,
                AssignData = true
            }.Build();

            IList<TrendRoot> trendRoots = new TrendRootBuilder().Build(data.GroupRoots, comparatorMap, areaTypeId, profileId,
                data.IndicatorMetadata, false);

            var highestQuintileCount = trendRoots.Select(x => x.DataPoints.Values.FirstOrDefault()[0].Significance).Count(significances => significances.ContainsValue((Significance)5));
            
            Assert.AreNotEqual(highestQuintileCount, 0);

        }

        [TestMethod]
        [Ignore]
        public void Build_Returns_Results_With_Trends()
        {
            var comparatorMap = GetComparatorMap(ComparatorMapBuilderTest.GetRegion102());

            var data = new GroupDataBuilderByGroupings
            {
                GroupId = GroupIds.Phof_WiderDeterminantsOfHealth,
                ProfileId = ProfileIds.Phof,
                ComparatorMap = comparatorMap,
                ChildAreaTypeId = AreaTypeIds.CountyAndUnitaryAuthority
            }.Build();

            var trendRoots = new TrendRootBuilder().Build(data.GroupRoots, comparatorMap,
                AreaTypeIds.CountyAndUnitaryAuthority, ProfileIds.Phof,
               data.IndicatorMetadata, false);

            Assert.IsTrue(trendRoots.Any() 
                && trendRoots.FirstOrDefault() != null
                && trendRoots.FirstOrDefault().TrendMarkers != null
                && trendRoots.FirstOrDefault().TrendMarkers.Count > 0
                );
        }

        private static ComparatorMap GetComparatorMap(ParentArea parentArea)
        {
            return new ComparatorMapBuilder(parentArea).ComparatorMap;
        }
    }
}