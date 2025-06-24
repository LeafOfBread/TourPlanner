using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Models;
using TourPlanner.BusinessLogic.Services;
using TourPlanner.UI.HelperClasses;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;
using TourPlanner.Views;
using System.Windows;
using log4net;
using log4net.Repository.Hierarchy;

namespace TourPlanner.UI.ViewModels
{
    public class TourLogViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ITourService _tourService;
        private readonly TourLogService _tourlogService;
        private readonly InputValidator _validator;
        private static readonly ILog _log = LogManager.GetLogger(typeof(TourLogViewModel));

        private UserControl _currentLogView;
        public UserControl CurrentLogView
        {
            get => _currentLogView;
            set
            {
                _currentLogView = value;
                OnPropertyChanged(nameof(CurrentLogView));
            }
        }

        private Tourlog _selectedTourLog;

        public Tourlog SelectedTourLog
        {
            get => _selectedTourLog;
            set
            {
                _selectedTourLog = value;
                OnPropertyChanged(nameof(SelectedTourLog));
            }
        }


        private ObservableCollection<Tourlog> _allTourLogs;
        public ObservableCollection<Tourlog> AllTourLogs
        {
            get => _allTourLogs;
            set
            {
                _allTourLogs = value;
                OnPropertyChanged(nameof(AllTourLogs));
            }
        }

        public ObservableCollection<Tourlog> TourLogDetails
        {
            get => _tourlogDetails;
            set
            {
                _tourlogDetails = value;
                OnPropertyChanged(nameof(TourLogDetails));
            }
        }

        private ObservableCollection<Tourlog> _tourlogDetails;

        public ICommand SubmitLogCommand { get; private set; }
        public ICommand DeleteLogCommand { get; private set; }
        public ICommand EditLogCommand { get; private set; }
        public ICommand ShowAddLogViewCommand { get; private set; }
        public ICommand ShowEditLogViewCommand { get; private set; }

        public TourLogViewModel() { }
        public TourLogViewModel(MainViewModel mainViewModel, ITourService tourService, TourLogService tourlogService, InputValidator validator)
        {
            _mainViewModel = mainViewModel;
            _tourService = tourService;
            _tourlogService = tourlogService;
            _validator = validator;

            CurrentLogView = new TourLogsView();

            AllTourLogs = new ObservableCollection<Tourlog>();
            TourLogDetails = new ObservableCollection<Tourlog>();

            Difficulties = new ObservableCollection<Difficulty>
            {
                Difficulty.TooEasy,
                Difficulty.Easy,
                Difficulty.Medium,
                Difficulty.Hard,
                Difficulty.TooHard
            };

            SubmitLogCommand = new RelayCommand(() => SaveLogAsync());
            DeleteLogCommand = new RelayCommand(() => DeleteTourLogAsync());
            EditLogCommand = new RelayCommand(() => EditTourLogAsync());
            ShowAddLogViewCommand = new RelayCommand(() => _mainViewModel.ShowAddTourLog());
            ShowEditLogViewCommand = new RelayCommand(() => _mainViewModel.ShowEditTourLog());

            _log.Info("Initialized TourLogViewModel");
        }

        public void UpdateTourLogDetails()
        {
            try
            {
                if (_mainViewModel.TourViewModel.SelectedTour == null)
                {
                    TourLogDetails?.Clear();
                    _log.Error("UpdateTourLogDetails returned early because the selected tour was null");
                    return;
                }

                if (AllTourLogs == null)
                {
                    TourLogDetails?.Clear();
                    _log.Error("UpdateTourLogDetails returned early because AllTourLogs was null");
                    return;
                }

                TourLogDetails = new ObservableCollection<Tourlog>(
                    AllTourLogs.Where(log => log.TourId == _mainViewModel.TourViewModel.SelectedTour.Id)
                );
                _log.Info("Successfully updated tourlog details");
            }
            catch (Exception ex)
            {
                _log.Error("An exception was thrown while trying to update tourlog details: ", ex);
                throw;
            }
        }

        public async Task SaveLogAsync()
        {
            try
            {
                var tourFromDb = await _tourService.GetTourById(_mainViewModel.TourViewModel.SelectedTour.Id);

                if (tourFromDb != null)
                {
                    NewTourLog.TourId = tourFromDb.Id;

                    string errMessage = _validator.ValidateTourlogInput(NewTourLog);

                    if (errMessage == "")
                    {
                        await _tourlogService.InsertTourLog(NewTourLog);
                        _log.Info("TourLogInput was valid");
                    }

                    else
                    {
                        MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _log.Error("TourLogInput was invalid");
                        return;
                    }
                }
                else
                    _log.Error("tourFromDB returned as null!");

                    var tourlogs = await _tourlogService.GetTourlogsAsync();
                AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
                _mainViewModel.ShowTourlogView();
                ClearInputs();
            }
            catch(Exception ex)
            {
                _log.Error("An exception occured while trying to save a tourlog: ", ex);
                throw;
            }
        }

        public async Task DeleteTourLogAsync()
        {
            try
            {
                if (SelectedTourLog != null)
                {
                    await _tourlogService.DeleteTourLog(SelectedTourLog);
                    var tourlogs = await _tourlogService.GetTourlogsAsync();
                    AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
                    _mainViewModel.ShowTourlogView();
                }
                _log.Error("SelectedTourLog was null!");
            }
            catch(Exception ex)
            {
                _log.Error("An exception was thrown trying to delete a tour: ", ex);
                throw;
            }
        }

