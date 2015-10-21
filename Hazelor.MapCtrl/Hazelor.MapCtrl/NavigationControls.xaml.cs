using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hazelor.MapCtrl
{
    /// <summary>
    /// Interaction logic for NavigationControls.xaml
    /// </summary>
    public partial class NavigationControls : UserControl
    {
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(MapCanvas), typeof(NavigationControls));

        /// <summary>Gets or sets the map control which will be used as a CommandTarget.</summary>
        public MapCanvas Map
        {
            get { return (MapCanvas)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }
        public NavigationControls()
        {
            InitializeComponent();
        }
    }
}
