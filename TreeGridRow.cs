using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FastGrid
{
    public class TreeGridRow
    {
        ~TreeGridRow()
        {
            Nodes.Clear();
            if (ChildRows != null)
            {
                ChildRows.Clear();
                ChildRows.Dispose();
            }
        }


        public DataTable ChildRows = null;

        public bool Expand
        {
            set
            {
                _expand = value;
                if (ExpandChanged != null)
                    ExpandChanged(this, new EventArgs());
            }
            get
            {
                return _expand;
            }
        }

        public TreeGridRow ParentGridRow = null;
        public EventHandler ExpandChanged;
        private bool _expand = true;
        public int Level = 0; // 0是根目录, 目前只支持两层结构
        public string Key = "";

        // 如果这里列表不是空的，则说明有的子项下面还有子项的，int值对应的是ChildNode
        // 中的原始位置索引，TreeGridRow是对应的
        public Dictionary<int, TreeGridRow> Nodes = new Dictionary<int, TreeGridRow>();

        // 叶子节点个数，现在只针对叶子节点过滤
        private int LeafDataCount
        {
            get
            {
                int nCount = 0;
                if (Nodes.Count == 0)
                {
                    if (ChildRows != null)
                        nCount = ChildRows.DefaultView.Count;
                }
                else
                {
                    foreach (KeyValuePair<int, TreeGridRow> kvp in Nodes)
                    {
                        nCount += kvp.Value.LeafDataCount;
                    }
                }

                return nCount;
            }
        }

        public int RowCount
        {
            get
            {
                if (Expand)
                {
                    int nCount = 0;
                    if (ChildRows != null)
                        nCount = ChildRows.DefaultView.Count;

                    foreach (KeyValuePair<int, TreeGridRow> kvp in Nodes)
                    {
                        // 如果叶子节点里没数据，则正表头就不应该存在，所以要去掉
                        if (kvp.Value.RowCount == 0 && kvp.Value.LeafDataCount == 0)
                            nCount--;
                        else
                            nCount += kvp.Value.RowCount;
                    }
                    if (nCount < 0)
                        nCount = 0;
                    return nCount;
                }
                else return 0;
            }
        }

        public TreeGridRow Copy()
        {
            TreeGridRow root = new TreeGridRow();
            CopyContent(this, root);
            return root;
        }

        private void CopyContent(TreeGridRow source, TreeGridRow target)
        {
            target.Level = source.Level;
            target.Key = source.Key;
            target._expand = source._expand;
            if (source.ChildRows != null)
                target.ChildRows = source.ChildRows.Copy();

            foreach(KeyValuePair<int, TreeGridRow> kvp in source.Nodes)
            {
                TreeGridRow sub = new TreeGridRow();
                target.Nodes.Add(kvp.Key, sub);
                sub.ParentGridRow = target;
                CopyContent(kvp.Value, sub);
            }
        }
    }
}
