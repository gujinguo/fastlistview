using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FastGrid
{
    public class TreeGridView
    {
        List<KeyValuePair<int, DataRow>> cacheDataRowIndex = new List<KeyValuePair<int, DataRow>>();
        Dictionary<int, TreeGridRow> cacheGroupHeaderIndex = new Dictionary<int, TreeGridRow>();

        private GB_GridView _dv = null;
        internal GB_GridView GridView
        {
            set { _dv = value; }
            get { return _dv; }
        }

        public List<KeyValuePair<int, DataRow>> CacheDataRowIndex
        {
            get { return cacheDataRowIndex; }
        }

        public Dictionary<int, TreeGridRow> CacheGroupHeaderIndex
        {
            get { return cacheGroupHeaderIndex; }
        }

        public Dictionary<string, bool> DicForExpandState
        {
            get { return dicForExpandState; }
        }

        private TreeGridRow m_MulitpleDataRoot = null;

        public TreeGridRow MulitpleDataRoot
        {
            set
            {
                m_MulitpleDataRoot = value;
                if (m_MulitpleDataRoot != null)
                {
                    OnFilterGroupData(m_MulitpleDataRoot);
                    UpdateCacheIndex();
                    _dv.RowCount = m_MulitpleDataRoot.RowCount;
                    _dv.InvalidateVisual();
                }
                else
                {
                    dicForExpandState.Clear(); // 清除所有的ExpandState
                }

            }
            get { return m_MulitpleDataRoot; }
        }

        private bool m_bExpandState = true;
        public bool DefalutExpandState
        {
            set { m_bExpandState = value; }
            get { return m_bExpandState; }
        }

        private DataTable _dt = null;
        public DataTable ItemSource
        {
            set { _dt = value; }
            get { return _dt; }
        }


        private int _rowcount = 0;
        public int RowCount
        {
            get { return _rowcount; }
        }

        public void ResetRowCount()
        {
            if (m_MulitpleDataRoot != null)
                _rowcount = m_MulitpleDataRoot.RowCount;
            else _rowcount = 0;
            if (_dv != null)
                _dv.RowCount = _rowcount;
        }

        private String _sortString = "";

        public String SortString
        {
            set
            {
                // 这里要判断SortString是否是合法的,有的列可能不存在
                _sortString = value;
            }
            get
            {
                return _sortString;
            }
        }

        public class GroupKey
        {
            public List<string> Keys = new List<string>();
            public GroupKey SubKey = null;
        }

        private void OnSortGroupData(TreeGridRow tgr, String sortString)
        {
            tgr.ChildRows.DefaultView.Sort = sortString;
            foreach (KeyValuePair<int, TreeGridRow> kvp in tgr.Nodes)
            {
                OnSortGroupData(kvp.Value, sortString);
            }

        }

        public void DoSort()
        {
            OnSortGroupData(m_MulitpleDataRoot, SortString);
            UpdateCacheIndex();
        }

        private GroupKey mainkey = new GroupKey();
        public GroupKey MainKey
        {
            get { return mainkey; }
        }
        // 这里是根据DataTable自动生成分组信息
        internal void MutipLevelUpdateGroupData()
        {
            if (_dv != null && _dv.ShowInGroup && _dt != null)
            {
                if (m_MulitpleDataRoot == null)
                    m_MulitpleDataRoot = new TreeGridRow();

                this.m_MulitpleDataRoot.ChildRows = _dt.Clone(); // 克隆数据结构
                this.m_MulitpleDataRoot.Nodes.Clear();

                if (_dt.Rows.Count == 0)
                {
                    UpdateCacheIndex();
                    ResetRowCount();
                    //RowCount = 0;
                    return;
                }

                DataRow[] drsRoot = _dt.Select("1=1", mainkey.Keys[0] + " ASC");
                m_MulitpleDataRoot.ChildRows.DefaultView.Sort = SortString;
                GenerateGroupData("", m_MulitpleDataRoot, mainkey, drsRoot);
                _dt.Rows.Clear();
                OnFilterGroupData(m_MulitpleDataRoot);
                UpdateCacheIndex();
                ResetRowCount();
                //if (_dv.ColumnCount > 0)
                //RowCount = m_MulitpleDataRoot.RowCount;
            }
            else if (_dt != null)
            {
                ResetRowCount();
                //// 这里要保证行数是正确的
                //if (RowCount != _dt.DefaultView.Count)
                //    RowCount = _dt.DefaultView.Count;
                //if (RowCount != 0) // 这里是强制滚动条始终是正确的
                //{
                //    // 更新视图行高
                //    //_dv.UpdateRowHeightInfo(0, true);
                //}
            }
        }

        private bool m_filterAll = true;

        public bool FilterAll
        {
            set { m_filterAll = value; }
            get { return m_filterAll; }
        }

        public void DoFilter()
        {
            if (m_MulitpleDataRoot != null)
            {
                OnFilterGroupData(m_MulitpleDataRoot);
                UpdateCacheIndex();
            }
        }

        public enum GroupStyle
        {
            GroupShow = 1, // 只显示组名信息
            CellShow = 2 // 显示单元格信息
        }

        public GroupStyle GroupShowStyle = GroupStyle.CellShow;

        private void OnFilterGroupData(TreeGridRow tgr)
        {
            if (tgr.ChildRows != null && tgr.ChildRows.Columns.Count > 0)
            {
                // 这里顺带保留一下之前的折叠状态
                bool bExpend;
                if (dicForExpandState.TryGetValue(tgr.Key, out bExpend))
                    tgr.Expand = bExpend;

                if (tgr.Nodes.Count > 0 && (GroupShowStyle == GroupStyle.GroupShow || m_filterAll == false))
                {
                }
                else
                    tgr.ChildRows.DefaultView.RowFilter = _dv.Filter;

                foreach (KeyValuePair<int, TreeGridRow> kvp in tgr.Nodes)
                {
                    OnFilterGroupData(kvp.Value);
                }
            }
        }

        // 全局更新
        internal void UpdateCacheIndex()
        {
            cacheDataRowIndex.Clear();
            cacheGroupHeaderIndex.Clear();
            EnumDataRow(m_MulitpleDataRoot, ref cacheDataRowIndex, ref cacheGroupHeaderIndex);
            ResetRowCount();
        }

        internal void UpdateCacheIndex(TreeGridRow tgr, int iRowIndex)
        {
            bool bExpand = tgr.Expand;
            int iCount = 0;
            if (bExpand == false) // 现在是收起，那只需要把不需要的row删除掉就可以了吧
            {
                EventHandler handler = tgr.ExpandChanged;
                tgr.ExpandChanged = null;
                tgr.Expand = !bExpand; // 先设回来用来得到正确的行数
                tgr.ExpandChanged = handler;
                iCount = tgr.RowCount;
                tgr.Expand = bExpand;

                cacheDataRowIndex.RemoveRange(iRowIndex + 1, iCount);
                // 再更新group数据吧
                Dictionary<int, TreeGridRow> dicGroup = new Dictionary<int, TreeGridRow>();
                foreach (KeyValuePair<int, TreeGridRow> kvp in cacheGroupHeaderIndex)
                {
                    if (kvp.Key <= iRowIndex)
                        dicGroup.Add(kvp.Key, kvp.Value);
                    else if (kvp.Key <= iRowIndex + iCount)
                    {
                        // Do nothing.
                    }
                    else
                    {
                        dicGroup.Add(kvp.Key - iCount, kvp.Value);
                    }
                }
                cacheGroupHeaderIndex = dicGroup;
            }
            else // 这里是展开，所以要添加Row
            {
                iCount = tgr.RowCount;
                List<KeyValuePair<int, DataRow>> tempRowIndex = new List<KeyValuePair<int, DataRow>>();
                Dictionary<int, TreeGridRow> tempdicGroup = new Dictionary<int, TreeGridRow>();
                EnumDataRow(tgr, ref tempRowIndex, ref tempdicGroup);
                cacheDataRowIndex.InsertRange(iRowIndex + 1, tempRowIndex);
                Dictionary<int, TreeGridRow> dicGroup = new Dictionary<int, TreeGridRow>();
                foreach (KeyValuePair<int, TreeGridRow> kvp in cacheGroupHeaderIndex)
                {
                    if (kvp.Key <= iRowIndex)
                    {
                        dicGroup.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        dicGroup.Add(kvp.Key + iCount, kvp.Value);
                    }
                }

                foreach (KeyValuePair<int, TreeGridRow> kvp in tempdicGroup)
                {
                    dicGroup.Add(kvp.Key + iRowIndex + 1, kvp.Value);
                }

                cacheGroupHeaderIndex = dicGroup;
            }
        }

        // 20130425 gjg 数组分组功能辅助结构，用来保存各行数据的索引，提高控件显示效率
        // 当下列条件发生时，要更新此内容
        // 1. 节点被展开或收缩
        // 2. 过滤条件改变
        // TODO:内容应该包括

        public TreeGridRow GetGridHeader(int rowIndex)
        {
            if (cacheGroupHeaderIndex.ContainsKey(rowIndex))
                return cacheGroupHeaderIndex[rowIndex];
            else return null;
        }

        public Dictionary<string, bool> GetExpandState()
        {
            return dicForExpandState;
        }

        public void SetExpandState(TreeGridRow temptgr, bool bExpandState, bool bUpdateIndex)
        {
            if (temptgr.Expand != bExpandState)
            {
                temptgr.Expand = bExpandState;
                dicForExpandState[temptgr.Key] = temptgr.Expand;

                if (bUpdateIndex)
                {
                    UpdateCacheIndex();
                    ResetRowCount();
                    _dv.RowCount = RowCount;
                    _dv.Redraw();
                }
            }
        }

        /// <summary>
        /// 这里的数据只可以是一个分组里的,只支持叶子节点
        /// </summary>
        /// <param name="currentgroupIndex">当前组在视图上的索引</param>
        /// <param name="dtData">要添加的新数据</param>
        internal int AppendOneGroupData(int currentgroupIndex, DataTable dtData)
        {
            if (cacheGroupHeaderIndex.ContainsKey(currentgroupIndex))
            {
                TreeGridRow tgr = cacheGroupHeaderIndex[currentgroupIndex];
                int iOrginalRowCount = tgr.RowCount;
                tgr.ChildRows.Merge(dtData);
                int iNowRowCount = tgr.RowCount;
                //这里开始处理索引
                if (tgr.Expand && iOrginalRowCount != iNowRowCount) //如果是展开的，就要开始更新索引了
                {
                    // 这个组里的分组全部要更新掉
                    List<KeyValuePair<int, DataRow>> tempRowIndex = new List<KeyValuePair<int, DataRow>>();
                    Dictionary<int, TreeGridRow> tempdicGroup = new Dictionary<int, TreeGridRow>();
                    EnumDataRow(tgr, ref tempRowIndex, ref tempdicGroup);
                    for (int i = 1; i <= iOrginalRowCount; ++i)
                    {
                        cacheDataRowIndex[i + currentgroupIndex] = tempRowIndex[i - 1];
                    }

                    tempRowIndex.RemoveRange(0, iOrginalRowCount);
                    cacheDataRowIndex.InsertRange(currentgroupIndex + iOrginalRowCount + 1, tempRowIndex);
                    Dictionary<int, TreeGridRow> dicGroup = new Dictionary<int, TreeGridRow>();
                    foreach (KeyValuePair<int, TreeGridRow> kvp in cacheGroupHeaderIndex)
                    {
                        if (kvp.Key <= currentgroupIndex)
                        {
                            dicGroup.Add(kvp.Key, kvp.Value);
                        }
                        else
                        {
                            dicGroup.Add(kvp.Key + iNowRowCount - iOrginalRowCount, kvp.Value);
                        }
                    }

                    foreach (KeyValuePair<int, TreeGridRow> kvp in tempdicGroup)
                    {
                        dicGroup.Add(kvp.Key + currentgroupIndex + 1, kvp.Value);
                    }

                    cacheGroupHeaderIndex = dicGroup;
                }
            }
            return 0;
        }

        private int MaxLevel = 0;
        private Dictionary<string, bool> dicForExpandState = new Dictionary<string, bool>();
        public void GenerateGroupData(String key, TreeGridRow root, GroupKey gk1, DataRow[] drs)
        {
            List<string> keyList = new List<string>();
            for (int i = 0; i < drs.Length; i++)
            {
                string subkey = key;
                foreach (string s in gk1.Keys)
                {
                    if (subkey == "")
                        subkey += s + "='" + drs[i][s].ToString() + "'";
                    else subkey += " and " + s + "='" + drs[i][s].ToString() + "'";
                }

                if (keyList.Contains(subkey))
                {
                    if (gk1.SubKey == null || (gk1.SubKey != null && gk1.SubKey.Keys.Count == 0)) // 没有子键了，就直接插入吧，叶子节点了
                    {
                        foreach (KeyValuePair<int, TreeGridRow> kvp in root.Nodes)
                        {
                            if (kvp.Value.Key == subkey)
                            {
                                // 这里先注释掉，只要是分组了，一行也分组
                                //if (kvp.Value.ChildRows.Rows.Count == 0) // 需要导入第一条数据
                                //    kvp.Value.ChildRows.ImportRow(m_GroupDataRoot.ChildRows.Rows[kvp.Key]);
                                kvp.Value.ChildRows.ImportRow(drs[i]);
                            }
                        }
                    }
                }
                else
                {
                    keyList.Add(subkey);
                    TreeGridRow subTreeGrid = new TreeGridRow();
                    subTreeGrid.ParentGridRow = root;
                    subTreeGrid.ChildRows = _dt.Clone();
                    subTreeGrid.ChildRows.ExtendedProperties["Parent"] = root;
                    subTreeGrid.ChildRows.DefaultView.Sort = SortString;
                    subTreeGrid.Key = keyList[keyList.Count - 1];
                    subTreeGrid.Level = root.Level + 1;
                    bool bTempExpand = false;
                    if (dicForExpandState.TryGetValue(subTreeGrid.Key, out bTempExpand))
                        subTreeGrid.Expand = bTempExpand;
                    else
                    {
                        subTreeGrid.Expand = m_bExpandState;
                        dicForExpandState.Add(subTreeGrid.Key, subTreeGrid.Expand);
                    }
                    MaxLevel = subTreeGrid.Level;
                    // 打开下面一行，只要是分组了，一行也分组
                    subTreeGrid.ChildRows.ImportRow(drs[i]);
                    if (keyList.Count != 1 || root == m_MulitpleDataRoot)
                    {
                        root.ChildRows.ImportRow(drs[i]);
                        if (_dv.ShowCheckBox && drs[i].Table.Columns.Contains(_dv.CheckBoxColName))
                            root.ChildRows.Rows[root.ChildRows.Rows.Count - 1][_dv.CheckBoxColName] = "0";
                    }
                    subTreeGrid.ChildRows.ExtendedProperties["ParentRow"] = root.ChildRows.Rows.Count - 1;
                    root.Nodes.Add(root.ChildRows.Rows.Count - 1, subTreeGrid);
                    if (gk1.SubKey != null && gk1.SubKey.Keys.Count > 0)
                        GenerateGroupData(subkey, subTreeGrid, gk1.SubKey, _dt.Select(subkey));
                }
            }
        }

        private void EnumDataRow(TreeGridRow tgr, ref List<KeyValuePair<int, DataRow>> ldr, ref Dictionary<int, TreeGridRow> dictgr)
        {
            for (int i = 0; i < tgr.ChildRows.DefaultView.Count; ++i)
            {
                int iIndex = tgr.ChildRows.Rows.IndexOf(tgr.ChildRows.DefaultView[i].Row);

                if (tgr.Nodes.ContainsKey(iIndex))
                {
                    if (tgr.Nodes[iIndex].RowCount > 0)
                    {
                        ldr.Add(new KeyValuePair<int, DataRow>(tgr.Level, tgr.ChildRows.DefaultView[i].Row));
                        dictgr.Add(ldr.Count - 1, tgr.Nodes[iIndex]);
                        if (tgr.Nodes[iIndex].Expand)
                            EnumDataRow(tgr.Nodes[iIndex], ref ldr, ref dictgr);
                    }
                    else if (tgr.Nodes[iIndex].Expand == false && tgr.Nodes[iIndex].ChildRows.DefaultView.Count > 0)
                    {
                        ldr.Add(new KeyValuePair<int, DataRow>(tgr.Level, tgr.ChildRows.DefaultView[i].Row));
                        dictgr.Add(ldr.Count - 1, tgr.Nodes[iIndex]);
                    }
                }
                else
                {
                    ldr.Add(new KeyValuePair<int, DataRow>(tgr.Level, tgr.ChildRows.DefaultView[i].Row));
                }
            }
        }

        // 这里丢掉所有的组头，只关注叶子节点
        void EnumLeafDataRow(TreeGridRow tgr, DataTable dtRoot)
        {
            for (int i = 0; i < tgr.ChildRows.Rows.Count; ++i)
            {
                if (tgr.Nodes.ContainsKey(i))
                    EnumLeafDataRow(tgr.Nodes[i], dtRoot);
                else dtRoot.ImportRow(tgr.ChildRows.Rows[i]);
            }
        }

        // 这里是全新的组织数据
        internal void UpdateShowGroup()
        {
            dicForExpandState.Clear(); // 清除状态
            if (_dv.ShowInGroup && (_dt != null || m_MulitpleDataRoot != null))
            {
                if (m_MulitpleDataRoot == null)
                    m_MulitpleDataRoot = new TreeGridRow();

                if (_dt == null || _dt.Columns.Count == 0)
                    _dt = (DataTable)m_MulitpleDataRoot.ChildRows.Clone();
                //_dt.Rows.Clear();
                EnumLeafDataRow(m_MulitpleDataRoot, _dt);

                this.m_MulitpleDataRoot.ChildRows = _dt.Clone(); // 克隆数据结构
                this.m_MulitpleDataRoot.Nodes.Clear();

 
                m_MulitpleDataRoot.ChildRows.DefaultView.Sort = SortString;
                DataRow[] drsRoot = _dt.Select("1=1", mainkey.Keys[0] + " ASC");
                GenerateGroupData("", m_MulitpleDataRoot, mainkey, drsRoot);
                _dt.Rows.Clear();
                OnFilterGroupData(m_MulitpleDataRoot);
                UpdateCacheIndex();

                if (_dv.Columns.Count > 0)
                    _dv.RowCount = m_MulitpleDataRoot.RowCount;
            }
        }
    }
}
