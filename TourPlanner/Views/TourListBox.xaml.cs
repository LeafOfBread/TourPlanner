using System;
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
using System.Windows.Shapes;
using TourPlannerClasses.Models;

namespace TourPlanner.Views
{
    /// <summary>
    /// Interaction logic for TourListBox.xaml
    /// </summary>
    public partial class TourListBox : UserControl
    {
        public TourListBox()
        {
            InitializeComponent();
        }

        public Tours SelectedTour
        {
            get { return (Tours)GetValue(SelectedTourProperty); }
            set { SetValue(SelectedTourProperty, value); }
        }

        private static readonly DependencyProperty SelectedTourProperty
            = DependencyProperty.Register("SelectedTour", typeof(Tours), typeof(TourListBox), new PropertyMetadata(null));


        private void DisplayTourDetails(object sender, RoutedEventArgs e)
        {
            if (TourList.SelectedItem is Tours selectedTour)
            {
                TourList.ItemsSource = new List<Tours> { selectedTour };
            }
        }

    }
}
