using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class Linker<T> where T : class
    {
        public Guid Id;
        public object DataObject;
        public Node<T> FromNode;
        public Node<T> ToNode;
    }
}
