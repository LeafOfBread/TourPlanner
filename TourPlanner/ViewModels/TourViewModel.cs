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
using TourPlanner.BusinessLogic.Services;
using TourPlanner.UI.HelperClasses;
using TourPlanner.Views;
using TourPlannerClasses.Models;
using Microsoft.Web.WebView2.Wpf;
using System.Globalization;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace TourPlanner.UI.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ITourService _tourService;
        private readonly TourLogService _tourlogService;
        private readonly InputValidator _validator;
        private readonly ApiHandler _apiHandler;
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
        public ICommand SingleReportCommand { get; private set; }
        public ICommand SummarizeReportCommand { get; private set; }
        public ICommand RandomTourCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

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
        public TourViewModel(MainViewModel mainViewModel, ITourService tourService, TourLogService tourlogService, InputValidator validator, ApiHandler apiHandler)
        {   //DI
            _mainViewModel = mainViewModel;
            _tourService = tourService;
            _tourlogService = tourlogService;
            _validator = validator;
            _apiHandler = apiHandler;

            //initialization
            TourDetails = new ObservableCollection<Tours>();
            Tours newTour = new Tours();
            TransportTypes = new ObservableCollection<TransportType>
            {
                TransportType.Walking,
                TransportType.Bicycle,
                TransportType.Car,
            };

            CurrentTourView = new TourListView();

            //initialize commands
            SaveTourCommand = new RelayCommand(() => SaveTourAsync());
            DeleteTourCommand = new RelayCommand(() => DeleteTourAsync());
            UpdateTourCommand = new RelayCommand(() => EditTourAsync());
            ImportTourCommand = new RelayCommand(() => ImportTourAsync());
            ExportTourCommand = new RelayCommand(() => ExportTourAsync());
            SingleReportCommand = new RelayCommand(() => HandleSingleReport());
            SummarizeReportCommand = new RelayCommand(() => HandleReportSummary());
            RandomTourCommand = new RelayCommand(() => RandomizeTour());
            RefreshCommand = new RelayCommand(() => RefreshTours());

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

        public async Task RefreshTours()
        {
            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            _log.Info("Refreshed all tours.");
        }

        public async Task SaveTourAsync()
        {
            string errMessage = _validator.ValidateTourInput(NewTour);
            if (errMessage != "")
            {
                _log.Warn($"Validation failed for new tour: {errMessage}");
                MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var coords = await _apiHandler.GetCoordinates(NewTour.From, NewTour.To);
            if (coords.Count != 4)
            {
                MessageBox.Show("Could not resolve locations.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewTour.FromLng = coords[0];
            NewTour.FromLat = coords[1];
            NewTour.ToLng = coords[2];
            NewTour.ToLat = coords[3];

            double approximateDistance = _apiHandler.Haversine(NewTour.FromLat, NewTour.FromLng, NewTour.ToLat, NewTour.ToLng);

            if(approximateDistance > 6000)
            {
                MessageBox.Show("Distance between Start and End cannot exceed 6000km!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Warn("User tried to add tour that exceeds 6000km");
                return;
            }

            string profile = TransportHandler(NewTour.Transport);

            var routeInfo = await _apiHandler.GetRouteDirections(
                coords[0], coords[1],
                coords[2], coords[3],
                profile);

            NewTour.Distance = Math.Round(routeInfo.DistanceMeters / 1000.0, 2);
            NewTour.Duration = TimeSpan.FromSeconds(
                (int)routeInfo.DurationSeconds
            );
            await _tourService.InsertTours(NewTour);

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
                    var coords = await _apiHandler.GetCoordinates(tourFromDb.From, tourFromDb.To);
                    if (coords.Count != 4)
                    {
                        MessageBox.Show("Could not resolve locations.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    tourFromDb.FromLng = coords[0];
                    tourFromDb.FromLat = coords[1];
                    tourFromDb.ToLng = coords[2];
                    tourFromDb.ToLat = coords[3];

                    double approximateDistance = _apiHandler.Haversine(tourFromDb.FromLat, tourFromDb.FromLng, tourFromDb.ToLat, tourFromDb.ToLng);

                    if (approximateDistance > 6000)
                    {
                        MessageBox.Show("Distance between Start and End cannot exceed 6000km!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Warn("User tried to add tour that exceeds 6000km");
                        var returnToNormal = await _tourService.GetAllTours();
                        AllTours = new ObservableCollection<Tours>(returnToNormal);
                        return;
                    }

                    string profile = TransportHandler(NewTour.Transport);

                    var routeInfo = await _apiHandler.GetRouteDirections(
                                    coords[0], coords[1],
                                    coords[2], coords[3],
                                    profile);

                    tourFromDb.Distance = Math.Round(routeInfo.DistanceMeters / 1000.0, 2);
                    tourFromDb.Duration = TimeSpan.FromSeconds(
                        (int)routeInfo.DurationSeconds
                    );

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

            _ = UpdateMapAsync();

            _mainViewModel.ShowTourListView();
            ClearInputs();
        }

        public async Task RandomizeTour()
        {
            string message = await _tourService.CreateRandomTour();
            if (message != "")
            {
                _log.Warn("Random tour generation failed.");
                MessageBox.Show("Oops, something went wrong while generating a random tour", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await UpdateMapAsync();

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            _log.Info("Tour list refreshed after random tour generation.");
        }

        public async Task ExportTourAsync()
        {
            if (SelectedTour == null)
            {
                MessageBox.Show("Please select a tour to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                FileName = $"{SelectedTour.Name}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await _tourService.ExportTourToFileAsync(SelectedTour, saveFileDialog.FileName);
                MessageBox.Show("Tour exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async Task ImportTourAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var importedTour = await _tourService.ImportTourFromFileAsync(openFileDialog.FileName);

                if (importedTour != null)
                {
                    var tours = await _tourService.GetAllTours();
                    var logs = await _tourlogService.GetTourlogsAsync();
                    AllTours = new ObservableCollection<Tours>(tours);
                    _mainViewModel.TourLogViewModel.AllTourLogs = new ObservableCollection<Tourlog>(logs);
                    MessageBox.Show("Tour imported successfully.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Import failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task HandleSingleReport()
        {
            if (SelectedTour == null)
            {
                _log.Warn("Cannot create tour report if SelectedTour is null");
                MessageBox.Show("Please select a tour first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"{SelectedTour.Name}_Report.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await CaptureMapScreenshotAsync("MyMap.png");
                var doc = new TourReportDocument(SelectedTour);

                byte[] pdfBytes;

                try
                {
                    pdfBytes = doc.GeneratePdf();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF generation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    MessageBox.Show("Generated PDF is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                MessageBox.Show("Report generated successfully!", "Success");

                if (File.Exists("MyMap.png"))
                        File.Delete("MyMap.png");
            }
        }

        public async Task HandleReportSummary()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = "SummaryReport.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var summaries = await GenerateTourSummaries();
                var summaryDoc = new TourSummaryReportDocument(summaries);
                
                try
                {
                    summaryDoc.GeneratePdf(saveDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF generation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show("Report generated successfully!", "Success");
            }
        }

        public async Task CaptureMapScreenshotAsync(string filePath)
        {
            var webview = _webView;

            if(webview?.CoreWebView2 == null)
            {
                MessageBox.Show("Map view is not ready yet.", "Error");
                return;
            }
            using var stream = new MemoryStream();
            await webview.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);

            await File.WriteAllBytesAsync(filePath, stream.ToArray());
        }
        public async Task<List<TourSummaryDto>> GenerateTourSummaries()
        {
            var allTours = await _tourService.GetAllTours();

            var summaries = allTours.Select(tour =>
            {
                var logs = tour.Tourlogs;

                return new TourSummaryDto
                {
                    TourName = tour.Name,
                    AverageDistance = logs.Any() ? logs.Average(l => l.TotalDistance) : 0,
                    AverageDuration = logs.Any() ? TimeSpan.FromSeconds(logs.Average(l => l.TotalTime.TotalSeconds)) : TimeSpan.Zero,
                    AverageRating = logs.Any() ? logs.Average(l => l.Rating) : 0
                };
            }).ToList();

            return summaries;
        }

        public void SetWebView(WebView2 webView)
        {
            _webView = webView;
        }

        public async Task UpdateMapAsync()
        {
            if (_webView == null || SelectedTour == null)
                return;

            double fromLat = SelectedTour.FromLat;
            double fromLng = SelectedTour.FromLng;
            double toLat = SelectedTour.ToLat;
            double toLng = SelectedTour.ToLng;

            try
            {
                string profile = TransportHandler(SelectedTour.Transport);

                RouteInfo route = await _apiHandler.GetRouteDirections(
                    (float)fromLng, (float)fromLat,
                    (float)toLng, (float)toLat,
                    profile
                );

                if (route?.Geometry == null || !route.Geometry.Any())
                {
                    _log.Warn($"No geometry returned for tour '{SelectedTour.Name}'");
                    return;
                }

                var leafletCoords = route.Geometry
                    .Select(coord => $"[{coord[1].ToString(CultureInfo.InvariantCulture)}, {coord[0].ToString(CultureInfo.InvariantCulture)}]");

                string jsArray = "[" + string.Join(",", leafletCoords) + "]";

                string jsCall = $"showFullRoute({jsArray});";
                await _webView.ExecuteScriptAsync(jsCall);

                _log.Info($"Updated map for tour '{SelectedTour.Name}' with OpenRouteService route.");
            }
            catch (Exception ex)
            {
                _log.Error($"Failed updating map for tour '{SelectedTour.Name}': {ex.Message}", ex);
            }
        }

        public int GetSelectedTourId()
        {
            if (SelectedTour != null)
                return SelectedTour.Id;
            else
                return 0;
        }

        public string TransportHandler(TransportType transport)
        {
            return transport switch
            {
                TransportType.Walking => "foot-walking",
                TransportType.Bicycle => "cycling-road",
                TransportType.Car => "driving-car",
                _ => "driving-car"
            };
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
