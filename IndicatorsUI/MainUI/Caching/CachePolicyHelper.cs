﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Profiles.MainUI.Caching
{
    public class CachePolicyHelper
    {
        public static DateTime Midnight
        {
            get { return DateTime.UtcNow.AddDays(1).Date; }
        }

        public static void CacheForOneMonth(HttpCachePolicyBase cache)
        {
            cache.SetExpires(DateTime.Now.AddMonths(1));
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetValidUntilExpires(true);
        }
    }
}