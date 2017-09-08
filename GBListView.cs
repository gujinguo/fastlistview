using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastGrid
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:FastGrid"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:FastGrid;assembly=FastGrid"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误: 
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:GBListView/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_GridView", Type = typeof(GB_GridView))]
    [TemplatePart(Name = "PART_GridHeader", Type = typeof(GB_GridHeader))]
    [TemplatePart(Name = "PART_HScrollBar", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_VScrollBar", Type = typeof(ScrollBar))]
    public class GBListView : Control
    {
        static GBListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GBListView), new FrameworkPropertyMetadata(typeof(GBListView)));
        }

        public GBListView()
        {
            Columns = new GridViewColumnCollection();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            AdjustScrollBars();
        }

        private void AdjustScrollBars()
        {
            if (_gridview == null)
                return;
            // 这里得解决ViewPort可能发生的更改
            _viewport.Width = _gridview.ActualWidth;
            _viewport.Height = _gridview.ActualHeight + _gridheader.ActualHeight;
            _extent.Height = _gridview.ExtentHeight + _gridheader.ActualHeight;
            _extent.Width = _gridheader.ExtentWidth - _gridview.ActualWidth > 0 ? _gridheader.ExtentWidth : _gridview.ActualWidth; 
            _hscrollbar.ViewportSize = _gridview.ActualWidth;
            _hscrollbar.Maximum = _extent.Width - _gridview.ActualWidth;
            _hscrollbar.LargeChange = _gridview.ActualWidth;
            _vscrollbar.LargeChange = _gridview.ActualHeight; // 翻页的单位

            if (_hscrollbar.Maximum < m_ptOffset.X)
                SetHorizontalOffset(_hscrollbar.Maximum);
            _hscrollbar.Value = m_ptOffset.X;
            _hscrollbar.SmallChange = 20;
            if (_hscrollbar.Maximum <= 0)
                _hscrollbar.Visibility = Visibility.Collapsed;
            else
            {
                if (_hscrollbar.Visibility == Visibility.Collapsed)
                {
                    _hscrollbar.Visibility = Visibility.Visible;
                    // 这里说明ViewPortSize更改了，则要重新处理下
                    this.Dispatcher.BeginInvoke(new Action(() =>{ AdjustScrollBars(); }));
                }

            }

            _vscrollbar.SmallChange = 23; // 向下滚动的单位
  
            _vscrollbar.ViewportSize = _gridview.ActualHeight + _gridheader.ActualHeight;
            _vscrollbar.Maximum = _extent.Height - _vscrollbar.ViewportSize;

            if (_vscrollbar.Maximum <= m_ptOffset.Y)
                SetVerticalOffset(_vscrollbar.Maximum);
            _vscrollbar.Value = m_ptOffset.Y;
            if (_vscrollbar.Maximum <= 0)
                _vscrollbar.Visibility = Visibility.Collapsed;
            else
            {
                if (_vscrollbar.Visibility == Visibility.Collapsed)
                {
                    _vscrollbar.Visibility = Visibility.Visible;
                    this.Dispatcher.BeginInvoke(new Action(() => { AdjustScrollBars(); }));
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _gridview = this.Template.FindName("PART_GridView", this) as GB_GridView;
            _gridheader = this.Template.FindName("PART_GridHeader", this) as GB_GridHeader;
            _gridheader.GridView = _gridview;
            _gridheader.OnExtentViewChanged += _gridheader_OnExtentWidthChanged;
            _gridview.OnExtentViewChanged += _gridheader_OnExtentWidthChanged;
            _gridview.ShowInGroup = ShowInGroup;
            _scrollviewer = this.Template.FindName("PART_ScrollViewer", this) as ScrollViewer;
            _hscrollbar = this.Template.FindName("PART_HScrollBar", this) as ScrollBar;
            _vscrollbar = this.Template.FindName("PART_VScrollBar", this) as ScrollBar;
            if (_hscrollbar != null)
                _hscrollbar.Scroll += _hscrollbar_Scroll;
            if (_vscrollbar != null)
                _vscrollbar.Scroll += _vscrollbar_Scroll;

            UpdateColumns(null, Columns);


            if (_gridheader != null)
            {
                _gridheader.AllowDrop = true;
                _gridheader.MouseLeftButtonUp += _gridheader_MouseLeftButtonUp;
                _gridheader.MouseLeftButtonDown += _gridheader_MouseLeftButtonDown;
                _gridheader.MouseMove += _gridheader_MouseMove;
                _gridheader.Drop += _gridheader_Drop;
                _gridheader.QueryContinueDrag += _gridheader_QueryContinueDrag;
            }
        }

        public bool ShowInGroup
        {
            set
            {
                m_showingroup = value;
                if (_gridview != null)
                    _gridview.ShowInGroup = value;
            }
            get
            {
                return m_showingroup;
            }
        }

        private bool m_showingroup = false;

        private void _gridheader_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            e.Handled = true;
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
            }
            if ((e.KeyStates.HasFlag(DragDropKeyStates.LeftMouseButton)))
            {
                e.Action = DragAction.Continue;
               
            }
            else
            {
                DragDropAdorner.Win32.POINT pt = new DragDropAdorner.Win32.POINT();
                DragDropAdorner.Win32.GetCursorPos(ref pt);

                Point ptNow = _gridheader.PointFromScreen(new Point(pt.X, pt.Y));
                if ( new Rect(0, 0, _gridheader.ActualWidth, _gridheader.ActualHeight).Contains(ptNow))
                {
                    e.Action = DragAction.Drop;
                }
                else
                {
                    e.Action = DragAction.Cancel;
                }
            }
            mAdornerLayer.Update();
        }


        private void _gridheader_Drop(object sender, DragEventArgs e)
        {
            Point ptLast = e.GetPosition(_gridheader);
            // 说明开始拖拽操作了
            double d = ptLast.X;
            int iColumn = _gridheader.GetCellColumn(ref d);
            if (e.Data != null)
            {
                int column = (int)e.Data.GetData(typeof(int));
                if (iColumn >= 0 && iColumn != column)
                {
                    Columns.Move(column, iColumn);

                    _gridheader.Redraw();
                    _gridview.Redraw();
                }
            }
            ptStartDrag = new Point(-1, -1);
        }

        private void _gridheader_MouseMove(object sender, MouseEventArgs e)
        {
            if (ptStartDrag.X != -1 && ptStartDrag.Y != -1)
            {
                Point ptNow = e.GetPosition(_gridheader);
                if (Math.Abs(ptNow.X - ptStartDrag.X) > 5 && _gridheader.IsAdjustingColumn == false)
                {

                    // 说明开始拖拽操作了
                    double d = ptStartDrag.X;
                    int iColumn = _gridheader.GetCellColumn(ref d);

                    DragDropAdorner adorner = new DragDropAdorner(_gridheader, _gridheader.GetColumSnapshot(iColumn));
                    DataObject data = new DataObject(typeof(int), iColumn);

                    mAdornerLayer = AdornerLayer.GetAdornerLayer(this);

                    mAdornerLayer.Add(adorner);
                    DragDrop.DoDragDrop(_gridheader, data, DragDropEffects.Move);
                    mAdornerLayer.Remove(adorner);
                    mAdornerLayer = null;
                    ptStartDrag = new Point(-1, -1); // 这里还原成初始状态
                }

            }
        }

        AdornerLayer mAdornerLayer = null;
        private Point ptStartDrag = new Point(-1, -1);
        private void _gridheader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_gridheader.IsAdjustingColumn == false)
                ptStartDrag = e.GetPosition(_gridheader);

            System.Diagnostics.Debug.WriteLine(e.ClickCount.ToString());
        }

        private void _gridheader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ptStartDrag = new Point(-1, -1);
            if (SortEnabled && _gridheader.IsAdjustingColumn == false)
            {
                _gridheader.SortEnabled = SortEnabled;
                Point pt = e.GetPosition(_gridheader);
                double dOffset = pt.X;
                int iColumn = _gridheader.GetCellColumn(ref dOffset);
                if (iColumn >= 0)
                {
                    System.Windows.Forms.SortOrder so = _gridheader.GetSortGraphColumn(iColumn);
                    _gridheader.ClearSortGraph();
                    string sGraph = "";
                    if (so == System.Windows.Forms.SortOrder.None || so == System.Windows.Forms.SortOrder.Descending)
                    {
                        _gridheader.SetSortGraphColumn(iColumn, System.Windows.Forms.SortOrder.Ascending);
                        sGraph = "ASC";
                    }
                    else
                    {
                        _gridheader.SetSortGraphColumn(iColumn, System.Windows.Forms.SortOrder.Descending);
                        sGraph = "DESC";
                    }

                    _gridheader.Redraw();
                    if (ItemsSource != null)
                    {
                        _gridview.SortString = string.Format("{0} {1}", _gridheader.GetColumnBindName(iColumn), sGraph);
                    }
                    _gridview.Redraw();
                }
            }
        }

        private void _gridheader_OnExtentWidthChanged()
        {
            AdjustScrollBars();
        }

        public GridViewColumnCollection Columns
        {
            set { SetValue(ColumnsProperty, value); }
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
        }

        private void _vscrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            SetVerticalOffset(e.NewValue);

        }

        private void _hscrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            SetHorizontalOffset(e.NewValue);
        }

        public void AddEditColumn(string columnName)
        {
            _gridview?.AddEditColumn(columnName);
        }


        private GB_GridView _gridview = null; // 主体
        private GB_GridHeader _gridheader = null; // 表头
        private ScrollViewer _scrollviewer = null;
        private ScrollBar _hscrollbar = null;
        private ScrollBar _vscrollbar = null;

        static public readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(GBListView), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));
        static public readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(GBListView), new FrameworkPropertyMetadata(null, OnColumnCollectionChanged));

        static public readonly DependencyProperty ItemHoverBackgroundProperty = DependencyProperty.Register("ItemHoverBackground", typeof(Brush), typeof(GBListView), new FrameworkPropertyMetadata(null, OnItemHoverBackgroundChanged));

        public Brush ItemHoverBackground
        {
            set
            {
                SetValue(ItemHoverBackgroundProperty, value);
                if (_gridview != null)
                    _gridview.HoverBackground = value;
            }
            get { return GetValue(ItemHoverBackgroundProperty) as Brush; }
        }
        static public readonly DependencyProperty SortEnabledProperty = DependencyProperty.RegisterAttached("SortEnabled", typeof(bool), typeof(GBListView), new PropertyMetadata(true, OnSortEnabledChanged));

        private static void OnSortEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listview = d as GBListView;
            if (e.NewValue != e.OldValue)
            {
                listview.SortEnabled = (bool)e.NewValue;
            }
        }

        public bool SortEnabled
        {
            set { SetValue(SortEnabledProperty, value); _gridheader.SortEnabled = value; }
            get { return (bool)GetValue(SortEnabledProperty); }
        }

        private static void OnItemHoverBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GBListView listview = d as GBListView;
            if (e.NewValue != e.OldValue)
                listview.ItemHoverBackground = e.NewValue as Brush;
        }

        private static void OnColumnCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GBListView listview = d as GBListView;
            listview.UpdateColumns(e.OldValue as GridViewColumnCollection, e.NewValue as GridViewColumnCollection);
        }

        private void UpdateColumns(GridViewColumnCollection OldValue, GridViewColumnCollection NewValue)
        {
            if (OldValue != null)
            {
                OldValue.CollectionChanged -= Column_CollectionChanged;
            }

            if (NewValue != null)
            {
                NewValue.CollectionChanged -= Column_CollectionChanged;
                NewValue.CollectionChanged += Column_CollectionChanged;
                if (_gridview != null)
                _gridview.M_Columns_CollectionChanged(_gridview, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
            }
        }

        private void Column_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_gridview != null)
            _gridview.M_Columns_CollectionChanged(sender, e);
            //AdjustScrollBars();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta < 0)
                MouseWheelDown();
            else MouseWheelUp();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GBListView view = (GBListView)d;
                if (view._gridview != null)
                {
                    view._gridview.ItemsSource = e.NewValue;
                    view._gridview.UpdateWidth();
                }
                view.AdjustScrollBars();
            }
        }

        public object ItemsSource
        {
            set
            {
                if (_gridview != null)
                _gridview.ItemsSource = value;
                //SetValue(ItemsSourceProperty, value);
            }
            get {
                if (_gridview != null)
                    return _gridview.ItemsSource;
                else return null;
            }
        }

        public int SelectedIndex
        {
            set { _gridview.SelectedIndex = value; }
            get { return _gridview.SelectedIndex; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt">相对于本窗口的坐标系统</param>
        /// <returns></returns>
        public int ItemFromPoint(Point pt)
        {
            Point ptNew = TranslatePoint(pt, _gridview);
            return _gridview.GetCellRow((int)ptNew.Y);
        }

        public event EventHandler<GB_GridView.CellSelectionEventArgs> SelectionChanged
        {
            add { if (_gridview != null) _gridview.SelectionChanged += value; }
            remove { if (_gridview != null) _gridview.SelectionChanged -= value; }
        }

        public object SelectedItem
        {
            get
            {
                if (_gridview != null)
                    return _gridview.SelectedItem;
                else return null;
            }
        }
        public GB_GridView.SelectionMode SelectionMode
        {
            set { _gridview.Mode = value; }
            get { return _gridview.Mode; }
        }

        public event EventHandler<GB_GridView.CellFormatEventArgs> CellFormat
        {
            add
            {
                _gridview.CellFormat += value;
            }
            remove
            {
                _gridview.CellFormat -= value;
            }
        }

        public event EventHandler<GB_GridView.CellPaintEventArgs> CellPaint
        {
            add { _gridview.CellPaint += value; }
            remove { _gridview.CellPaint -= value; }
        }

        public event EventHandler<GB_GridView.CellEditEventArgs> CellEdit
        {
            add { _gridview.CellEditEvent += value; }
            remove { _gridview.CellEditEvent -= value; }
        }

        private Point m_ptOffset;

        public void LineUp()
        {
            SetVerticalOffset(m_ptOffset.Y - 1);
        }

        public void LineDown()
        {
            SetVerticalOffset(m_ptOffset.Y + 1);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(m_ptOffset.X - 1);
        }

        public void LineRight()
        {
            SetHorizontalOffset(m_ptOffset.X + 1);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - 23);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + 23);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - 20);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + 20);
        }

        public void SetHorizontalOffset(double offset)
        {

            offset = (int)offset;
            if (offset < 0 || _viewport.Width >= _extent.Width)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Width >= _extent.Width)
                {
                    offset = _extent.Width - _viewport.Width;
                }
            }

            // 这里产生了水平滚动，识别哪些数据可以使用就只需copy一下就可以了


            m_ptOffset.X = offset;
            _transform.X = -offset;

            // TODO: 
            _gridheader.SetHorizontalOffset(m_ptOffset.X);
            _gridview.SetHorizontalOffset(m_ptOffset.X);
            if (_hscrollbar != null)
                _hscrollbar.Value = offset;
            if (_scrollviewer != null)
                _scrollviewer.InvalidateScrollInfo();

        }

        TranslateTransform _transform = new TranslateTransform();

        public void SetVerticalOffset(double offset)
        {
            offset = (int)offset;
            if (offset < 0 || _viewport.Height >= ExtentHeight)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }


            m_ptOffset.Y = offset;
            _gridview.SetVerticalOffset(m_ptOffset.Y);

            if (_vscrollbar != null)
                _vscrollbar.Value = offset;
        }

        public bool CanVerticallyScroll
        {
            set; get;
        }

        public bool CanHorizontallyScroll
        {
            set; get;
        }

        public double ExtentWidth
        {
            get
            {
                return _extent.Width;
            }
        }

        public double ExtentHeight
        {
            get
            {
                return _extent.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return _viewport.Width;
            }
        }

        public double ViewportHeight
        {
            get
            {
                return _viewport.Height;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                return m_ptOffset.X;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return m_ptOffset.Y;
            }
        }

        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);


        public class DragDropAdorner : Adorner
        {
            public DragDropAdorner(UIElement parent, WriteableBitmap bmp)
                : base(parent)
            {
                contentbmp = bmp;
                IsHitTestVisible = false; // Seems Adorner is hit test visible?
                mDraggedElement = parent as FrameworkElement;

                lgArrow = new PathGeometry();
                PathFigure pf = new PathFigure();
                lgArrow.Figures.Add(pf);
                Point pos1 = new Point(0, 0);
                pf.StartPoint = pos1;
                pf.Segments.Add(new LineSegment(new Point(pos1.X - 3, pos1.Y + 3), true));
                pf.Segments.Add(new LineSegment(new Point(pos1.X - 2, pos1.Y + 3), true));
                pf.Segments.Add(new LineSegment(new Point(pos1.X - 2, pos1.Y + 7), true));
                pf.Segments.Add(new LineSegment(new Point(pos1.X + 2, pos1.Y + 7), true));
                pf.Segments.Add(new LineSegment(new Point(pos1.X + 2, pos1.Y + 3), true));
                pf.Segments.Add(new LineSegment(new Point(pos1.X + 3, pos1.Y + 3), true));
                pf.IsClosed = true;
            }

            PathGeometry lgArrow;

            private WriteableBitmap contentbmp = null;

            public static class Win32
            {
                public struct POINT { public Int32 X; public Int32 Y; }

                // During drag-and-drop operations, the position of the mouse cannot be 
                // reliably determined through GetPosition. This is because control of 
                // the mouse (possibly including capture) is held by the originating 
                // element of the drag until the drop is completed, with much of the 
                // behavior controlled by underlying Win32 calls. As a workaround, you 
                // might need to use Win32 externals such as GetCursorPos.
                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern bool GetCursorPos(ref POINT point);
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                if (mDraggedElement != null)
                {
                    Win32.POINT screenPos = new Win32.POINT();
                    if (Win32.GetCursorPos(ref screenPos))
                    {
                        int iPosition = -1;
                        GB_GridHeader header = mDraggedElement as GB_GridHeader;
                        if (header != null)
                        {
                            Point pt = header.PointFromScreen(new Point(screenPos.X, screenPos.Y));
                            System.Diagnostics.Debug.WriteLine("pt  = " + pt.X);
                            double dX = pt.X;
                            int iColumn = header.GetCellColumn(ref dX);
                            if (iColumn >= 0)
                            {
                                iPosition = header.GetCellPosition(iColumn);
                            }
                        }
                        Point pos = PointFromScreen(new Point(screenPos.X, screenPos.Y));

                        Rect rect = new Rect(pos.X, pos.Y, contentbmp.PixelWidth, contentbmp.PixelHeight);
                        drawingContext.PushOpacity(0.7);
                        drawingContext.DrawImage(contentbmp, rect);
                        if (iPosition >= 0)
                        {
                            Point pos1 = header.PointToScreen(new Point(iPosition, header.ActualHeight));
                            pos1 = PointFromScreen(pos1);


                            lgArrow.Transform = new TranslateTransform(pos1.X, pos1.Y);
                            drawingContext.DrawGeometry(header.Foreground, new Pen(header.Foreground, 1), lgArrow);
                        }
                        drawingContext.Pop();
                    }
                }
            }
            FrameworkElement mDraggedElement = null;

        }
    }
}
