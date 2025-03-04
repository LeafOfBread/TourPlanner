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
using TourPlannerClasses.Tour;

namespace TourPlanner.ViewModels
{
    public class TourViewModel : INotifyPropertyChanged
    {
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
                }
            }
        }

        public TourViewModel(TourService tourService)
        {
            _tourService = tourService;
            TourDetails = new ObservableCollection<Tours>();
            LoadTours();
        }

        private async void LoadTours()
        {
            var tours = await _tourService.GetAllTours();
            AllTours = new ObservableCollection<Tours>(tours);
        }

        public void UpdateTourDetails()
        {
            if (SelectedTour == null)
            {
                TourDetails.Clear();
                return;
            }
            TourDetails.Clear();
            TourDetails.Add(SelectedTour);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
