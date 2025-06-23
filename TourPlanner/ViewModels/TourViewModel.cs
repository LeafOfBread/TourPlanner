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

namespace TourPlanner.UI.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ITourService _tourService;
        private readonly InputValidator _validator;
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

        //Commands
        public ICommand SaveTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand UpdateTourCommand { get; private set; }

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
                    UpdateTourDetails();
                    _mainViewModel.TourLogViewModel.UpdateTourLogDetails();
                    HandleTourSelectionChanged();
                }
            }
        }

        public TourViewModel() { }
        public TourViewModel(MainViewModel mainViewModel, ITourService tourService, InputValidator validator)
        {   //DI
            _mainViewModel = mainViewModel;
            _tourService = tourService;
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

            ClearInputs();
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
            string errMessage = _validator.ValidateTourInput(NewTour);  //if validator returns no error -> insert new tour
            if (errMessage == "")
                await _tourService.InsertTours(NewTour);
            else
            {
                MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // reload all tours to also display the new one
            var tours = await _tourService.GetAllTours();

            // new collection to trigger property change
            AllTours = new ObservableCollection<Tours>(tours);
            _mainViewModel.ShowTourListView();
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
            _mainViewModel.ShowTourListView();
            ClearInputs();    //clear user inputs when insertion is finished
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
        }

        private void HandleTourSelectionChanged()
        {
            if (CurrentTourView is AddTourView || CurrentTourView is EditTourView)
                _mainViewModel.ShowTourListView();

            if (_mainViewModel.TourLogViewModel.CurrentLogView is AddTourLogView || _mainViewModel.TourLogViewModel.CurrentLogView is EditTourlogView)
                _mainViewModel.ShowTourlogView();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
