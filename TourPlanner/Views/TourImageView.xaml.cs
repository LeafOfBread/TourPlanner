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
        public TourImageView()
        {
            InitializeComponent();
            this.Loaded += TourImageView_Loaded;
            this.DataContext = this;
        }

        private async void TourImageView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();

                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string relativePath = Path.Combine("..", "..", "..", "Views", "Map", "map.html");
                string absolutePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));


                if (File.Exists(absolutePath))
                {
                    webView.Source = new Uri(absolutePath);
                }
                else
                {
                    MessageBox.Show($"File not found: {absolutePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Microsoft.Web.WebView2.Wpf.WebView2 Browser => webView;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
