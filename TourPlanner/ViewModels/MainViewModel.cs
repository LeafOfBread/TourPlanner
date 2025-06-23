using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TourPlanner.BusinessLogic.Services;
using TourPlanner.UI.HelperClasses;
using TourPlanner.Views;
using TourPlannerClasses.Models;

namespace TourPlanner.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainViewModel));
        public TourViewModel TourViewModel { get; }
        public TourLogViewModel TourLogViewModel { get; }
        public SearchViewModel SearchViewModel { get; }

        public ICommand AddTourCommand { get; private set; }
        public ICommand EditTourViewCommand { get; private set; }
        public ICommand ShowAddLogViewCommand { get; private set; }
        public ICommand ShowEditLogViewCommand { get; private set; }
        public ICommand ShowHomeMenuCommand { get; private set; }

        public MainViewModel(ITourService tourService, TourLogService tourlogService, InputValidator validator)
        {
            TourViewModel = new TourViewModel(this, tourService, validator);
            TourLogViewModel = new TourLogViewModel(this, tourService, tourlogService, validator);
            SearchViewModel = new SearchViewModel(this, tourService);

            AddTourCommand = new RelayCommand(ShowAddTourView);
            EditTourViewCommand = new RelayCommand(() => ShowEditTourView());
            ShowAddLogViewCommand = new RelayCommand(() => ShowAddTourLog());
            ShowEditLogViewCommand = new RelayCommand(() => ShowEditTourLog());
            ShowHomeMenuCommand = new RelayCommand(() => ShowHomeMenu(tourService, tourlogService));

            _ = LoadDataAsync(tourService, tourlogService);

            _log.Info("Initialized MainViewModel");
        }


        private async Task LoadDataAsync(ITourService _tourService, TourLogService _tourlogService)
        {
            try
            {
                await LoadTourLogs(_tourlogService);
                await LoadTours(_tourService);
                _log.Info("Successfully loaded available Tours and Tourlogs");
            }
            catch(Exception ex)
            {
                _log.Error("Error loading Tours and Tourlogs", ex);
                throw;
            }
        }

        private async Task LoadTours(ITourService _tourService)
        {
            var tours = await _tourService.GetAllTours();
            TourViewModel.AllTours = new ObservableCollection<Tours>(tours);
            TourViewModel.MasterTours = new ObservableCollection<Tours>(tours);
        }

        private async Task LoadTourLogs(TourLogService _tourlogService)
        {
            var tourlogs = await _tourlogService.GetTourlogsAsync();
            TourLogViewModel.AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
        }

        public void ShowTourListView()
        {
            TourViewModel.CurrentTourView = new TourListView();
        }
        public void ShowAddTourView()
        {
            if (TourViewModel.SelectedTour != null)
            {
                TourViewModel.NewTour = new Tours();
                TourViewModel.CurrentTourView = new AddTourView();
            }
        }

        public void ShowTourlogView()
        {
            TourLogViewModel.CurrentLogView = new TourLogsView();
        }

        public void ShowEditTourView()
        {
            if (TourViewModel.SelectedTour != null)
            {
                TourViewModel.AddTourName = TourViewModel.SelectedTour.Name;
                TourViewModel.AddTourFrom = TourViewModel.SelectedTour.From;
                TourViewModel.AddTourTo = TourViewModel.SelectedTour.To;
                TourViewModel.AddTourDescription = TourViewModel.SelectedTour.Description;

                TourViewModel.CurrentTourView = new EditTourView();
            }
        }

        public void ShowAddTourLog()
        {
            TourLogViewModel.ClearInputs();
            if (TourViewModel.SelectedTour != null)
                TourLogViewModel.CurrentLogView = new AddTourLogView();
        }

        public void ShowEditTourLog()
        {
            if (TourViewModel.SelectedTour != null && TourLogViewModel.SelectedTourLog != null)
            {
                TourLogViewModel.AddAuthor = TourLogViewModel.SelectedTourLog.Author;
                TourLogViewModel.AddDate = TourLogViewModel.SelectedTourLog.Date;
                TourLogViewModel.AddDifficulty = TourLogViewModel.SelectedTourLog.Difficulty;
                TourLogViewModel.AddDistance = TourLogViewModel.SelectedTourLog.TotalDistance;
                TourLogViewModel.AddTime = TourLogViewModel.SelectedTourLog.TotalTime;
                TourLogViewModel.AddRating = TourLogViewModel.SelectedTourLog.Rating;
                TourLogViewModel.AddComment = TourLogViewModel.SelectedTourLog.Comment;


                TourLogViewModel.CurrentLogView = new EditTourlogView();
            }
        }

        public void ShowHomeMenu(ITourService tourService, TourLogService tourlogService)
        {
            TourViewModel.CurrentTourView = new TourListView();
            TourLogViewModel.CurrentLogView = new TourLogsView();
            _ = LoadDataAsync(tourService, tourlogService);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
