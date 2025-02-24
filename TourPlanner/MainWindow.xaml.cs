using Microsoft.EntityFrameworkCore;
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
using TourPlannerClasses;
using TourPlannerClasses.DB;
using TourPlannerClasses.Tour;

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
            var options = new DbContextOptionsBuilder<TourDbContext>()
                          .UseNpgsql("Host=localhost;Port=5432;Database=TourDB;Username=postgres;Password=fhtw")
                          .Options;
            var dbContext = new TourDbContext(options);

            var tourService = new TourService(dbContext);
            DataContext = new TourViewModel(new TourService(dbContext));
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

        private void AddTour(object sender, RoutedEventArgs e)
        {
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
        }

        public bool CanExecute(object parameter) => _canExecute();

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}