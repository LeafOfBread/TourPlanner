﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourPlanner.DataAccess
{
    public class DataAccessException : Exception
    {
        public DataAccessException(string message, Exception inner = null)
            : base(message, inner) { }
    }
}
