using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ComponentModel;

namespace TourPlanner.Views
{
    /// <summary>
    /// Interaction logic for TourImageView.xaml
    /// </summary>
    public partial class TourImageView : UserControl, INotifyPropertyChanged
    {
        private string _sourceUri;

        public string SourceUri
        {
            get => _sourceUri;
            set
            {
                _sourceUri = value;
                OnPropertyChanged(nameof(SourceUri));
            }
        }

        public TourImageView()
        {
            InitializeComponent();
            SourceUri = GetSourceUri();
            this.DataContext = this;
        }

        public static string GetSourceUri()
        {
            return Path.GetFullPath("../../../Views/Images/placeholder1.jpg");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
