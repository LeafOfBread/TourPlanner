using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;

namespace TourPlanner.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        //Commands
        public ICommand AddTourCommand { get; private set; }
        public ICommand SaveTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand EditTourViewCommand { get; private set; }
        public ICommand UpdateTourCommand { get; private set; }


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
                }
            }
        }

        //tourlog fields and services
        private readonly TourLogService _tourlogService;
        private ObservableCollection<Tourlog> _allTourLogs;
        private ObservableCollection<Tourlog> _tourlogDetails;

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

        public TourViewModel(TourService tourService, TourLogService tourlogService)
        {   //DI
            _tourService = tourService;
            _tourlogService = tourlogService;

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
            //set base view
            CurrentView = new Views.TourListView();

            //initialize commands
            AddTourCommand = new RelayCommand(ShowAddTourView);
            SaveTourCommand = new RelayCommand(() => SaveTour());
            DeleteTourCommand = new RelayCommand(() => DeleteTourAsync());
            EditTourViewCommand = new RelayCommand(() => ShowEditTourView());
            UpdateTourCommand = new RelayCommand(() => EditTourAsync());

            //fire and forget - load all the necessary data asap
            _ = LoadDataAsync();
        }

        public void ShowAddTourView()
        {
            NewTour = new Tours();
            CurrentView = new Views.AddTourView();
        }

        public void ShowTourListView()
        {
            CurrentView = new Views.TourListView();
        }

        public void ShowEditTourView()
        {
            if (SelectedTour != null)
                CurrentView = new Views.EditTourView();
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

        public ObservableCollection<TransportType> TransportTypes { get; }

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


        public async Task SaveTour()
        {
            await _tourService.InsertTours(NewTour);

            // reload all tours from the database
            var tours = await _tourService.GetAllTours();

            // new collection to replace the old one and trigger property change
            AllTours = new ObservableCollection<Tours>(tours);
            ShowTourListView();
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
                
                await _tourService.UpdateTour(EditTour);
            }

            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
            ShowTourListView(); //show tourlist again
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
