using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TourPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            SearchTextBox.Foreground = Brushes.Black;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        private void AdjustColumnWidths()
        {
            if (TourListView.View is GridView gridView)
            {
                double totalWidth = TourListView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                int columnCount = gridView.Columns.Count;

                if (columnCount > 0)
                {
                    double columnWidth = totalWidth / columnCount; // Equal width for all
                    foreach (var column in gridView.Columns)
                    {
                        column.Width = columnWidth;
                    }
                }
            }
        }

        // Call this method when the window resizes
        private void TourListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustColumnWidths();
        }
    }
}