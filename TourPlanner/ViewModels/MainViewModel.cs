﻿using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TourPlanner.BusinessLogic.Services;
using TourPlanner.UI.HelperClasses;
using TourPlanner.Views;
using TourPlannerClasses.DB;
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

        public MainViewModel(ITourService tourService, TourLogService tourlogService, InputValidator validator, ApiHandler apiHandler, ConfigReader configReader)
        {
            TourViewModel = new TourViewModel(this, tourService, tourlogService, validator, apiHandler);
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


        private async Task LoadDataAsync(ITourService tourService, TourLogService tourlogService)
        {
            try
            {
                await LoadTourLogs(tourlogService);
                await LoadTours(tourService);
                _log.Info("Successfully loaded available Tours and Tourlogs");
            }
            catch (Exception ex)
            {
                _log.Error("Error loading Tours and Tourlogs", ex);
                throw new UiException("An error occurred while loading the data.", ex);
            }
        }

        private async Task LoadTours(ITourService _tourService)
        {
            try
            {
                var tours = await _tourService.GetAllTours();
                TourViewModel.AllTours = new ObservableCollection<Tours>(tours);
                TourViewModel.MasterTours = new ObservableCollection<Tours>(tours);
                if (tours != null)
                    _log.Info("Successfully loaded tours from database");
                else
                    _log.Error("LoadTours could not find any tours.");
            }
            catch (Exception ex)
            {
                _log.Error("Exception thrown during LoadTours:", ex);
                throw new UiException("An error occurred while loading the tours.", ex);
            }
        }

        private async Task LoadTourLogs(TourLogService _tourlogService)
        {
            try
            {
                var tourlogs = await _tourlogService.GetTourlogsAsync();
                TourLogViewModel.AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
                if (tourlogs != null)
                    _log.Info("Successfully loaded tourlogs from database");
                else
                    _log.Error("LoadTourLogs could not find any tourlogs");
            }
            catch (Exception ex)
            {
                _log.Error("Exception thrown during LoadTourLogs:", ex);
                throw new UiException("An error occurred while loading the tourlogs.", ex);
            }
        }

        public void ShowTourListView()
        {
            try
            {
                TourViewModel.CurrentTourView = new TourListView();
                _log.Info("Navigated to TourListView");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize TourListView.", ex);
                throw new ViewInitializationException(nameof(TourListView), "Could not load the Tour List view.", ex);
            }
        }
        public void ShowAddTourView()
        {
            try
            {
                TourViewModel.NewTour = new Tours();
                TourViewModel.CurrentTourView = new AddTourView();
                _log.Info("Navigated to AddTourView.");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize AddTourView.", ex);
                throw new ViewInitializationException(nameof(AddTourView), "Could not load the Add Tour view.", ex);
            }
        }

        public void ShowTourlogView()
        {
            try
            {
                TourLogViewModel.CurrentLogView = new TourLogsView();
                _log.Info("Navigated to ShowTourlogView.");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize ShowTourlogView.", ex);
                throw new ViewInitializationException(nameof(AddTourView), "Could not load the Add Tour view.", ex);
            }
        }

        public void ShowEditTourView()
        {
            try
            {
                if (TourViewModel.SelectedTour != null)
                {
                    TourViewModel.AddTourName = TourViewModel.SelectedTour.Name;
                    TourViewModel.AddTourFrom = TourViewModel.SelectedTour.From;
                    TourViewModel.AddTourTo = TourViewModel.SelectedTour.To;
                    TourViewModel.AddTourDescription = TourViewModel.SelectedTour.Description;

                    TourViewModel.CurrentTourView = new EditTourView();
                    _log.Info("Navigated to ShowEditTourView");
                }
                else
                    _log.Error("Tried to navigate to ShowEditTourView, but the selected tour was null!");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize ShowEditTourView.", ex);
                throw new ViewInitializationException(nameof(AddTourView), "Could not load the Add Tour view.", ex);
            }

        }

        public void ShowAddTourLog()
        {
            try
            {
                TourLogViewModel.ClearInputs();
                if (TourViewModel.SelectedTour != null)
                {
                    TourLogViewModel.CurrentLogView = new AddTourLogView();
                    _log.Info("Navigated to ShowAddTourLog");
                }
                else
                    _log.Error("Tried to navigate to ShowAddTourLog, but the selected tour was null!");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize ShowAddTourLog.", ex);
                throw new ViewInitializationException(nameof(AddTourView), "Could not load the Add Tour view.", ex);
            }
        }

        public void ShowEditTourLog()
        {
            try
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

                    _log.Info("Navigated to ShowEditTourLog");
                }
                _log.Error("Tried to navigate to ShowEditTourLog, but either the selected tour or tourlog was null!");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize ShowEditTourLog.", ex);
                throw new ViewInitializationException(nameof(AddTourView), "Could not load the Add Tour view.", ex);
            }
        }

        public void ShowHomeMenu(ITourService tourService, TourLogService tourlogService)
        {
            try
            {
                TourViewModel.CurrentTourView = new TourListView();
                TourLogViewModel.CurrentLogView = new TourLogsView();
                //_ = LoadDataAsync(tourService, tourlogService);   //unnecessary, list of MasterTours now stored in memory instead of pulling from the database each time

                _log.Info("Navigated to home menu.");
            }
            catch (Exception ex)
            {
                _log.Error("Failed to initialize ShowHomeMenu.", ex);
                throw new ViewInitializationException(nameof(AddTourView), "Could not load the Add Tour view.", ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
