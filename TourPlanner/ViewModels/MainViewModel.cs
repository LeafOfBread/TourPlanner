using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;

namespace TourPlanner.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public TourViewModel TourVM { get; }

        public MainViewModel(TourService tourService, TourLogService tourlogService)
        {
            TourVM = new TourViewModel(tourService, tourlogService);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
