using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TourPlannerClasses.Models;

namespace TourPlanner.Views
{
    /// <summary>
    /// Interaction logic for TourLogsView.xaml
    /// </summary>
    public partial class TourLogsView : UserControl
    {
        public TourLogsView()
        {
            InitializeComponent();
        }

        public Tours SelectedTour
        {
            get { return (Tours)GetValue(SelectedTourProperty); }
            set { SetValue(SelectedTourProperty, value); }
        }

        public static readonly DependencyProperty SelectedTourProperty =
            DependencyProperty.Register("SelectedTour", typeof(Tours), typeof(TourLogsView), new PropertyMetadata(null, OnSelectedTourChanged));

        private static void OnSelectedTourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TourListView tourListView && e.NewValue is Tours newTour)
            {
                tourListView.TourList.ItemsSource = new List<Tours> { newTour };
            }
        }

        private void TourLogsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TourLogs.View is GridView gridView)
            {
                double totalWidth = TourLogs.ActualWidth - SystemParameters.VerticalScrollBarWidth; // Account for scrollbar
                double usedWidth = 0;

                // Sum up all column widths except "Comment"
                foreach (var column in gridView.Columns)
                {
                    if (column != CommentColumn)
                    {
                        usedWidth += column.Width;
                    }
                }

                // Assign remaining space to the Comment column
                double remainingWidth = totalWidth - usedWidth;
                if (remainingWidth > 0)
                {
                    CommentColumn.Width = remainingWidth;
                }
            }
        }
    }
}
