using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TourPlanner.UI.HelperClasses
{
    public static class ListViewHelper
    {
        public static void AdjustColumnsWidth(ListView listview)
        {
            if (listview.View is GridView gridview && gridview.Columns.Count > 0)
            {
                double columnWidth = (listview.ActualWidth - SystemParameters.VerticalScrollBarWidth) / gridview.Columns.Count;
                foreach(var column in gridview.Columns)
                {
                    column.Width = columnWidth;
                }
            }
        }
    }
}
