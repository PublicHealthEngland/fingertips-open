﻿
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.Formatting
{
    public abstract class NumericFormatter
    {
        public const string NoValue = "-";

        protected delegate string FormatMethod(double val);
        protected FormatMethod formatMethod;

        protected delegate string FormatNullableMethod(double? val);
        protected FormatNullableMethod formatNullableMethod;

        protected Limits limits;
        protected IndicatorMetadata metadata;

        /// <summary>
        /// For Mock object creation
        /// </summary>
        protected NumericFormatter() { }

        internal NumericFormatter(IndicatorMetadata metadata, Limits limits)
        {
            this.metadata = metadata;
            this.limits = limits;
            SetFormatMethod();
        }

        public abstract IndicatorStatsPercentilesFormatted FormatStats(IndicatorStatsPercentiles stats);

        protected abstract void SetFormatMethod();

        public abstract void Format(ValueData data);

        public abstract void FormatConfidenceIntervals(ValueWithCIsData data);

        protected static IndicatorStatsPercentilesFormatted GetNullStats()
        {
            return new IndicatorStatsPercentilesFormatted
            {
                Min = NoValue,
                Max = NoValue,
                Median = NoValue,
                Percentile5 = NoValue,
                Percentile25 = NoValue,
                Percentile75 = NoValue,
                Percentile95 = NoValue
            };
        }

        public static string FormatZeroDP(double val)
        {
            return val == ValueData.NullValue
                ? NoValue
                : string.Format("{0:0}", val);
        }

        public static string Format1DP(double val)
        {
            return val == ValueData.NullValue
                ? NoValue
                : string.Format("{0:0.0}", val);
        }

        public static string Format2DP(double val)
        {
            return val == ValueData.NullValue
                ? NoValue
                : string.Format("{0:0.00}", val);
        }

        public static string Format3DP(double val)
        {
            return val == ValueData.NullValue
                ? NoValue
                : string.Format("{0:0.000}", val);
        }

        public static string FormatNullable2DP(double? val)
        {
            return val.HasValue
                ? string.Format("{0:0.00}", val)
                : NoValue;
        }

        public static string FormatNullable3DP(double? val)
        {
            return val.HasValue
                ? string.Format("{0:0.000}", val)
                : NoValue;
        }
    }
}
