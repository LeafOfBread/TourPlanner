using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TourPlanner.BusinessLogic.Services;
using TourPlannerClasses.Models;

namespace TourPlanner.UI.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        private readonly ITourService _tourService;
        private readonly MainViewModel _mainViewModel;

        private string _searchInput;
        public string SearchInput
        {
            get => _searchInput;
            set
            {
                _searchInput = value;
                OnPropertyChanged(nameof(SearchInput));
            }
            
        }

        private ObservableCollection<Tours> _foundTours;
        public ObservableCollection<Tours> FoundTours
        {
            get => _foundTours;
            set
            {
                _foundTours = value;
                OnPropertyChanged(nameof(FoundTours));
            }
        }


        public ICommand SearchTourCommand { get; private set; }

        public SearchViewModel() { }
        public SearchViewModel(MainViewModel mainViewModel, ITourService tourService)
        {
            _mainViewModel = mainViewModel;
            _tourService = tourService;

            FoundTours = new ObservableCollection<Tours>();

            SearchTourCommand = new RelayCommand(() => SearchForTours());
        }

        public async Task SearchForTours()
        {
            var foundTours = await _tourService.SearchForTours(SearchInput, _mainViewModel.TourViewModel.AllTours, _mainViewModel.TourLogViewModel.AllTourLogs);

            if (foundTours != null)
            {
                FoundTours = foundTours;
                await UpdateTourBox(FoundTours);
            }
        }

        public async Task UpdateTourBox(ObservableCollection<Tours> newTourCollection)
        {
            _mainViewModel.TourViewModel.AllTours = newTourCollection;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
