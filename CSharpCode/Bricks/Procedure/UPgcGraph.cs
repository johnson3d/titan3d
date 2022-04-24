using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{    
    public partial class UPgcNodeBase : UNodeBase
    {
        public int RootDistance;
        protected int mPreviewResultIndex = -1;
        protected RHI.CShaderResourceView PreviewSRV;
        ~UPgcNodeBase()
        {
            if (PreviewSRV != null)
            {
                PreviewSRV?.FreeTextureHandle();
                PreviewSRV = null;
            }
        }
        [Rtti.Meta]
        public virtual int PreviewResultIndex { 
            get => mPreviewResultIndex;
            set
            {
                if (mPreviewResultIndex == value)
                    return;
                mPreviewResultIndex = value;
                if (value >= 0)
                {
                    PrevSize = new Vector2(100, 100);
                }
                else
                {
                    PrevSize = Vector2.Zero;
                }
                OnPositionChanged();
            }
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (mPreviewResultIndex < 0)
                return;

            if (PreviewSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage(PreviewSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        public virtual Int32_2 GetOutPinSize(PinOut pin)
        {
            return new Int32_2(1,1);
        }
        public virtual UBufferConponent GetResultBuffer(int index)
        {
            if (index < 0 || index >= Outputs.Count)
                return null;
            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(Outputs[index]);
        }
        public virtual bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        public bool DoProcedure(UPgcGraph graph)
        {
            var ret = OnProcedure(graph);
            if (graph.GraphEditor != null)
            {
                if (mPreviewResultIndex >= 0)
                {
                    var previewBuffer = GetResultBuffer(mPreviewResultIndex);
                    if (previewBuffer != null)
                    {
                        if (PreviewSRV != null)
                        {
                            PreviewSRV?.FreeTextureHandle();
                            PreviewSRV = null;
                        }
                        float minV, maxV;
                        previewBuffer.GetRange(out minV, out maxV);
                        PreviewSRV = previewBuffer.CreateAsHeightMapTexture2D(minV, maxV, EPixelFormat.PXF_R16_FLOAT, true);
                    }
                }
            }
            return ret;
        }
        public virtual bool OnProcedure(UPgcGraph graph)
        {
            return true;
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            var graph = this.ParentGraph as UPgcGraph;

            if (graph != null && graph.GraphEditor != null)
            {
                graph.GraphEditor.NodePropGrid.Target = this;
            }
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (iPin.Link.CanLinks.Contains("Value"))
            {
                ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        [Rtti.Meta()]
        public UBufferConponent FindBuffer(string name)
        {
            var graph = this.ParentGraph as UPgcGraph;
            var pin = this.FindPinIn(name) as NodePin;
            if (pin == null)
            {
                pin = this.FindPinOut(name);
                if (pin == null)
                {
                    return null;
                }
            }
            return graph.BufferCache.FindBuffer(pin);
        }
    }
    public class UPgcGraph : UNodeGraph
    {
        [Rtti.Meta]
        public int DefaultWidth { get; set; } = 1;
        [Rtti.Meta]
        public int DefaultHeight { get; set; } = 1;

        public UPgcEditor GraphEditor;
        public UPgcBufferCache BufferCache { get; } = new UPgcBufferCache();
        public Node.UEndingNode Root { get; set; }
        public UPgcGraph()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();

            //Root = new Buffer2D.UEndingNode();
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            CanvasMenus.AddMenuItem(
            $"UserNode", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UUserNode();
                node.Name = "UUserNode";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"Bezier", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UBezier();
                node.Name = "Bezeir";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"ImageLoader", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UImageLoader();
                node.Name = "ImageLoader";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"PreviewImage", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UPreviewImage();
                node.Name = "PreviewImage";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"HeightMappingNode", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UHeightMappingNode();
                node.Name = "HeightMappingNode";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            }); 

            CanvasMenus.AddMenuItem(
            $"CopyRect", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UCopyRect();
                node.Name = "CopyRect";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"Stretch", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UStretch();
                node.Name = "Stretch";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"StretchBlt", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UStretchBlt();
                node.Name = "StretchBlt";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"MulValue", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UMulValue();
                node.Name = "MulValue";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"SmoothGaussion", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.USmoothGaussion();
                node.Name = "SmoothGaussion";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"NoisePerlin", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UNoisePerlin();
                node.Name = "NoisePerlin";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"PixelAdd", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UPixelAdd();
                node.Name = "PixelAdd";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"PixelSub", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UPixelSub();
                node.Name = "PixelSub";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"PixelMul", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UPixelMul();
                node.Name = "PixelMul";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });

            CanvasMenus.AddMenuItem(
            $"PixelDiv", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UPixelDiv();
                node.Name = "PixelDiv";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });
            CanvasMenus.AddMenuItem(
            $"CalcNormal", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UCalcNormal();
                node.Name = "CalcNormal";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            });
            CanvasMenus.AddMenuItem(
            $"Normalize3D", null,
            (UMenuItem item, object sender) =>
            {
                var node = new Node.UNormalize3D();
                node.Name = "Normalize3D";
                node.Position = PopMenuPosition;
                this.AddNode(node);
            }); 

            //foreach (var i in GraphNodeTypes)
            //{
            //    CanvasMenus.AddMenuItem(
            //    $"{i.FullName}", null,
            //    (UMenuItem item, object sender) =>
            //    {
            //        var node = new UPolicyNode();
            //        var rgNode = Rtti.UTypeDescManager.CreateInstance(i) as Graphics.Pipeline.Common.URenderGraphNode;
            //        //rgNode.RenderGraph = this;
            //        rgNode.InitNodePins();
            //        node.InitNode(rgNode);
            //        node.Name = rgNode.Name;
            //        node.Position = PopMenuPosition;
            //        this.AddNode(node);
            //    });
            //}
        }

        public List<UPgcNodeBase> Compile()
        {
            List<UPgcNodeBase> allNodes = new List<UPgcNodeBase>();
            //foreach (UPgcNodeBase i in Nodes)
            //{
            //    //i.InitProcedure(this);
            //    allNodes.Add(i);
            //}
            Root.InvTourNodeTree((pin, linker) =>
            {
                if (linker == null)
                    return true;
                var inNode = linker.InNode as UPgcNodeBase;
                var outNode = linker.OutNode as UPgcNodeBase;

                inNode.RootDistance = -1;
                if (!allNodes.Contains(inNode))
                {
                    allNodes.Add(inNode);
                }
                outNode.RootDistance = -1;
                if (!allNodes.Contains(outNode))
                {
                    allNodes.Add(outNode);
                }
                return true;
            });

            Root.RootDistance = 0;
            Root.InvTourNodeTree((pin, linker) =>
            {
                if (linker == null)
                    return true;
                var inNode = linker.InNode as UPgcNodeBase;
                var outNode = linker.OutNode as UPgcNodeBase;
                if (inNode.RootDistance + 1 > outNode.RootDistance)
                {
                    outNode.RootDistance = inNode.RootDistance + 1;
                }
                return true;
            });

            allNodes.Sort((lh, rh) =>
            {
                return rh.RootDistance.CompareTo(lh.RootDistance);
            });
            return allNodes;
        }
    }
}
