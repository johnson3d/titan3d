using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class LogicGraph<T> where T : class
    {
        public Dictionary<Guid, Node<T>> AllNodes
        {
            get;
        } = new Dictionary<Guid, Node<T>>();
        public Dictionary<Guid, Linker<T>> AllLinkers
        {
            get;
        } = new Dictionary<Guid, Linker<T>>();
        
        public Node<T> NewNode()
        {
            var node = new Node<T>();
            node.Id = Guid.NewGuid();
            AllNodes[node.Id] = node;
            return node;
        }
        public void LinkNode(Node<T> from, Node<T> to)
        {
            using (var i = AllLinkers.GetEnumerator())
            {
                while(i.MoveNext())
                {
                    var lnk = i.Current.Value;
                    if (lnk.FromNode == from && lnk.ToNode == to)
                        return;
                }
            }
            var linker = new Linker<T>();
            linker.Id = Guid.NewGuid();
            AllLinkers.Add(linker.Id, linker);
        }
        public bool DelLinker(Node<T> from, Node<T> to)
        {
            using (var i = AllLinkers.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    var lnk = i.Current.Value;
                    if (lnk.FromNode == from && lnk.ToNode == to)
                    {
                        AllLinkers.Remove(i.Current.Key);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool DelNode(Guid id)
        {
            Node<T> node;
            if (AllNodes.TryGetValue(id, out node)==false)
            {
                return false;
            }
            AllNodes.Remove(id);
            var rmv = new List<Guid>();
            using (var i = AllLinkers.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    var lnk = i.Current.Value;
                    if (lnk.FromNode == node || lnk.ToNode == node)
                    {
                        rmv.Add(lnk.Id);
                    }
                }
            }
            for (int i = 0; i < rmv.Count; i++)
            {
                AllLinkers.Remove(rmv[i]);
            }
            return true;
        }
    }
}
