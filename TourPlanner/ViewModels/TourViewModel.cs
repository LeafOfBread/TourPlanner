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
using TourPlanner.HelperClasses;
using TourPlanner.Views;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;

namespace TourPlanner.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
        private UserControl _currentTourView;
        public UserControl CurrentTourView
        {
            get => _currentTourView;
            set
            {
                _currentTourView = value;
                OnPropertyChanged(nameof(CurrentTourView));
            }
        }

        private UserControl _currentLogView;
        public UserControl CurrentLogView
        {
            get => _currentLogView;
            set
            {
                _currentLogView = value;
                OnPropertyChanged(nameof(CurrentLogView));
            }
        }

        private readonly InputValidator _validator;

        //Commands
        public ICommand AddTourCommand { get; private set; }
        public ICommand SaveTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand EditTourViewCommand { get; private set; }
        public ICommand UpdateTourCommand { get; private set; }
        public ICommand ShowAddLogViewCommand { get; private set; }
        public ICommand SubmitLogCommand { get; private set; }
        public ICommand DeleteLogCommand { get; private set; }
        public ICommand ShowEditLogViewCommand { get; private set; }
        public ICommand EditLogCommand { get; private set; }

        //tour fields and services
        private readonly TourService _tourService;
        private ObservableCollection<Tours> _allTours;
        private Tours _selectedTour;
        private ObservableCollection<Tours> _tourDetails;

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
                    UpdateTourDetails();
                    UpdateTourLogDetails();
                    HandleTourSelectionChanged();
                }
            }
        }

        //tourlog fields and services
        private readonly TourLogService _tourlogService;
        private ObservableCollection<Tourlog> _allTourLogs;
        private ObservableCollection<Tourlog> _tourlogDetails;
        private Tourlog _selectedTourLog;

        public ObservableCollection<Tourlog> TourLogDetails
        {
            get => _tourlogDetails;
            set
            {
                _tourlogDetails = value;
                OnPropertyChanged(nameof(TourLogDetails));
            }
        }

        public ObservableCollection<Tourlog> AllTourLogs
        {
            get => _allTourLogs;
            set
            {
                _allTourLogs = value;
                OnPropertyChanged(nameof(AllTourLogs));
            }
        }

        public Tourlog SelectedTourLog
        {
            get => _selectedTourLog;
            set
            {
                _selectedTourLog = value;
                OnPropertyChanged(nameof(SelectedTourLog));
            }
        }

        public TourViewModel(TourService tourService, TourLogService tourlogService, InputValidator validator)
        {   //DI
            _tourService = tourService;
            _tourlogService = tourlogService;
            _validator = validator;

            //initialization
            TourDetails = new ObservableCollection<Tours>();
            TourLogDetails = new ObservableCollection<Tourlog>();
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
            Difficulties = new ObservableCollection<Difficulty>
            {
                Difficulty.TooEasy,
                Difficulty.Easy,
                Difficulty.Medium,
                Difficulty.Hard,
                Difficulty.TooHard
            };

            //set base view
            CurrentTourView = new Views.TourListView();
            CurrentLogView = new Views.TourLogsView();

            _newTourLog = new Tourlog
            {
                TourId = GetSelectedTourId()
            };

            //initialize commands
            AddTourCommand = new RelayCommand(ShowAddTourView);
            SaveTourCommand = new RelayCommand(() => SaveTourAsync());
            DeleteTourCommand = new RelayCommand(() => DeleteTourAsync());
            EditTourViewCommand = new RelayCommand(() => ShowEditTourView());
            UpdateTourCommand = new RelayCommand(() => EditTourAsync());
            ShowAddLogViewCommand = new RelayCommand(() => ShowAddTourLog());
            SubmitLogCommand = new RelayCommand(() => SaveLogAsync());
            DeleteLogCommand = new RelayCommand(() => DeleteTourLogAsync());
            ShowEditLogViewCommand = new RelayCommand(() => ShowEditTourLog());
            EditLogCommand = new RelayCommand(() => EditTourLogAsync());

            //fire and forget - load all the necessary data asap
            _ = LoadDataAsync();
            ClearInputs();
        }

        public void ShowAddTourView()
        {
            NewTour = new Tours();
            CurrentTourView = new Views.AddTourView();
        }

        public void ShowTourListView()
        {
            CurrentTourView = new Views.TourListView();
        }

        public void ShowTourlogView()
        {
            CurrentLogView = new Views.TourLogsView();
        }

        public void ShowEditTourView()
        {
            if (SelectedTour != null)
                CurrentTourView = new Views.EditTourView();
        }

        public void ShowAddTourLog()
        {
            if (SelectedTour != null)
                CurrentLogView = new Views.AddTourLogView();
        }

        public void ShowEditTourLog()
        {
            if (SelectedTour != null && SelectedTourLog != null)
                CurrentLogView = new Views.EditTourlogView();
        }

        private async Task LoadDataAsync()
        {
            await LoadTourLogs();
            await LoadTours();
        }

        private async Task LoadTours()
        {
            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
        }

        private async Task LoadTourLogs()
        {
            var tourlogs = await _tourlogService.GetTourlogsAsync();
            AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
        }

        public void UpdateTourDetails()
        {
            if (SelectedTour == null || AllTours == null)
                return;

            TourDetails = new ObservableCollection<Tours> { SelectedTour };
        }

        public void UpdateTourLogDetails()
        {
            if (SelectedTour == null)
            {
                TourLogDetails.Clear();
                return;
            }
            if (TourLogDetails == null)
                TourLogDetails = new ObservableCollection<Tourlog>();

            //all logs that belong to the tour with the same ID
            var matchingLogs = AllTourLogs.Where(log => log.TourId == SelectedTour.Id).ToList();

            TourLogDetails.Clear();
            foreach(var log in matchingLogs)    //fill TourLogDetails
            {
                TourLogDetails.Add(log);
            }
        }



        //required add tour properties
        private string _addTourName;
        private string _addTourFrom;
        private string _addTourTo;
        private string _addTourDescription;
        private TransportType _addTourTransport;

        private string _addAuthor;
        private DateTime _addDate;
        private Difficulty _addDifficulty;
        private double _addDistance;
        private TimeSpan _addTime;
        private int _addRating;
        private string _addComment;

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

        private Tourlog _newTourLog = new Tourlog();
        private Tourlog _tourlogToEdit = new Tourlog();

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

        public Tourlog NewTourLog
        {
            get => _newTourLog;
            set
            {
                if (_newTourLog != null)
                {
                    OnPropertyChanged(nameof(NewTourLog));
                    OnPropertyChanged(nameof(AddAuthor));
                    OnPropertyChanged(nameof(AddDate));
                    OnPropertyChanged(nameof(AddDifficulty));
                    OnPropertyChanged(nameof(AddDistance));
                    OnPropertyChanged(nameof(AddTime));
                    OnPropertyChanged(nameof(AddRating));
                    OnPropertyChanged(nameof(AddComment));
                }
            }
        }

        public Tourlog TourlogToEdit
        {
            get => _tourlogToEdit;
            set
            {
                if (_tourlogToEdit != null)
                {
                    OnPropertyChanged(nameof(TourlogToEdit));
                    OnPropertyChanged(nameof(AddAuthor));
                    OnPropertyChanged(nameof(AddDate));
                    OnPropertyChanged(nameof(AddDifficulty));
                    OnPropertyChanged(nameof(AddDistance));
                    OnPropertyChanged(nameof(AddTime));
                    OnPropertyChanged(nameof(AddRating));
                    OnPropertyChanged(nameof(AddComment));
                }
            }
        }

        public ObservableCollection<TransportType> TransportTypes { get; }
        public ObservableCollection<Difficulty> Difficulties { get; }

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

        public string AddAuthor
        {
            get => _addAuthor;
            set
            {
                _addAuthor = value;
                _newTourLog.Author = value;
                _tourlogToEdit.Author = value;
                OnPropertyChanged(nameof(AddAuthor));
            }
        }
        public DateTime AddDate
        {
            get => _addDate;
            set
            {
                _addDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                _newTourLog.Date = _addDate;
                _tourlogToEdit.Date = _addDate;
                OnPropertyChanged(nameof(AddDate));
            }
        }
        public Difficulty AddDifficulty
        {
            get => _addDifficulty;
            set
            {
                _addDifficulty = value;
                _newTourLog.Difficulty = value;
                _tourlogToEdit.Difficulty = value;
                OnPropertyChanged(nameof(AddDifficulty));
            }
        }
        public double AddDistance
        {
            get => _addDistance;
            set
            {
                _addDistance = value;
                _newTourLog.TotalDistance = value;
                _tourlogToEdit.TotalDistance = value;
                OnPropertyChanged(nameof(AddDistance));
            }
        }
        public TimeSpan AddTime
        {
            get => _addTime;
            set
            {
                _addTime = value;
                _newTourLog.TotalTime = value;
                _tourlogToEdit.TotalTime = value;
                OnPropertyChanged(nameof(AddTime));
            }
        }
        public int AddRating
        {
            get => _addRating;
            set
            {
                _addRating = value;
                _newTourLog.Rating = value;
                _tourlogToEdit.Rating = value;
                OnPropertyChanged(nameof(AddRating));
            }
        }
        public string AddComment
        {
            get => _addComment;
            set
            {
                _addComment = value;
                _newTourLog.Comment = value;
                _tourlogToEdit.Comment = value;
                OnPropertyChanged(nameof(AddComment));
            }
        }

        public async Task SaveTourAsync()
        {
            string errMessage = _validator.ValidateTourInput(NewTour);
            if (errMessage == "")
                await _tourService.InsertTours(NewTour);
            else
            {
                MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // reload all tours from the database
            var tours = await _tourService.GetAllTours();

            // new collection to replace the old one and trigger property change
            AllTours = new ObservableCollection<Tours>(tours);
            ShowTourListView();
            ClearInputs();
        }

        public async Task DeleteTourAsync()
        {
            await _tourService.DeleteTour(SelectedTour);

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
        }

        public async Task EditTourAsync()
        {
            var tourFromDb = await _tourService.GetTourById(SelectedTour.Id);

            if (tourFromDb != null)
            {
                tourFromDb.Name = EditTour.Name;
                tourFromDb.From = EditTour.From;
                tourFromDb.To = EditTour.To;
                tourFromDb.Description = EditTour.Description;
                tourFromDb.Transport = EditTour.Transport;
                tourFromDb.Duration = EditTour.Duration;
                tourFromDb.Distance = EditTour.Distance;

                string errMessage = _validator.ValidateTourInput(tourFromDb);
                if (errMessage == "")
                    await _tourService.UpdateTour(tourFromDb);

                else
                {
                    MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            ShowTourListView(); //show tourlist again
            ClearInputs();    //clear user inputs when insertion is finished
        }

        public async Task SaveLogAsync()
        {
            var tourFromDb = await _tourService.GetTourById(SelectedTour.Id);

            if(tourFromDb != null)
            {
                NewTourLog.TourId = tourFromDb.Id;

                string errMessage = _validator.ValidateTourlogInput(NewTourLog);

                if (errMessage == "")
                    await _tourlogService.InsertTourLog(NewTourLog);

                else
                {
                    MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var tourlogs = await _tourlogService.GetTourlogsAsync();
            AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
            ShowTourlogView();
            ClearInputs();
        }

        public async Task DeleteTourLogAsync()
        {
            if (SelectedTourLog != null)
            {
                await _tourlogService.DeleteTourLog(SelectedTourLog);
                var tourlogs = await _tourlogService.GetTourlogsAsync();
                AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
                ShowTourlogView();
            }
        }

        public async Task EditTourLogAsync()
        {
            if (SelectedTourLog != null)
            {
                var tourlogFromDb = await _tourlogService.GetTourlogById(SelectedTourLog.TourLogId);
                if (tourlogFromDb != null)
                {
                    tourlogFromDb.Author = TourlogToEdit.Author;
                    tourlogFromDb.Date = DateTime.SpecifyKind(TourlogToEdit.Date, DateTimeKind.Utc);
                    tourlogFromDb.Difficulty = TourlogToEdit.Difficulty;
                    tourlogFromDb.TotalDistance = TourlogToEdit.TotalDistance;
                    tourlogFromDb.TotalTime = TourlogToEdit.TotalTime;
                    tourlogFromDb.Comment = TourlogToEdit.Comment;

                    string errMessage = _validator.ValidateTourlogInput(tourlogFromDb);

                    if (errMessage == "")
                        await _tourlogService.EditTourLog(tourlogFromDb);

                    else
                    {
                        MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                var tourlogs = await _tourlogService.GetTourlogsAsync();
                AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
                ShowTourlogView();
                ClearInputs();
            }
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
            //Tour
            _addTourName = "";
            _addTourFrom = "";
            _addTourTo = "";
            _addTourDescription = "";

            //Tourlog
            _addAuthor = "";
            _addComment = "";
            _addDistance = 0;
            _addTime = TimeSpan.Zero;
            _addRating = 5;
            _addDate = DateTime.Now;
        }

        private void HandleTourSelectionChanged()
        {
            if (CurrentTourView is AddTourView || CurrentTourView is EditTourView)
                ShowTourListView();

            if (CurrentLogView is AddTourLogView || CurrentLogView is EditTourlogView)
                ShowTourlogView();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
