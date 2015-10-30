using Hazelor.MapCtrl.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static readonly DependencyProperty EndLatitudeProperty =
           DependencyProperty.RegisterAttached("EndLatitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>Identifies the Longitude attached property.</summary>
        public static readonly DependencyProperty EndLongitudeProperty =
            DependencyProperty.RegisterAttached("EndLongitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));


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
        public MapCanvas()
        {
            _offsetX = new MapOffset(_translate.GetType().GetProperty("X"), this.OnOffsetChanged);
            _offsetY = new MapOffset(_translate.GetType().GetProperty("Y"), this.OnOffsetChanged);

            _tilePanel.RenderTransform = _translate;
            this.Background = Brushes.Transparent; // Register all mouse clicks
            this.Children.Add(_cache);
            this.Children.Add(_tilePanel);
            this.ClipToBounds = true;
            this.Focusable = true;
            this.FocusVisualStyle = null;
            this.SnapsToDevicePixels = true;

            this.Cursor = Cursors.Arrow;

        }
        public Rect Viewport
        {
            get { return (Rect)this.GetValue(ViewportProperty); }
            private set { this.SetValue(ViewportKey, value); }
        }

        /// <summary>Gets or sets the zoom level of the map.</summary>
        public int Zoom
        {
            get { return (int)this.GetValue(ZoomProperty); }
            set { this.SetValue(ZoomProperty, value); }
        }
        /// <summary>Creates a static image of the current view.</summary>
        /// <returns>An image of the current map.</returns>
        public ImageSource CreateImage()
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Default);
            bitmap.Render(_tilePanel);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>Gets the value of the Latitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element from which the property value is read.</param>
        /// <returns>The Latitude coordinate of the specified element.</returns>
        public static double GetLatitude(DependencyObject obj)
        {
            
            return (double)obj.GetValue(LatitudeProperty);
        }

        /// <summary>
        /// 获取EndLatitude的value,为Line类型obj提供
        /// </summary>
        /// <param name="obj">Line类型的输入对象</param>
        /// <returns>The EndLatitude coordinate of the specified element.</returns>
        public static double GetEndLatitude(DependencyObject obj)
        {

            return (double)obj.GetValue(EndLatitudeProperty);
        }

        /// <summary>Gets the value of the Longitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element from which the property value is read.</param>
        /// <returns>The Longitude coordinate of the specified element.</returns>
        public static double GetLongitude(DependencyObject obj)
        {
            
            return (double)obj.GetValue(LongitudeProperty);
        }

        /// <summary>
        /// 获取EndLongitude的value,为Line类型obj提供
        /// </summary>
        /// <param name="obj">Line类型的输入对象</param>
        /// <returns>The EndLatitude coordinate of the specified element.</returns>
        public static double GetEndLongitude(DependencyObject obj)
        {

            return (double)obj.GetValue(EndLongitudeProperty);
        }

        /// <summary>Sets the value of the Latitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the Latitude coordinate of the specified element.</param>
        public static void SetLatitude(DependencyObject obj, double value)
        {
            obj.SetValue(LatitudeProperty, value);
        }

        /// <summary>Sets the value of the EndLatitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the EndLatitude coordinate of the specified element.</param>
        public static void SetEndLatitude(DependencyObject obj, double value)
        {
            obj.SetValue(EndLatitudeProperty, value);
        }


        /// <summary>Sets the value of the Longitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the Longitude coordinate of the specified element.</param>
        public static void SetLongitude(DependencyObject obj, double value)
        {
            obj.SetValue(LongitudeProperty, value);
        }

        /// <summary>Sets the value of the EndLongitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the EndLongitude coordinate of the specified element.</param>
        public static void SetEndLongitude(DependencyObject obj, double value)
        {
            obj.SetValue(EndLongitudeProperty, value);
        }


        /// <summary>Centers the map on the specified coordinates.</summary>
        /// <param name="latitude">The latitude cooridinate.</param>
        /// <param name="longitude">The longitude coordinates.</param>
        /// <param name="zoom">The zoom level for the map.</param>
        public void Center(double latitude, double longitude, int zoom)
        {
            this.BeginUpdate();
            this.Zoom = zoom;
            _offsetX.CenterOn(TileGenerator.GetTileX(longitude, this.Zoom));
            _offsetY.CenterOn(TileGenerator.GetTileY(latitude, this.Zoom));
            this.EndUpdate();
        }
        /// <summary>Centers the map on the specified coordinates, calculating the required zoom level.</summary>
        /// <param name="latitude">The latitude cooridinate.</param>
        /// <param name="longitude">The longitude coordinates.</param>
        /// <param name="size">The minimum size that must be visible, centered on the coordinates.</param>
        public void Center(double latitude, double longitude, Size size)
        {
            double left = TileGenerator.GetTileX(longitude - (size.Width / 2.0), 0);
            double right = TileGenerator.GetTileX(longitude + (size.Width / 2.0), 0);
            double top = TileGenerator.GetTileY(latitude - (size.Height / 2.0), 0);
            double bottom = TileGenerator.GetTileY(latitude + (size.Height / 2.0), 0);

            double height = (top - bottom) * TileGenerator.TileSize;
            double width = (right - left) * TileGenerator.TileSize;
            int zoom = Math.Min(TileGenerator.GetZoom(this.ActualHeight / height), TileGenerator.GetZoom(this.ActualWidth / width));
            this.Center(latitude, longitude, zoom);
        }
        private static void OnLatitudeLongitudePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Search for a MapControl parent
            MapCanvas canvas = null;
            FrameworkElement child = d as FrameworkElement;
            while (child != null)
            {
                canvas = child as MapCanvas;
                if (canvas != null)
                {
                    break;
                }
                child = child.Parent as FrameworkElement;
            }
            if (canvas != null)
            {
                canvas.RepositionChildren();
            }
        }

        private static void OnZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MapCanvas)d).OnZoomChanged();
        }

        private static object OnZoomPropertyCoerceValue(DependencyObject d, object baseValue)
        {
            return TileGenerator.GetValidZoom((int)baseValue);
        }
        
        private static bool IsKeyboardCommand(RoutedCommand command)
        {
            foreach (var gesture in command.InputGestures)
            {
                var key = gesture as KeyGesture;
                if (key != null)
                {
                    if (Keyboard.IsKeyDown(key.Key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static void Pan(object sender, ICommand command, double x, double y)
        {
            if (!IsKeyboardCommand((RoutedCommand)command)) // Move a whole square instead of a pixel if it wasn't the keyboard who sent it
            {
                x *= TileGenerator.TileSize;
                y *= TileGenerator.TileSize;
            }
            MapCanvas instance = (MapCanvas)sender;
            instance._offsetX.AnimateTranslate(x);
            instance._offsetY.AnimateTranslate(y);
            instance.Focus();
        }

        private void OnOffsetChanged(object sender, EventArgs e)
        {
            this.BeginUpdate();
            MapOffset offset = (MapOffset)sender;
            offset.Property.SetValue(_translate, offset.Offset, null);
            this.EndUpdate();
        }

        private void OnZoomChanged()
        {
            this.BeginUpdate();
            _offsetX.ChangeZoom(this.Zoom, this.ActualWidth / 2.0);
            _offsetY.ChangeZoom(this.Zoom, this.ActualHeight / 2.0);
            _tilePanel.Zoom = this.Zoom;
            this.EndUpdate();
        }
        private void BeginUpdate()
        {
            _updateCount++;
        }


        private void EndUpdate()
        {
            System.Diagnostics.Debug.Assert(_updateCount != 0, "Must call BeginUpdate first");
            if (--_updateCount == 0)
            {
                _tilePanel.LeftTile = _offsetX.Tile;
                _tilePanel.TopTile = _offsetY.Tile;
                if (_tilePanel.RequiresUpdate)
                {
                    _cache.Visibility = Visibility.Visible; // Display a pretty picture while we play with the tiles
                    _tilePanel.Update(); // This will block our thread for a while (UI events will still be processed)
                    this.RepositionChildren();
                    _cache.Visibility = Visibility.Hidden;
                    _cache.Source = this.CreateImage(); // Save our image for later
                }

                // Update viewport
                Point topleft = this.GetLocation(new Point(0, 0));
                Point bottomRight = this.GetLocation(new Point(this.ActualWidth, this.ActualHeight));
                this.Viewport = new Rect(topleft, bottomRight);
            }
        }
        public Point GetLocation(Point point)
        {
            Point output = new Point();
            output.X = TileGenerator.GetLongitude((_offsetX.Pixels + point.X) / TileGenerator.TileSize, this.Zoom);
            output.Y = TileGenerator.GetLatitude((_offsetY.Pixels + point.Y) / TileGenerator.TileSize, this.Zoom);
            return output;
        }
        private void RepositionChildren()
        {
            foreach (UIElement element in this.Children)
            {
                double latitude = GetLatitude(element);
                double longitude = GetLongitude(element);
                if (latitude != double.PositiveInfinity && longitude != double.PositiveInfinity)
                {
                    double x = (TileGenerator.GetTileX(longitude, this.Zoom) - _offsetX.Tile) * TileGenerator.TileSize;
                    double y = (TileGenerator.GetTileY(latitude, this.Zoom) - _offsetY.Tile) * TileGenerator.TileSize;
                    Canvas.SetLeft(element, x);
                    Canvas.SetTop(element, y);
                    element.RenderTransform = _translate;

                    //for line type object
                    double endlatitude = GetEndLatitude(element);
                    double endlongitude = GetEndLongitude(element);
                    ILineElement eline = element as ILineElement;
                    if (endlatitude != double.PositiveInfinity && endlongitude != double.PositiveInfinity && eline!=null)
                    {
                        Point startpos = this.ConverterPosition(new Point { X = longitude, Y = latitude });
                        Point endpos = this.ConverterPosition(new Point { X = endlongitude, Y = endlatitude });
                        eline.LineObject.X2 = endpos.X - startpos.X;
                        eline.LineObject.Y2 = endpos.Y - startpos.Y;
                    }
                }
            }
        }
        public void MoveChild(UIElement child, Point pos)
        {
            if (pos.X != double.PositiveInfinity && pos.Y != double.PositiveInfinity)
            {
                double tx = (TileGenerator.GetTileX(pos.X, this.Zoom) - _offsetX.Tile) * TileGenerator.TileSize;
                double ty = (TileGenerator.GetTileY(pos.Y, this.Zoom) - _offsetY.Tile) * TileGenerator.TileSize;
                Canvas.SetLeft(child, tx);
                Canvas.SetTop(child, ty);
                child.RenderTransform = _translate;
            }
        }
        //从经纬度转换到当前坐标系统值
        public Point ConverterPosition(Point pos)
        {
            Point curpos = new Point();
            curpos.X = (TileGenerator.GetTileX(pos.X, this.Zoom) - _offsetX.Tile) * TileGenerator.TileSize;
            curpos.Y = (TileGenerator.GetTileY(pos.Y, this.Zoom) - _offsetY.Tile) * TileGenerator.TileSize;
            return curpos;
        }

        private double rad(double deg)
        {
            return deg / 180 * Math.PI;
        }

        #region Mouse Progress
        private DateTime _ClickedTime;

        /// <summary>Tries to capture the mouse to enable dragging of the map.</summary>
        /// <param name="e">The MouseButtonEventArgs that contains the event data.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.Focus(); // Make sure we get the keyboard
            if (this.CaptureMouse())
            {
                this.Cursor = Cursors.ScrollAll;
                _mouseCaptured = true;
                _previousMouse = e.GetPosition(null);
                var element = (FrameworkElement)this;
                if (e.ClickCount == 1)
                {
                    var timer = new System.Timers.Timer(500);
                    timer.AutoReset = false;
                    timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                    {
                        var timer2 = (System.Timers.Timer)element.Tag;
                        timer2.Stop();
                        timer2.Dispose();
                        //单击
                    })));
                    timer.Start();
                    element.Tag = timer;
                }
                if (e.ClickCount > 1)
                {
                    var timer = element.Tag as System.Timers.Timer;
                    if (timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                        UIElement_DoubleClick(e);
                    }
                }
            }
        }
        private void UIElement_DoubleClick(MouseButtonEventArgs e)
        {
            Point cpos = e.GetPosition(this);
            cpos = GetLocation(cpos);
            //TrajNode node = new TrajNode();
            //node.Longitude = cpos.X;
            //node.Latitude = cpos.Y;
            //node.FrontNode = null;
            //node.LineCircle = 0;
            //node.LineSwitch = 0;
            //node.TaskCommand = 0;

            //if (IsNodesEditable)
            //{

            //    this._eventAggregator.GetEvent<TrajNodeEditedEvent>().Publish(new TrajNodeEditedInfo { Opt = TrajNodeEditedInfo.Operation.Add, Node = node });

            //    //test for draw real line


            //}
            //if (_IsMeasureDis)
            //{
            //    SiteMark siteMark = new SiteMark();
            //    siteMark.Longitude = node.Longitude;
            //    siteMark.Latitude = node.Latitude;

            //    this.Children.Add(siteMark);
            //    this.MoveChild(siteMark, new Point { X = siteMark.Longitude, Y = siteMark.Latitude });

            //    _mline.AddNode(node);

            //    siteMark.Distance = _mline.GetDistance();

            //    this._eventAggregator.GetEvent<UpdateDisEvent>().Publish(_mline.GetDistance());
            //}
            //OnLineNodeChanged(node);
        }
        /// <summary>Releases the mouse capture and stops dragging of the map.</summary>
        /// <param name="e">The MouseButtonEventArgs that contains the event data.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.ReleaseMouseCapture();
            _mouseCaptured = false;

            this.Cursor = Cursors.Arrow;
        }
        /// <summary>Drags the map, if the mouse was succesfully captured.</summary>
        /// <param name="e">The MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseCaptured)
            {
                this.BeginUpdate();
                Point position = e.GetPosition(null);
                _offsetX.Translate(position.X - _previousMouse.X);
                _offsetY.Translate(position.Y - _previousMouse.Y);
                _previousMouse = position;
                this.EndUpdate();
            }
        }

        /// <summary>Alters the zoom of the map, maintaing the same point underneath the mouse at the new zoom level.</summary>
        /// <param name="e">The MouseWheelEventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            int newZoom = TileGenerator.GetValidZoom(this.Zoom + (e.Delta / Mouse.MouseWheelDeltaForOneLine));
            Point mouse = e.GetPosition(this);

            this.BeginUpdate();
            _offsetX.ChangeZoom(newZoom, mouse.X);
            _offsetY.ChangeZoom(newZoom, mouse.Y);
            this.Zoom = newZoom; // Set this after we've altered the offsets
            this.EndUpdate();
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.BeginUpdate();
            _offsetX.ChangeSize(sizeInfo.NewSize.Width);
            _offsetY.ChangeSize(sizeInfo.NewSize.Height);
            _tilePanel.Width = sizeInfo.NewSize.Width;
            _tilePanel.Height = sizeInfo.NewSize.Height;
            this.EndUpdate();
        }

        #endregion

        #region common methods

        /// <summary>
        ///     双击事件句柄，当在地图上左键双击时响应执行
        /// </summary>
        public event EventHandler<MapEventArgs> DoubleLeftClickedHandler;

        /// <summary>
        ///     双击事件句柄,当在地图上右键双击时响应执行
        /// </summary>
        public event EventHandler<MapEventArgs> DoubleRightClickedHandler;
        #endregion

        #region Single Object
        /// <summary>
        ///     添加单一图标
        /// </summary>
        /// <param name="key">控件名称,用于索引控件</param>
        /// <param name="obj">添加的图标控件</param>
        /// <param name="dataContext">空间DataContext,用于提供地图上显示的坐标信息及其他</param>
        public void AddSingleObject(string key, FrameworkElement obj, ISingleObjectContext dataContext)
        {
            System.Diagnostics.Debug.Assert(dataContext != null, "Cannot pass in null values.");
            System.Diagnostics.Debug.Assert(obj != null, "Cannot pass in null values.");
            System.Diagnostics.Debug.Assert(key != null, "Cannot pass in null values.");

            obj.Name = key;
            obj.DataContext = dataContext;
            this.Children.Add(obj);

            Binding LatitudeBind = new Binding();
            LatitudeBind.Source = dataContext;
            LatitudeBind.Path = new PropertyPath("Latitude");
            LatitudeBind.Mode = BindingMode.TwoWay;
            obj.SetBinding(MapCanvas.LatitudeProperty, LatitudeBind);
            Binding LongitudeBind = new Binding();
            LongitudeBind.Source = dataContext;
            LongitudeBind.Path = new PropertyPath("Longitude");
            LongitudeBind.Mode = BindingMode.TwoWay;
            obj.SetBinding(MapCanvas.LongitudeProperty, LongitudeBind);
            
            
        }

        /// <summary>
        ///     添加Line图标
        /// </summary>
        /// <param name="key">控件名称，用于索引控件</param>
        /// <param name="obj">添加的Line控件</param>
        /// <param name="dataContext">DataContext,用于提供地图上显示的坐标信息及其他</param>
        public void AddLineObject(string key, FrameworkElement obj, ILineOjbectContext dataContext)
        {
            System.Diagnostics.Debug.Assert(dataContext != null, "Cannot pass in null values.");
            System.Diagnostics.Debug.Assert(obj != null, "Cannot pass in null values.");
            System.Diagnostics.Debug.Assert(key != null, "Cannot pass in null values.");

            obj.Name = key;
            obj.DataContext = dataContext;
            this.Children.Add(obj);

            Binding LatitudeBind = new Binding();
            LatitudeBind.Source = dataContext;
            LatitudeBind.Path = new PropertyPath("Latitude");
            LatitudeBind.Mode = BindingMode.TwoWay;
            obj.SetBinding(MapCanvas.LatitudeProperty, LatitudeBind);
            Binding LongitudeBind = new Binding();
            LongitudeBind.Source = dataContext;
            LongitudeBind.Path = new PropertyPath("Longitude");
            LongitudeBind.Mode = BindingMode.TwoWay;
            obj.SetBinding(MapCanvas.LongitudeProperty, LongitudeBind);

            Binding EndLatitudeBind = new Binding();
            EndLatitudeBind.Source = dataContext;
            EndLatitudeBind.Path = new PropertyPath("EndLatitude");
            EndLatitudeBind.Mode = BindingMode.TwoWay;
            obj.SetBinding(MapCanvas.EndLatitudeProperty, EndLatitudeBind);
            Binding EndLongitudeBind = new Binding();
            EndLongitudeBind.Source = dataContext;
            EndLongitudeBind.Path = new PropertyPath("EndLongitude");
            EndLongitudeBind.Mode = BindingMode.TwoWay;
            obj.SetBinding(MapCanvas.EndLongitudeProperty, EndLongitudeBind);
        }

        /// <summary>
        /// 删除添加的子控件
        /// </summary>
        /// <param name="key">子控件的名称</param>
        public void DelSubObject(string key)
        {
            System.Diagnostics.Debug.Assert(key != null, "Cannot pass in null values.");
            for (int i = 0; i < this.Children.Count; i++)
            {
                FrameworkElement f = this.Children[i] as FrameworkElement;
                if (f!=null && f.Name == key)
                {
                    this.Children.RemoveAt(i);
                    return;
                }
            }
        }
        #endregion

        #region MapOffset
        private class MapOffset
        {
            private EventHandler _offsetChanged;

            private double _mapSize = TileGenerator.TileSize; // Default to zoom 0
            private double _offset;
            private double _size;
            private int _maximumTile;
            private int _tile;

            // Used for animation
            private bool _animating;
            private double _step;
            private double _target;
            private double _value;

            /// <summary>Initializes a new instance of the MapOffset class.</summary>
            /// <param name="property">The property this MapOffset represents.</param>
            /// <param name="offsetChanged">Called when the Offset changes.</param>
            internal MapOffset(PropertyInfo property, EventHandler offsetChanged)
            {
                System.Diagnostics.Debug.Assert(property != null, "property cannot be null");
                System.Diagnostics.Debug.Assert(offsetChanged != null, "offsetChanged cannot be null");

                _offsetChanged = offsetChanged;
                this.Property = property;
                this.Frames = 24;
                CompositionTarget.Rendering += this.OnRendering; // Used for manual animation
            }

            /// <summary>Gets or sets the number of steps when animating.</summary>
            public int Frames { get; set; }

            /// <summary>Gets the offset from the tile edge to the screen edge.</summary>
            public double Offset
            {
                get
                {
                    return _offset;
                }
                private set
                {
                    if (_offset != value)
                    {
                        _offset = value;
                        _offsetChanged(this, EventArgs.Empty);
                    }
                }
            }

            /// <summary>Gets the location of the starting tile in pixels.</summary>
            public double Pixels
            {
                get { return (this.Tile * TileGenerator.TileSize) - this.Offset; }
            }

            /// <summary>Gets the PropertyInfo associated with this offset.</summary>
            /// <remarks>This is used so a generic handler can be called for the _offsetChanged delegate.</remarks>
            public PropertyInfo Property { get; private set; }

            /// <summary>Gets the starting tile index.</summary>
            public int Tile
            {
                get
                {
                    return _tile;
                }
                private set
                {
                    if (_tile != value)
                    {
                        _tile = value;
                    }
                }
            }

            /// <summary>Smoothly translates by the specified amount.</summary>
            /// <param name="value">The total distance to translate.</param>
            public void AnimateTranslate(double value)
            {
                if (value == 0)
                {
                    _animating = false;
                }
                else
                {
                    _value = 0;
                    if (value < 0)
                    {
                        _target = -value;
                        _step = Math.Min(value / this.Frames, -1.0);
                    }
                    else
                    {
                        _target = value;
                        _step = Math.Max(value / this.Frames, 1);
                    }
                    _animating = true;
                }
            }

            /// <summary>Adjusts the offset so the specifed tile is in the center of the control.</summary>
            /// <param name="tile">The tile (allowing fractions of the tile) to be centered.</param>
            public void CenterOn(double tile)
            {
                double pixels = (tile * TileGenerator.TileSize) - (_size / 2.0);
                this.Translate(this.Pixels - pixels);
            }

            /// <summary>Called when the size of the parent control changes.</summary>
            /// <param name="size">The nes size of the parent control.</param>
            public void ChangeSize(double size)
            {
                _size = size;
                _maximumTile = (int)((_mapSize - _size) / TileGenerator.TileSize); // Only interested in the integer part, the rest will be truncated
                this.Translate(0); // Force a refresh
            }

            /// <summary>Updates the starting tile index based on the zoom level.</summary>
            /// <param name="zoom">The zoom level.</param>
            /// <param name="offset">The distance from the edge to keep the same when changing zoom.</param>
            public void ChangeZoom(int zoom, double offset)
            {
                int currentZoom = TileGenerator.GetZoom(_mapSize / TileGenerator.TileSize);
                if (currentZoom != zoom)
                {
                    _animating = false;

                    double scale = Math.Pow(2, zoom - currentZoom); // 2^delta
                    double location = ((this.Pixels + offset) * scale) - offset; // Bias new location on the offset

                    _mapSize = TileGenerator.GetSize(zoom);
                    _maximumTile = (int)((_mapSize - _size) / TileGenerator.TileSize);

                    this.Translate(this.Pixels - location);
                }
            }

            /// <summary>Changes the offset by the specified amount.</summary>
            /// <param name="value">The amount to change the offset by.</param>
            public void Translate(double value)
            {
                if (_size > _mapSize)
                {
                    this.Tile = 0;
                    this.Offset = (_size - _mapSize) / 2;
                }
                else
                {
                    double location = this.Pixels - value;
                    if (location < 0)
                    {
                        this.Tile = 0;
                        this.Offset = 0;
                    }
                    else if (location + _size > _mapSize)
                    {
                        this.Tile = _maximumTile;
                        this.Offset = _size - (_mapSize - (_maximumTile * TileGenerator.TileSize));
                    }
                    else
                    {
                        this.Tile = (int)(location / TileGenerator.TileSize);
                        this.Offset = (this.Tile * TileGenerator.TileSize) - location;
                    }
                }
            }

            // Used for animating the Translate.
            private void OnRendering(object sender, EventArgs e)
            {
                if (_animating)
                {
                    this.Translate(_step);
                    _value += Math.Abs(_step);
                    _animating = _value < _target; // Stop animating once we've reached/exceeded the target
                }
            }
           

        }
        #endregion
    }
}
