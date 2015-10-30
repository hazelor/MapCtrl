using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelor.MapCtrl.Interfaces
{
    public interface ILineOjbectContext 
    {
        double Latitude { get; set; }
        double Longitude { get; set; }

        double EndLatitude { get; set; }
        double EndLongitude { get; set; }
    }
}
