﻿using PholioVisualisation.DataAccess;
using PholioVisualisation.DataAccess.Repositories;
using PholioVisualisation.ExceptionLogging;
using PholioVisualisation.PholioObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PholioVisualisation.DataConstruction
{
    public abstract class GroupDataBuilderBase
    {
        protected IAreasReader AreasReader = ReaderFactory.GetAreasReader();
        protected IGroupDataReader GroupDataReader = ReaderFactory.GetGroupDataReader();
        private CoreDataProcessor _coreDataProcessor = new CoreDataProcessor(null);
        private IDateChangeHelper _dateChangeHelper;
        private IProfileReader _profileReader;
        private ProfileConfig _profileConfig;

        public bool AssignAreas = true;
        public bool AssignChildAreaData = true;
        public bool AssignData = true;

        public ComparatorMap ComparatorMap;

        protected GroupData GroupData;
        protected IList<Grouping> Groupings;
        public int ProfileId;

        /// <summary>
        ///     The specific time point at which to assign core data and comparator data.
        /// </summary>
        public TimePeriod TimePeriodOfData;

        public GroupDataBuilderBase()
        {
            _dateChangeHelper = new DateChangeHelper(new MonthlyReleaseHelper(), new CoreDataAuditRepository(), new CoreDataSetRepository());
            _profileReader = ReaderFactory.GetProfileReader();
        }

        public GroupData Build()
        {
            GroupData = new GroupData();

            try
            {
                ReadGroupings(GroupDataReader);
                if (Groupings.Count > 0)
                {
                    if (AssignAreas)
                    {
                        GroupData.Areas = GetChildAreas();
                    }

                    IndicatorMetadataCollection indicatorMetadataCollection =
                        IndicatorMetadataProvider.Instance.GetIndicatorMetadataCollection(Groupings);

                    GroupData.InitIndicatorMetadata(indicatorMetadataCollection);

                    BuildGroupRoots();

                    if (AssignData)
                    {
                        AssignCoreData();
                    }

                    // Assign trend markers
                    _profileConfig = _profileReader.GetProfileConfig(ProfileId);

                    if (_profileConfig != null && _profileConfig.HasTrendMarkers)
                    {
                        AssignRecentTrends();
                    }

                    if (_profileConfig != null && ApplicationConfiguration.Instance.IsFeatureActive("recentlyChangedData"))
                    {
                        AssignDataChanges(_profileConfig.NewDataDeploymentCount);
                    }
                }
                else
                {
                    GroupData.Clear();
                }
            }
            catch (Exception ex)
            {
                ExceptionLog.LogException(ex, "");
            }
            return GroupData;
        }

        protected abstract void ReadGroupings(IGroupDataReader groupDataReader);
        protected abstract IList<IArea> GetChildAreas();

        private void BuildGroupRoots()
        {
            var rootBuilder = new GroupRootBuilder(ProfileId, GroupDataReader);
            GroupData.GroupRoots = rootBuilder.BuildGroupRoots(Groupings);
        }

        private void AssignCoreData()
        {
            string[] areaCodes = AssignChildAreaData
                ? GroupData.Areas.Select(x => x.Code).ToArray()
                : null;

            foreach (GroupRoot root in GroupData.GroupRoots)
            {
                Grouping firstGrouping = root.Grouping.FirstOrDefault();
                TimePeriod timePeriod = TimePeriodOfData ?? TimePeriod.GetDataPoint(firstGrouping);

                if (AssignChildAreaData)
                {
                    root.Data = GroupDataReader.GetCoreData(firstGrouping, timePeriod, areaCodes);
                    _coreDataProcessor.RemoveRedundantValueNotesForDataList(root.Data);
                }

                AssignComparatorData(root, timePeriod);
            }
        }

        private void AssignComparatorData(GroupRoot root, TimePeriod timePeriod)
        {
            IndicatorMetadata indicatorMetaData = GroupData.GetIndicatorMetadataById(root.IndicatorId);
            var benchmarkDataProvider = new BenchmarkDataProvider(GroupDataReader);

            // Comparator data
            foreach (Grouping grouping in root.Grouping)
            {
                ComparatorDetails comparatorDetails = ComparatorMap.GetComparatorById(grouping.ComparatorId, grouping.AreaTypeId);
                CoreDataSet data;
                if (comparatorDetails == null)
                {
                    data = CoreDataSet.GetNullObject(string.Empty);
                }
                else
                {
                    var comparatorArea = comparatorDetails.Area;

                    // Only get subnational data, do not want to prefetch England data as usually won't be needed
                    var dataList = grouping.ComparatorId == ComparatorIds.Subnational
                        ? root.Data
                        : null;

                    AverageCalculator averageCalculator = AverageCalculatorFactory.New(dataList, indicatorMetaData);

                    data = benchmarkDataProvider.GetBenchmarkData(grouping, timePeriod,
                            averageCalculator, comparatorArea);

                    // Calculate England data if not found in DB (this done after so do not have to read all data when necessary)
                    if (data.IsValueValid == false && grouping.ComparatorId == ComparatorIds.England)
                    {
                        dataList = GroupDataReader.GetCoreDataForAllAreasOfType(grouping, timePeriod);
                        averageCalculator = AverageCalculatorFactory.New(dataList, indicatorMetaData);
                        data = benchmarkDataProvider.GetBenchmarkData(grouping, timePeriod,
                            averageCalculator, comparatorArea);
                    }

                    _coreDataProcessor.RemoveRedundantValueNote(data);
                }
                grouping.ComparatorData = data;
            }
        }

        protected string GetParentAreaCode()
        {
            return (ComparatorMap.GetSubnationalComparator() ?? ComparatorMap.GetNationalComparator()).Area.Code;
        }

        protected IList<IArea> ReadChildAreas(string parentAreaCode, int childAreaTypeId)
        {
            return new FilteredChildAreaListProvider(AreasReader)
                .ReadChildAreas(parentAreaCode, ProfileId, childAreaTypeId);
        }

        private void AssignDataChanges(int newDataDeploymentCount)
        {
            var metadataCollection = new IndicatorMetadataCollection(GroupData.IndicatorMetadata);
            foreach (var groupRoot in GroupData.GroupRoots)
            {
                var indicatorMetadata = metadataCollection.GetIndicatorMetadataById(groupRoot.IndicatorId);

                groupRoot.DateChanges = _dateChangeHelper.GetIndicatorDateChange(indicatorMetadata,
                    newDataDeploymentCount, groupRoot.AreaTypeId);
            }
        }

        private void AssignRecentTrends()
        {
            if (GroupData.Areas != null)
            {
                var trendMarkersProvider = TrendMarkersProvider.New();
                var areas = GroupData.Areas.ToList();
                AddParentAreas(areas);

                var metadataCollection = new IndicatorMetadataCollection(GroupData.IndicatorMetadata);
                foreach (var groupRoot in GroupData.GroupRoots)
                {
                    var grouping = groupRoot.Grouping.FirstOrDefault();

                    var indicatorMetadata = metadataCollection.GetIndicatorMetadataById(groupRoot.IndicatorId);

                    var recentTrends = indicatorMetadata == null
                        ? new Dictionary<string, TrendMarkerResult>()
                        : trendMarkersProvider.GetTrendResults(areas, indicatorMetadata, grouping);

                    groupRoot.RecentTrends = recentTrends;
                }
            }
        }

        private void AddParentAreas(List<IArea> areas)
        {
            //Add the national and subnational areas in order to calculate the trends for these.
            var subnationalArea = ComparatorMap.GetSubnationalComparator().Area;
            var nationalComparator = ComparatorMap.GetNationalComparator();
            IArea nationalArea = null;
            if (nationalComparator != null)
            {
                nationalArea = nationalComparator.Area;
                areas.Add(nationalArea);
            }
            if (nationalComparator == null || nationalArea.Code != subnationalArea.Code)
            {
                areas.Add(subnationalArea);
            }
        }
    }
}