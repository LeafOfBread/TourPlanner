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
using TourPlanner.UI.HelperClasses;
using TourPlannerClasses.Models;

namespace TourPlanner.Views
{
    /// <summary>
    /// Interaction logic for TourListView.xaml
    /// </summary>
    public partial class TourListView : UserControl
    {
        public TourListView()
        {
            InitializeComponent();
        }

        public Tours SelectedTour
        {
            get { return (Tours)GetValue(SelectedTourProperty); }
            set { SetValue(SelectedTourProperty, value); }
        }

        public static readonly DependencyProperty SelectedTourProperty =
            DependencyProperty.Register("SelectedTour", typeof(Tours), typeof(TourListView), new PropertyMetadata(null, OnSelectedTourChanged));

        private static void OnSelectedTourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TourListView tourListView && e.NewValue is Tours newTour)
            {
                tourListView.TourList.ItemsSource = new List<Tours> { newTour };
            }
        }

        private void TourListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListViewHelper.AdjustColumnsWidth(TourList);
        }
    }
}
