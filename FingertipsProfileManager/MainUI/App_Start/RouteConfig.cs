﻿using System;
using System.Web.Mvc;
using System.Web.Routing;
using Fpm.MainUI.Routes;

namespace Fpm.MainUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @".*?favicon.ico" });
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");

            CoreRoutes.RegisterRoutes(routes);

            routes.MapMvcAttributeRoutes();

            routes.MapRoute("NotFound", "{*url}",
                new { controller = "Error", action = "Http404" });

            
        }
    }
}