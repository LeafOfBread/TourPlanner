﻿using Microsoft.EntityFrameworkCore;
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
using TourPlanner.Views;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.UI.HelperClasses;
using TourPlanner.UI.ViewModels;
using TourPlanner.BusinessLogic.Services;
using Microsoft.Web.WebView2.WinForms;

namespace TourPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        public MainWindow(ITourService tourService, TourLogService tourlogService, InputValidator validator, ConfigReader configReader, ApiHandler apiHandler)
        {
            InitializeComponent();

            _viewModel = new MainViewModel(tourService, tourlogService, validator, apiHandler, configReader);
            _viewModel.TourViewModel.SetWebView(TourMapView.Browser);
            DataContext = _viewModel;
        }

        public MainWindow() : this(App.ServiceProvider.GetRequiredService<TourService>(),
                                   App.ServiceProvider.GetRequiredService<TourLogService>(),
                                   App.ServiceProvider.GetRequiredService<InputValidator>(),
                                   App.ServiceProvider.GetRequiredService<ConfigReader>(),
                                   App.ServiceProvider.GetRequiredService<ApiHandler>())
        {
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                _ = vm.SearchViewModel.SearchForTours();
            }
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