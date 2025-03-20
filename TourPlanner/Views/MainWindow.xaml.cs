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
using TourPlannerClasses.Models;
using TourPlannerClasses.Tour;
using TourPlanner.ViewModels;
using TourPlanner.Views;
using TourPlannerClasses.Services;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.HelperClasses;

namespace TourPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TourViewModel _viewModel;
        public MainWindow(TourService tourService, TourLogService tourlogService, InputValidator validator)
        {
            InitializeComponent();
            _viewModel = new TourViewModel(tourService, tourlogService, validator);
            DataContext = _viewModel;
        }

        public MainWindow() : this(App.ServiceProvider.GetRequiredService<TourService>(),
                                   App.ServiceProvider.GetRequiredService<TourLogService>(),
                                   App.ServiceProvider.GetRequiredService<InputValidator>())
        {
        }

        private void AddTourButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowAddTourView();
        }

        private void BackToTourList_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowTourListView();
            _viewModel.ShowTourlogView();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SetSearchBoxPlaceholder(string.Empty, Brushes.Black);
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SetSearchBoxPlaceholder("Search", Brushes.Gray);
            }
        }

        private void SetSearchBoxPlaceholder(string text, Brush color)
        {
            SearchTextBox.Text = text;
            SearchTextBox.Foreground = color;
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