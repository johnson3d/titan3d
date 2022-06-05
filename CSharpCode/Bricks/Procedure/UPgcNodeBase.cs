using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    public partial class UPgcNodeBase : UNodeBase
    {
        [Rtti.Meta]
        public UBufferCreator DefaultInputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        [Rtti.Meta]
        public UBufferCreator DefaultBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
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
        public void AddInput(PinIn pin, string name, UBufferCreator desc, string linkType = "Value")
        {
            pin.Name = name;
            pin.LinkDesc = UPgcEditorStyles.Instance.NewInOutPinDesc(linkType);

            pin.Tag = desc;
            AddPinIn(pin);
        }
        public void AddOutput(PinOut pin, string name, UBufferCreator desc, string linkType = "Value")
        {
            pin.Name = name;
            pin.LinkDesc = UPgcEditorStyles.Instance.NewInOutPinDesc(linkType);

            pin.Tag = desc;
            AddPinOut(pin);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {

        }
        #region GUI
        public override void OnMouseStayPin(NodePin stayPin)
        {
            var creator = stayPin.Tag as UBufferCreator;
            if (creator != null)
            {
                EGui.Controls.CtrlUtility.DrawHelper($"{creator.ElementType.ToString()}");
            }
        }
        [Rtti.Meta]
        public virtual int PreviewResultIndex
        {
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
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            var graph = this.ParentGraph as UPgcGraph;

            if (graph != null && graph.GraphEditor != null)
            {
                graph.GraphEditor.NodePropGrid.Target = this;
            }
        }
        #endregion

        #region Link
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;
            var input = iPin.Tag as UBufferCreator;
            var output = oPin.Tag as UBufferCreator;

            if (IsMatchLinkedPin(input, output) == false)
            {
                return false;
            }
            return true;
        }
        public virtual bool IsMatchLinkedPin(UBufferCreator input, UBufferCreator output)
        {
            if (input.ElementType != output.ElementType)
            {
                return false;
            }
            if (input.XSize > output.XSize)
            {
                return false;
            }
            if (input.YSize > output.YSize)
            {
                return false;
            }
            if (input.ZSize > output.ZSize)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Cache
        public virtual Hash160 GetOutBufferHash(PinOut pin)
        {
            return Hash160.Emtpy;
        }
        public void SaveOutBufferToCache(UPgcGraph graph, PinOut pin)
        {
            if (graph.IsTryCacheBuffer == false)
                return;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return;
            var hash = GetOutBufferHash(pin);
            if (hash == Hash160.Emtpy)
                return;
            var rootDir = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.FileManager.CombinePath(rootDir, graph.AssetName.Name);
            IO.FileManager.SureDirectory(dir);
            buffer.SaveToCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        public void SaveOutBufferToCache(UPgcGraph graph, PinOut pin, in Hash160 hash)
        {
            if (graph.IsTryCacheBuffer == false)
                return;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return;
            var rootDir = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.FileManager.CombinePath(rootDir, graph.AssetName.Name);
            IO.FileManager.SureDirectory(dir);
            buffer.SaveToCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        public bool TryLoadOutBufferFromCache(UPgcGraph graph, PinOut pin)
        {
            if (graph.IsTryCacheBuffer == false)
                return false;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return false;
            var hash = GetOutBufferHash(pin);
            var rootDir = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.FileManager.CombinePath(rootDir, graph.AssetName.Name);

            return buffer.LoadFromCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        public bool TryLoadOutBufferFromCache(UPgcGraph graph, PinOut pin, in Hash160 hash)
        {
            if (graph.IsTryCacheBuffer == false)
                return false;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return false;
            var rootDir = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.FileManager.CombinePath(rootDir, graph.AssetName.Name);

            return buffer.LoadFromCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        #endregion

        public UBufferCreator GetInputBufferCreator(PinIn pin)
        {
            return pin.Tag as UBufferCreator;
        }
        public virtual UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            //return pin.Tag as UBufferCreator;
            return DefaultBufferCreator;
        }
        public virtual UBufferConponent GetResultBuffer(int index)
        {
            if (index < 0 || index >= Outputs.Count)
                return null;
            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(Outputs[index]);
        }

        #region procedure
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
                    if (PreviewSRV != null)
                    {
                        PreviewSRV?.FreeTextureHandle();
                        PreviewSRV = null;
                    }

                    var previewBuffer = GetResultBuffer(mPreviewResultIndex);
                    if (previewBuffer != null)
                    {
                        if (previewBuffer.BufferCreator.ElementType == Rtti.UTypeDescGetter<float>.TypeDesc)
                        {
                            float minV = float.MaxValue;
                            float maxV = float.MinValue;
                            previewBuffer.GetRangeUnsafe<float, FFloatOperator>(out minV, out maxV);
                            PreviewSRV = previewBuffer.CreateAsHeightMapTexture2D(minV, maxV, EPixelFormat.PXF_R16_FLOAT, true);
                        }
                        else if (previewBuffer.BufferCreator.ElementType == Rtti.UTypeDescGetter<Vector3>.TypeDesc)
                        {
                            Vector3 minV = Vector3.MaxValue;
                            Vector3 maxV = Vector3.MinValue;
                            previewBuffer.GetRangeUnsafe<Vector3, FFloat3Operator>(out minV, out maxV);
                            //float fMax = maxV.GetMaxValue();
                            //float fMin = minV.GetMinValue();
                            PreviewSRV = previewBuffer.CreateVector3Texture2D(minV, maxV);
                        }
                    }
                }


            }
            OnAfterProcedure(graph);
            return ret;
        }
        public virtual bool OnProcedure(UPgcGraph graph)
        {
            return true;
        }
        public virtual void OnAfterProcedure(UPgcGraph graph)
        {

        }
        [Rtti.Meta]
        public void DispatchBuffer(UPgcGraph graph, UBufferConponent result, object tag, bool bMultThread = false)
        {
            if (result == null)
                return;
            if (bMultThread == false)
            {
                for (int i = 0; i < result.Depth; i++)
                {
                    for (int j = 0; j < result.Height; j++)
                    {
                        for (int k = 0; k < result.Width; k++)
                        {
                            OnPerPixel(graph, this, result, k, j, i, tag);
                        }
                    }
                }
            }
            else
            {
                var smp = Thread.ASyncSemaphore.CreateSemaphore(result.Depth * result.Height * result.Width);
                if (result.Depth == 1 && result.Height == 1)
                {
                    for (int i = 0; i < result.Depth; i++)
                    {
                        for (int j = 0; j < result.Height; j++)
                        {
                            for (int k = 0; k < result.Width; k++)
                            {
                                int x = k;
                                int y = j;
                                int z = i;
                                UEngine.Instance.EventPoster.RunOn(() =>
                                {
                                    OnPerPixel(graph, this, result, x, y, z, tag);
                                    smp.Release();
                                    return null;
                                }, Thread.Async.EAsyncTarget.TPools);
                            }
                        }
                    }
                }
                else if (result.Depth == 1)
                {
                    for (int i = 0; i < result.Depth; i++)
                    {
                        for (int j = 0; j < result.Height; j++)
                        {
                            int y = j;
                            int z = i;
                            UEngine.Instance.EventPoster.RunOn(() =>
                            {
                                for (int k = 0; k < result.Width; k++)
                                {
                                    OnPerPixel(graph, this, result, k, y, z, tag);
                                    smp.Release();
                                }
                                return null;
                            }, Thread.Async.EAsyncTarget.TPools);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < result.Depth; i++)
                    {
                        int z = i;
                        UEngine.Instance.EventPoster.RunOn(() =>
                        {
                            for (int j = 0; j < result.Height; j++)
                            {
                                for (int k = 0; k < result.Width; k++)
                                {
                                    OnPerPixel(graph, this, result, k, j, z, tag);
                                    smp.Release();
                                }
                            }
                            return null;
                        }, Thread.Async.EAsyncTarget.TPools);
                    }
                }
                smp.Wait(int.MaxValue);
                smp.FreeSemaphore();
            }
        }
        public virtual void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferConponent resuilt, int x, int y, int z, object tag)
        {

        }
        #endregion

        #region Macross
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
        public UPgcNodeBase GetInputNode(UPgcGraph graph, int index, System.Type retType = null)
        {
            var linker = graph.FindInLinkerSingle(Inputs[index]);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        [Rtti.Meta]
        public UPgcNodeBase GetInputNodeByName(UPgcGraph graph, string pinName,
            [Rtti.MetaParameter(FilterType = typeof(UPgcNodeBase),
            ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type retType = null)
        {
            var pin = this.FindPinIn(pinName);
            if (pin == null)
                return null;
            var linker = graph.FindInLinkerSingle(pin);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        [Rtti.Meta]
        public UPgcNodeBase GetInputNode(UPgcGraph graph, PinIn pin)
        {
            var linker = graph.FindInLinkerSingle(pin);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        #endregion
    }
}
