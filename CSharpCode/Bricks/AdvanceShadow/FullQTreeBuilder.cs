using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AdvanceShadow
{
    public partial class TtQNode
    {
        public int IndexInLayer;
    }
    public class TtFullQTreeLayer
    {
        public TtQNode[] Nodes;
        public int LayerIndex;
        public DVector2 GridSize;
        public void InitLayer(int layer)
        {
            LayerIndex = layer;
            Nodes = new TtQNode[Side * Side];
            for (int i = 0; i < Nodes.Length; i++)
            {
                Nodes[i] = new TtQNode(layer);
                Nodes[i].IndexInLayer = i;
            }
        }
        public int Side
        {
            get => (int)Math.Pow(2, LayerIndex);
        }
        public int LayerStartIndex
        {
            get => (Nodes.Length - 1) / 3;
        }
        public TtQNode GetNode(int x, int y)
        {
            if (x >= Side || y >= Side)
                return null;
            return Nodes[y * Side + x];
        }
    }
    public class TtFullQTreeBuilder
    {
        public TtQNode[] Nodes = null;
        public FAdvShadowNodeData[] AdvShadowNodeDatas = null;
        public TtFullQTreeLayer[] Layers = null;
        public TtQNode Root
        {
            get => Layers[0].GetNode(0,0);
        }
        public void InitBuilder(int layer, in DBoundingBox2D aabb)
        {
            Layers = new TtFullQTreeLayer[layer];
            int total = 0;
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i] = new TtFullQTreeLayer();
                Layers[i].InitLayer(i);
                Layers[i].GridSize = aabb.GetSize() / Layers[i].Side;
                total += Layers[i].Nodes.Length;
            }
            
            for (int i = 0; i < Layers.Length - 1; i++)
            {
                Build(i);
            }
            Nodes = new TtQNode[total];
            AdvShadowNodeDatas = new FAdvShadowNodeData[total];

            for (int i = 0; i < Layers.Length; i++)
            {
                BuildNodeArray(i);
            }
        }
        public void Build(int layer)
        {
            var curLayer = Layers[layer];
            var childLayer = Layers[layer + 1];
            for (int y = 0; y < curLayer.Side; y++)
            {
                for (int x = 0; x < curLayer.Side; x++)
                {
                    var node = curLayer.GetNode(x, y);
                    node.Child00 = childLayer.GetNode(x * 2, y * 2);
                    node.Child01 = childLayer.GetNode(x * 2 + 1, y * 2);
                    node.Child10 = childLayer.GetNode(x * 2, y * 2 + 1);
                    node.Child11 = childLayer.GetNode(x * 2 + 1, y * 2 + 1);
                }
            }
        }
        public void BuildNodeArray(int layer)
        {
            var curLayer = Layers[layer];
            foreach(var i in curLayer.Nodes)
            {
                i.NodeIndex = Layers[layer].LayerStartIndex + i.IndexInLayer;
                Nodes[i.NodeIndex] = i;
                i.SetToGpuData(ref AdvShadowNodeDatas[i.NodeIndex]);
            }
        }
    }
}
