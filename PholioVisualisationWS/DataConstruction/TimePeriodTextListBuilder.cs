﻿using System;
using System.Collections.Generic;
using System.Linq;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataConstruction
{
    public class TimePeriodTextListBuilder : TimePeriodTextFormatter, ITimePeriodTextListBuilder
    {
        private List<string> periods = new List<string>();

        public TimePeriodTextListBuilder(IndicatorMetadata metadata) : base(metadata)
        {
        }

        public void Add(TimePeriod timePeriod)
        {
            periods.Add(Format(timePeriod));
        }

        public void AddRange(IEnumerable<TimePeriod> timePeriods)
        {
            foreach (var period in timePeriods)
            {
                Add(period);
            }
        }

        public IList<string> ToList()
        {
            return periods;
        }

    }
}