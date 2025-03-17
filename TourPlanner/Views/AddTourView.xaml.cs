using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using TourPlannerClasses.Models;
using System.Runtime.Serialization.DataContracts;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace TourPlanner.ViewModels
{
    public class AddTourView : UserControl ,INotifyPropertyChanged
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

        public AddTourView(MainViewModel mainVM)
        {
            //InitializeComponent();
            SaveTourCommand = new RelayCommand(SaveTour);
            DataContext = mainVM;
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