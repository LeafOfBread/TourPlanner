using log4net;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using TourPlanner.BusinessLogic.Services;
using TourPlanner.UI.HelperClasses;
using TourPlanner.Views;
using TourPlannerClasses.Models;
using Microsoft.Web.WebView2.Wpf;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace TourPlanner.UI.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ITourService _tourService;
        private readonly TourLogService _tourlogService;
        private readonly InputValidator _validator;
        private UserControl _currentTourView;
        private static readonly ILog _log = LogManager.GetLogger(typeof(TourViewModel));
        private WebView2 _webView;

        public UserControl CurrentTourView
        {
            get => _currentTourView;
            set
            {
                _currentTourView = value;
                OnPropertyChanged(nameof(CurrentTourView));
            }
        }

        //Commands
        public ICommand SaveTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand UpdateTourCommand { get; private set; }
        public ICommand ImportTourCommand { get; private set; }
        public ICommand ExportTourCommand { get; private set; }

        //tour fields
        private ObservableCollection<Tours> _allTours;
        private Tours _selectedTour;
        private ObservableCollection<Tours> _tourDetails;

        public ObservableCollection<Tours> MasterTours { get; set; } = new ObservableCollection<Tours>();

        public ObservableCollection<Tours> TourDetails
        {
            get => _tourDetails;
            set
            {
                _tourDetails = value;
                OnPropertyChanged(nameof(TourDetails));
            }
        }

        public ObservableCollection<Tours> AllTours
        {
            get => _allTours;
            set
            {
                _allTours = value;
                OnPropertyChanged(nameof(AllTours));
            }
        }

        public Tours SelectedTour
        {
            get => _selectedTour;
            set
            {
                if (_selectedTour != value)
                {
                    _selectedTour = value;
                    OnPropertyChanged(nameof(SelectedTour));

                    if (_selectedTour != null)
                    {
                        _log.Info($"Selected tour changed to: ID={_selectedTour.Id}, Name={_selectedTour.Name}");
                        _ = UpdateMapAsync();
                    }
                    else
                        _log.Warn("Selected tour set to null.");

                    UpdateTourDetails();
                    _mainViewModel.TourLogViewModel.UpdateTourLogDetails();
                    HandleTourSelectionChanged();
                }
            }
        }


        public TourViewModel() { }
        public TourViewModel(MainViewModel mainViewModel, ITourService tourService, TourLogService tourlogService, InputValidator validator)
        {   //DI
            _mainViewModel = mainViewModel;
            _tourService = tourService;
            _tourlogService = tourlogService;
            _validator = validator;

            //initialization
            TourDetails = new ObservableCollection<Tours>();
            Tours newTour = new Tours();
            TransportTypes = new ObservableCollection<TransportType>
            {
                TransportType.Walking,
                TransportType.Bicycle,
                TransportType.Tram,
                TransportType.Bus,
                TransportType.Train,
                TransportType.Car,
                TransportType.Boat,
                TransportType.Plane
            };

            CurrentTourView = new TourListView();

            //initialize commands
            SaveTourCommand = new RelayCommand(() => SaveTourAsync());
            DeleteTourCommand = new RelayCommand(() => DeleteTourAsync());
            UpdateTourCommand = new RelayCommand(() => EditTourAsync());
            ImportTourCommand = new RelayCommand(() => ImportTourAsync());
            ExportTourCommand = new RelayCommand(() => ExportTourAsync());

            ClearInputs();

            _log.Info("Initialized TourViewModel");
        }

        public void UpdateTourDetails()
        {
            if (SelectedTour == null || AllTours == null)
                return;

            TourDetails = new ObservableCollection<Tours> { SelectedTour };
        }

        //required add tour properties
        private string _addTourName;
        private string _addTourFrom;
        private string _addTourTo;
        private string _addTourDescription;
        private TransportType _addTourTransport;
        public ObservableCollection<TransportType> TransportTypes { get; }

        private Tours _newTour = new Tours
        {
            Duration = new TimeSpan(1, 30, 0),
            Distance = 5.5,
        };

        private Tours _editTour = new Tours
        {
            Duration = new TimeSpan(2, 0, 0),
            Distance = 10.1
        };

        public Tours NewTour
        {
            get => _newTour;
            set
            {
                if (_newTour != null)
                {
                    OnPropertyChanged(nameof(NewTour));
                    OnPropertyChanged(nameof(AddTourName));
                    OnPropertyChanged(nameof(AddTourFrom));
                    OnPropertyChanged(nameof(AddTourTo));
                    OnPropertyChanged(nameof(AddTourDescription));
                    OnPropertyChanged(nameof(AddTourTransport));
                }
            }
        }

        public Tours EditTour
        {
            get => _editTour;
            set
            {
                if (_editTour != null)
                {
                    OnPropertyChanged(nameof(EditTour));
                    OnPropertyChanged(nameof(AddTourName));
                    OnPropertyChanged(nameof(AddTourFrom));
                    OnPropertyChanged(nameof(AddTourTo));
                    OnPropertyChanged(nameof(AddTourDescription));
                    OnPropertyChanged(nameof(AddTourTransport));
                }
            }
        }
        public string AddTourName
        {
            get => _addTourName;
            set
            {
                _addTourName = value;
                _newTour.Name = value;
                _editTour.Name = value;
                OnPropertyChanged(nameof(AddTourName));
            }
        }
        public string AddTourFrom
        {
            get => _addTourFrom;
            set
            {
                _addTourFrom = value;
                _newTour.From = value;
                _editTour.From = value;
                OnPropertyChanged(nameof(AddTourFrom));
            }
        }
        public string AddTourTo
        {
            get => _addTourTo;
            set
            {
                _addTourTo = value;
                _newTour.To = value;
                _editTour.To = value;
                OnPropertyChanged(nameof(AddTourTo));
            }
        }
        public string AddTourDescription
        {
            get => _addTourDescription;
            set
            {
                _addTourDescription = value;
                _newTour.Description = value;
                _editTour.Description = value;
                OnPropertyChanged(nameof(AddTourDescription));
            }
        }
        public TransportType AddTourTransport
        {
            get => _addTourTransport;
            set
            {
                _addTourTransport = value;
                _newTour.Transport = value;
                _editTour.Transport = value;
                OnPropertyChanged(nameof(AddTourTransport));
            }
        }

        public async Task SaveTourAsync()
        {
            string errMessage = _validator.ValidateTourInput(NewTour);
            if (errMessage == "")
            {
                _log.Info($"Saving new tour: {NewTour.Name}");
                await _tourService.InsertTours(NewTour);
                _log.Info("New tour saved successfully.");
            }
            else
            {
                _log.Warn($"Validation failed for new tour: {errMessage}");
                MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            _log.Info("Tour list refreshed after save.");

            _mainViewModel.ShowTourListView();
            ClearInputs();
        }


        public async Task DeleteTourAsync()
        {
            if (SelectedTour != null)
            {
                _log.Info($"Deleting tour: ID={SelectedTour.Id}, Name={SelectedTour.Name}");
                await _tourService.DeleteTour(SelectedTour);
            }
            else
            {
                _log.Warn("DeleteTourAsync called, but SelectedTour was null.");
                return;
            }

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            _log.Info("Tour list refreshed after deletion.");
        }


        public async Task EditTourAsync()
        {
            if (SelectedTour == null)
            {
                _log.Warn("EditTourAsync called, but SelectedTour was null.");
                return;
            }

            var tourFromDb = await _tourService.GetTourById(SelectedTour.Id);

            if (tourFromDb != null)
            {
                _log.Info($"Editing tour: ID={tourFromDb.Id}, Name={tourFromDb.Name}");

                tourFromDb.Name = EditTour.Name;
                tourFromDb.From = EditTour.From;
                tourFromDb.To = EditTour.To;
                tourFromDb.Description = EditTour.Description;
                tourFromDb.Transport = EditTour.Transport;
                tourFromDb.Duration = EditTour.Duration;
                tourFromDb.Distance = EditTour.Distance;

                string errMessage = _validator.ValidateTourInput(tourFromDb);
                if (errMessage == "")
                {
                    await _tourService.UpdateTour(tourFromDb);
                    _log.Info("Tour updated successfully.");
                }
                else
                {
                    _log.Warn($"Validation failed during edit: {errMessage}");
                    MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                _log.Error($"EditTourAsync failed: Tour with ID={SelectedTour.Id} not found in DB.");
                return;
            }

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            _log.Info("Tour list refreshed after edit.");

            _mainViewModel.ShowTourListView();
            ClearInputs();
        }

        public async Task ImportTourAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*json",
                Multiselect = false
            };
           
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = await File.ReadAllTextAsync(openFileDialog.FileName);
                    var importedTour = JsonSerializer.Deserialize<Tours>(json,
                        new JsonSerializerOptions
                        {
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                        });

                    if (importedTour != null)
                    {
                        foreach(Tours tour in AllTours)
                        {
                            if (tour.Name == importedTour.Name)
                            {
                                MessageBox.Show("This tour already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        await _tourService.InsertTours(importedTour);

                        foreach(Tourlog log in importedTour.Tourlogs)
                        {
                            log.Tour = importedTour;
                            log.Tour.Id = importedTour.Id;
                            await _tourlogService.InsertTourLog(log);
                        }

                        var tours = await _tourService.GetAllTours();
                        AllTours = new ObservableCollection<Tours>(tours);

                        var tourlogs = await _tourlogService.GetTourlogsAsync();
                        _mainViewModel.TourLogViewModel.AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);

                        MessageBox.Show("Tour imported successfully", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                        _log.Info($"Imported tour from {openFileDialog.FileName}");
                    }
                    else
                        MessageBox.Show("Could not read tour from file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch(Exception ex)
                {
                    _log.Error("Error importing tour: ", ex);
                    MessageBox.Show($"Error importing tour:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task ExportTourAsync()
        {
            if (SelectedTour == null)
            {
                MessageBox.Show("Please first select a tour you would like to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                _log.Info("Tried to export a tour, but SelectedTour was NULL");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                FileName = $"{SelectedTour.Name}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = JsonSerializer.Serialize(SelectedTour, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    });
                    await File.WriteAllTextAsync(saveFileDialog.FileName, json);
                    MessageBox.Show("Tour exported successfully", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    _log.Info($"Exported tour to {saveFileDialog.FileName}");
                }
                catch(Exception ex)
                {
                    _log.Error("Error exporting tour: ", ex);
                    MessageBox.Show($"Error exporting tour:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void SetWebView(WebView2 webView)
        {
            _webView = webView;
        }

        public async Task UpdateMapAsync()
        {
            if (_webView == null || SelectedTour == null)
                return;

            double fromLat = SelectedTour.FromLat; // Vienna
            double fromLng = SelectedTour.FromLng;
            double toLat = SelectedTour.ToLat;   // Linz
            double toLng = SelectedTour.ToLng;

            string jsCall = string.Format(CultureInfo.InvariantCulture,
                "showRoute({0}, {1}, {2}, {3});",
                fromLat, fromLng, toLat, toLng);

            await _webView.ExecuteScriptAsync(jsCall);

            _log.Info($"Map updated for tour '{SelectedTour.Name}'");
        }

        public int GetSelectedTourId()
        {
            if (SelectedTour != null)
                return SelectedTour.Id;
            else
                return 0;
        }

        private void ClearInputs()
        {
            _log.Info("Cleared user input fields.");
            _addTourName = "";
            _addTourFrom = "";
            _addTourTo = "";
            _addTourDescription = "";
        }


        private void HandleTourSelectionChanged()
        {
            if (CurrentTourView is AddTourView || CurrentTourView is EditTourView)
            {
                _log.Info("Tour selection changed — returning to TourListView.");
                _mainViewModel.ShowTourListView();
            }

            if (_mainViewModel.TourLogViewModel.CurrentLogView is AddTourLogView || _mainViewModel.TourLogViewModel.CurrentLogView is EditTourlogView)
            {
                _log.Info("Tour selection changed — returning to TourLogView.");
                _mainViewModel.ShowTourlogView();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
