using log4net;
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
        private static readonly ILog _log = LogManager.GetLogger(typeof(SearchViewModel));

        private string _searchInput;
        public string SearchInput
        {
            get => _searchInput;
            set
            {
                if (value != _searchInput)
                {
                    _searchInput = value;
                    OnPropertyChanged(nameof(SearchInput));
                    _ = SearchForTours(); // trigger search on each input change
                }
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

            _log.Info("Initialized SearchViewModel");
        }

        public async Task SearchForTours()
        {
            try
            {
                var foundTours = await _tourService.SearchForTours(SearchInput, _mainViewModel.TourViewModel.MasterTours, _mainViewModel.TourLogViewModel.AllTourLogs);

                if (foundTours != null)
                {
                    FoundTours = foundTours;
                    await UpdateTourBox(FoundTours);
                    _log.Info($"Successfully searched for {foundTours.Count}");
                }
                else
                    _log.Error("foundTours returned as null!");
            }
            catch(Exception ex)
            {
                _log.Error("Tried to search for tours, but an exception was thrown: ", ex);
                throw;
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
