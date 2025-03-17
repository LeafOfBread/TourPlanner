using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;

namespace TourPlanner.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
        //tour fields
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
                    OnPropertyChanged(nameof(SelectedTour));    //if selected tour changes, call updatetourdetails and updatetourlogs
                    UpdateTourDetails();
                    UpdateTourLogDetails();
                }
            }
        }

        //tourlog fields
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
        {
            _tourService = tourService;
            _tourlogService = tourlogService;
            TourDetails = new ObservableCollection<Tours>();
            TourLogDetails = new ObservableCollection<Tourlog>();
            _ = LoadDataAsync();
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

            TourDetails = AllTours;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
