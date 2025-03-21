using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.HelperClasses;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;

namespace TourPlanner.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public TourViewModel TourVM { get; }

        public MainViewModel(ITourService tourService, TourLogService tourlogService, InputValidator validator)
        {
            TourVM = new TourViewModel(tourService, tourlogService, validator);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
