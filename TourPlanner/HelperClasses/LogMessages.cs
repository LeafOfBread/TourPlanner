using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourPlanner.UI.HelperClasses
{
    public static class LogMessages
    {
        // View navigation
        public static string NavigatedTo(string viewName) =>
            $"Navigated to {viewName}.";

        public static string NavigationFailed(string viewName, string reason) =>
            $"Failed to navigate to {viewName}: {reason}";

        // Data loading
        public static string LoadedTours(int count) =>
            $"Loaded {count} tours from database.";

        public static string LoadedTourLogs(int count) =>
            $"Loaded {count} tour logs from database.";

        public static string DataLoadException(string context, Exception ex) =>
            $"Exception during {context}: {ex.Message}";

        // Validation / UI
        public static string NullSelection(string entity) =>
            $"Expected selection was null: {entity}";

        // Generic
        public static string UnexpectedError(string action, Exception ex) =>
            $"Unexpected error during {action}: {ex.Message}";
    }
}
