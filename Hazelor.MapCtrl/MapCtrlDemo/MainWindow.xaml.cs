using Hazelor.MapCtrl;
using Hazelor.MapCtrl.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MapCtrlDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            TileGenerator.CacheFolder = @"Maps.db";
            TileGenerator.IsDBCaches = true;
            TileGenerator.DownloadCountChanged += this.OnDownloadCountChanged;
            TileGenerator.DownloadError += this.OnDownloadError;
            InitializeComponent();
        }

        private void OnDownloadCountChanged(object sender, EventArgs e)
        {
        }
        private void OnDownloadError(object sender, EventArgs e)
        {
        }

        private void OnLoadMap(object sender, RoutedEventArgs e)
        {
            this.tileCanvas.Center(44.2829, 115.8901, 11);
        }
        private void OnCloseMap(object sender, EventArgs e)
        {
            TileGenerator.DestructMap();
        }


        private MapGenericLine n = new MapGenericLine();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            n.Longitude = 115.8901;
            n.Latitude = 44.2829;

            n.EndLatitude = n.Latitude + 0.1;
            n.EndLongitude = n.Longitude + 0.1;

            SignLine s = new SignLine();

            this.tileCanvas.AddLineObject("test", s, n);
            //this.tileCanvas.Children.Add(s);

            ////MapCanvas.SetLatitude(s, n.latitude);
            ////MapCanvas.SetLongitude(s, n.longtitude);
            //Binding LatitudeBind = new Binding();
            //LatitudeBind.Source = n;
            //LatitudeBind.Path = new PropertyPath("latitude");
            //LatitudeBind.Mode = BindingMode.TwoWay;
            //s.SetBinding(MapCanvas.LatitudeProperty, LatitudeBind);
            //Binding LongitudeBind = new Binding();
            //LongitudeBind.Source = n;
            //LongitudeBind.Path = new PropertyPath("longtitude");
            //LongitudeBind.Mode = BindingMode.TwoWay;
            //s.SetBinding(MapCanvas.LongitudeProperty, LongitudeBind);
            
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            this.tileCanvas.DelSubObject("test");
            //n.longtitude += 0.1;
            //n.latitude += 0.1;
            //sign s = new sign();
            //this.tileCanvas.Children.Add(s);

            //MapCanvas.SetLatitude(s, n.latitude);
            //MapCanvas.SetLongitude(s, n.longtitude);
            //Binding LatitudeBind = new Binding();
            //LatitudeBind.Source = n;
            //LatitudeBind.Path = new PropertyPath("Latitude");
            //LatitudeBind.Mode = BindingMode.TwoWay;
            //s.SetBinding(MapCanvas.LatitudeProperty, LatitudeBind);
            //Binding LongitudeBind = new Binding();
            //LongitudeBind.Source = n;
            //LongitudeBind.Path = new PropertyPath("Longitude");
            //LongitudeBind.Mode = BindingMode.TwoWay;
            //s.SetBinding(MapCanvas.LongitudeProperty, LongitudeBind);

        }
        
    }

    
}
