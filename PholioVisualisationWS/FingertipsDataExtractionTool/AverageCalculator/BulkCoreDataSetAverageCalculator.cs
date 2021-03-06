﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using PholioVisualisation.DataAccess;
using PholioVisualisation.DataAccess.Repositories;
using PholioVisualisation.DataConstruction;
using PholioVisualisation.DataSorting;
using PholioVisualisation.PholioObjects;

namespace FingertipsDataExtractionTool.AverageCalculator
{
    public interface IBulkCoreDataSetAverageCalculator
    {
        void Calculate(IEnumerable<Grouping> groupings, AverageCalculationConfig config);
    }

    public class BulkCoreDataSetAverageCalculator : IBulkCoreDataSetAverageCalculator
    {
        private const int UndefinedIndicatorId = -1;

        // Dependencies
        private IAreasReader _areasReader;
        private IGroupDataReader _groupDataReader;
        private ICoreDataSetRepository _coreDataSetRepository;
        private IParentAreaProvider _parentAreaProvider;
        private ILogger _logger;
        private IIndicatorMetadataRepository _indicatorMetadataRepository;

        // Instance variables
        private int _valueCount;
        private int _currentIndicatorId = UndefinedIndicatorId;
        private FileTimer _fileTimer;

        public BulkCoreDataSetAverageCalculator(IAreasReader areasReader, IParentAreaProvider parentAreaProvider,
            IGroupDataReader groupDataReader, ICoreDataSetRepository coreDataSetRepository, ILogger logger,
            IIndicatorMetadataRepository indicatorMetadataRepository)
        {
            _areasReader = areasReader;
            _parentAreaProvider = parentAreaProvider;
            _groupDataReader = groupDataReader;
            _coreDataSetRepository = coreDataSetRepository;
            _logger = logger;
            _indicatorMetadataRepository = indicatorMetadataRepository;
        }

        /// <summary>
        /// Calculate core data for all possible parent areas that could be required for the groupings.
        /// </summary>
        /// <param name="groupings"></param>
        public void Calculate(IEnumerable<Grouping> groupings, AverageCalculationConfig config)
        {
            groupings = FilterGroupings(groupings, config);

            var coreDataSetProviderFactory = new CoreDataSetProviderFactory();
            var indicatorMetadataProvider = IndicatorMetadataProvider.Instance;

            foreach (var grouping in groupings)
            {
                var indicatorMetadata = indicatorMetadataProvider.GetIndicatorMetadata(grouping);
                var timePeriods = grouping.GetTimePeriodIterator(indicatorMetadata.YearType).TimePeriods;
                var areas = _parentAreaProvider.GetParentAreas(grouping.AreaTypeId);

                LogIndicatorInformation(indicatorMetadata);

                foreach (var area in areas)
                {
                    try
                    {
                        SaveDataIfRequired(timePeriods, coreDataSetProviderFactory, area,
                            grouping, indicatorMetadata);
                    }
                    catch (Exception ex)
                    {
                        // Frequently thrown when run on command line, usually because DB is not accessible temporarily
                        WriteException(ex, indicatorMetadata, area);
                        WaitIfRequired();
                    }
                }
            }
        }

        /// <summary>
        /// Assigns properties to the core data set according to the area. 
        /// </summary>
        public static void AssignProperties(CoreDataSet coreData, IArea parentArea)
        {
            coreData.ValueNoteId = ValueNoteIds.ValueAggregatedFromAllKnownGeographyValuesByFingertips;

            var categoryArea = parentArea as CategoryArea;
            if (categoryArea != null)
            {
                coreData.CategoryId = categoryArea.CategoryId;
                coreData.CategoryTypeId = categoryArea.CategoryTypeId;

                // Categorical data only ever calculated within England
                coreData.AreaCode = AreaCodes.England;
            }
            else
            {
                coreData.CategoryId = CategoryIds.Undefined;
                coreData.CategoryTypeId = CategoryTypeIds.Undefined;
                coreData.AreaCode = parentArea.Code;
            }
        }

