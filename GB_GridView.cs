using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using GDI = System.Drawing;

namespace FastGrid
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfApplication3"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfApplication3;assembly=WpfApplication3"
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
    ///     <MyNamespace:GB_GridView/>
    ///
    /// </summary>
    public class GB_GridView : FrameworkElement
    {
        static GB_GridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GB_GridView), new FrameworkPropertyMetadata(typeof(GB_GridView)));
        }

        

        public object SelectedItem
        {
            get
            {
                int i = SelectedIndex;
                if (i >= 0)
                {
                    return _treeitem.CacheDataRowIndex[i].Value;
                }
                else return null;
            }
        }
        private HashSet<string> m_editColumns = new HashSet<string>();
        public void AddEditColumn(string columnname)
        {
            m_editColumns.Add(columnname);
        }
        public static GDI.Brush ConvertWpfBrush2GDI(Brush brush, int rowHeight)
        {
            // 目前只支持这两种画刷的Style
            if (brush is SolidColorBrush)
            {
                Color color = (brush as SolidColorBrush).Color;
                return new GDI.SolidBrush(GDI.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
            else if (brush is LinearGradientBrush)
            {
                LinearGradientBrush wpfBrush = brush as LinearGradientBrush;

                GDI.Drawing2D.LinearGradientBrush gdibrush = new GDI.Drawing2D.LinearGradientBrush(new GDI.PointF((float)wpfBrush.StartPoint.X, 0), new GDI.PointF((float)wpfBrush.EndPoint.X, (float)rowHeight), GDI.Color.White, GDI.Color.White);
                GDI.Drawing2D.ColorBlend cblend = new GDI.Drawing2D.ColorBlend(wpfBrush.GradientStops.Count);
                cblend.Colors = new GDI.Color[wpfBrush.GradientStops.Count];
                cblend.Positions = new float[wpfBrush.GradientStops.Count];
                // 先对offset排个序吧
                SortedList<float, int> valuelist = new SortedList<float, int>();
                for (int j = 0; j < cblend.Colors.Length; ++j)
                {
                    valuelist.Add((float)wpfBrush.GradientStops[j].Offset, j);
                }
                int i = 0;
                foreach (KeyValuePair<float, int> kvp in valuelist)
                {
                    cblend.Colors[i] = GDI.Color.FromArgb(wpfBrush.GradientStops[kvp.Value].Color.A, wpfBrush.GradientStops[kvp.Value].Color.R, wpfBrush.GradientStops[kvp.Value].Color.G, wpfBrush.GradientStops[kvp.Value].Color.B);
                    cblend.Positions[i] = kvp.Key;
                    i++;
                }

                gdibrush.InterpolationColors = cblend;
                return gdibrush;
            }
            else return null; // 暂不支持转换
        }


        public GB_GridView()
        {
            RowHeight = 23;
            m_bufferedBmp = new WriteableBitmap(1, 23, 96.0, 96.0, PixelFormats.Pbgra32, null);
            //m_bufferedBmp = new WriteableBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32, null);
            this.SizeChanged += GB_GridView_SizeChanged;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            UseLayoutRounding = true;
        }

        //private List<Visual> visuals = new List<Visual>();
        //protected override Visual GetVisualChild(int index)
        //{
        //    return visuals[index];
        //}
        //protected override int VisualChildrenCount
        //{
        //    get
        //    {
        //        return visuals.Count;
        //    }
        //}

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    if (this.VisualChildrenCount > 0)
        //    {
        //        UIElement child = this.GetVisualChild(0) as UIElement;
        //        System.Diagnostics.Debug.Assert(child != null); // !Assert  
        //        child.Measure(availableSize);
        //        return child.DesiredSize;
        //    }

        //    return availableSize;
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    Rect arrangeRect = new Rect()
        //    {
        //        Width = finalSize.Width,
        //        Height = finalSize.Height
        //    };

        //    if (this.VisualChildrenCount > 0)
        //    {
        //        UIElement child = this.GetVisualChild(0) as UIElement;
        //        System.Diagnostics.Debug.Assert(child != null); // !Assert  
        //        child.Arrange(arrangeRect);
        //    }

        //    return finalSize;
        //}

        //public void AddVisual(Visual visual)
        //{
        //    visuals.Add(visual);
        //    base.AddVisualChild(visual);
        //    base.AddLogicalChild(visual);
        //}

        //public void DeleteVisual(Visual visual)
        //{
        //    visuals.Remove(visual);
        //    base.RemoveVisualChild(visual);
        //    base.RemoveLogicalChild(visual);
        //}
        //public Visual GetVisual(Point point)
        //{
        //    HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
        //    return hitResult.VisualHit as Visual;
        //}


        public void BitBlt(GDI.Rectangle rectSrc, GDI.Rectangle rectDest)
        {
            Bitblt(m_bufferedBmp, rectSrc, rectDest);
        }

        private List<string> m_ColumnsSource = new List<string>();

        internal void M_Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for(int i = 0; i < e.NewItems.Count; ++i)
                    {
                        GridViewColumn gvc = e.NewItems[i] as GridViewColumn;
                        if (double.IsNaN(gvc.Width))
                            gvc.Width = 60;
                        if (gvc.DisplayMemberBinding != null)
                        {
                            Binding bind = gvc.DisplayMemberBinding as Binding;
                            if (bind != null)
                                m_ColumnsSource.Add(bind.Path.Path);
                            else m_ColumnsSource.Add("");
                        }
                        else
                        {
                            m_ColumnsSource.Add("");
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {// 这里处理顺序变化了
                        List<string> scolums = new List<string>();
                        for (int i = 0; i < e.OldItems.Count; i++)
                            scolums.Add(m_ColumnsSource[e.OldStartingIndex + i]);

                        m_ColumnsSource.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        if (e.NewStartingIndex < e.OldStartingIndex)
                        {
                            m_ColumnsSource.InsertRange(e.NewStartingIndex, scolums);
                        }
                        else
                        {
                            m_ColumnsSource.InsertRange(e.NewStartingIndex - e.OldItems.Count + 1, scolums);
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        m_ColumnsSource.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    {
                        for (int i = 0, j = e.OldStartingIndex; i < e.NewItems.Count; ++i)
                        {
                            GridViewColumn gvc = e.NewItems[i] as GridViewColumn;
                            if (double.IsNaN(gvc.Width))
                                gvc.Width = 60;
                            if (gvc.DisplayMemberBinding != null)
                            {
                                Binding bind = gvc.DisplayMemberBinding as Binding;
                                if (bind != null)
                                    m_ColumnsSource[j++] = (bind.Path.Path);
                                else m_ColumnsSource[j++] = ("");
                            }
                            else
                            {
                                m_ColumnsSource[j++] = ( "");
                            }
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        m_ColumnsSource.Clear();
                        foreach (GridViewColumn gvc in Columns)
                        {
                            if (double.IsNaN(gvc.Width))
                                gvc.Width = 60;
                            if (gvc.DisplayMemberBinding != null)
                            {
                                Binding bind = gvc.DisplayMemberBinding as Binding;
                                if (bind != null)
                                    m_ColumnsSource.Add(bind.Path.Path);
                                else m_ColumnsSource.Add("");
                            }
                            else
                            {
                                m_ColumnsSource.Add("");
                            }
                        }
                    }
                    break;
            }
            
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background",
            typeof(Brush),
            typeof(GB_GridView), new PropertyMetadata(Brushes.White, OnBackgroundChanged));

        public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.RegisterAttached(
            "SelectedBackground",
            typeof(Brush),
            typeof(GB_GridView), new PropertyMetadata(Brushes.White, OnSelectedBackgroundChanged));


        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.RegisterAttached(
            "HoverBackground",
            typeof(Brush),
            typeof(GB_GridView), new PropertyMetadata(Brushes.White, OnHoverBackgroundChanged));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(GB_GridView), new PropertyMetadata(Brushes.Black, OnForegroundChanged));

        public static readonly DependencyProperty SelectedForegroundProperty = DependencyProperty.RegisterAttached(
    "SelectedForeground", typeof(Brush), typeof(GB_GridView), new PropertyMetadata(Brushes.Black, OnSelectedForegroundChanged));

        private static void OnSelectedForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridView view = d as GB_GridView;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, view.RowHeight);
                if (brush != null)
                {
                    view.m_selectedforeground.Dispose();
                    view.m_selectedforeground = brush;
                }
            }
        }

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridView view = d as GB_GridView;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, view.RowHeight);
                if (brush != null)
                {
                    view.m_foreground.Dispose();
                    view.m_foreground = brush;
                }
            }
        }


        private static void OnSelectedBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridView view = d as GB_GridView;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, view.RowHeight);
                if (brush != null)
                {
                    view.m_selectedbackground.Dispose();
                    view.m_selectedbackground = brush;
                }
            }
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridView view = d as GB_GridView;
                GDI.Brush brush = GB_GridHeader.ConvertWpfBrush2GDI(e.NewValue as Brush, view.RowHeight);
                if (brush != null)
                {
                    view.m_background.Dispose();
                    view.m_background = brush;
                }
            }
        }

        public Brush HoverBackground
        {
            set
            {
                SetValue(HoverBackgroundProperty, value);
            }

            get
            {
                return (Brush)GetValue(HoverBackgroundProperty);
            }
        }

        public Brush Background
        {
            set
            {
                SetValue(BackgroundProperty, value);
            }

            get
            {
                return (Brush)GetValue(BackgroundProperty);
            }
        }

        public Brush Foreground
        {
            set
            {
                SetValue(ForegroundProperty, value);
            }

            get
            {
                return (Brush)GetValue(ForegroundProperty);
            }
        }

        public Brush SelectedBackground
        {
            set
            {
                SetValue(SelectedBackgroundProperty, value);
            }

            get
            {
                return (Brush)GetValue(SelectedBackgroundProperty);
            }
        }

        public Brush SelectedForeground
        {
            set
            {
                SetValue(SelectedForegroundProperty, value);
            }

            get
            {
                return (Brush)GetValue(SelectedForegroundProperty);
            }
        }

        private GDI.Brush m_background = new GDI.SolidBrush(GDI.Color.White);
        private GDI.Brush m_foreground = new GDI.SolidBrush(GDI.Color.Black);
        private GDI.Brush m_selectedbackground = new GDI.SolidBrush(GDI.SystemColors.Highlight);
        private GDI.Brush m_selectedforeground = new GDI.SolidBrush(GDI.SystemColors.HighlightText);
        private GDI.Brush m_hoverbackground = new GDI.SolidBrush(GDI.SystemColors.ActiveCaption);
        private static void OnHoverBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridView view = d as GB_GridView;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, view.RowHeight);
                if (brush != null)
                {
                    view.m_hoverbackground.Dispose();
                    view.m_hoverbackground = brush;
                }
            }
        }

        private GDI.Font m_font = new GDI.Font("微软雅黑", 9);

        public GDI.Font Font
        {
            set { m_font = value; }
            get { return m_font; }
        }

        public int RowHeight
        { set; get; }

        public class CellSelectionEventArgs : EventArgs
        {
            public int CellRowIndex = -1; // 本次鼠标单击所在的行
            public int CellColumnIndex = -1; // 本次鼠标单击所在的列
        }

        private EventHandler<CellSelectionEventArgs> m_selectionChanged;
        public event EventHandler<CellSelectionEventArgs> SelectionChanged
        {
            add
            {
                lock(m_eventLock)
                m_selectionChanged += value;
            }
            remove
            {
                lock (m_eventLock)
                  m_selectionChanged -= value;
            }
        }

        public enum SelectionMode
        {
            Single, Mutiple,
        }

        private SelectionMode m_mode = SelectionMode.Single;
        public SelectionMode Mode
        {
            set { m_mode = value; }
            get { return m_mode; }
        }


        public int RowCount
        {
            get
            {
                if (ItemsSource != null)
                {
                    //if (m_rowcount == -1) // 采用数据源的Count的吧
                        return _treeitem.RowCount;
                   // else
                   //     return m_rowcount > ItemsSource.Count ? ItemsSource.Count : m_rowcount;
                }
                else return 0;
            }
            set
            {
                m_rowcount = value; // 调用者自己控制吧，别乱设大小
                // 进行视图大小刷新
                this.OnExtentViewChanged?.Invoke();

                // 选择项也要进行同步
                m_selectionRows.RemoveWhere(new Predicate<int>((int i) => { return i >= m_rowcount; }));
                if (m_mouseRow >= m_rowcount)
                    m_mouseRow = -1; // 全部清除不合法的数据

                m_selectionChanged?.Invoke(this, new CellSelectionEventArgs());

                Redraw();
            }
        }

        private int m_rowcount = -1;

        public Size GetInternalSize()
        {
            Size size = new Size(Width, RowCount * RowHeight);
            return size;
        }

        // 当前相对偏移量，如果不全为0，说明出现滚动条了
        private Point m_ptOffset;
        public Point PointOffset
        {
            set
            {
                m_ptOffset = value;
                //UpdateData();
            }
            get { return m_ptOffset; }
        }

        public class GridColumn
        {
            public int Width;
            public string ColName;
            public string DisplayName;
        }

        static public readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(GB_GridView), new FrameworkPropertyMetadata(null, null));


        public GridViewColumnCollection Columns
        {
            set { SetValue(ColumnsProperty, value); }
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
        }

        public void UpdateWidth()
        {
            m_width = 0;
            for (int i = 0; i < Columns.Count; ++i)
            {
                m_width += Columns[i].Width;
            }
        }

        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);

       

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
                if (m_width == 0)
                    return this.ActualWidth;
                else
                return m_width;
            }
        }

        public double ExtentHeight
        {
            get
            {
                if (ItemsSource == null)
                    return this.ActualHeight;
                else
                    return _treeitem.RowCount * RowHeight;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return ActualWidth;
            }
        }

        private double m_width = 0;

        public double ViewportHeight
        {
            get
            {
                return ActualHeight;
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

        public enum GridViewElementStates
        {
            //
            // 摘要:
            //     Indicates that an element is in its default state.
            None = 0,
            //
            // 摘要:
            //     Indicates the an element is currently displayed onscreen.
            Displayed = 1,
            //
            // 摘要:
            //     Indicates that an element cannot be scrolled through the UI.
            Frozen = 2,
            //
            // 摘要:
            //     Indicates that an element will not accept user input to change its value.
            ReadOnly = 4,
            //
            // 摘要:
            //     Indicates that an element can be resized through the UI. This value is ignored
            //     except when combined with the System.Windows.Forms.DataGridViewElementStates.ResizableSet
            //     value.
            Resizable = 8,
            //
            // 摘要:
            //     Indicates that an element does not inherit the resizable state of its parent.
            ResizableSet = 16,
            //
            // 摘要:
            //     Indicates that an element is in a selected (highlighted) UI state.
            Selected = 32,
            //
            // 摘要:
            //     Indicates that an element is visible (displayable).
            Visible = 64,
        }

        public class CellPaintEventArgs : EventArgs
        {
            public int RowIndex;
            public int ColumnIndex;
            public GDI.Rectangle Bounds;
            public GridViewElementStates States;
            public object Value;
            public GDI.Graphics Graphics;
            public bool Handled = false;
            public Thickness Margain = new Thickness();
            public GDI.StringAlignment Alignment = GDI.StringAlignment.Near;
        }

        public class CellFormatEventArgs :EventArgs
        {
            public int RowIndex;
            public int ColumnIndex;
            public object Value;
            public bool FormattingApplied;
            public GDI.Brush Foreground;
            public GDI.Brush Background;
            public GDI.StringAlignment Alignment = GDI.StringAlignment.Near;
        }

        // 用于存储当前选中的行的索引信息
        private HashSet<int> m_selectionRows = new HashSet<int>();
        public int SelectedIndex
        {
            get
            {
                if (m_selectionRows.Count > 0)
                    return m_selectionRows.First();
                else return -1;
            }
            set
            {
                m_selectionRows.Clear();
                m_selectionRows.Add(value);
                // 以后需要优化的
                Invalidate();
            }
        }

        public int[] SelectedRows
        {
            get { return m_selectionRows.ToArray(); }
        }

        static int MakeInt(ushort high, ushort low)
        {
            return ((int)(((ushort)(low)) | ((int)((ushort)(high))) << 16));
        }

        static ushort MakeLowUshort(int iValue)
        {
            return ((ushort)(((int)(iValue)) & 0xffff));
        }

        static ushort MakeHighUshort(int iValue)
        {
            return ((ushort)((((int)(iValue)) >> 16) & 0xffff));
        }

        internal Int32Rect paintRow(GDI.Graphics g, int row)
        {
            int iRowPos = GetRowPosition(row);

            if (iRowPos + RowHeight <= 0 || iRowPos >= m_bufferedBmp.PixelHeight)
                return new Int32Rect(0, 0, 0, 0);

            CellPaintEventArgs e = new CellPaintEventArgs();
            e.Graphics = g;
            e.RowIndex = row;
            e.States = GridViewElementStates.Visible;
            if (m_selectionRows.Contains(row))
                e.States |= GridViewElementStates.Selected;

            double iFirstDisplayColumnX = 0;
            int iFirstDisplayColumn = GetCellColumn(ref iFirstDisplayColumnX);

            double iColumnWidth = 0;
            
            for (int i = iFirstDisplayColumn; i < Columns.Count; ++i)
            {
                if (-iFirstDisplayColumnX + iColumnWidth > ActualWidth)
                    break; // 说明画完了

                e.Bounds = new GDI.Rectangle((int)(-iFirstDisplayColumnX + iColumnWidth), iRowPos, (int)Columns[i].Width, RowHeight);
                if (e.Bounds.Height + e.Bounds.Y < 0)
                    break; // 本行都不在屏幕中，那就直接跳出吧

                if (e.Bounds.Width + e.Bounds.X < 0)
                    continue; // 这里就别画了，已经在屏幕外面了
                iColumnWidth += Columns[i].Width;
                e.ColumnIndex = i;
                // TODO： 这里要重新写代码来获取已绑定的内容
                
                if (_treeitem.MulitpleDataRoot.ChildRows.Columns.Contains(m_ColumnsSource[i]))
                    e.Value = _treeitem.CacheDataRowIndex[row].Value[m_ColumnsSource[i]];
                else e.Value = "";

                DefaultGroupPaintCell(e);
                //DefaultPaintCell(e);

            }
            return new Int32Rect(0, iRowPos, (int)ActualWidth, RowHeight);
        }

        public event GB_GridHeader.ExtentViewChanged OnExtentViewChanged; // 暂时只支持一个事件订阅者吧

        // 这里Source就先强制指定为DataView吧，要通用，以后再说
        public object ItemsSource
        {
            set
            {
                DataView dv = value as DataView;
                // 这里兼容一下DataView
                if (dv != null)
                {
                    _treeitem.GridView = this;
                    _treeitem.MulitpleDataRoot.ChildRows = dv.Table;
                    _treeitem.MulitpleDataRoot.Nodes.Clear();
                    _treeitem.UpdateCacheIndex();
                    _treeitem.ResetRowCount();
                    OnExtentViewChanged?.Invoke();
                }
                else
                {
                    _treeitem.GridView = this;
                    _treeitem.MulitpleDataRoot = value as TreeGridRow;
                    _treeitem.ResetRowCount();
                }
                // 主动调一下数据源重置方法
                //ItemSource_ListChanged(m_dv, new System.ComponentModel.ListChangedEventArgs(System.ComponentModel.ListChangedType.Reset, -1));
            }
            get { return _treeitem.MulitpleDataRoot; }
        }

        private string m_filter = "";
        public string Filter
        {
            get { return m_filter; }
            internal set
            {
                m_filter = value;
                this._treeitem.DoFilter();
            }
        }
        public string SortString
        {
            set
            {
                _treeitem.SortString = value;
                _treeitem.DoSort();
            }
            get
            {
                return _treeitem.SortString;
            }
        }

        // 以下用于支持CheckBox选项
        public bool ShowCheckBox { get; internal set; }
        public string CheckBoxColName { get; internal set; }

        // 采用分组显示 true - 分组（树形）， false - 平面显示
        private bool m_bshowgroup = false;
        public bool ShowInGroup { get { return m_bshowgroup; } set { m_bshowgroup = value; } }

        private TreeGridView _treeitem = new TreeGridView();
        public object GetItemData(int index)
        {
            if (index >= 0 && index < _treeitem.CacheDataRowIndex.Count)
                return _treeitem.CacheDataRowIndex[index].Value;
            else return null;
        }

        public bool IsHeaderRow(int index)
        {
            return _treeitem.CacheGroupHeaderIndex.ContainsKey(index);
        }

        private void ItemSource_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
#if (false)
            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded)
            {
                // 判断下是不是插入进来的
                if (e.NewIndex == ItemsSource.Count - 1) // 说明是添加在最后，这种情况比较简单，不对其它行造成影响
                {
                    Int32Rect rect = paintRow(g, e.NewIndex);
                    if (!rect.IsEmpty)
                        Invalidate(rect);
                }
                else
                {
                    GDI.Size size = GetCellPosition(0, e.NewIndex);
                    if (size.Height <= 0)
                    {
                        //// 说明新添加的行在当前屏幕上面
                        //size.Height = 0;
                        //// 1、当前屏幕内容下移一行，
                        //BitBlt(new GDI.Rectangle(0, size.Height, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight),
                        //new GDI.Rectangle(0, size.Height + RowHeight, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight));
                        //// 2、画上第一行屏幕上的内容
                        //paintArea(new GDI.Rectangle(0, 0, m_bufferedBmp.PixelWidth, RowHeight));
                        //Invalidate(); // 整个屏幕都变了，直接刷新吧
                        Redraw(); // 这里有选中的情况，会导致复制的数据不对，所以还是直接重绘吧
                        m_selectionChanged?.Invoke(this, new CellSelectionEventArgs());
                    }
                    else if (size.Height < m_bufferedBmp.PixelHeight)
                    {
                        //BitBlt(new GDI.Rectangle(0, size.Height, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight),
                        //new GDI.Rectangle(0, size.Height + RowHeight, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight));
                        //paintRow(g, e.NewIndex);
                        //Invalidate(new Int32Rect(0, size.Height, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height));
                        Redraw(); // 这里有选中的情况，会导致复制的数据不对，所以还是直接重绘吧
                        m_selectionChanged?.Invoke(this, new CellSelectionEventArgs());
                    }
                    else // 这就什么都不做了，添加的行在屏幕下方，没有任何影响
                    {
                        // do nothing
                    }
                    
                }

                // 这里要通知外面，虚拟数据高度发生变化了
                // TODO
                RowCount = ItemsSource.Count;
                OnExtentViewChanged?.Invoke();
            }
            else if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemChanged)
            {
                // 只刷新这一个item吧
                Int32Rect rect = paintRow(g, e.NewIndex);
                if (!rect.IsEmpty)
                    Invalidate(rect);
            }
            else if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemDeleted)
            {
                int iTempCount = ItemsSource.Count;
                RowCount = iTempCount;
                // 选择项也要进行同步
                m_selectionRows.RemoveWhere(new Predicate<int>((int i) => { return i >= iTempCount; }));
                if (m_mouseRow >= iTempCount)
                    m_mouseRow = -1; // 全部清除不合法的数据

                GDI.Size size = GetCellPosition(0, e.NewIndex);
                if (size.Height <= 0)
                {
                    Redraw();
                    //// 说明删除的行在当前屏幕上面
                    //size.Height = 0;
                    //// 1、当前屏幕内容上移一行，
                    //BitBlt(new GDI.Rectangle(0, RowHeight, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight),
                    //new GDI.Rectangle(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight));
                    //// 2、画上最后一行屏幕上的内容
                    //paintArea(new GDI.Rectangle(0, m_bufferedBmp.PixelHeight - RowHeight, m_bufferedBmp.PixelWidth, RowHeight));
                    //Invalidate(); // 整个屏幕都变了，直接刷新吧
                }
                else if (size.Height < m_bufferedBmp.PixelHeight)
                {
                    Redraw();
                    //BitBlt(new GDI.Rectangle(0, size.Height + RowHeight, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height - RowHeight),
                    //new GDI.Rectangle(0, size.Height, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height -  RowHeight));
                    //g.FillRectangle(m_background, new GDI.Rectangle(0, m_bufferedBmp.PixelHeight - RowHeight, m_bufferedBmp.PixelWidth, RowHeight));
                    //paintArea(new GDI.Rectangle(0, m_bufferedBmp.PixelHeight - RowHeight, m_bufferedBmp.PixelWidth, RowHeight));
                    //Invalidate(new Int32Rect(0, size.Height, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - size.Height));
                }
                else // 这就什么都不做了，删除的行在屏幕下方，没有任何影响
                {
                    // do nothing
                }

                // 这里要通知外面，虚拟数据高度发生变化了
                // TODO
                OnExtentViewChanged?.Invoke();
                m_selectionChanged?.Invoke(this, new CellSelectionEventArgs());
            }
            else if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemMoved)
            {
                GDI.Size sizeold = GetCellPosition(0, e.OldIndex);
                GDI.Size sizenew = GetCellPosition(0, e.NewIndex);
                if ((sizeold.Height <= -RowHeight && sizenew.Height <= -RowHeight) ||
                    (sizeold.Height <= -RowHeight && sizenew.Height >= m_bufferedBmp.PixelHeight) ||
                    (sizeold.Height >= m_bufferedBmp.PixelHeight && sizenew.Height >= m_bufferedBmp.PixelHeight) ||
                    (sizeold.Height >= m_bufferedBmp.PixelHeight && sizenew.Height <= -RowHeight)
                    )
                { // 不需要处理，因为移的数据不在屏幕范围内
                    // do nothing
                }
                else
                {
                    // 直接重画吧，以后再来优化
                    Redraw();
                }

                m_selectionChanged?.Invoke(this, new CellSelectionEventArgs());
            }
            else if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset)
            {
                if (ItemsSource == null)
                {
                    m_selectionRows.Clear();
                    RowCount = 0;
                    m_mouseRow = -1; // 全部清除不合法的数据
                }
                else
                {
                    int iTempCount = ItemsSource.Count;
                    RowCount = iTempCount;
                    // 选择项也要进行同步
                    m_selectionRows.RemoveWhere(new Predicate<int>((int i) => { return i >= iTempCount; }));
                    if (m_mouseRow >= iTempCount)
                        m_mouseRow = -1; // 全部清除不合法的数据
                }
                Redraw();
                OnExtentViewChanged?.Invoke();

                m_selectionChanged?.Invoke(this, new CellSelectionEventArgs());
            }
