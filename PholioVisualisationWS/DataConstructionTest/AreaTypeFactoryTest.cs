﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.DataAccess;
using PholioVisualisation.DataConstruction;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataConstructionTest
{
    [TestClass]
    public class AreaTypeFactoryTest
    {
        [TestMethod]
        public void NewAreaType()
        {
            var id = AreaTypeIds.CombinedAuthorities;
            var areaType = AreaTypeFactory.New(ReaderFactory.GetAreasReader(), new ParentAreaGroup { ParentAreaTypeId = id });
            Assert.AreEqual(id, areaType.Id);
        }

        [TestMethod]
        public void NewCategoryAreaType()
        {
            var id = CategoryTypeIds.DeprivationDecileCountyAndUA2010;
            var areaType = AreaTypeFactory.New(ReaderFactory.GetAreasReader(), new ParentAreaGroup { CategoryTypeId = id });
            Assert.IsNotNull(areaType);
        }

        [TestMethod]
        public void NewAreaListAreaType()
        {
            var id = AreaTypeIds.AreaList;
            var areaType = AreaTypeFactory.New(ReaderFactory.GetAreasReader(), id);
            Assert.IsNotNull(areaType);
        }

        [TestMethod]
        public void NewCategoryAreaType_NameIsDefined()
        {
            var name = "decile";
            var id = CategoryTypeIds.DeprivationDecileCountyAndUA2010;
            var areaType = AreaTypeFactory.New(ReaderFactory.GetAreasReader(), new ParentAreaGroup { CategoryTypeId = id });

            Assert.IsTrue(areaType.Name.ToLower().Contains(name));
            Assert.IsTrue(areaType.ShortName.ToLower().Contains(name));
        }

        [TestMethod]
        public void NewAreaTypeFromCategoryAreaTypeId()
        {
            var areaTypeId = CategoryAreaType.GetAreaTypeIdFromCategoryTypeId(CategoryTypeIds.DeprivationDecileCountyAndUA2010);
            var areaType = AreaTypeFactory.New(ReaderFactory.GetAreasReader(), areaTypeId);
            Assert.AreEqual(areaTypeId, areaType.Id);
        }

        [TestMethod]
        public void NewAreaTypeFromStandardAreaTypeId()
        {
            var areaTypeId = AreaTypeIds.CcgsPostApr2019;
            var areaType = AreaTypeFactory.New(ReaderFactory.GetAreasReader(), areaTypeId);
            Assert.AreEqual(areaTypeId, areaType.Id);
        }
    }
}
