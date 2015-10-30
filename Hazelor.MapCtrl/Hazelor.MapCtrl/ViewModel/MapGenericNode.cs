using Hazelor.MapCtrl.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Hazelor.MapCtrl.ViewModel
{
    public class MapGenericNode : INotifyPropertyChanged, ISingleObjectContext
    {
        double _Longtitude;
        double _Latitude;

        public double Longitude { get { return this._Longtitude; } set { this._Longtitude = value; this.OnPropertyChanged("longtitude"); } }
        public double Latitude { get { return this._Latitude; } set { this._Latitude = value; this.OnPropertyChanged("latitude"); } }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

    }
}
