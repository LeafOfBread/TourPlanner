using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using TourPlannerClasses.Models;

namespace TourPlanner.ViewModels
{
    public class AddTourViewModel : INotifyPropertyChanged
    {
        private string _tourName;
        private string _tourFrom;
        private string _tourTo;
        private string _tourDescription;

        public string TourName
        {
            get => _tourName;
            set
            {
                _tourName = value;
                OnPropertyChanged(nameof(TourName));
            }
        }

        public string TourFrom
        {
            get => _tourFrom;
            set
            {
                _tourFrom = value;
                OnPropertyChanged(nameof(TourFrom));
            }
        }

        public string TourTo
        {
            get => _tourTo;
            set
            {
                TourTo = value;
                OnPropertyChanged(nameof(TourTo));
            }
        }

        public string TourDescription
        {
            get => _tourDescription;
            set
            {
                TourDescription = value;
                OnPropertyChanged(nameof(TourDescription));
            }
        }

        public ICommand SaveTourCommand { get; }

        public AddTourViewModel()
        {
            SaveTourCommand = new RelayCommand(SaveTour);
        }

        public void SaveTour()
        {
            //TODO
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}