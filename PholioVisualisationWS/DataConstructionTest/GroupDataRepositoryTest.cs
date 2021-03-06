﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.DataConstruction;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataConstructionTest
{
    [TestClass]
    public class GroupDataRepositoryTest
    {
        [TestMethod]
        public void TestGetGroupDataProcessed_ForCategoryArea()
        {
            var areaCode = CategoryArea.New(CategoryTypeIds.DeprivationDecileCountyAndUA2010, 1).Code;

            var groupData = new GroupDataAtDataPointRepository().GetGroupDataProcessed(areaCode,
                AreaTypeIds.CountyAndUnitaryAuthorityPreApr2019, ProfileIds.Phof,GroupIds.Phof_WiderDeterminantsOfHealth);
            Assert.IsNotNull(groupData.GroupRoots);
        }

        [TestMethod]
        public void TestGetGroupDataProcessed_ForNearestNeighbour()
        {
            var areaCode = NearestNeighbourArea.CreateAreaCode(
                NearestNeighbourTypeIds.Cipfa, AreaCodes.CountyUa_Cambridgeshire);

            var groupData = new GroupDataAtDataPointRepository().GetGroupDataProcessed(areaCode,
                AreaTypeIds.CountyAndUnitaryAuthorityPreApr2019, ProfileIds.Phof, GroupIds.Phof_WiderDeterminantsOfHealth);

            // Check group roots are defined
            var groupRoots = groupData.GroupRoots;
            Assert.IsNotNull(groupRoots);

            // Check expected number of data points
            var count = groupRoots[0].Data.Count;
            Assert.IsTrue(count > 10 && count < 20);
        }
    }
}