#endif
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (m_mouseColumn != -1)
            {
                int m_prev_mouseColumn = m_mouseColumn;
                int m_prev_mouseRow = m_mouseRow;
                m_mouseColumn = -1;
                m_mouseRow = -1;
                if (m_prev_mouseRow >= 0)
                    Invalidate(paintRow(g, m_prev_mouseRow));
            }
        }


        /// <summary>
        /// 坐标测试结果返回的结构体
        /// </summary>
        public struct HitTestInfo
        {
            public int ColumnIndex;
            public int RowIndex;
            public Rect Rect;
        }

        /// <summary>
        /// return - 返回X所在的列索引
        /// </summary>
        /// <param name="x">相对当前窗口的X坐标，不包含滚动条偏移位置</param>
        /// <returns></returns>
        int GetCellColumn(ref double x)
        {
            x += (int)m_ptOffset.X;
            for (int i = 0; i < Columns.Count; ++i)
            {
                if (x < Columns[i].Width)
                    return i;
                else x -= Columns[i].Width;
            }
            return -1;
        }

        internal int GetCellRow(int y)
        {
            int iRow = (y + (int)PointOffset.Y) / RowHeight;
            return iRow >= RowCount ? -2 : iRow;
        }

        int GetRowPosition(int row)
        {
            if (row == -1)
            {
                return -1;
            }
            else if (row >= 0)
            {
                return row * RowHeight - (int)PointOffset.Y;
            }
            else return -1;
        }

        internal GDI.Size GetCellPosition(int col, int row)
        {
            if (col >= 0 && col < Columns.Count)
            {
                double iWidth = 0;
                for (int i = 0; i < col; ++i)
                {
                    iWidth += Columns[i].Width;
                }

                if (row >= 0)
                {
                    return new GDI.Size((int)iWidth - (int)PointOffset.X, row * RowHeight - (int)PointOffset.Y);
                }
                else
                    return new GDI.Size(-1, -1);
            }
            else
                return new GDI.Size(0, 0); // 这里就返回初始坐标吧
        }

        internal int ElementHitTest(Point pt, ref HitTestInfo info)
        {
            int iOffsetY = (int)pt.Y;

            info.RowIndex = GetCellRow(iOffsetY);
            double iXOffset = (pt.X);
            info.ColumnIndex = GetCellColumn(ref iXOffset);
            if (info.RowIndex == -1 || info.ColumnIndex == -1)
                return -1;
            else
            {
                GDI.Size size = GetCellPosition(info.ColumnIndex, info.RowIndex);
                info.Rect = new Rect(size.Width, size.Height, Columns[info.ColumnIndex].Width, RowHeight);
                return 1;
            }
        }

        // 以下变量为运行状态时的临时变量，用于展示互操作时的状态
        private int m_mouseColumn = -1; // 记录鼠标所在的列
        private int m_mouseRow = -1; // 记录鼠标所在的行
        private int m_lastRowIndex = -1; // 该变量使用在多选时使用

        private delegate Int32Rect[] PartPaint(GDI.Graphics g);
        private void doUpdateUI(PartPaint pp)
        {

            m_bufferedBmp.Lock();
            //using (GDI.Bitmap backbmp = new GDI.Bitmap((int)m_bufferedBmp.Width, (int)m_bufferedBmp.Height, m_bufferedBmp.BackBufferStride, GDI.Imaging.PixelFormat.Format32bppPArgb, m_bufferedBmp.BackBuffer))
            {
                //using (GDI.Graphics g = GDI.Graphics.FromImage(backbmp))
                {
                    //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    Int32Rect[] rects = pp(g);
                    if (rects != null && rects.Length > 0)
                    {
                        for (int i = 0; i < rects.Length; ++i)
                        {
                            if (!rects[i].IsEmpty)
                            {
                                if (rects[i].X < 0)
                                {
                                    rects[i].Width += rects[i].X;
                                    rects[i].X = 0;
                                }
                                if (rects[i].Y < 0)
                                {
                                    rects[i].Height += rects[i].Y;
                                    rects[i].Y = 0;
                                }

                                if (rects[i].X + rects[i].Width > m_bufferedBmp.PixelWidth)
                                    rects[i].Width = Math.Abs((int)m_bufferedBmp.PixelWidth - rects[i].X);

                                if (rects[i].Y + rects[i].Height > m_bufferedBmp.PixelHeight)
                                    rects[i].Height = Math.Abs((int)m_bufferedBmp.PixelHeight - rects[i].Y);
                                m_bufferedBmp.AddDirtyRect(rects[i]);
                            }
                        }
                    }
                }
            }
            m_bufferedBmp.Unlock();
        }

        /// Return Type: void*
        ///_Dst: void*
        ///_Src: void*
        ///_Size: size_t->unsigned int
        [System.Runtime.InteropServices.DllImportAttribute("ntdll.dll", EntryPoint = "memmove", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static extern System.IntPtr memmove(System.IntPtr _Dst,
            [System.Runtime.InteropServices.InAttribute()] System.IntPtr _Src,
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.U4)] uint _Size);


        static public void Bitblt(WriteableBitmap bmp, GDI.Rectangle rectSrc, GDI.Rectangle rectDest)
        {
            //// TODO: 这里有bug，memmove有问题
            if (rectDest.Width < rectSrc.Width)
                rectSrc.Width = rectDest.Width;
            else rectDest.Width = rectSrc.Width;

            if (rectDest.Height < rectSrc.Height)
                rectSrc.Height = rectDest.Height;
            else rectDest.Height = rectSrc.Height;


            // 这里要判断下是图片内存是否足够

            if (rectDest.Top < rectSrc.Top)
            {
                for (int srcrow = rectSrc.Top, destrow = rectDest.Top; srcrow < rectSrc.Bottom; ++srcrow, ++destrow)
                {
                    memmove(new IntPtr(bmp.BackBuffer.ToInt64() + destrow * bmp.BackBufferStride + rectDest.Left * 4),
                        new IntPtr(bmp.BackBuffer.ToInt64() + srcrow * bmp.BackBufferStride + rectSrc.Left * 4),
                        (uint)rectSrc.Width * 4);
                }
            }
            else
            {
                for (int srcrow = rectSrc.Bottom - 1, destrow = rectDest.Bottom - 1; srcrow >= rectSrc.Top; --srcrow, --destrow)
                {
                    memmove(new IntPtr(bmp.BackBuffer.ToInt64() + destrow * bmp.BackBufferStride + rectDest.Left * 4),
                        new IntPtr(bmp.BackBuffer.ToInt64() + srcrow * bmp.BackBufferStride + rectSrc.Left * 4),
                        (uint)rectSrc.Width * 4);
                }
            }
        }

        private CellEditEventArgs editargs = new CellEditEventArgs();
        public enum EditAction {None = 0, EditCreate = 1, EditMove = 2, EditDelete = 3, ValueRetrieve = 4};
        public class CellEditEventArgs : EventArgs
        {
            public int RowIndex;
            public int ColumnIndex;
            public EditAction Action;
            public Rect Bounds;
            public object Value;
            public Control UI;
        }

        private EventHandler<CellEditEventArgs> m_celledit;
        public event EventHandler<CellEditEventArgs> CellEditEvent
        {
            add
            {
                lock (m_eventLock) { m_celledit += value; }
            }
            remove
            {
                lock (m_eventLock) { m_celledit -= value; }
            }
        }

        internal object RetrieveEditValue()
        {
            editargs.Action = EditAction.ValueRetrieve;
            m_celledit?.Invoke(this, editargs);
            return editargs.Value;
        }

        private void AddEditUI(Control UI)
        {
            Grid grid = this.Parent as Grid;
            if (grid != null)
            {
                grid.Children.Add(UI);
                UI.HorizontalAlignment = HorizontalAlignment.Left;
                UI.VerticalAlignment = VerticalAlignment.Top;
                UI.Margin = new Thickness(editargs.Bounds.Left, editargs.Bounds.Top, 0, 0);
                UI.Width = editargs.Bounds.Width;
                UI.Height = editargs.Bounds.Height;
                UI.Visibility = Visibility.Visible;
                UI.LostFocus += UI_LostFocus;
                UI.PreviewKeyDown += UI_PreviewKeyDown;
            }
        }

        private void UI_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // 如果是回车的话，那就结束编辑去取值吧
            {
                object obj = RetrieveEditValue();
                _treeitem.CacheDataRowIndex[editargs.RowIndex].Value[editargs.ColumnIndex] = obj;
                (sender as Control).LostFocus -= UI_LostFocus;
                (sender as Control).PreviewKeyDown -= UI_PreviewKeyDown;
                editargs.Action = EditAction.EditDelete;
                RemoveEditUI(editargs.UI);
                m_celledit?.Invoke(this, editargs);
                editargs.ColumnIndex = -1;
                editargs.RowIndex = -1;
                editargs.Action = EditAction.None;
            }
        }

        private void UI_LostFocus(object sender, RoutedEventArgs e)
        {
           
            (sender as Control).LostFocus -= UI_LostFocus;

        }

        private void RemoveEditUI(Control UI)
        {
            Grid grid = this.Parent as Grid;
            if (grid != null)
            {
                grid.Children.Remove(UI);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            DoEditEvent(e);

            DoSelectingEvent(e);
        }

        private void DoEditEvent(MouseButtonEventArgs e)
        {
            HitTestResult htr = VisualTreeHelper.HitTest(Window.GetWindow(this), e.GetPosition(Window.GetWindow(this)));
            if (htr != null && htr.VisualHit != this)
            {
                // 结束所有的输入情况
                if (editargs.ColumnIndex >= 0 && editargs.RowIndex >= 0)
                {
                    if (editargs.UI != null)
                    {
                        object obj = RetrieveEditValue();
                        _treeitem.CacheDataRowIndex[editargs.RowIndex].Value[editargs.ColumnIndex] = obj;
                        editargs.Action = EditAction.EditDelete;
                        m_celledit?.Invoke(this, editargs);
                        editargs.ColumnIndex = -1;
                        editargs.RowIndex = -1;
                        ReleaseMouseCapture();
                        if (editargs.UI != null)
                        {
                            RemoveEditUI(editargs.UI);
                            editargs.UI = null;
                        }
                    }
                }
                editargs.Action = EditAction.None;
                editargs.ColumnIndex = m_mouseColumn;
                editargs.RowIndex = m_mouseRow;
                editargs.UI = null;
                return;
            }
            if (m_editColumns.Count > 0 && m_mouseColumn >= 0 && m_celledit != null)
            {
                if (m_editColumns.Contains(m_ColumnsSource[m_mouseColumn]))
                {
                    if (editargs.ColumnIndex == m_mouseColumn && editargs.RowIndex == m_mouseRow)
                    {
                        editargs.ColumnIndex = m_mouseColumn;
                        editargs.RowIndex = m_mouseRow;
                        // 这里是第二次鼠标按下了，应该要产生创建对象了
                        System.Diagnostics.Debug.WriteLine(string.Format("editargs.ColumnIndex = {0}, ediargs.RowIndex = {1}", editargs.ColumnIndex, editargs.RowIndex));
                        editargs.Action = EditAction.EditCreate;
                        GDI.Size size = GetCellPosition(editargs.ColumnIndex, editargs.RowIndex);
                        editargs.Bounds = new Rect(size.Width, size.Height, Columns[editargs.ColumnIndex].Width, RowHeight);
                        editargs.Value = _treeitem.CacheDataRowIndex[editargs.RowIndex].Value[m_ColumnsSource[editargs.ColumnIndex]].ToString();
                        m_celledit?.Invoke(this, editargs);
                        if (editargs.UI != null)
                        {
                            AddEditUI(editargs.UI);
                            editargs.UI.Focus();
                            CaptureMouse();

                        }


                    }
                    else
                    {
                        if (editargs.ColumnIndex >= 0 && editargs.RowIndex >= 0)
                        {
                            if (editargs.UI != null)
                            {
                                object obj = RetrieveEditValue();
                                _treeitem.CacheDataRowIndex[editargs.RowIndex].Value[editargs.ColumnIndex] = obj;
                                editargs.Action = EditAction.EditDelete;
                                m_celledit?.Invoke(this, editargs);
                                editargs.ColumnIndex = -1;
                                editargs.RowIndex = -1;
                                ReleaseMouseCapture();
                                if (editargs.UI != null)
                                {
                                    RemoveEditUI(editargs.UI);
                                    editargs.UI = null;
                                }
                            }
                        }
                        editargs.Action = EditAction.None;
                        editargs.ColumnIndex = m_mouseColumn;
                        editargs.RowIndex = m_mouseRow;
                        editargs.UI = null;
                    }
                }
                else
                {
                    if (editargs.ColumnIndex >= 0 && editargs.RowIndex >= 0)
                    {
                        if (editargs.UI != null)
                        {
                            object obj = RetrieveEditValue();
                            _treeitem.CacheDataRowIndex[editargs.RowIndex].Value[editargs.ColumnIndex] = obj;

                            editargs.Action = EditAction.EditDelete;
                            m_celledit?.Invoke(this, editargs);

                            editargs.ColumnIndex = -1;
                            editargs.RowIndex = -1;
                            editargs.Action = EditAction.None;
                            ReleaseMouseCapture();

                            RemoveEditUI(editargs.UI);
                            editargs.UI = null;
                        }
                    }
                }
            }
            else
            {
                if (editargs.ColumnIndex >= 0 && editargs.RowIndex >= 0)
                {
                    editargs.Action = EditAction.EditDelete;
                    m_celledit?.Invoke(this, editargs);
                    editargs.ColumnIndex = -1;
                    editargs.RowIndex = -1;
                    ReleaseMouseCapture();
                    if (editargs.UI != null)
                    {
                        RemoveEditUI(editargs.UI);
                        editargs.UI = null;
                    }
                    editargs.UI = null;
                }
            }
        }

        // 用来判断当前选中状态，更新相应选中行记录的
        private void DoSelectingEvent(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) // 单击事件
            {
                // 如果已经命中目标了，就不要再参与选中命中测试了
                if (MouseLeftButtonJudge(e) == true)
                    return;

                if (m_mouseRow >= 0 && m_mouseColumn >= 0)
                {
                    if (Mode == SelectionMode.Single)
                    {
                        System.Diagnostics.Debug.Assert(m_selectionRows.Count <= 1);

                        int prevSelectedRow = m_selectionRows.Count == 1 ? m_selectionRows.Single() : -1;
                        // 单选模式下，如果再次点中此行，则什么都不做，直接返回
                        if (m_mouseRow == prevSelectedRow)
                        {
                            return;
                        }
                        else
                        {
                            // 这里选中行发生变化了，那就清除老的行，记录下新行
                            m_selectionRows.Clear();
                            if (prevSelectedRow != -1)
                                Invalidate(paintRow(g, prevSelectedRow));
                            m_selectionRows.Add(m_mouseRow);
                            Invalidate(paintRow(g, m_mouseRow));

                            m_lastRowIndex = m_mouseRow;
                            CellSelectionEventArgs args = new CellSelectionEventArgs();
                            args.CellColumnIndex = m_mouseColumn;
                            args.CellRowIndex = m_mouseRow;
                            m_selectionChanged?.Invoke(this, args);
                        }
                    }
                    else if (Mode == SelectionMode.Mutiple)
                    {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) // 说明Control键被按住了，则开始处理吧
                        {
                            // 单个单个地选
                            if (!m_selectionRows.Contains(m_mouseRow))
                                m_selectionRows.Add(m_mouseRow);
                            else m_selectionRows.Remove(m_mouseRow);
                            Invalidate(paintRow(g, m_mouseRow));
                            m_lastRowIndex = m_mouseRow;

                            CellSelectionEventArgs args = new CellSelectionEventArgs();
                            args.CellColumnIndex = m_mouseColumn;
                            args.CellRowIndex = m_mouseRow;
                            m_selectionChanged?.Invoke(this, args);
                        }
                        else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) // 说明shift 键被按住了
                        {
                            if (m_selectionRows.Count == 0) // 直接选中这一条吧
                            {
                                m_selectionRows.Add(m_mouseRow);
                                Invalidate(paintRow(g, m_mouseRow));
                                m_lastRowIndex = m_mouseRow;

                                CellSelectionEventArgs args = new CellSelectionEventArgs();
                                args.CellColumnIndex = m_mouseColumn;
                                args.CellRowIndex = m_mouseRow;
                                m_selectionChanged?.Invoke(this, args);
                            }
                            else
                            {
                                // 开始跨度选吧
                                if (m_mouseRow == m_lastRowIndex)
                                {
                                    // 就清除吧
                                    int[] removedIndex = m_selectionRows.ToArray();
                                    m_selectionRows.Clear();
                                    foreach (int i in removedIndex)
                                    {
                                        Invalidate(paintRow(g, i));
                                    }

                                }
                                else if (m_mouseRow < m_lastRowIndex)
                                {
                                    int step = -1;
                                    int iTemp = m_lastRowIndex;
                                    while (m_mouseRow != iTemp)
                                    {
                                        iTemp += step;
                                        if (m_selectionRows.Add(iTemp))
                                        {
                                            Invalidate(paintRow(g, iTemp));
                                        }
                                    }

                                    List<int> removedIndex = new List<int>();
                                    m_selectionRows.RemoveWhere(new Predicate<int>((int i) =>
                                    {
                                        if (i > m_lastRowIndex || i < m_mouseRow)
                                        {
                                            removedIndex.Add(i);
                                            return true;
                                        }
                                        else return false;
                                    }));

                                    foreach (int i in removedIndex)
                                    {
                                        Invalidate(paintRow(g, i));
                                    }
                                }
                                else if (m_mouseRow > m_lastRowIndex)
                                {
                                    int step = 1;
                                    int iTemp = m_lastRowIndex;
                                    while (m_mouseRow != iTemp)
                                    {
                                        iTemp += step;
                                        if (m_selectionRows.Add(iTemp))
                                        {
                                            Invalidate(paintRow(g, iTemp));
                                        }
                                    }
                                    List<int> removedIndex = new List<int>();
                                    m_selectionRows.RemoveWhere(new Predicate<int>((int i) =>
                                    {
                                        if (i < m_lastRowIndex || i > m_mouseRow)
                                        {
                                            removedIndex.Add(i);
                                            return true;
                                        }
                                        else return false;
                                    }));

                                    foreach (int i in removedIndex)
                                    {
                                        Invalidate(paintRow(g, i));
                                    }
                                }

                                CellSelectionEventArgs args = new CellSelectionEventArgs();
                                args.CellColumnIndex = m_mouseColumn;
                                args.CellRowIndex = m_mouseRow;
                                m_selectionChanged?.Invoke(this, args);
                            }
                        }
                        else
                        {
                            int[] iArray = m_selectionRows.ToArray();
                            m_selectionRows.Clear();
                            bool bNeedPaint = true;
                            for (int i = 0; i < iArray.Length; ++i)
                            {
                                if (iArray[i] != m_mouseRow)
                                    Invalidate(paintRow(g, iArray[i]));
                                else
                                {
                                    bNeedPaint = false;
                                    m_selectionRows.Add(iArray[i]);
                                }
                            }

                            if (bNeedPaint)
                            {
                                m_selectionRows.Add(m_mouseRow);
                                m_lastRowIndex = m_mouseRow;
                                Invalidate(paintRow(g, m_mouseRow));
                            }

                            CellSelectionEventArgs args = new CellSelectionEventArgs();
                            args.CellColumnIndex = m_mouseColumn;
                            args.CellRowIndex = m_mouseRow;
                            m_selectionChanged?.Invoke(this, args);
                        }
                    }
                }
            }

        }

        private bool MouseLeftButtonJudge(MouseButtonEventArgs e)
        {
            if (m_mouseColumn < 0 || m_mouseRow < 0)
                return false;

            // 如果是在组合列上，且点在+/-号图标上，则展开的闭合，闭合地展开
            // TODO :
            Point ptTemp = e.GetPosition(this);
            GDI.Point pt = new GDI.Point((int)ptTemp.X, (int)ptTemp.Y) ;

            GDI.Size size = GetCellPosition(m_mouseColumn, m_mouseRow);
            //int iZeroIndex = 0;

            GDI.Rectangle rectTreeView = new GDI.Rectangle(size.Width, size.Height, (int)Columns[m_mouseColumn].Width, RowHeight);
            GDI.Rectangle rectCheckBox = rectTreeView;
            //if (_dv.Columns[iZeroIndex].Frozen == false) // 非冻结列时才减去滚动条的值
            {
                
                rectTreeView.X = 0 - (int)m_ptOffset.X;
                rectCheckBox.X = 0 - (int)m_ptOffset.X;
            }

            rectTreeView.Width = 20;
            rectCheckBox.Width = 15;

            if (_treeitem.CacheGroupHeaderIndex.ContainsKey(m_mouseRow))
            {
                TreeGridRow temptgr = _treeitem.CacheGroupHeaderIndex[m_mouseRow];
                rectTreeView.X += 15 * (temptgr.Level - 1);
                rectCheckBox.X += 15 * (temptgr.Level);
                if (rectTreeView.Contains(pt))
                {
                    if (temptgr != null)
                    {

                        temptgr.Expand = !temptgr.Expand;
                        _treeitem.DicForExpandState[temptgr.Key] = temptgr.Expand;

                       _treeitem.UpdateCacheIndex(temptgr, m_mouseRow);

                        //int iScrollVertical = _dv.FirstDisplayedScrollingRowIndex;
                        //int iScrollHorizontal = _dv.HorizontalScrollingOffset;
                        //int iIndex = _dv.CurrentRow.Index;
                        //_dv.Rows.Clear();
                        //_dv.RowCount = _treeitem.CacheDataRowIndex.Count;
                        //_dv.FirstDisplayedScrollingRowIndex = iScrollVertical;
                        //_dv.BeginInvoke(new Action(delegate () { _dv.HorizontalScrollingOffset = iScrollHorizontal; }));

                        //_dv.Rows[iIndex].Selected = true;
                        //_dv.CurrentCell = _dv.Rows[iIndex].Cells[0];
                        _treeitem.ResetRowCount();
                        //paintArea(new GDI.Rectangle(0, rectTreeView.Top, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - rectTreeView.Top));
                        //Invalidate(new Int32Rect(0, rectTreeView.Top, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight - rectTreeView.Top));
                        //OnExtentViewChanged?.Invoke();
                        return true;
                    }
                }
                else if (ShowCheckBox && rectCheckBox.Contains(pt))
                {
                    // 得到CheckBox的状态
                    DataRow dr = _treeitem.CacheDataRowIndex[m_mouseRow].Value;
                    if (dr.Table.Columns.Contains(CheckBoxColName))
                    {
                        string sCheck = "";
                        if (dr[CheckBoxColName] == DBNull.Value)
                            sCheck = "0";
                        else
                            sCheck = dr[CheckBoxColName].ToString();

                        if ("1" == sCheck || "True" == sCheck)
                            dr[CheckBoxColName] = "0";
                        else if ("0" == sCheck || "False" == sCheck)
                            dr[CheckBoxColName] = "1";
                        else
                        {
                            // Do nothing, 这里肯定是三态了，让其它程序自己处理吧，这里不知道怎么处理恰当
                        }

                        TreeGridRow tgr = null;
                        DataRow drParent = null;
                        if (temptgr.ParentGridRow != null)
                        {
                            tgr = temptgr.ParentGridRow;
                            if (tgr.ParentGridRow != null)
                            {
                                // 可以获取到他的ParentRow
                                foreach (KeyValuePair<int, TreeGridRow> kvp in tgr.ParentGridRow.Nodes)
                                {
                                    if (kvp.Value == tgr)
                                        drParent = tgr.ParentGridRow.ChildRows.Rows[kvp.Key];
                                    break;
                                }

                            }
                        }
                        else
                        {
                            if (dr.Table.ExtendedProperties.Contains("Parent"))
                                tgr = dr.Table.ExtendedProperties["Parent"] as TreeGridRow;
                            else tgr = _treeitem.MulitpleDataRoot;

                            int iRowIndex = 0;
                            if (dr.Table.ExtendedProperties.Contains("ParentRow"))
                                int.TryParse(dr.Table.ExtendedProperties["ParentRow"].ToString(), out iRowIndex);

                            if (tgr != null && tgr.ChildRows.Rows.Count > iRowIndex)
                                drParent = tgr.ChildRows.Rows[iRowIndex];
                        }

                        // TODO: 通知外面CheckBox改变了
                        //_dv.OnCheckStateChanged(e, dr, drParent, tgr);
                        Int32Rect iRect = paintRow(g, m_mouseRow);
                        Invalidate(iRect);
                        return true;
                    }
                }
            }
            else
            {
                rectCheckBox.X += MaxLevel * 15;
                rectCheckBox.Width = 15;
                if (ShowCheckBox && rectCheckBox.Contains(pt))
                {
                    // 得到CheckBox的状态
                    DataRow dr = _treeitem.CacheDataRowIndex[m_mouseRow].Value;
                    if (dr.Table.Columns.Contains(CheckBoxColName))
                    {
                        string sCheck = "";
                        if (dr[CheckBoxColName] == DBNull.Value)
                            sCheck = "0";
                        else
                            sCheck = dr[CheckBoxColName].ToString();

                        if ("1" == sCheck)
                            dr[CheckBoxColName] = "0";
                        else if ("0" == sCheck)
                            dr[CheckBoxColName] = "1";
                        else
                        {
                            // Do nothing, 这里肯定是三态了，让其它程序自己处理吧，这里不知道怎么处理恰当
                        }

                        TreeGridRow tgr = null;

                        if (dr.Table.ExtendedProperties.Contains("Parent"))
                            tgr = dr.Table.ExtendedProperties["Parent"] as TreeGridRow;
                        else tgr = _treeitem.MulitpleDataRoot;

                        int iRowIndex = 0;
                        if (dr.Table.ExtendedProperties.Contains("ParentRow"))
                            int.TryParse(dr.Table.ExtendedProperties["ParentRow"].ToString(), out iRowIndex);
                        DataRow drParent = null;
                        if (tgr != null)
                            drParent = tgr.ChildRows.Rows[iRowIndex];

                        // TODO: 通知外面CheckBox改变了
                        //_dv.OnCheckStateChanged(e, dr, drParent, tgr);
                        Int32Rect iRect = paintRow(g, m_mouseRow);
                        Invalidate(iRect);

                        return true;
                        //_dv.OnCheckStateChanged(e, dr, drParent, tgr);
                        //_dv.Invalidate(rectCheckBox);
                    }
                }
            }
            return false;
        }


        private int MaxLevel = 0;

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        internal int getperhapsWidth(int col)
        {
            int row = GetCellRow(0);
            int rowOffset = (int)PointOffset.Y % RowHeight;
            int iWidth = 0;
            for (int i = row; i < _treeitem.RowCount; ++i)
            {
                if ((i - row) * RowHeight - rowOffset >= (int)m_bufferedBmp.PixelHeight)
                    break;

                if (_treeitem.CacheDataRowIndex[i].Value.Table.Columns.Contains(m_ColumnsSource[col]))
                {
                    // 增加Format功能
                    object value = _treeitem.CacheDataRowIndex[i].Value[m_ColumnsSource[col]];
                    if (m_cellformat != null)
                    {
                        CellFormatEventArgs args = new CellFormatEventArgs();
                        args.ColumnIndex = col;
                        args.RowIndex = i;
                        args.Value = value;
                        args.Background = m_background;
                        args.Foreground = m_foreground;
                        m_cellformat(this, args);
                        value = args.Value;
                    }
                    GDI.Size size = System.Windows.Forms.TextRenderer.MeasureText(value.ToString(), m_font);
                    if (size.Width > iWidth)
                        iWidth = size.Width + 10;
                }
                else
                {
                    return 10; // 默认给10个像素吧
                }
            }

            return iWidth;
        }

        /// <summary>
        /// 重新绘制本区域内的内容
        /// </summary>
        /// <param name="rect">相对当前屏幕的坐标矩形</param>
        public void paintArea(GDI.Rectangle rect)
        {
            if (rect.Height == 0 || rect.Width == 0)
                return;

            if (g == null || ItemsSource == null)
                return;

            GDI.Drawing2D.GraphicsState state = g.Save();
            g.SetClip(rect);
            // 开始画图吧

            double iOffsetX = rect.X;
            int iColumn = GetCellColumn(ref iOffsetX);
            if (iColumn == -1)
            {
                g.Restore(state);
                return;
            }

            int iRow = 0;
            int canvasY = 0;

            iRow = GetCellRow(rect.Y);
            canvasY = GetCellPosition(0, iRow).Height;

            if (iRow < 0)
                iRow = 0; // 说明没有偏移任何行，从第0行开始绘制

            int iRowOffset = (rect.Y + (int)m_ptOffset.Y) % RowHeight;
            
            int iPaintCount = Math.Min((int)(rect.Height - iRowOffset) / RowHeight, _treeitem.RowCount);
            if (iRowOffset > 0)
                iPaintCount++;
            if ((int)(rect.Height - iRowOffset) % RowHeight > 0)
                iPaintCount++;

            g.FillRectangle(m_background, rect);
            CellPaintEventArgs e = new CellPaintEventArgs();
            
            for (int i = 0; i < iPaintCount + 1; ++i)
            {
                double iColumnWidth = rect.X - iOffsetX;
                if (i + iRow >= _treeitem.RowCount)
                    break;

                for (int j = iColumn; j < Columns.Count; ++j)
                {
                    if (iColumnWidth > ActualWidth)
                        break; // 说明这一行画完了，开始画下一行吧

                    if (i + iRow == -1)
                        continue;
                   
                    e.Bounds = new GDI.Rectangle((int)iColumnWidth, canvasY + i * RowHeight, (int)Columns[j].Width, RowHeight);

                    iColumnWidth += Columns[j].Width;
                    e.ColumnIndex = j;
                    e.RowIndex = i + iRow;
                    e.States = GridViewElementStates.Visible;
                    if (m_selectionRows.Contains(e.RowIndex))
                        e.States |= GridViewElementStates.Selected;
                    e.Graphics = g;
                    if (_treeitem.CacheDataRowIndex[e.RowIndex].Value.Table.Columns.Contains(m_ColumnsSource[j]))
                        e.Value = _treeitem.CacheDataRowIndex[e.RowIndex].Value[m_ColumnsSource[j]];
                    else e.Value = "";
                    DefaultGroupPaintCell(e);
                    //DefaultPaintCell(e);
                }
            }

            g.Restore(state);
        }


        private Int32Rect paintColumns(int col, int startPos)
        {
            CellPaintEventArgs e = new CellPaintEventArgs();
            e.Graphics = g;
            e.States = GridViewElementStates.Visible;
            e.ColumnIndex = col;

            int row = GetCellRow(0);
            int rowOffset = (int)PointOffset.Y % RowHeight; 
            for(int i = row; row < _treeitem.RowCount; ++i)
            {
                if ((i - row) * RowHeight - rowOffset >= (int)m_bufferedBmp.PixelHeight)
                    break;

                e.RowIndex = i;
                e.Bounds = new GDI.Rectangle(startPos, (i - row) * RowHeight - rowOffset, (int)Columns[col].Width, RowHeight);
                e.States = GridViewElementStates.Visible;
                e.Value = _treeitem.CacheDataRowIndex[i].Value[m_ColumnsSource[col]];
                DefaultGroupPaintCell(e);
                //DefaultPaintCell(e);
            }

            return new Int32Rect(startPos, 0, (int)Columns[col].Width, (int)m_bufferedBmp.PixelHeight);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point ptOwn = e.GetPosition(this);
            double iOffsetX = ptOwn.X;
            int iOffsetY = (int)ptOwn.Y;
            int iCol = GetCellColumn(ref iOffsetX);
            int iRow = GetCellRow(iOffsetY);


            if (iCol != m_mouseColumn || iRow != m_mouseRow)
            {
                // 说明图形位置更新了，那就需要来替换了啊
                // 高亮显示列吧
                doUpdateUI(new PartPaint((GDI.Graphics g) =>
                {
                    List<Int32Rect> invaliateRect = new List<Int32Rect>();
                    CellPaintEventArgs e1 = new CellPaintEventArgs();

                    int m_prev_mouseColumn = m_mouseColumn;
                    int m_prev_mouseRow = m_mouseRow;

                    m_mouseColumn = iCol;
                    m_mouseRow = iRow;
                   
                    // 先清除老的行
                    if (m_prev_mouseColumn != -1)
                    {
                        System.Diagnostics.Debug.WriteLine("m_prev_mouseColumn = " + m_prev_mouseColumn.ToString());
                        // 这里还要判断是不是上次的已经滚动走了
                        GDI.Size size = GetCellPosition(0, m_prev_mouseRow);
                        if (m_prev_mouseRow >= 0 && size.Height + RowHeight >= 0 && size.Height < ActualHeight) // 如果之前的行还在屏幕上的话，则刷新掉吧
                            invaliateRect.Add(paintRow(g, m_prev_mouseRow));
                    }

                    System.Diagnostics.Debug.WriteLine("m_mouseColumn = " + m_mouseColumn.ToString());

                    // 如果鼠标已经换到别的行，则再画新的行
                    if ((m_mouseRow != m_prev_mouseRow || m_prev_mouseColumn == -1) && m_mouseRow >= 0 && m_mouseColumn != -1)
                        invaliateRect.Add(paintRow(g, m_mouseRow));

                    return invaliateRect.ToArray();
                }));
            }


            // 这里在列上面，所以得判断光标是不是在两个列之间边缘，这种情况下要允许拖动列大小
            if (iCol != -1 && iRow == -1)
            { 
                if (iOffsetX < 3 || (iCol < Columns.Count - 1 && Columns[iCol].Width - iOffsetX < 3))
                {
                    //这里光标要变化
                    Cursor = Cursors.SizeWE;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }
            }
        }

        private readonly Object m_eventLock = new Object();
        private EventHandler<CellFormatEventArgs> m_cellformat;

        public event EventHandler<CellFormatEventArgs> CellFormat
        {
            add
            {
                lock (m_eventLock) { m_cellformat += value; }
            }
            remove
            {
                lock (m_eventLock) { m_cellformat -= value; }
            }
        }


        private EventHandler<CellPaintEventArgs> m_cellPaint;

        public event EventHandler<CellPaintEventArgs> CellPaint
        {
            add
            {
                lock (m_eventLock) { m_cellPaint += value; }
            }
            remove
            {
                lock (m_eventLock) { m_cellPaint -= value; }
            }
        }

        CellFormatEventArgs ef = new CellFormatEventArgs();

        
        void  DefaultGroupPaintCell(CellPaintEventArgs e)
        {
            int level = 0;
            if (ShowInGroup)
            {
                KeyValuePair<int, DataRow> dr = _treeitem.CacheDataRowIndex[e.RowIndex];
                level = dr.Key;
                if (_treeitem.CacheGroupHeaderIndex.ContainsKey(e.RowIndex))
                {
                    TreeGridRow temptgr = _treeitem.CacheGroupHeaderIndex[e.RowIndex];
                    level = temptgr.Level - 1;
                    GDI.Rectangle rtClip = new GDI.Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                    if (e.Bounds.Left < 0)
                    {
                        rtClip.X =  1;
                        e.Graphics.SetClip(rtClip);
                    }

                    if (temptgr.ChildRows.Rows.Count > 0) // 这里屏蔽掉只要分组了就算是一行也分组，
                    {
                        string sValue = "";
                        if (_treeitem.GroupShowStyle == TreeGridView.GroupStyle.GroupShow)
                        {
                            TreeGridView.GroupKey gk = _treeitem.MainKey;
                            for (int k = 1; k < temptgr.Level; ++k)
                            {
                                gk = gk.SubKey;
                            }

                            foreach (string s in gk.Keys)
                            {
                                string valueText = "";
                                //if (!_dict.ContainsKey(s) || !_dict[s].TryGetValue(dr.Value[s] + "", out valueText))
                                    valueText = dr.Value[s].ToString();

                                //if (sValue == "")
                                //    sValue += Columns[s].HeaderText + "：" + valueText;
                                //else
                                //    sValue += ", " + _dv.Columns[s].HeaderText + "：" + valueText;
                            }
                            sValue += " [ " + temptgr.ChildRows.DefaultView.Count + " ]";
                            GDI.Rectangle rectBackground = new GDI.Rectangle(  1, e.Bounds.Top, m_bufferedBmp.PixelWidth - 0, e.Bounds.Height);
                            GDI.Brush backbrush = m_background;
                            GDI.Brush foreBrush = m_foreground;
                            e.Graphics.FillRectangle(backbrush, e.Bounds);

                            //if (Columns.Count - 1 == e.ColumnIndex) // 最后一列
                            //{
                            //    e.Graphics.DrawLines(Pens.Gray, new Point[]{new Point(e.CellBounds.Left - 1, e.CellBounds.Top - 1), new Point(e.CellBounds.Right - 1, e.CellBounds.Top - 1),
                            //        new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1), new Point(e.CellBounds.Left - 1, e.CellBounds.Bottom - 1)});
                            //}
                            //else if (_dv.Columns[e.ColumnIndex].DisplayIndex == 0) // 第一列
                            //{
                            //    e.Graphics.DrawLines(Pens.Gray, new Point[]{new Point(e.CellBounds.Right - 1, e.CellBounds.Top - 1), new Point(e.CellBounds.Left - 1, e.CellBounds.Top - 1),
                            //        new Point(e.CellBounds.Left - 1, e.CellBounds.Bottom - 1), new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1)});
                            //}
                            //else // 中间列
                            //{
                            //    e.Graphics.DrawLine(Pens.Gray, new Point(e.CellBounds.Left - 1, e.CellBounds.Top - 1), new Point(e.CellBounds.Right - 1, e.CellBounds.Top - 1));
                            //    e.Graphics.DrawLine(Pens.Gray, new Point(e.CellBounds.Left - 1, e.CellBounds.Bottom - 1), new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1));
                            //}
                        }
                        else
                        {
                            DefaultPaintBackground(e);
                        }

                        //Size szCheckBox = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.CheckedNormal);

                        Thickness paddding = e.Margain;
                        if (e.ColumnIndex == 0)
                        {
                            paddding.Left = 4 + 15 * (temptgr.Level);

                            GDI.Brush foreBrush = m_foreground;
                            GDI.Brush backBrush = m_background;
                            if ((e.States & GridViewElementStates.Selected) == GridViewElementStates.Selected)
                            {
                                foreBrush = m_selectedforeground;
                                backBrush = m_selectedbackground;
                            }
                            using (GDI.Pen treepen = new GDI.Pen(foreBrush, 1))
                            {
                                GDI.Drawing2D.SmoothingMode mode = e.Graphics.SmoothingMode;
                                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                if (temptgr.Expand == false)
                                {
                                    
                                    int iOffset = (e.Bounds.Height - (int)Math.Sqrt(200)) / 2;
                                    int Bianchang = (int)Math.Sqrt(100 - (int)Math.Sqrt(200) * (int)Math.Sqrt(200) / 4);
                                    GDI.Point[] ptlist = new GDI.Point[] { new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1), e.Bounds.Top + iOffset), new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1), e.Bounds.Bottom - iOffset), new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + Bianchang, e.Bounds.Height / 2 + e.Bounds.Top) };
                                    //e.Graphics.FillPolygon(foreBrush, ptlist, GDI.Drawing2D.FillMode.Winding);
                                    e.Graphics.DrawPolygon(treepen, ptlist);
                                }
                                else
                                {
                                    int iOffset = (e.Bounds.Height - 10) / 2;
                                    GDI.Point[] ptlist = new GDI.Point[] { new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1), e.Bounds.Bottom - iOffset), new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + 10, e.Bounds.Bottom - iOffset), new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + 10, e.Bounds.Top + iOffset) };
                                    e.Graphics.DrawPolygon(treepen, ptlist);
                                }
                                e.Graphics.SmoothingMode = mode;
                                //e.Graphics.DrawRectangle(treepen, new GDI.Rectangle(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1), e.Bounds.Y + (e.Bounds.Height / 2) - 4, 10, 10));
                                //e.Graphics.DrawLine(treepen, new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + 2, e.Bounds.Y + (e.Bounds.Height / 2) + 1), new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + 8, e.Bounds.Y + (e.Bounds.Height / 2) + 1));
                                //if (temptgr.Expand == false)
                                //    e.Graphics.DrawLine(treepen, new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + 5, e.Bounds.Y + (e.Bounds.Height / 2) - 2), new GDI.Point(e.Bounds.Left + 4 + 15 * (temptgr.Level - 1) + 5, e.Bounds.Y + (e.Bounds.Height / 2) + 4));
                            }
                            if (ShowCheckBox) // 如果显示CheckBox
                            {

                                paddding.Left += 16;
                                // 得到CheckBoxState
                                string sColName = CheckBoxColName;
                                // TODO: 这里得绘制CheckBox
                                if (dr.Value.Table.Columns.Contains(sColName))
                                {
                                    string sBoolValue = dr.Value[sColName].ToString();
                                    GDI.Rectangle rectNew = new GDI.Rectangle(e.Bounds.Left + 4 + 15 * temptgr.Level, e.Bounds.Top + (e.Bounds.Height - 16) / 2, 12, 12);
                                    e.Graphics.DrawRectangle(GDI.Pens.Black, rectNew);
                                    // 三态先不管了
                                    if (sBoolValue == "1")
                                    {
                                        using (GDI.Pen pen = new GDI.Pen(GDI.Brushes.Black, 2))
                                        {
                                            GDI.Pens.Black.StartCap = GDI.Drawing2D.LineCap.Round;
                                            pen.EndCap = GDI.Drawing2D.LineCap.Round;
                                            pen.LineJoin = GDI.Drawing2D.LineJoin.Round;
                                            e.Graphics.DrawLines(GDI.Pens.Black, new GDI.Point[] { new GDI.Point(rectNew.Left + 2, rectNew.Top + 7), new GDI.Point(rectNew.Left + 5, rectNew.Top + 13), new GDI.Point(rectNew.Left + 13, rectNew.Top + 5) });
                                        }
                                    }
                                    //CheckBoxState cs = CheckBoxState.UncheckedNormal;
                                    //if (sBoolValue == "1")
                                    //    cs = CheckBoxState.CheckedNormal;
                                    //else if (sBoolValue == "0")
                                    //    cs = CheckBoxState.UncheckedNormal;
                                    //else if (sBoolValue == "-1") // 三态
                                    //{
                                    //    cs = CheckBoxState.MixedNormal;
                                    //}
                                    //CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.CellBounds.Left + 4 + 15 * temptgr.Level, e.CellBounds.Top + (e.CellBounds.Height - szCheckBox.Height) / 2), cs);
                                }
                            }

                        }
                        else paddding.Left = 0;


                        if (_treeitem.GroupShowStyle == TreeGridView.GroupStyle.GroupShow)
                        {
                            //using (Font boldFont = new Font(e.CellStyle.Font, FontStyle.Bold))
                            //{
                            //    Rectangle rectBackground = new Rectangle(_dv.RowHeadersWidth + 1 - _dv.HorizontalScrollingOffset, e.CellBounds.Top, _dv.Width - _dv.RowHeadersWidth, e.CellBounds.Height);

                            //    System.Drawing.StringFormat sf = new System.Drawing.StringFormat();
                            //    sf.Alignment = StringAlignment.Near;
                            //    sf.LineAlignment = StringAlignment.Center;
                            //    int iCheckBoxWidth = szCheckBox.Width;
                            //    if (_dv.ShowCheckBox == false)
                            //        iCheckBoxWidth = 0;

                            //    if (rectBackground.Left < _dv.RowHeadersWidth)
                            //        e.Graphics.SetClip(new Rectangle(_dv.RowHeadersWidth, rectBackground.Top, rectBackground.Width - (rectBackground.Left - _dv.RowHeadersWidth), rectBackground.Height));
                            //    // 这里添加组头上额外绘制自定义元素
                            //    int nPaddingLeft = rectBackground.Left + 20 + 15 * (temptgr.Level - 1) + iCheckBoxWidth;
                            //    //nPaddingLeft += _dv.PaintingRowHeader(sender, e, new Rectangle(nPaddingLeft, rectBackground.Top, rectBackground.Width - 20, rectBackground.Height));
                            //    //e.Graphics.DrawString(sValue, boldFont, SystemBrushes.WindowText, new Rectangle(nPaddingLeft, rectBackground.Top, rectBackground.Width - 20, rectBackground.Height), sf);
                            //}
                        }
                        else
                        {
                            e.Margain = paddding;
                            DefaultPaintCellContent(e);

                        }

                        e.Graphics.ResetClip();
                        e.Handled = true;
                    }

                    return;
                }
                else // 这里是叶子节点
                {
                    Thickness pad = e.Margain;

                    if (ShowInGroup && e.ColumnIndex == 0)
                        pad.Left = 15 * (level + 1);
                    else pad.Left = 0;
                    DefaultPaintBackground(e);
                    // TODO
                    //e.PaintBackground(e.ClipBounds, (e.State & DataGridViewElementStates.Selected) > 0 ? true : false);
                    if (ShowCheckBox && e.ColumnIndex == 0)
                    {
                        string sColName = CheckBoxColName;
                        if (dr.Value.Table.Columns.Contains(sColName))
                        {
                            string sBoolValue = dr.Value[sColName].ToString();
                            GDI.Rectangle rectNew = new GDI.Rectangle(e.Bounds.Left + 4 + 15 * level, e.Bounds.Top + (e.Bounds.Height - 16) / 2, 12, 12);
                            e.Graphics.DrawRectangle(GDI.Pens.Black, rectNew);
                            // 三态先不管了
                            if (sBoolValue == "1")
                            {
                                using (GDI.Pen pen = new GDI.Pen(GDI.Brushes.Black, 2))
                                {
                                    GDI.Pens.Black.StartCap = GDI.Drawing2D.LineCap.Round;
                                    pen.EndCap = GDI.Drawing2D.LineCap.Round;
                                    pen.LineJoin = GDI.Drawing2D.LineJoin.Round;
                                    e.Graphics.DrawLines(GDI.Pens.Black, new GDI.Point[] { new GDI.Point(rectNew.Left + 2, rectNew.Top + 7), new GDI.Point(rectNew.Left + 5, rectNew.Top + 13), new GDI.Point(rectNew.Left + 13, rectNew.Top + 5) });
                                }
                            }
                            //string sBoolValue = dr.Value[sColName].ToString();
                            //CheckBoxState cs = CheckBoxState.UncheckedNormal;
                            //if (sBoolValue == "1")
                            //    cs = CheckBoxState.CheckedNormal;
                            //else if (sBoolValue == "0")
                            //    cs = CheckBoxState.UncheckedNormal;
                            //else if (sBoolValue == "-1") // 三态
                            //{
                            //    cs = CheckBoxState.MixedNormal;
                            //}
                            //Size sz = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.CheckedNormal);
                            //CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.CellBounds.Left + 4 + 15 * level, e.CellBounds.Top + (e.CellBounds.Height - sz.Height) / 2), cs);
                        }
                        e.Margain = pad;
                        DefaultPaintCellContent(e);
                        //e.PaintContent(e.ClipBounds);
                        e.Handled = true;
                    }
                    else
                    {
                        e.Margain = pad;
                        DefaultPaintCellContent(e);
                    }
                }
            }
            else
            {
                DefaultPaintBackground(e);
                DefaultPaintCellContent(e);
            }
        }

        void DefaultPaintBackground(CellPaintEventArgs e)
        {
            // 有外部Paint的，这里就不要管了，直接忽略吧
            if (m_cellPaint != null)
                return;

            GDI.Brush backbrush = m_background;
            if ((e.States & GridViewElementStates.Selected) == GridViewElementStates.Selected)
            {
                backbrush = m_selectedbackground;
            }
            else
            {
                if (e.ColumnIndex == m_mouseColumn && e.RowIndex == m_mouseRow)
                {
                    backbrush = m_hoverbackground;
                }
                else if (e.RowIndex == m_mouseRow && m_mouseColumn != -1)
                {
                    backbrush = m_hoverbackground;
                }
            }

            if (backbrush is System.Drawing.Drawing2D.LinearGradientBrush)
            {
                System.Drawing.Drawing2D.LinearGradientBrush lgb = backbrush as System.Drawing.Drawing2D.LinearGradientBrush;
                e.Graphics.ResetTransform();
                e.Graphics.TranslateTransform(e.Bounds.X, e.Bounds.Y);
                e.Graphics.FillRectangle(backbrush, 0, 0, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.ResetTransform();
            }
            else
                e.Graphics.FillRectangle(backbrush, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        }


        void DefaultPaintCellContent(CellPaintEventArgs e)
        {
            GDI.Brush foreBrush = m_foreground;

            if ((e.States & GridViewElementStates.Selected) == GridViewElementStates.Selected)
            {
                foreBrush = m_selectedforeground;
            }

            if (m_cellformat != null)
            {
                ef.RowIndex = e.RowIndex;
                ef.Value = e.Value;
                ef.ColumnIndex = e.ColumnIndex;
                ef.Foreground = null;
                ef.Background = null;
                m_cellformat(this, ef);
                if (ef.FormattingApplied)
                    e.Value = ef.Value;

                if (ef.Foreground != null)
                    foreBrush = ef.Foreground;
            }

            if (m_cellPaint != null)
            {
                e.Alignment = ef.Alignment;
                m_cellPaint(this, e);
                if (e.Handled == true)
                    return;
            }

            if (e.Value != null)
            {
                GDI.StringFormat sf = new System.Drawing.StringFormat();
                sf.Alignment = ef.Alignment;
                sf.FormatFlags = System.Drawing.StringFormatFlags.NoWrap;
                sf.LineAlignment = System.Drawing.StringAlignment.Center;
                sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                e.Graphics.DrawString(e.Value.ToString(), Font, foreBrush, new GDI.Rectangle(e.Bounds.Left + 5 + (int)e.Margain.Left, e.Bounds.Top + (int)e.Margain.Top, e.Bounds.Width - 5 - (int)e.Margain.Left - (int)e.Margain.Right, e.Bounds.Height - (int)e.Margain.Top - (int)e.Margain.Bottom), sf);
            }
        }
        void DefaultPaintCell(CellPaintEventArgs e)
        {

            GDI.Brush backbrush = m_background;
            GDI.Brush foreBrush = m_foreground;

           
            if ((e.States & GridViewElementStates.Selected) == GridViewElementStates.Selected)
            {
                foreBrush = m_selectedforeground;
                backbrush = m_selectedbackground;
            }
            else
            {
                if (e.ColumnIndex == m_mouseColumn && e.RowIndex == m_mouseRow)
                {
                    backbrush = m_hoverbackground;
                }
                else if (e.RowIndex == m_mouseRow && m_mouseColumn != -1)
                {
                    backbrush = m_hoverbackground;
                }
            }

            if (m_cellformat != null)
            {
                ef.RowIndex = e.RowIndex;
                ef.Value = e.Value;
                ef.ColumnIndex = e.ColumnIndex;
                ef.Foreground = null;
                ef.Background = null;
                m_cellformat(this, ef);
                if (ef.FormattingApplied)
                    e.Value = ef.Value;

                if (ef.Foreground != null)
                    foreBrush = ef.Foreground;
                if (ef.Background != null)
                    backbrush = ef.Background;
            }

            if (m_cellPaint != null)
            {
                m_cellPaint(this, e);
                if (e.Handled == true)
                    return;
            }


            if (backbrush is System.Drawing.Drawing2D.LinearGradientBrush)
            {
                System.Drawing.Drawing2D.LinearGradientBrush lgb = backbrush as System.Drawing.Drawing2D.LinearGradientBrush;
                e.Graphics.ResetTransform();
                e.Graphics.TranslateTransform(e.Bounds.X, e.Bounds.Y);
                e.Graphics.FillRectangle(backbrush, 0, 0, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.ResetTransform();
            }
            else
                e.Graphics.FillRectangle(backbrush, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            if (e.Value != null)
            {
                GDI.StringFormat sf = new System.Drawing.StringFormat();
                sf.Alignment = System.Drawing.StringAlignment.Near;
                sf.FormatFlags = System.Drawing.StringFormatFlags.NoWrap;
                sf.LineAlignment = System.Drawing.StringAlignment.Center;
                sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                e.Graphics.DrawString(e.Value.ToString(), Font, foreBrush, new GDI.Rectangle(e.Bounds.Left + 5 + (int)e.Margain.Left, e.Bounds.Top + (int)e.Margain.Top, e.Bounds.Width - 5 - (int)e.Margain.Left - (int)e.Margain.Right, e.Bounds.Height - (int)e.Margain.Top - (int)e.Margain.Bottom), sf);
            }
        }

        public void Redraw()
        {
            if (m_bufferedBmp != null)
            {
                paintArea(new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
                Invalidate(new Int32Rect(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
            }
        }

        ///// <summary>
        ///// 强制刷新界面
        ///// </summary>
        //private void ForceRefresh()
        //{
        //    m_bufferedBmp.Lock();
        //    //using (GDI.Bitmap backBuffer = new GDI.Bitmap((int)m_bufferedBmp.Width, (int)m_bufferedBmp.Height, m_bufferedBmp.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, m_bufferedBmp.BackBuffer))
        //    {
        //       // using (GDI.Graphics g = GDI.Graphics.FromImage(backBuffer))
        //        {
        //            g.SmoothingMode = GDI.Drawing2D.SmoothingMode.Default;
        //            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
        //            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        //            //g.Clear(GDI.Color.Transparent);

        //            if (ItemsSource == null)
        //                return;

        //            int iOffsetY = (int)PointOffset.Y;
        //            int iOffsetX = 0;
        //            int iColumn = GetCellColumn(ref iOffsetX);
        //            if (iColumn < 0)
        //                return;
        //            int iRow = iOffsetY / RowHeight;
        //            int iRowOffset = iOffsetY % RowHeight;
        //            int iPaintCount = Math.Min((int)(ActualHeight) / RowHeight, ItemsSource.Count);
        //            CellPaintEventArgs e = new CellPaintEventArgs();
                    
        //            for (int i = 0; i < iPaintCount + 1; ++i)
        //            {
        //                int iColumnWidth = 0;
        //                for (int j = iColumn; j < Columns.Count; ++j)
        //                {
        //                    if (-iOffsetX + iColumnWidth > ActualWidth)
        //                        break; // 说明这一行画完了，开始画下一行吧

        //                    e.Bounds = new GDI.Rectangle(-iOffsetX + iColumnWidth, i * RowHeight - iRowOffset, Columns[j].Width, RowHeight);
        //                    iColumnWidth += Columns[j].Width;
        //                    e.ColumnIndex = j;
        //                    e.RowIndex = i + iRow;
        //                    e.States = GridViewElementStates.Visible;
        //                    e.Graphics = g;
        //                    e.Value = ItemsSource[e.RowIndex][Columns[j].ColName];
        //                    DefaultPaintCell(e);
        //                }
        //            }
        //            // 开始绘画
        //            m_bufferedBmp.AddDirtyRect(new Int32Rect(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
        //            g.Flush();
        //        }
        //    }
        //    m_bufferedBmp.Unlock();
        //}

        GDI.Bitmap m_bmp = null;
        GDI.Graphics g = null;
        private void GB_GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize != e.PreviousSize && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                int iTempWidth = (int)ActualWidth;
                if (ActualWidth < 1)
                    iTempWidth = 1;
                int iTempHeight = (int)ActualHeight;
                if (ActualHeight < 1)
                    iTempHeight = 1;
                m_bufferedBmp = new WriteableBitmap(iTempWidth, iTempHeight, 96.0, 96.0, PixelFormats.Pbgra32, null);
                if (m_bmp != null)
                    m_bmp.Dispose();
                m_bmp = new GDI.Bitmap(m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight,
                    m_bufferedBmp.BackBufferStride, GDI.Imaging.PixelFormat.Format32bppPArgb,
                    m_bufferedBmp.BackBuffer);
                
                g = GDI.Graphics.FromImage(m_bmp);
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                paintArea(new GDI.Rectangle(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
                Invalidate(new Int32Rect(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
                
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
           
            drawingContext.DrawImage(m_bufferedBmp, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
            base.OnRender(drawingContext);
        }

        public void SetHorizontalOffset(double offset)
        {
            offset = (int)offset;
            

            // 这里产生了水平滚动，识别哪些数据可以使用就只需copy一下就可以了
            int OffsetX = (int)Math.Abs(m_ptOffset.X - offset);
            if (OffsetX < ActualWidth - 20) // 说明是可以使用部分数据的， -20是如果只有20象素宽可用的话，那也没意义了，直接刷新效率更好
            {
                if (m_ptOffset.X < offset)
                {
                    if ((int)ActualWidth <= m_bufferedBmp.PixelWidth && (int)ActualHeight <= m_bufferedBmp.PixelHeight)
                        Bitblt(m_bufferedBmp, new GDI.Rectangle(OffsetX, 0, (int)m_bufferedBmp.PixelWidth - OffsetX, (int)m_bufferedBmp.PixelHeight),
                            new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth - OffsetX, (int)m_bufferedBmp.PixelHeight));

                    m_ptOffset.X = offset;
                    if ((int)ActualWidth <= m_bufferedBmp.PixelWidth && (int)ActualHeight <= m_bufferedBmp.PixelHeight)
                        paintArea(new GDI.Rectangle((int)m_bufferedBmp.PixelWidth - OffsetX, 0, OffsetX, (int)m_bufferedBmp.PixelHeight));
                }
                else
                {
                    if ((int)ActualWidth <= m_bufferedBmp.PixelWidth && (int)ActualHeight <= m_bufferedBmp.PixelHeight)
                        Bitblt(m_bufferedBmp, new GDI.Rectangle(0, 0, m_bufferedBmp.PixelWidth - OffsetX, m_bufferedBmp.PixelHeight),
                        new GDI.Rectangle(OffsetX, 0, (int)m_bufferedBmp.PixelWidth - OffsetX, (int)m_bufferedBmp.PixelHeight));
                    m_ptOffset.X = offset;
                    if ((int)ActualWidth <= m_bufferedBmp.PixelWidth && (int)ActualHeight <= m_bufferedBmp.PixelHeight)
                        paintArea(new GDI.Rectangle(0, 0, OffsetX, (int)m_bufferedBmp.PixelHeight));
                }
            }
            else
            {
                m_ptOffset.X = offset;
                if ((int)ActualWidth <= m_bufferedBmp.PixelWidth && (int)ActualHeight <= m_bufferedBmp.PixelHeight)
                    paintArea(new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
            }

            Invalidate(new Int32Rect(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));

            // 这里要产生Move事件了
            if (editargs.ColumnIndex >= 0 && editargs.RowIndex >= 0 && editargs.Action != EditAction.None)
            {
                GDI.Size size = GetCellPosition(editargs.ColumnIndex, editargs.RowIndex);
                //editargs.Bounds = new Rect(size.Width, size.Height, Columns[editargs.ColumnIndex].Width, RowHeight);
                if (editargs.UI != null)
                {
                    double xOffset = size.Width;
                    double xWidth = editargs.Bounds.Width;
                    if (size.Width < 0)
                    {
                        xOffset = 0;
                        xWidth += size.Width;

                    }
                    double yOffset = size.Height;
                    double yHeight = editargs.Bounds.Height;
                    if (size.Height < 0)
                    {
                        yOffset = 0;
                        yHeight += size.Height;
                    }

                    if (xWidth > 0 && yHeight > 0)
                    {
                        editargs.UI.Margin = new Thickness(xOffset, yOffset, 0, 0);
                        editargs.UI.Width = xWidth;
                        editargs.UI.Height = yHeight;
                        editargs.UI.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        editargs.UI.Visibility = Visibility.Hidden;
                    }

                }
            }
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    bool bNeedUpdate = false;
        //    if (m_bufferedBmp != null &&  m_ptOffset.X + ViewportWidth > ExtentWidth)
        //    {
        //        bNeedUpdate = true;
        //        m_ptOffset.X = ExtentWidth - ViewportWidth;
        //        if (m_ptOffset.X < 0)
        //            m_ptOffset.X = 0;

        //    }

        //    if (m_bufferedBmp != null && m_ptOffset.Y + ViewportHeight > ExtentHeight)
        //    {
        //        bNeedUpdate = true;
        //        m_ptOffset.Y = ExtentHeight - ViewportHeight;
        //        if (m_ptOffset.Y < 0)
        //            m_ptOffset.Y = 0;
        //    }
        //    if (bNeedUpdate)
        //        UpdateUI();
        //    return base.MeasureOverride(availableSize);
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    return base.ArrangeOverride(finalSize);
        //}

        public void SetVerticalOffset(double offset)
        {
            offset = (int)offset;
            if (offset < 0 || m_bufferedBmp.PixelHeight >= ExtentHeight)
            {
                offset = 0;
            }
            else
            {
                if (offset + m_bufferedBmp.PixelHeight >= ExtentHeight)
                {
                    offset = ExtentHeight - m_bufferedBmp.PixelHeight;
                }
            }

            // 这里产生了垂直滚动，识别哪些数据可以使用就只需copy一下就可以了
            int OffsetY = (int)Math.Abs(m_ptOffset.Y - offset);
            if (OffsetY < ActualHeight - 20) // 说明是可以使用部分数据的， -20是如果只有20象素宽可用的话，那也没意义了，直接刷新效率更好
            {
                if (m_ptOffset.Y < offset)
                {
                    // 这里要避开列表头的拷贝
                    Bitblt(m_bufferedBmp, new GDI.Rectangle(0, OffsetY, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight - OffsetY),
                        new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight - OffsetY));

                    m_ptOffset.Y = offset;
                    paintArea(new GDI.Rectangle(0, (int)m_bufferedBmp.PixelHeight - OffsetY, (int)m_bufferedBmp.PixelWidth, OffsetY));
                }
                else
                {
                    Bitblt(m_bufferedBmp, new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight - OffsetY),
                        new GDI.Rectangle(0, OffsetY, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight - OffsetY));
                    m_ptOffset.Y = offset;
                    paintArea(new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, OffsetY));
                }

                System.Diagnostics.Debug.WriteLine("OffsetY = " + OffsetY.ToString());
            }
            else
            {
                m_ptOffset.Y = offset;
                paintArea(new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
            }

            Invalidate(new Int32Rect(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));

            // 这里要产生Move事件了
            if (editargs.ColumnIndex >= 0 && editargs.RowIndex >= 0 && editargs.Action != EditAction.None)
            {
                GDI.Size size = GetCellPosition(editargs.ColumnIndex, editargs.RowIndex);
                //editargs.Bounds = new Rect(size.Width, size.Height, Columns[editargs.ColumnIndex].Width, RowHeight);
                //editargs.Action = EditAction.EditMove;
                System.Diagnostics.Debug.WriteLine("Offset = " + size.Height.ToString());
                if (editargs.UI != null)
                {
                    double xOffset = size.Width;
                    double xWidth = editargs.Bounds.Width;
                    if (size.Width < 0)
                    {
                        xOffset = 0;
                        xWidth += size.Width;

                    }
                    double yOffset = size.Height;
                    double yHeight = editargs.Bounds.Height;
                    if (size.Height < 0)
                    {
                        yOffset = 0;
                        yHeight += size.Height;
                    }

                    if (xWidth > 0 && yHeight > 0)
                    {
                        editargs.UI.Margin = new Thickness(xOffset, yOffset, 0, 0);
                        editargs.UI.Width = xWidth;
                        editargs.UI.Height = yHeight;
                        editargs.UI.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        editargs.UI.Visibility = Visibility.Hidden;
                    }
                    
                }
            }
        }

        public void Invalidate(Int32Rect rect)
        {
            if ( rect.Y + rect.Height < 0 || rect.X + rect.Width < 0 || rect.Y > m_bufferedBmp.PixelHeight)
                return;
            m_bufferedBmp.Lock();
            if (rect.Width + rect.X > m_bufferedBmp.Width)
                rect.Width = m_bufferedBmp.PixelWidth - rect.X;
            if (rect.Height + rect.Y > m_bufferedBmp.Height)
                rect.Height = m_bufferedBmp.PixelHeight - rect.Y;

            if (rect.X < 0)
            {
                rect.Width += rect.X;
                rect.X = 0;
            }
            if (rect.Y < 0)
            {
                rect.Height += rect.Y;
                rect.Y = 0;
            }
            m_bufferedBmp.AddDirtyRect(rect);
            m_bufferedBmp.Unlock();
        }

        public void Invalidate()
        {
            m_bufferedBmp.Lock();
            m_bufferedBmp.AddDirtyRect(new Int32Rect(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
            m_bufferedBmp.Unlock();
        }

        WriteableBitmap m_bufferedBmp = null;

    }
}
