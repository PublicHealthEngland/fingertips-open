﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.DataAccess;
using PholioVisualisation.DataConstruction;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataConstructionTest
{
    [TestClass]
    public class IndicatorMetadataCollectionTest
    {
        [TestMethod]
        public void TestInitIndicatorMetadata_For_Collection()
        {
            var indicatorId1 = IndicatorIds.LifeExpectancyAtBirth;
            var indicatorId2 = IndicatorIds.ExcessWinterDeaths;

            List<Grouping> grouping = new List<Grouping>{
                new Grouping { IndicatorId = indicatorId1},
                new Grouping { IndicatorId = indicatorId2}
            };

            IndicatorMetadataCollection collection = IndicatorMetadataProvider.Instance.GetIndicatorMetadataCollection(grouping);
            Assert.AreEqual(2, collection.IndicatorMetadata.Count);

            IndicatorMetadata metadata = collection.GetIndicatorMetadataById(indicatorId1);

            Assert.IsNotNull(metadata.Name);
            Assert.IsNotNull(collection.GetIndicatorMetadataById(indicatorId2).Name);
            Assert.IsNull(collection.GetIndicatorMetadataById(IndicatorIds.ObesityYear6));
        }

        [TestMethod]
        public void TestDuplicateIndicatorIdsIgnored()
        {
            IndicatorMetadataCollection collection = new IndicatorMetadataCollection(new List<IndicatorMetadata>{
                new IndicatorMetadata { IndicatorId = 1 },
                new IndicatorMetadata{IndicatorId = 1}
            });
            Assert.AreEqual(1, collection.IndicatorMetadata.Count);
        }

        [TestMethod]
        public void TestAddIndicatorMetadataList()
        {
            IndicatorMetadataCollection collection = new IndicatorMetadataCollection(new List<IndicatorMetadata>());
            Assert.AreEqual(0, collection.IndicatorMetadata.Count);

            // Add metadata
            collection.AddIndicatorMetadata(new List<IndicatorMetadata>{
                new IndicatorMetadata{IndicatorId = 1}
            });

            Assert.AreEqual(1, collection.IndicatorMetadata.Count);
        }
    }
}
