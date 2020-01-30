using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCClassEditor
{
    public class TreeViewNode
    {
        public List<TreeViewNode> Nodes { get; set; }
        public TreeViewNode()
        {
            this.Nodes = new List<TreeViewNode>();
            this.ParentId = 0;//主节点的父id默认为0
        }
        public TreeViewNode(int _id, int _pid, string _show)
        {
            id = _id;
            ParentId = _pid;
            ToShow = _show;
        }
        public int id { get; set; }//id
        public int ParentId { get; set; }//父类id
        public string ToShow { get; set; }//要展示的字符串，类名或者是属性类型名+属性名
    }
}