        /// <summary>
        /// Filter groupings to only contain those for which indicators have changed in the 
        /// last number of specified days
        /// </summary>
        private IEnumerable<Grouping> FilterGroupings(IEnumerable<Grouping> groupings, AverageCalculationConfig config)
        {
            groupings = groupings.OrderBy(x => x.IndicatorId);

            // No filtering required if doing all indicators
            if (config.CalculateAveragesWhetherOrNotIndicatorsHaveChanged == false)
            {
                // Get indicator IDs that have changed in recent days
                var date = DateTime.Now.Date.AddDays(-config.DaysToCheckForChangedIndicators);
                var indicatorIds = _indicatorMetadataRepository
                    .GetIndicatorsDeletedSinceDate(date).ToList();
                indicatorIds.AddRange(_indicatorMetadataRepository.GetIndicatorsUploadedSinceDate(date));
                indicatorIds = indicatorIds.Distinct().ToList();

                // Only use groupings for indicators that have changed
                groupings = new GroupingListFilter(groupings).SelectForIndicatorIds(indicatorIds);
            }

            // Filter to start at a certain indicator
            var indicatorIdToStartFrom = config.IndicatorIdToStartFrom;
            groupings = groupings.Where(x => x.IndicatorId >= indicatorIdToStartFrom);

            return groupings;
        }

        /// <summary>
        /// If required the data for each time period is calculated and saved.
        /// </summary>
        private void SaveDataIfRequired(IList<TimePeriod> timePeriods, CoreDataSetProviderFactory coreDataSetProviderFactory,
            IArea parentArea, Grouping grouping, IndicatorMetadata indicatorMetadata)
        {
            foreach (var timePeriod in timePeriods)
            {
                var coreDataSetProvider = coreDataSetProviderFactory.New(parentArea);
                var coreData = coreDataSetProvider.GetData(grouping, timePeriod, indicatorMetadata);

                if (coreData == null)
                {
                    // Get parent core data set
                    var childAreaListBuilder = new ChildAreaListBuilder(_areasReader);
                    coreData = new SubnationalAreaAverageCalculator(_groupDataReader, childAreaListBuilder)
                        .CalculateAverage(grouping, timePeriod, indicatorMetadata, parentArea);

                    if (coreData == null)
                    {
                        coreData = CoreDataSet.GetNullObject();
                    }

                    coreData.CopyValues(grouping, timePeriod);
                    AssignProperties(coreData, parentArea);

                    _valueCount++;
                    _coreDataSetRepository.Save(coreData);
                }
            }
        }

        /// <summary>
        /// Log when starting to process an indicator and then the number of values that
        /// were calculated.
        /// </summary>
        private void LogIndicatorInformation(IndicatorMetadata indicatorMetadata)
        {
            if (indicatorMetadata.IndicatorId != _currentIndicatorId)
            {
                // Current indicator finished
                if (_currentIndicatorId != UndefinedIndicatorId)
                {
                    _logger.Info(_valueCount + " values added for " + _currentIndicatorId);
                    if (_fileTimer != null)
                    {
                        _fileTimer.Stop();
                    }
                }

                // New indicator
                _valueCount = 0;
                _currentIndicatorId = indicatorMetadata.IndicatorId;
                _logger.Info(string.Format("# Starting indicator {0} {1}", _currentIndicatorId,
                    indicatorMetadata.Name));
                _fileTimer = new FileTimer(_logger);
            }
        }

        /// <summary>
        /// Writes to command line for logging when app is being run this way
        /// </summary>
        private static void WriteException(Exception ex, IndicatorMetadata metadata, IArea area)
        {
            Console.WriteLine(DateTime.Now.ToLongDateString() + " " + metadata.IndicatorId + " - " + area.Code + ":" + area.Name);
            Console.WriteLine("Exception message: " + ex.Message);
            Console.WriteLine("Exception type: " + ex.GetType().FullName);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("");
        }

        /// <summary>
        /// Wait before continuing
        /// </summary>
        private static void WaitIfRequired()
        {
            var now = DateTime.Now;
            if (now.Hour == 4 && now.Minute >= 30)
            {
                // Wait 5 mins while DB is backed up
                Thread.Sleep(5 * 60 * 1000);
            }
            else
            {
                // Wait few seconds
                Thread.Sleep(3 * 1000);
            }
        }
    }
}