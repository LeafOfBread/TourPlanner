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
using TourPlannerClasses.Tour;

namespace TourPlanner
{
    public class TourViewModel : INotifyPropertyChanged
    {
        private readonly TourService _tourService;
        private ObservableCollection<Tours> _tours;

        public ObservableCollection<Tours> Tours
        {
            get => _tours;
            set
            {
                _tours = value;
                OnPropertyChanged(nameof(Tours));
            }
        }

        public TourViewModel(TourService tourService)
        {
            _tourService = tourService;
            LoadTours();
        }

        private async void LoadTours()
        {
            var tours = await _tourService.GetAllTours();
            Tours = new ObservableCollection<Tours>(tours);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
