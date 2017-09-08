using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    ///     <MyNamespace:GB_GridHeader/>
    ///
    /// </summary>
    public class GB_GridHeader : FrameworkElement
    {
        static GB_GridHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GB_GridHeader), new FrameworkPropertyMetadata(typeof(GB_GridHeader)));
        }


        public GB_GridHeader()
        {
            m_bufferedBmp = new WriteableBitmap(1, 23, 96.0, 96.0, PixelFormats.Pbgra32, null);
            SizeChanged += GB_GridHeader_SizeChanged;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            UseLayoutRounding = true;
            //SnapsToDevicePixels = true;
        }

        private Dictionary<string, System.Windows.Forms.SortOrder> m_sortgraph = new Dictionary<string, System.Windows.Forms.SortOrder>();
        public void ClearSortGraph()
        {
            m_sortgraph.Clear();
        }

        public void SetSortGraphColumn(int index, System.Windows.Forms.SortOrder so)
        {
            Binding bind = Columns[index].DisplayMemberBinding as Binding;
            if (bind != null)
            {
                m_sortgraph[bind.Path.Path] = so;
            }
        }

        public void SetSortGraphColumn(string columnname, System.Windows.Forms.SortOrder so)
        {
            m_sortgraph[columnname] = so;
        }

        public string GetColumnBindName(int iColumn)
        {
            Binding bind = Columns[iColumn].DisplayMemberBinding as Binding;
            if (bind != null)
            {
                return bind.Path.Path;
            }
            else
                return "";
        }

        public void Redraw()
        {
            paintArea(new System.Drawing.Rectangle(0, 0, (int)ActualWidth, (int)ActualHeight));
            Invalidate();
        }

        public System.Windows.Forms.SortOrder GetSortGraphColumn(int index)
        {
            System.Windows.Forms.SortOrder so;
            Binding bind = Columns[index].DisplayMemberBinding as Binding;
            if (bind != null)
            {
                if (m_sortgraph.TryGetValue(bind.Path.Path, out so))
                    return so;
                else return System.Windows.Forms.SortOrder.None;
            }
            else return System.Windows.Forms.SortOrder.None;
        }

        public System.Windows.Forms.SortOrder GetSortGraphColumn(string column)
        {
            System.Windows.Forms.SortOrder so;
            if (m_sortgraph.TryGetValue(column, out so))
                return so;
            else return System.Windows.Forms.SortOrder.None;
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background",
            typeof(Brush),
            typeof(GB_GridHeader), new PropertyMetadata(Brushes.White, OnBackgroundChanged));

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.RegisterAttached(
            "HoverBackground",
            typeof(Brush),
            typeof(GB_GridHeader), new PropertyMetadata(Brushes.White, OnHoverBackgroundChanged));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
            "Foreground", typeof(Brush), typeof(GB_GridHeader), new PropertyMetadata(Brushes.Black, OnForegroundChanged));

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridHeader header = d as GB_GridHeader;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, header.m_bufferedBmp.PixelHeight);
                if (brush != null)
                {
                    header.m_foreground.Dispose();
                    header.m_foreground = brush;
                }
            }
        }

        private static void OnHoverBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridHeader header = d as GB_GridHeader;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, header.m_bufferedBmp.PixelHeight);
                if (brush != null)
                {
                    header.m_hoverbackground.Dispose();
                    header.m_hoverbackground = brush;
                }
            }
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GB_GridHeader header = d as GB_GridHeader;
                GDI.Brush brush = ConvertWpfBrush2GDI(e.NewValue as Brush, header.m_bufferedBmp.PixelHeight);
                if (brush != null)
                {
                    header.m_background.Dispose();
                    header.m_background = brush;
                }
            }
        }

        public Brush Background
        {
            set
            {
                SetValue(BackgroundProperty, value);
            }
            get { return (Brush)GetValue(BackgroundProperty); }
        }

        public Brush Foreground
        {
            set
            {
                SetValue(ForegroundProperty, value);
            }
            get { return (Brush)GetValue(ForegroundProperty); }
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
                for(int j = 0; j < cblend.Colors.Length; ++j)
                {
                    valuelist.Add((float)wpfBrush.GradientStops[j].Offset, j);
                }
                int i = 0;
                foreach(KeyValuePair<float, int> kvp in valuelist)
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

        private GDI.Brush m_background = new GDI.SolidBrush(GDI.Color.White);
        private GDI.Brush m_hoverbackground = new GDI.SolidBrush(GDI.Color.White);
        static public readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(GB_GridHeader), new FrameworkPropertyMetadata(null, null));


        public GridViewColumnCollection Columns
        {
            set { SetValue(ColumnsProperty, value); }
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
        }

        private void GB_GridHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize != e.PreviousSize && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                m_bufferedBmp = new WriteableBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                if (m_bmp != null)
                    m_bmp.Dispose();

                if (g != null)
                    g.Dispose();

                m_bmp = new GDI.Bitmap(m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight,
                    m_bufferedBmp.BackBufferStride, GDI.Imaging.PixelFormat.Format32bppArgb,
                    m_bufferedBmp.BackBuffer);

                g = GDI.Graphics.FromImage(m_bmp);
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit
                paintArea(new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
                Invalidate(new Int32Rect(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));

                //m_bmp.Save("Test.png");
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            drawingContext.DrawImage(m_bufferedBmp, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
        }

        public void Invalidate()
        {
            m_bufferedBmp.Lock();
            m_bufferedBmp.AddDirtyRect(new Int32Rect(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
            m_bufferedBmp.Unlock();
        }

        public delegate void ExtentViewChanged();
        public event ExtentViewChanged OnExtentViewChanged; // 暂时只支持一个事件订阅者吧

        private GDI.Font m_font = new GDI.Font("微软雅黑", 9, GDI.FontStyle.Bold);

        public GDI.Font Font
        {
            set { m_font = value; }
            get { return m_font; }
        }


        private GDI.Bitmap m_bmp = null;
        private GDI.Graphics g = null;
        private WriteableBitmap m_bufferedBmp = null;
        private int m_mouseColumn = -1;
        private int m_needadjustcolumn;
        private Point m_ptMouseDown;
        private bool m_bdragingColumn;
        private double m_tempWidth;
        private Point m_ptOffset;

        internal  WriteableBitmap GetColumSnapshot(int iColumn)
        {
            
            GB_GridView.CellPaintEventArgs e = new GB_GridView.CellPaintEventArgs();
            e.Bounds = new GDI.Rectangle(0, 0, (int)Columns[iColumn].Width, (int)ActualHeight);
            e.ColumnIndex = iColumn;

            WriteableBitmap temp = new WriteableBitmap(e.Bounds.Width, e.Bounds.Height, 96.0, 96.0, PixelFormats.Pbgra32, null);
            GDI.Bitmap tempbmp = new GDI.Bitmap(temp.PixelWidth, temp.PixelHeight,
                    temp.BackBufferStride, GDI.Imaging.PixelFormat.Format32bppArgb,
                    temp.BackBuffer);

            using (GDI.Graphics gg = GDI.Graphics.FromImage(tempbmp))
            {

                e.Graphics = gg;
                e.RowIndex = -1;
                e.States = GB_GridView.GridViewElementStates.Selected;
                e.Value = Columns[iColumn].Header;
                DefaultPaintCell(e);
            }

            tempbmp.Dispose();

            temp.Lock();
            temp.AddDirtyRect(new Int32Rect(0, 0, temp.PixelWidth, temp.PixelHeight));
            temp.Unlock();

            return temp;
        }
        public bool IsAdjustingColumn
        {
            get { return m_bdragingColumn || Cursor == Cursors.SizeWE; }
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (m_mouseColumn != -1)
            {

                int m_prev_mouseColumn = m_mouseColumn;

                m_mouseColumn = -1;

                GB_GridView.CellPaintEventArgs ce = new GB_GridView.CellPaintEventArgs();
                int iPos = GetCellPosition(m_prev_mouseColumn);
                ce.Bounds = new GDI.Rectangle(iPos, 0, (int)(Columns[m_prev_mouseColumn].Width), m_bufferedBmp.PixelHeight);
                ce.ColumnIndex = m_prev_mouseColumn;
                ce.Graphics = g;
                ce.RowIndex = -1;
                ce.States = GB_GridView.GridViewElementStates.Visible;
                ce.Value = Columns[m_prev_mouseColumn].Header.ToString();
                DefaultPaintCell(ce);
                Invalidate(new Int32Rect(ce.Bounds.X, ce.Bounds.Y, ce.Bounds.Width, ce.Bounds.Height));
            }
        }

        public void Invalidate(Int32Rect rect)
        {
            if (rect.Y + rect.Height < 0 || rect.X + rect.Width < 0)
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

        internal int GetCellPosition(int col)
        {
            if (col >= 0 && col < Columns.Count)
            {
                double iWidth = 0;
                for (int i = 0; i < col; ++i)
                {
                    iWidth += Columns[i].Width;
                }

                return (int)iWidth - (int)PointOffset.X;
            }
            else
                return 0; // 这里就返回初始坐标吧
        }

        private EventHandler<GB_GridView.CellPaintEventArgs> m_cellPaint;
        private readonly Object m_eventLock = new Object();
        private GDI.Brush m_foreground = new GDI.SolidBrush(GDI.Color.Black);

        public event EventHandler<GB_GridView.CellPaintEventArgs> CellPaint
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

        System.Windows.Forms.SortOrder GetColumnSortGraph(int index)
        {
            if (Columns.Count <= index || index < 0)
                return System.Windows.Forms.SortOrder.None;
            else
            {
                Binding bind = Columns[index].DisplayMemberBinding as Binding;
                if (bind != null)
                {
                    System.Windows.Forms.SortOrder so;
                    if (m_sortgraph.TryGetValue(bind.Path.Path, out so))
                    {
                        return so;
                    }
                    else return System.Windows.Forms.SortOrder.None;
                }
                else return System.Windows.Forms.SortOrder.None;
            }
        }

        void DefaultPaintCell(GB_GridView.CellPaintEventArgs e)
        {
            if (m_cellPaint != null)
            {
                m_cellPaint(this, e);
                if (e.Handled)
                    return;
            }
            GDI.Brush brush = m_background;

            if (e.ColumnIndex == m_mouseColumn)
            {
                brush = m_hoverbackground;
            }

            e.Graphics.FillRectangle(brush, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height - 1);
            //e.Graphics.DrawRectangle(System.Drawing.Pens.Black, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height - 1);
            // 判断下有没有SortGraph
            System.Windows.Forms.SortOrder so = GetColumnSortGraph(e.ColumnIndex);
            if (so != System.Windows.Forms.SortOrder.None)
            {
                GDI.Drawing2D.GraphicsPath path = new GDI.Drawing2D.GraphicsPath();
                // 创建一个5*9的三角形
                GDI.Point[] ptList = null;

                if (so == System.Windows.Forms.SortOrder.Ascending) // 箭头朝上
                {
                    ptList = new GDI.Point[]{
                        new GDI.Point(5, 0), new GDI.Point(0, 5), new GDI.Point(9, 5)
                        };
                }
                else
                {
                    ptList = new GDI.Point[]{
                            new GDI.Point(4, 5), new GDI.Point(0, 0), new GDI.Point(9, 0)
                        };
                }
                GDI.Size offset = new GDI.Size(e.Bounds.Right - e.Bounds.Width / 2 - 5, e.Bounds.Top);
                path.AddLines(new GDI.Point[] { GDI.Point.Add(ptList[0], offset), GDI.Point.Add(ptList[1], offset), GDI.Point.Add(ptList[2], offset) });
                e.Graphics.FillPath(m_foreground, path);
                path.Dispose();
            }

            if (e.Value != null)
            {
                GDI.StringFormat sf = new System.Drawing.StringFormat();
                sf.Alignment = System.Drawing.StringAlignment.Center;
                sf.FormatFlags = System.Drawing.StringFormatFlags.NoWrap;
                sf.LineAlignment = System.Drawing.StringAlignment.Center;
                sf.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
                System.Drawing.Text.TextRenderingHint hint = e.Graphics.TextRenderingHint;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                e.Graphics.DrawString(e.Value.ToString(), Font, m_foreground, e.Bounds, sf);
                e.Graphics.TextRenderingHint = hint;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point ptOwn = e.GetPosition(this);
            double iOffsetX = ptOwn.X;
            int iCol = GetCellColumn(ref iOffsetX);

            //正在拖动列头
            if (m_bdragingColumn)
            {
                // 改变列的宽度吧
                int iWidthOffset = (int)(ptOwn.X - m_ptMouseDown.X);

                if (m_tempWidth == 0)
                    m_tempWidth = Columns[m_needadjustcolumn].Width;
                double iNewWidth = m_tempWidth + iWidthOffset;
                if (iNewWidth <= 0)
                {
                    iNewWidth = 3;
                }

                Columns[m_needadjustcolumn].Width = iNewWidth;
                paintArea(new GDI.Rectangle((int)(ptOwn.X - iNewWidth), 0, (int)(m_bufferedBmp.PixelWidth - (int)ptOwn.X + iNewWidth), m_bufferedBmp.PixelHeight));
                Invalidate(new Int32Rect((int)(ptOwn.X - iNewWidth), 0, (int)(m_bufferedBmp.PixelWidth - (int)ptOwn.X + iNewWidth), m_bufferedBmp.PixelHeight));
                // 这里需要暴露出来，列宽已经改变了，需要更新ExtentWidth
                if (OnExtentViewChanged != null)
                    OnExtentViewChanged();

                if (GridView != null)
                {
                    GridView.paintArea(new GDI.Rectangle(0, 0, (int)GridView.ActualWidth, (int)GridView.ActualHeight));
                    GridView.Invalidate();
                }
                return;

            }

            if (iCol != m_mouseColumn)
            {
                // 说明图形位置更新了，那就需要来替换了啊
                // 高亮显示列吧
                int m_prev_mouseColumn = m_mouseColumn;
                m_mouseColumn = iCol;

               
                // 先清除老的行
                if (m_prev_mouseColumn != -1)
                {
                    // 这里还要判断是不是上次的已经滚动走了
                    int iOldColPos = GetCellPosition(m_prev_mouseColumn);
                    GB_GridView.CellPaintEventArgs e1 = new GB_GridView.CellPaintEventArgs();
                    e1.Bounds = new GDI.Rectangle(iOldColPos, 0, (int)Columns[m_prev_mouseColumn].Width, m_bufferedBmp.PixelHeight);
                    e1.ColumnIndex = m_prev_mouseColumn;
                    e1.RowIndex = -1;
                    e1.Graphics = g;
                    e1.Value = Columns[m_prev_mouseColumn].Header.ToString();
                    DefaultPaintCell(e1);
                    Invalidate(new Int32Rect(e1.Bounds.X, e1.Bounds.Y, e1.Bounds.Width, e1.Bounds.Height));
                }

                // 如果鼠标已经换到别的列，则再画新列
                if (m_mouseColumn != m_prev_mouseColumn && m_mouseColumn != -1)
                {
                    GB_GridView.CellPaintEventArgs e1 = new GB_GridView.CellPaintEventArgs();
                    e1.Bounds = new GDI.Rectangle((int)(ptOwn.X - iOffsetX), 0, (int)Columns[m_mouseColumn].Width, m_bufferedBmp.PixelHeight);
                    e1.ColumnIndex = m_mouseColumn;
                    e1.RowIndex = -1;
                    e1.Value = Columns[m_mouseColumn].Header.ToString();
                    e1.Graphics = g;
                    DefaultPaintCell(e1);
                    Invalidate(new Int32Rect(e1.Bounds.X, e1.Bounds.Y, e1.Bounds.Width, e1.Bounds.Height));
                }

            }


            // 这里在列上面，所以得判断光标是不是在两个列之间边缘，这种情况下要允许拖动列大小
            if (iCol != -1)
            {
                if (iOffsetX < 3 ||  Columns[iCol].Width - iOffsetX < 3)
                {
                    //这里光标要变化
                    Cursor = Cursors.SizeWE;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }


        public double ExtentWidth
        {
            get
            {
                double iWidth = 0;
                for (int i = 0; i < Columns.Count; ++i)
                    iWidth += Columns[i].Width;
                return iWidth;
            }
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    bool bNeedUpdate = false;
        //    if (m_bufferedBmp != null && m_ptOffset.X + (int)ActualWidth > ExtentWidth)
        //    {
        //        bNeedUpdate = true;
        //        m_ptOffset.X = ExtentWidth - (int)ActualWidth;
        //        if (m_ptOffset.X < 0)
        //            m_ptOffset.X = 0;

        //    }
        //    if (bNeedUpdate)
        //    {
        //        paintArea(new GDI.Rectangle(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
        //        Invalidate();
        //    }
        //    return base.MeasureOverride(availableSize);
        //}

        internal void SetHorizontalOffset(double offset)
        {
            offset = (int)offset;


            // 这里产生了水平滚动，识别哪些数据可以使用就只需copy一下就可以了
            int OffsetX = (int)Math.Abs(m_ptOffset.X - offset);
            if (OffsetX < ActualWidth - 20) // 说明是可以使用部分数据的， -20是如果只有20象素宽可用的话，那也没意义了，直接刷新效率更好
            {
                if (m_ptOffset.X < offset)
                {
                    GB_GridView.Bitblt(m_bufferedBmp, new GDI.Rectangle(OffsetX, 0, (int)ActualWidth - OffsetX, (int)ActualHeight),
                        new GDI.Rectangle(0, 0, (int)ActualWidth - OffsetX, (int)ActualHeight));

                    m_ptOffset.X = offset;

                    paintArea(new GDI.Rectangle((int)ActualWidth - OffsetX, 0, OffsetX, (int)ActualHeight));
                }
                else
                {
                    GB_GridView.Bitblt(m_bufferedBmp, new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth - OffsetX, (int)m_bufferedBmp.PixelHeight),
                        new GDI.Rectangle(OffsetX, 0, (int)m_bufferedBmp.PixelWidth - OffsetX, (int)m_bufferedBmp.PixelHeight));
                    m_ptOffset.X = offset;

                    paintArea(new GDI.Rectangle(0, 0, OffsetX, (int)m_bufferedBmp.PixelHeight));
                }
            }
            else
            {
                m_ptOffset.X = offset;
                paintArea(new GDI.Rectangle(0, 0, (int)m_bufferedBmp.PixelWidth, (int)m_bufferedBmp.PixelHeight));
            }



            m_bufferedBmp.Lock();
            m_bufferedBmp.AddDirtyRect(new Int32Rect(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
            m_bufferedBmp.Unlock();
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
        /// return - 返回X所在的列索引, 参数X的值会被同时改掉
        /// </summary>
        /// <param name="x">相对当前窗口的X坐标，不包含滚动条偏移位置</param>
        /// <returns></returns>
        internal int GetCellColumn(ref double x)
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

        public Point PointOffset
        { set { m_ptOffset = value; } get { return m_ptOffset; } }


        public GB_GridView GridView
        {
            set; get;
        }
        public bool SortEnabled
        { get; internal set; }



        public void BitBlt(GDI.Rectangle rectSrc, GDI.Rectangle rectDest)
        {
            GB_GridView.Bitblt(m_bufferedBmp, rectSrc, rectDest);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (Cursor == Cursors.SizeWE)
            {
                if (e.ClickCount >= 2)
                {
                    // 双击，这里要计算所有的大小了，只计算当前显示的元素大小吧，为了性能
                    System.Diagnostics.Debug.WriteLine("双击事件" + m_mouseColumn.ToString());
                    double iOffsetX = e.GetPosition(this).X;
                    int iCol = GetCellColumn(ref iOffsetX);
                    if (iOffsetX < 3) // 本列不是要调整的列，而是前面一列
                    {
                        iCol -= 1;
                    }

                    System.Diagnostics.Debug.Assert(iCol >= 0);
                    GDI.Size cellSize = GridView.GetCellPosition(iCol, 0);
                    double iColumnWidthOffset = GridView.getperhapsWidth(iCol) - Columns[iCol].Width;

                    //直接内存拷贝
                    GridView.BitBlt(new GDI.Rectangle((int)(cellSize.Width + Columns[iCol].Width), 0, (int)(m_bufferedBmp.PixelWidth - (cellSize.Width + Columns[iCol].Width)), (int)GridView.ActualHeight),
                        new GDI.Rectangle((int)(cellSize.Width + Columns[iCol].Width + iColumnWidthOffset), 0, (int)(m_bufferedBmp.PixelWidth - (cellSize.Width + Columns[iCol].Width + iColumnWidthOffset)), (int)GridView.ActualHeight));


                    Columns[iCol].Width += iColumnWidthOffset;

                    if (iColumnWidthOffset < 0)
                    {
                        // 最后面还有部分数据是要画出来的
                        GridView.paintArea(new GDI.Rectangle((int)(m_bufferedBmp.PixelWidth + iColumnWidthOffset), 0, (int)-iColumnWidthOffset, (int)GridView.ActualHeight));
                    }
                    else // 画前面部分数据
                    {
                        // 只画这一列
                        GridView.paintArea(new GDI.Rectangle((int)(cellSize.Width), 0, (int)Columns[iCol].Width, (int)GridView.ActualHeight));
                    }

                    GridView.Invalidate();

                    // 就全部重画一下吧
                    paintArea(new GDI.Rectangle(0, 0, m_bufferedBmp.PixelWidth, m_bufferedBmp.PixelHeight));
                    m_bufferedBmp.Lock();
                    m_bufferedBmp.AddDirtyRect(new Int32Rect(cellSize.Width, 0, (int)m_bufferedBmp.PixelWidth - cellSize.Width, (int)m_bufferedBmp.PixelHeight));
                    m_bufferedBmp.Unlock();

                    m_bdragingColumn = false;
                    m_needadjustcolumn = -1;
                    ReleaseMouseCapture();
                    InvalidateMeasure();
                }
                else // 说明可以开始拖动列了
                {
                    
                    double iOffsetX = e.GetPosition(this).X;
                    int iCol = GetCellColumn(ref iOffsetX);
                    if (iCol >= 0)
                    {
                        if (iOffsetX < 3) // 本列不是要调整的列，而是前面一列
                        {
                            iCol -= 1;
                        }
                        m_needadjustcolumn = iCol;
                        m_ptMouseDown = e.GetPosition(this);
                        m_bdragingColumn = true;
                        CaptureMouse();
                    }

                }
            }
        }

        /// <summary>
        /// 重新绘制本区域内的内容
        /// </summary>
        /// <param name="rect">相对当前屏幕的坐标矩形</param>
        public void paintArea(GDI.Rectangle rect)
        {
            if (rect.Height == 0 || rect.Width == 0)
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

            GB_GridView.CellPaintEventArgs e = new GB_GridView.CellPaintEventArgs();
            g.FillRectangle(m_background, rect);
            double iColumnWidth = rect.X - iOffsetX;

            for (int j = iColumn; j < Columns.Count; ++j)
            {
                if (iColumnWidth > ActualWidth)
                    break; // 说明这一行画完了，开始画下一行吧

                e.Bounds = new GDI.Rectangle((int)iColumnWidth, 0, (int)Columns[j].Width, m_bufferedBmp.PixelHeight);
                iColumnWidth += Columns[j].Width;
                e.ColumnIndex = j;
                e.RowIndex = -1;
                e.States = GB_GridView.GridViewElementStates.Visible;
                e.Graphics = g;
                e.Value = Columns[j].Header.ToString();
                DefaultPaintCell(e);
            }


            g.Restore(state);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            if (m_bdragingColumn)
                e.Handled = true;
            m_bdragingColumn = false;
            m_needadjustcolumn = -1;
            m_tempWidth = 0;
            m_ptMouseDown = new Point(-1, -1);

        }
    }
}
