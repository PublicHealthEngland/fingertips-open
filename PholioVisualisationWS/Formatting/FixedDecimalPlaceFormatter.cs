﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.Formatting
{
    public class FixedDecimalPlaceFormatter : NumericFormatter
    {
        private string formatString;
        private int decimalPlacesToDisplay;

        public FixedDecimalPlaceFormatter(int decimalPlacesToDisplay) :
            base(null, null)
        {
            this.decimalPlacesToDisplay = decimalPlacesToDisplay;
            SetFormatString();
        }

        public override void Format(ValueData data)
        {
            if (data != null && data.HasFormattedValue == false)
            {
                data.ValueFormatted = FormatNumber(data.Value);
            }
        }

        public override void FormatConfidenceIntervals(ValueWithCIsData data)
        {
            if (data != null && data.HasFormattedCIs == false)
            {
                data.UpperCIF = FormatNumber(data.UpperCI);
                data.LowerCIF = FormatNumber(data.LowerCI);
            }
        }

        public override IndicatorStatsPercentilesFormatted FormatStats(IndicatorStatsPercentiles stats)
        {
            if (stats == null)
            {
                return GetNullStats();
            }
            return new IndicatorStatsPercentilesFormatted
            {
                Min = FormatNumber(stats.Min),
                Max = FormatNumber(stats.Max),
                Median = FormatNumber(stats.Median),
                Percentile5 = FormatNumber(stats.Percentile5),
                Percentile25 = FormatNumber(stats.Percentile25),
                Percentile75 = FormatNumber(stats.Percentile75),
                Percentile95 = FormatNumber(stats.Percentile95)
            };
        }

        private void SetFormatString()
        {
            StringBuilder sb = new StringBuilder("{0:0.");
            for (int i = 0; i < decimalPlacesToDisplay; i++)
            {
                sb.Append("0");
            }
            sb.Append("}");
            formatString = sb.ToString();
        }

        private string FormatNumber(double val)
        {
            if (val.Equals(ValueData.NullValue))
            {
                return NoValue;
            }

            return string.Format(formatString, val);
        }

        protected override void SetFormatMethod() { }
    }
}
