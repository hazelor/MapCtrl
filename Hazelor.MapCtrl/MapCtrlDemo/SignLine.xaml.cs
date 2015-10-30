using Hazelor.MapCtrl.Interfaces;
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

namespace MapCtrlDemo
{
    /// <summary>
    /// Interaction logic for SignLine.xaml
    /// </summary>
    public partial class SignLine : UserControl, ILineElement
    {
        public SignLine()
        {
            InitializeComponent();
        }

        public Line LineObject { get{return this.TrajectoryLine;} }
    }
}