        public async Task EditTourLogAsync()
        {
            try
            {
                if (SelectedTourLog != null)
                {
                    var tourlogFromDb = await _tourlogService.GetTourlogById(SelectedTourLog.TourLogId);
                    if (tourlogFromDb != null)
                    {
                        tourlogFromDb.Author = TourlogToEdit.Author;
                        tourlogFromDb.Date = DateTime.SpecifyKind(TourlogToEdit.Date, DateTimeKind.Utc);
                        tourlogFromDb.Difficulty = TourlogToEdit.Difficulty;
                        tourlogFromDb.TotalDistance = TourlogToEdit.TotalDistance;
                        tourlogFromDb.TotalTime = TourlogToEdit.TotalTime;
                        tourlogFromDb.Rating = TourlogToEdit.Rating;
                        tourlogFromDb.Comment = TourlogToEdit.Comment;

                        string errMessage = _validator.ValidateTourlogInput(tourlogFromDb);

                        if (errMessage == "")
                        {
                            await _tourlogService.EditTourLog(tourlogFromDb);
                            _log.Info("Tourlog input was valid!");
                        }

                        else
                        {
                            MessageBox.Show(errMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            _log.Error("Tourlog input was invalid!");
                            return;
                        }
                    }
                    else
                        _log.Error("tourlogFromDb was null!");

                    var tourlogs = await _tourlogService.GetTourlogsAsync();
                    AllTourLogs = new ObservableCollection<Tourlog>(tourlogs);
                    _mainViewModel.ShowTourlogView();
                    ClearInputs();
                }
                _log.Error("Selected tourlog was null!");
            }
            catch(Exception ex)
            {
                _log.Error("An exception was thrown trying to edit a tourlog: ", ex);
                throw;
            }
        }
        public void ClearInputs()
        {
            try
            {
                _addAuthor = "";
                _addComment = "";
                _addDistance = 0;
                _addTime = TimeSpan.Zero;
                _addRating = 5;
                _addDate = DateTime.Now;
            }
            catch(Exception ex)
            {
                _log.Error("An exception occured while trying to clear input fields: ", ex);
                throw;
            }
        }


        //input fields
        private string _addAuthor;
        private DateTime _addDate;
        private Difficulty _addDifficulty;
        private double _addDistance;
        private TimeSpan _addTime;
        private int _addRating;
        private string _addComment;

        private Tourlog _newTourLog = new Tourlog();
        private Tourlog _tourlogToEdit = new Tourlog();

        public Tourlog NewTourLog
        {
            get => _newTourLog;
            set
            {
                if (_newTourLog != null)
                {
                    OnPropertyChanged(nameof(NewTourLog));
                    OnPropertyChanged(nameof(AddAuthor));
                    OnPropertyChanged(nameof(AddDate));
                    OnPropertyChanged(nameof(AddDifficulty));
                    OnPropertyChanged(nameof(AddDistance));
                    OnPropertyChanged(nameof(AddTime));
                    OnPropertyChanged(nameof(AddRating));
                    OnPropertyChanged(nameof(AddComment));
                }
            }
        }

        public Tourlog TourlogToEdit
        {
            get => _tourlogToEdit;
            set
            {
                if (_tourlogToEdit != null)
                {
                    OnPropertyChanged(nameof(TourlogToEdit));
                    OnPropertyChanged(nameof(AddAuthor));
                    OnPropertyChanged(nameof(AddDate));
                    OnPropertyChanged(nameof(AddDifficulty));
                    OnPropertyChanged(nameof(AddDistance));
                    OnPropertyChanged(nameof(AddTime));
                    OnPropertyChanged(nameof(AddRating));
                    OnPropertyChanged(nameof(AddComment));
                }
            }
        }
        public ObservableCollection<Difficulty> Difficulties { get; }

        public string AddAuthor
        {
            get => _addAuthor;
            set
            {
                _addAuthor = value;
                _newTourLog.Author = value;
                _tourlogToEdit.Author = value;
                OnPropertyChanged(nameof(AddAuthor));
            }
        }
        public DateTime AddDate
        {
            get => _addDate;
            set
            {
                _addDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                _newTourLog.Date = _addDate;
                _tourlogToEdit.Date = _addDate;
                OnPropertyChanged(nameof(AddDate));
            }
        }
        public Difficulty AddDifficulty
        {
            get => _addDifficulty;
            set
            {
                _addDifficulty = value;
                _newTourLog.Difficulty = value;
                _tourlogToEdit.Difficulty = value;
                OnPropertyChanged(nameof(AddDifficulty));
            }
        }
        public double AddDistance
        {
            get => _addDistance;
            set
            {
                _addDistance = value;
                _newTourLog.TotalDistance = value;
                _tourlogToEdit.TotalDistance = value;
                OnPropertyChanged(nameof(AddDistance));
            }
        }
        public TimeSpan AddTime
        {
            get => _addTime;
            set
            {
                _addTime = value;
                _newTourLog.TotalTime = value;
                _tourlogToEdit.TotalTime = value;
                OnPropertyChanged(nameof(AddTime));
            }
        }
        public int AddRating
        {
            get => _addRating;
            set
            {
                _addRating = value;
                _newTourLog.Rating = value;
                _tourlogToEdit.Rating = value;
                OnPropertyChanged(nameof(AddRating));
            }
        }
        public string AddComment
        {
            get => _addComment;
            set
            {
                _addComment = value;
                _newTourLog.Comment = value;
                _tourlogToEdit.Comment = value;
                OnPropertyChanged(nameof(AddComment));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
