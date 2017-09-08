# fastlistview
WPF下的Listview，采用内存绘制，抛弃模板方式，但也支持简单的模板颜色设置

由于在项目中对WPF下的Listview的速度不满足，简单重写基本的绘制接口

支持树形结构，快速展开与折叠
支持编辑某个字段，自定义编辑控件
后台数据源采用DataTable方式
支持列调整顺序

:xaml
<Window x:Class="ViewTest.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:ViewTest"
        xmlns:fast="clr-namespace:FastGrid;assembly=FastGrid"
        mc:Ignorable="d"
        Title="MainWindow" Height="350" Width="525">
        
<fast:GBListView SortEnabled="True" Margin="10"  Grid.Column="1"  x:Name="lvList" BorderBrush="Gray" BorderThickness="1">
  <fast:GBListView.Columns>
      <GridViewColumn Header="价格0"  DisplayMemberBinding="{Binding Price0}" />
      <GridViewColumn Header="价格1"  DisplayMemberBinding="{Binding Price1}" />
      <GridViewColumn Header="价格2"  DisplayMemberBinding="{Binding Price2}" />
      <GridViewColumn Header="价格3"  DisplayMemberBinding="{Binding Price3}" />
      <GridViewColumn Header="价格4"  DisplayMemberBinding="{Binding Price4}" />
      <GridViewColumn Header="价格5"  DisplayMemberBinding="{Binding Price5}" />
      <GridViewColumn Header="价格6"  DisplayMemberBinding="{Binding Price6}" />
      <GridViewColumn Header="价格7"  DisplayMemberBinding="{Binding Price7}" />
      <GridViewColumn Header="价格8"  DisplayMemberBinding="{Binding Price8}" />
      <GridViewColumn Header="价格9"  DisplayMemberBinding="{Binding Price9}" />
  </fast:GBListView.Columns>
</fast:GBListView>
<fast:GBListView SortEnabled="True" Margin="10"  Grid.Row="1" x:Name="lvList2" BorderBrush="Gray" BorderThickness="1">
  <fast:GBListView.Columns>
      <GridViewColumn Header="中国0"  DisplayMemberBinding="{Binding Price0}" />
      <GridViewColumn Header="中国1"  DisplayMemberBinding="{Binding Price1}" />
      <GridViewColumn Header="中国2"  DisplayMemberBinding="{Binding Price2}" />
      <GridViewColumn Header="中国3"  DisplayMemberBinding="{Binding Price3}" />
      <GridViewColumn Header="中国4"  DisplayMemberBinding="{Binding Price4}" />

  </fast:GBListView.Columns>
</fast:GBListView>

:cs
DataTable dtSource = null;
  private void MainWindow_Loaded(object sender, RoutedEventArgs e)
  {
      dtSource = new DataTable();
      for(int i = 0; i < 10; ++i)
      {
          dtSource.Columns.Add("Price" + i.ToString());
      }

      for(int i = 0; i < 20; ++i)
      {
          DataRow drNew = dtSource.NewRow();
          for(int j = 0; j < 10; ++j)
          {
              drNew[j] = "廊坊发展" + i.ToString() + "," + j.ToString();
          }

          dtSource.Rows.Add(drNew);
      }

      FastGrid.TreeGridRow RootRow = new FastGrid.TreeGridRow();
      RootRow.ChildRows = dtSource.Clone();
      RootRow.ChildRows.ImportRow(dtSource.Rows[0]);
      FastGrid.TreeGridRow Child1 = new FastGrid.TreeGridRow();
      Child1.Level = 1;
      Child1.ChildRows = dtSource;
      RootRow.Nodes.Add(0, Child1);
      DataTable dt1 = dtSource.Copy();
      FastGrid.TreeGridRow Child2 = new FastGrid.TreeGridRow();
      Child2.Level = 1;
      Child2.ChildRows = dt1;
      RootRow.ChildRows.ImportRow(dtSource.Rows[1]);
      RootRow.Nodes.Add(1, Child2);

      lvList.ShowInGroup = true;
      lvList.AddEditColumn("Price1");
      lvList.AddEditColumn("Price3");
      lvList.ItemsSource = RootRow;
      lvList.CellEdit += LvList_CellEdit;
  }


  private void LvList_CellEdit(object sender, FastGrid.GB_GridView.CellEditEventArgs e)
  {
      if (e.Action == FastGrid.GB_GridView.EditAction.EditCreate)
      {
         TextBox m_txtBox = new TextBox();
          m_txtBox.Text = e.Value.ToString();
          e.UI = m_txtBox;
          m_txtBox.Focus();
          m_txtBox.SelectAll();
      }
      else if (e.Action == FastGrid.GB_GridView.EditAction.ValueRetrieve)
      {
          if (e.UI != null)
          {
              TextBox tb = e.UI as TextBox;
              if (tb != null)
                  e.Value = tb.Text;
          }
      }
  }

