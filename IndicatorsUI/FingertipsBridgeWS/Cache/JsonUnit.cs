﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FingertipsBridgeWS.Cache
{
    public class JsonUnit
    {
        public double DurationInMs;
        public string ServiceId;
        public string CacheKey;
        public byte[] Json;
        public string Url;

        public JsonUnit(string serviceId, string cacheKey, byte[] json, string url, double durationInMs)
            : this(serviceId, cacheKey, json)
        {
            Url = url;
            DurationInMs = durationInMs;
        }

        public JsonUnit(string serviceId, string cacheKey, byte[] json)
        {
            ServiceId = serviceId;
            CacheKey = cacheKey;
            Json = json;
        }

        public bool IsJsonOk()
        {
            return IsJsonOk(Json);
        }

        public static bool IsJsonOk(byte[] data)
        {
            return data != null && data.Length > 0;
        }
    }
}
