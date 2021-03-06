﻿using System;

namespace FingertipsUploadService.ProfileData
{
    public class FpmException : Exception
    {
        public FpmException(string message)
            : base(message)
        {
        }

        public FpmException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}