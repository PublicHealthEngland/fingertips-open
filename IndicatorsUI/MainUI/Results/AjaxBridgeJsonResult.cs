﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IndicatorsUI.MainUI.Results
{
    public class AjaxBridgeJsonResult : JsonResult
    {
        private byte[] jsonBytes;

        public AjaxBridgeJsonResult(byte[] jsonBytes)
        {
            this.jsonBytes = jsonBytes;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/json; charset=utf-8";
            response.BinaryWrite(jsonBytes);
        }
    }
}