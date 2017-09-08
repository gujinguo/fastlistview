using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FastGrid
{
    public class YTFastListView : GBListView
    {
        /// <summary>
        /// Theme Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty ViewNameProperty =
            DependencyProperty.RegisterAttached("ViewName", typeof(string), typeof(GBListView),
                new FrameworkPropertyMetadata("",
                    new PropertyChangedCallback(OnViewNameChanged)));


        private static void OnViewNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                YTFastListView ytlistview = d as YTFastListView;
                ytlistview.CreateColumns();
            }
        }

        private void CreateColumns()
        {
            // 开始自动创建列
            DataTable dtConfig = new DataTable(); // Configure.ViewConfig.GetConfig(ViewName);
            if (dtConfig == null)
                return;


            for (int i = 0; i < dtConfig.Rows.Count; ++i)
            {
                // 得到列信息，开始创建列，并创建绑定
                GridViewColumn gvc = new GridViewColumn();
                gvc.Header = dtConfig.Rows[i]["Caption"].ToString();
                double dWidth = 0;
                double.TryParse(dtConfig.Rows[i]["Width"].ToString(), out dWidth);
                if (dWidth > 0)
                    gvc.Width = dWidth;
                else gvc.Width = 60;

                string scolname = dtConfig.Rows[i]["Name"].ToString();


                {
                    // 这里要看看有没有字典绑定需要
                    Binding bind = new Binding(scolname);

                    gvc.DisplayMemberBinding = bind;

                    if (dtConfig.Columns.Contains("Format") && dtConfig.Rows[i]["Format"] != DBNull.Value && !string.IsNullOrWhiteSpace(dtConfig.Rows[i]["Format"].ToString()))
                        bind.StringFormat = dtConfig.Rows[i]["Format"].ToString();
                    if (dtConfig.Columns.Contains("DicCode") && dtConfig.Rows[i]["DicCode"] != DBNull.Value && !string.IsNullOrWhiteSpace(dtConfig.Rows[i]["DicCode"].ToString()))
                    {
                        //Dictionary<string, string> dtDictionory = Configure.Configure.FieldDictionary.TryGetDictionary(dtConfig.Rows[i]["DicCode"].ToString());
                        // bind.ConverterParameter = dtDictionory;
                        //bind.Converter = fieldConverter;
                    }


                    //datatype为2--日期格式化 ；3--时间格式化
                    else if (dtConfig.Columns.Contains("DataType") && dtConfig.Rows[i]["DataType"] != DBNull.Value)
                    {
                        if (dtConfig.Rows[i]["DataType"].ToString() == "2")
                        {
                            //bind.Converter = field_date;
                        }
                        else if (dtConfig.Rows[i]["DataType"].ToString() == "3")
                        {
                            //bind.Converter = field_time;
                        }
                    }


                }

                Columns.Add(gvc);
            }
        }
    }
}
