﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PholioVisualisation.DataAccess;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataConstruction
{
    public class BenchmarkDataProvider
    {
        private IGroupDataReader _groupDataReader;

        public BenchmarkDataProvider(IGroupDataReader groupDataReader)
        {
            this._groupDataReader = groupDataReader;
        }
        /// <summary>
        /// Retrieve benchmark data from database or else calculate the average
        /// </summary>
        public CoreDataSet GetBenchmarkData(Grouping grouping, TimePeriod timePeriod,
            AverageCalculator averageCalculator, IArea parentArea)
        {
            var benchmarkData = GetDataFromDatabase(grouping, timePeriod, parentArea);

            if (benchmarkData == null && averageCalculator != null)
            {
                benchmarkData = averageCalculator.Average;
                AssignDataProperties(grouping, timePeriod, parentArea, benchmarkData);
            }

            if (benchmarkData == null)
            {
                benchmarkData = CoreDataSet.GetNullObject(parentArea.Code);
            }

            return benchmarkData;
        }

        /// <summary>
        /// Calculate the average
        /// </summary>
        public CoreDataSet CalculateBenchmarkDataAverage(Grouping grouping, TimePeriod timePeriod, AverageCalculator averageCalculator, IArea parentArea)
        {
            CoreDataSet benchmarkData = null;

            if (averageCalculator != null)
            {
                benchmarkData = averageCalculator.Average;
                AssignDataProperties(grouping, timePeriod, parentArea, benchmarkData);
            }

            if (benchmarkData == null)
            {
                benchmarkData = CoreDataSet.GetNullObject(parentArea.Code);
            }

            return benchmarkData;
        }

        private void SetAreaCode(IArea area, CoreDataSet data)
        {
            ICategoryArea categoryArea = area as ICategoryArea;
            if (categoryArea != null)
            {
                // Category area
                data.AreaCode = categoryArea.ParentAreaCode;
                data.CategoryTypeId = categoryArea.CategoryTypeId;
                data.CategoryId = categoryArea.CategoryId;
            }
            else
            {
                // Standard area
                data.AreaCode = area.Code;
                data.CategoryTypeId = CategoryTypeIds.Undefined;
                data.CategoryId = CategoryIds.Undefined;
            }
        }

        private void AssignDataProperties(Grouping grouping, TimePeriod timePeriod, IArea parentArea, CoreDataSet benchmarkData)
        {
            if (benchmarkData != null)
            {
                SetAreaCode(parentArea, benchmarkData);
                benchmarkData.AgeId = grouping.AgeId;
                benchmarkData.SexId = grouping.SexId;

                // Assign time properties
                benchmarkData.YearRange = timePeriod.YearRange;
                benchmarkData.Year = timePeriod.Year;
                benchmarkData.Quarter = timePeriod.Quarter;
                benchmarkData.Month = timePeriod.Month;
            }
        }

        private CoreDataSet GetDataFromDatabase(Grouping grouping, TimePeriod timePeriod, IArea parentArea)
        {
            CoreDataSet benchmarkData;

            var categoryArea = parentArea as ICategoryArea;
            if (categoryArea != null)
            {
                benchmarkData = _groupDataReader.GetCoreDataForCategoryArea(grouping, timePeriod, categoryArea);
            }
            else
            {
                IList<CoreDataSet> data = _groupDataReader.GetCoreData(grouping, timePeriod, parentArea.Code);
                CheckOnlyOneCoreDataSetFound(grouping, parentArea, data);
                benchmarkData = data.FirstOrDefault();
            }
            return benchmarkData;
        }

        private static void CheckOnlyOneCoreDataSetFound(Grouping grouping, IArea area,
            IList<CoreDataSet> dataList)
        {
            if (dataList.Count > 1)
            {
                throw new FingertipsException(string.Format("More than one CoreDataSet row for: IndicatorId={0} Area={1}",
                    grouping.IndicatorId, area.Code));
            }
        }
    }
}