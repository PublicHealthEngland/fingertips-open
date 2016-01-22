﻿
using Newtonsoft.Json;

namespace PholioVisualisation.PholioObjects
{
    public class ValueWithCIsData : ValueData
    {
        public ValueWithCIsData GetValueWithCIsData()
        {
            return new ValueWithCIsData
            {
                Value = Value,
                ValueFormatted = ValueFormatted,
                LowerCI = LowerCI,
                LowerCIF = LowerCIF,
                UpperCI = UpperCI,
                UpperCIF = UpperCIF,
                Count = Count
            };
        }

        [JsonProperty(PropertyName = "LoCI")]
        public double LowerCI { get; set; }

        [JsonProperty(PropertyName = "UpCI")]
        public double UpperCI { get; set; }

        [JsonProperty(PropertyName = "LoCIF")]
        public string LowerCIF { get; set; }

        [JsonProperty(PropertyName = "UpCIF")]
        public string UpperCIF { get; set; }

        /// <summary>
        /// Whether or not LowerCIF should be serialised by JSON.NET.
        /// </summary>
        public bool ShouldSerializeLowerCIF()
        {
            return ShouldSerializeLowerCI() && LowerCIF != null;
        }

        /// <summary>
        /// Whether or not UpperCIF should be serialised by JSON.NET.
        /// </summary>
        public bool ShouldSerializeUpperCIF()
        {
            return ShouldSerializeUpperCI() && UpperCIF != null;
        }

        /// <summary>
        /// Whether or not LowerCI should be serialised by JSON.NET.
        /// </summary>
        public bool ShouldSerializeLowerCI()
        {
            return LowerCI != NullValue;
        }

        /// <summary>
        /// Whether or not UpperCI should be serialised by JSON.NET.
        /// </summary>
        public bool ShouldSerializeUpperCI()
        {
            return UpperCI != NullValue;
        }

        [JsonIgnore]
        public bool AreCIsValid
        {
            get { return UpperCI != NullValue && LowerCI != NullValue; }
        }

        public static ValueWithCIsData GetNullObject()
        {
            ValueWithCIsData data = new ValueWithCIsData();
            SetValueWithCIsDataDefaults(data);
            return data;
        }

        protected static void SetValueWithCIsDataDefaults(ValueWithCIsData data)
        {
            data.Value = NullValue;
            data.LowerCI = NullValue;
            data.UpperCI = NullValue;
        }

        /// <summary>
        /// Parse list of values.
        /// </summary>
        /// <param name="valueListString">e.g. 1,3,4</param>
        public static ValueWithCIsData Parse(string valueListString)
        {
            string[] bits = valueListString.Split(',');

            ValueWithCIsData data = null;

            if (bits.Length == 1)
            {
                data = new ValueWithCIsData { Value = double.Parse(bits[0]) };
            }
            else if (bits.Length == 3)
            {
                data = GetNullObject();

                if (CanValueBeParsed(bits[0]))
                {
                    // No data
                }
                else if (CanValueBeParsed(bits[1]))
                {
                    // No CIs
                    data.Value = double.Parse(bits[0]);
                }
                else
                {
                    data.Value = double.Parse(bits[0]);
                    data.LowerCI = double.Parse(bits[1]);
                    data.UpperCI = double.Parse(bits[2]);
                }
            }

            return data;
        }

        private static bool CanValueBeParsed(string s)
        {
            return s == "-" || string.IsNullOrWhiteSpace(s);
        }
    }
}