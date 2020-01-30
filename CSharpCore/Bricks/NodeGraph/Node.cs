using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class Node<T> where T : class
    {
        public Guid Id;
        public T DataObject;
        public List<Linker<T>> Linkers
        {
            get;
        } = new List<Linker<T>>();
        public bool FindPath(ref Guid target, LinkedList<Node<T>> path=null)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].ToNode.Id == target)
                {
                    path.AddLast(Linkers[i].ToNode);
                    return true;
                }
                if (path.Contains(Linkers[i].ToNode))
                    continue;

                path.AddLast(Linkers[i].ToNode);
                var ret = Linkers[i].ToNode.FindPath(ref target, path);
                if (ret != true)
                    return true;
                path.RemoveLast();
            }
            return false;
        }
        public Node<T> FindFirstChild(ref Guid id, int depth = 0, int depthLimit = 0)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if(Linkers[i].ToNode.Id == id)
                {
                    return Linkers[i].ToNode;
                }
                if (depth < depthLimit)
                {
                    var ret = Linkers[i].ToNode.FindFirstChild(ref id, depth, depthLimit);
                    if (ret != null)
                        return ret;
                }
            }
            return null;
        }
        internal void RemoveLinker(Guid id)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].ToNode.Id == id)
                {
                    Linkers.RemoveAt(i);
                    break;
                }
            }
        }
        internal void AddLinker(Linker<T> linker)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i] == linker)
                    return;
            }
            Linkers.Add(linker);
        }
    }
}
