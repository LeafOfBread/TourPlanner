using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourPlanner.BusinessLogic
{
    public class SingleNotFoundException : Exception
    {
        public string EntityName { get; }
        public SingleNotFoundException(string entityName, string message = null)
            : base(message ?? $"{entityName} was not found.")
        {
            EntityName = entityName;
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message)
            : base(message) { }
    }

    public class InsertionException : Exception
    {
        public InsertionException(string message = "A insertion / edit / deletion error occured")
            : base(message) { }
    }

    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("Validation failed.")
        {
            Errors = errors;
        }
    }

    public class TourReportGenerationException : Exception
    {
        public TourReportGenerationException(string message, Exception inner = null)
            : base(message, inner) { }
    }

    public class ApiException : Exception
    {
        public ApiException(string message, Exception inner = null)
            : base(message, inner) { }
    }

    public class ImportExportException : Exception
    {
        public ImportExportException(string message)
            : base(message) { }
    }
}
