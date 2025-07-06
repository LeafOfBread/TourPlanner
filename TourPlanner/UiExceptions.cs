using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourPlanner.UI
{
    public class UiException : Exception
    {
        public UiException(string message)
            : base(message)
        {
        }

        public UiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
    public class ViewInitializationException : UiException
    {
        public ViewInitializationException(string viewName, string message, Exception inner = null)
            : base($"View initialization failed for {viewName}: {message}", inner)
        {
        }
    }
}
