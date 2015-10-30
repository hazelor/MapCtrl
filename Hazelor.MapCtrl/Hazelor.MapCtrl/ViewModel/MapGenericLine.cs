using Hazelor.MapCtrl.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelor.MapCtrl.ViewModel
{
    public class MapGenericLine : MapGenericNode, ILineOjbectContext
    {
        double _EndLongitude;
        double _EndLatitude;

        public double EndLongitude { 
            get { 
                return this._EndLongitude;
            } 
            set {
                this._EndLongitude = value;
                this.OnPropertyChanged("EndLongitude");
            }
        }
        public double EndLatitude {
            get { 
                return this._EndLatitude; 
            } 
            set { 
                this._EndLatitude = value;
                this.OnPropertyChanged("EndLatitude");
            } 
        }


    }
}
