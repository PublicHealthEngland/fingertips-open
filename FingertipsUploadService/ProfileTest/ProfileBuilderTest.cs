﻿using FingertipsUploadService.ProfileData;
using FingertipsUploadService.ProfileData.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProfileDataTest
{
    [TestClass]
    public class ProfileBuilderTest
    {
        private const string UrlKey = UrlKeys.HealthProfiles;
        private ProfileRepository _profileRepository;

        [TestInitialize]
        public void Init()
        {
            _profileRepository = new ProfileRepository();
        }


        [TestCleanup]
        public void CleanUp()
        {
            _profileRepository.Dispose();
        }

        [TestMethod]
        public void TestGetSelectedGroupingMetadata()
        {
            Profile profile = new ProfileBuilder(_profileRepository).Build(UrlKey, 1, AreaTypeIds.CountyAndUnitaryAuthority);
            Assert.AreEqual(profile.GetSelectedGroupingMetadata(1).GroupId, profile.GroupingMetadatas[0].GroupId);

            profile = new ProfileBuilder(_profileRepository).Build(UrlKey, 3, AreaTypeIds.CountyAndUnitaryAuthority);
            Assert.AreEqual(profile.GetSelectedGroupingMetadata(3).GroupId, profile.GroupingMetadatas[2].GroupId);

            // First is selected by default
            profile = new ProfileBuilder(_profileRepository).Build(UrlKey);
            Assert.AreEqual(profile.GetSelectedGroupingMetadata(1).GroupId, profile.GroupingMetadatas[0].GroupId);
        }

        [TestMethod]
        public void TestGroupingMetadatas()
        {
            Profile profile = new ProfileBuilder(_profileRepository).Build(UrlKey, 1, AreaTypeIds.CountyAndUnitaryAuthority);
            Assert.AreEqual(6, profile.GroupingMetadatas.Count);
        }

        [TestMethod]
        public void TestIndicatorNamesDefinedIfOnlyOneGrouping()
        {
            Profile profile = new ProfileBuilder(_profileRepository).Build(
                UrlKeys.Tobacco, 1, AreaTypeIds.CountyAndUnitaryAuthority);
            Assert.IsTrue(profile.IndicatorNames.Count > 0);
        }
    }
}