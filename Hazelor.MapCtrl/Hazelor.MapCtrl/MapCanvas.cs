using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Hazelor.MapCtrl
{
    public sealed partial class MapCanvas : Canvas
    {
        /// <summary>Identifies the Latitude attached property.</summary>
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.RegisterAttached("Latitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>Identifies the Longitude attached property.</summary>
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.RegisterAttached("Longitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>Identifies the Viewport dependency property.</summary>
        public static readonly DependencyProperty ViewportProperty;

        /// <summary>Identifies the Zoom dependency property.</summary>
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(int), typeof(MapCanvas), new UIPropertyMetadata(9, OnZoomPropertyChanged, OnZoomPropertyCoerceValue));

        private static readonly DependencyPropertyKey ViewportKey =
            DependencyProperty.RegisterReadOnly("Viewport", typeof(Rect), typeof(MapCanvas), new PropertyMetadata());

        private TilePanel _tilePanel = new TilePanel();
        private Image _cache = new Image();
        private int _updateCount;
        private bool _mouseCaptured;
        private Point _previousMouse;
        private MapOffset _offsetX;
        private MapOffset _offsetY;
        private TranslateTransform _translate = new TranslateTransform();

        static MapCanvas()
        {
            ViewportProperty = ViewportKey.DependencyProperty; // Need to set it here after ViewportKey has been initialized.

            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveDown, (sender, e) => Pan(sender, e.Command, 0, -1)));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveLeft, (sender, e) => Pan(sender, e.Command, 1, 0)));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveRight, (sender, e) => Pan(sender, e.Command, -1, 0)));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveUp, (sender, e) => Pan(sender, e.Command, 0, 1)));

            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(NavigationCommands.DecreaseZoom, (sender, e) => ((MapCanvas)sender).Zoom--));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(NavigationCommands.IncreaseZoom, (sender, e) => ((MapCanvas)sender).Zoom++));
        }

    }
}
