using EngineNS.Graphics.Pipeline.Common;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtRenderGraphPin
    {
        public TtAttachBuffer.ELifeMode LifeMode { get; set; } = TtAttachBuffer.ELifeMode.Transient;
        public enum EPinType
        {
            Input,
            Output,
            InputOutput,
        }
        public string Name { get; private set; }
        public EPinType PinType { get; private set; }
        public string LinkType { get; set; } = "GraphNode";//如果需要特定Type才能链接，那就再PinOut和PinIn上做一个LinkType的匹配
        public bool IsAutoResize { get; set; } = true;
        public bool IsAllowInputNull { get; set; } = false;
        public TtRenderGraphNode HostNode { get; set; }
        public T GetNakedHostNode<T>() where T : TtRenderGraphNode
        {
            if (HostNode is TtFindNode)
            {
                return (HostNode as TtFindNode).GetReferNode() as T;
            }
            else
            {
                return HostNode as T;
            }
        }

        public TtAttachmentDesc Attachement = new TtAttachmentDesc();
        public TtAttachBuffer ImportedBuffer = null;
        public TtRenderGraphLinker FindInLinker()
        {
            return HostNode.RenderGraph.FindInLinker(this);
        }
        public static TtRenderGraphPin CreateInput(string name, NxRHI.EBufferType types)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.Input;
            result.IsAutoResize = false;
            result.Attachement.Format = EPixelFormat.PXF_UNKNOWN;
            result.Attachement.BufferViewTypes = types;
            return result;
        }
        public static TtRenderGraphPin CreateOutput(string name, bool isAutoSize, EPixelFormat defaultFormat, NxRHI.EBufferType types)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.Output;
            result.IsAutoResize = isAutoSize;
            result.Attachement.Format = defaultFormat;
            result.Attachement.BufferViewTypes = types;
            return result;
        }
        public static TtRenderGraphPin CreateInputOutput(string name, NxRHI.EBufferType types)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.InputOutput;
            result.IsAutoResize = false;
            result.Attachement.Format = EPixelFormat.PXF_UNKNOWN;
            result.Attachement.BufferViewTypes = types;
            return result;
        }
        public static TtRenderGraphPin CreateInputOutput(string name, bool isAutoSize, EPixelFormat defaultFormat, NxRHI.EBufferType types)
        {
            var result = new TtRenderGraphPin();
            result.Name = name;
            result.PinType = EPinType.InputOutput;
            result.IsAutoResize = isAutoSize;
            result.Attachement.Format = defaultFormat;
            result.Attachement.BufferViewTypes = types;
            return result;
        }
    }
    public class TtRenderGraphLinker
    {
        public TtRenderGraphPin OutPin;
        public TtRenderGraphPin InPin;
        public bool IsSame(TtRenderGraphLinker rh)
        {
            if (OutPin != rh.OutPin ||
                InPin != rh.InPin)
            {
                return false;
            }
            return true;
        }
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public abstract class TtRenderGraphNode : IO.BaseSerializer, IDisposable
    {
        ~TtRenderGraphNode()
        {
            Dispose();
        }
        public virtual void Dispose()
        {

        }
        internal int mMaxLeafDistance = 0;
        [Category("Option")]
        public int MaxLeafDistance
        {
            get => mMaxLeafDistance;
        }
        public int TempRootDistance = 0;
        public virtual bool IsUsed
        {
            get;
            set;
        } = true;
        [Category("Option")]
        [Rtti.Meta("")]
        public virtual bool Enable
        {
            get;
            set;
        } = true;
        private string mName;
        [Category("Option")]
        public virtual string Name
        {
            get => mName;
            set
            {
                mName = value;
                //foreach (var i in OutputGraphPins)
                //{
                //    i.Attachement.AttachmentName = FHashText.Create($"{Name}->{i.Name}");
                //}
            }
        }
        public Guid UniqueId { get; } = Guid.NewGuid();
        public virtual Color4b GetTileColor()
        {
            return Color4b.FromRgb(255, 0, 255);
        }
        public TtRenderGraph RenderGraph { get; internal set; }
        protected List<TtRenderGraphPin> InputGraphPins { get; } = new List<TtRenderGraphPin>();
        protected List<TtRenderGraphPin> OutputGraphPins { get; } = new List<TtRenderGraphPin>();
        public virtual int NumOfInput
        {
            get
            {
                return InputGraphPins.Count;
            }
        }
        public virtual int NumOfOutput
        {
            get
            {
                return OutputGraphPins.Count;
            }
        }
        public bool AddInput(TtRenderGraphPin pin)
        {
            if (pin.PinType != TtRenderGraphPin.EPinType.Input)
                return false;
            foreach (var i in InputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            pin.HostNode = this;
            InputGraphPins.Add(pin);
            return true;
        }
        public bool AddOutput(TtRenderGraphPin pin)
        {
            if (pin.PinType != TtRenderGraphPin.EPinType.Output)
                return false;
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            pin.Attachement.AttachmentName = FHashText.Create($"{UniqueId}->{pin.Name}");
            pin.HostNode = this;
            OutputGraphPins.Add(pin);
            return true;
        }
        public bool AddInputOutput(TtRenderGraphPin pin)
        {
            if (pin.PinType != TtRenderGraphPin.EPinType.InputOutput)
                return false;
            pin.Attachement.AttachmentName = FHashText.Create($"{UniqueId}->{pin.Name}");
            pin.HostNode = this;
            foreach (var i in InputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            InputGraphPins.Add(pin);
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == pin.Name)
                    return false;
            }
            OutputGraphPins.Add(pin);
            return true;
        }
        public virtual TtRenderGraphPin GetInput(int index)
        {
            return InputGraphPins[index];
        }
        public virtual TtRenderGraphPin GetOutput(int index)
        {
            return OutputGraphPins[index];
        }
        public TtRenderGraphPin FindInput(string name)
        {
            foreach (var i in InputGraphPins)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public TtRenderGraphPin FindOutput(string name)
        {
            foreach (var i in OutputGraphPins)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public TtAttachBuffer ImportAttachment(TtRenderGraphPin pin, TtAttachBuffer attachBuffer)
        {
            return RenderGraph.AttachmentCache.ImportAttachment(pin, attachBuffer);
        }
        public TtAttachBuffer FindAttachBuffer(TtRenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
        }
        public TtAttachBuffer GetAttachBuffer(TtRenderGraphPin pin)
        {
            return RenderGraph.AttachmentCache.GetAttachement(pin.Attachement.AttachmentName, pin.Attachement);
        }
        public bool MoveAttachment(TtRenderGraphPin pinFrom, TtRenderGraphPin pinTo)
        {
            return RenderGraph.AttachmentCache.MoveAttachement(pinFrom.Attachement.AttachmentName, pinTo.Attachement.AttachmentName);
            //UAttachBuffer attachment;
            //if (RenderGraph.AttachmentCache.CachedAttachments.TryGetValue(pinFrom.Attachement.AttachmentName, out attachment))
            //{
            //    RenderGraph.AttachmentCache.CachedAttachments.Remove(pinFrom.Attachement.AttachmentName);
            //    RenderGraph.AttachmentCache.CachedAttachments.Add(pinTo.Attachement.AttachmentName, attachment);
            //    return true;
            //}
            //return false;
        }

        public virtual async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
        }
        public virtual void InitNodePins()
        {

        }
        public virtual void FrameBuild(TtRenderPolicy policy)
        {
        }
        public virtual void OnLinkIn(TtRenderGraphLinker linker)
        {
            if ((linker.InPin.Attachement.BufferViewTypes & linker.OutPin.Attachement.BufferViewTypes) != linker.OutPin.Attachement.BufferViewTypes)
            {
                return;
            }
            //linker.InPin.Attachement.Format = linker.OutPin.Attachement.Format;
        }
        public virtual void OnLinkOut(TtRenderGraphLinker linker)
        {

        }
        public virtual void OnResize(TtRenderPolicy policy, float x, float y)
        {

        }
        public virtual void BeginTick(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {

        }
        public virtual void EndTick(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {

        }
        public abstract ref Profiler.TimeScope GetThreadStaticRDGTickScope();
        public Profiler.TimeScope RDGTickScope
        {
            get
            {
                ref var scorp = ref GetThreadStaticRDGTickScope();
                if (scorp == null)
                {
                    scorp = new Profiler.TimeScope(this.GetType(), nameof(Tick));
                }
                return scorp;
            }
        }
        public virtual void BeforeTick(TtRenderPolicy policy)
        {

        }
        public virtual void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {

        }
        public virtual void TickSync(TtRenderPolicy policy)
        {

        }

        public void TryReleaseBufers(List<TtRenderGraphLinker> linkers, Action<TtRenderGraphNode, TtRenderGraphPin, TtAttachBuffer> onRemove)
        {
            var j = this;
            for (int k = 0; k < j.NumOfOutput; k++)
            {
                var t = j.GetOutput(k);
                RenderGraph.FindOutLinkers(t, linkers);
                if (linkers.Count > 0)
                {
                    var buffer = RenderGraph.AttachmentCache.FindAttachement(t.Attachement.AttachmentName);
                    if (buffer != null && buffer.LifeMode == TtAttachBuffer.ELifeMode.Transient)
                    {
                        buffer.AddRef(linkers.Count);
                    }
                }
                else
                {
                    var buffer = RenderGraph.AttachmentCache.FindAttachement(t.Attachement.AttachmentName);
                    if (buffer != null && buffer.LifeMode == TtAttachBuffer.ELifeMode.Transient)
                    {
                        if (buffer.RefCount == 0)
                        {
                            if (onRemove != null)
                            {
                                onRemove(this, t, buffer);
                            }
                            RenderGraph.AttachmentCache.RemoveAttachement(t.Attachement.AttachmentName);
                        }
                    }
                }
            }

            for (int k = 0; k < j.NumOfInput; k++)
            {
                var t = j.GetInput(k);
                if (t.FindInLinker() == null)
                    continue;
                var buffer = RenderGraph.AttachmentCache.FindAttachement(t.Attachement.AttachmentName);
                if (buffer != null && buffer.LifeMode == TtAttachBuffer.ELifeMode.Transient)
                {
                    var count = buffer.Release();
                    if (count <= 0)
                    {
                        if (onRemove != null)
                        {
                            onRemove(this, t, buffer);
                        }
                        this.RenderGraph.AttachmentCache.RemoveAttachement(t.Attachement.AttachmentName);
                    }
                }
            }
        }

        public void UpdateNodeDistance(TtRenderGraph graph, TtRenderGraphNode root)
        {
            for (int i = 0; i < NumOfInput; i++)
            {
                var pin = this.GetInput(i);
                var linker = graph.FindInLinker(pin);
                if (linker != null)
                {
                    linker.OutPin.HostNode.TempRootDistance = TempRootDistance + 1;
                    if (root.mMaxLeafDistance < linker.OutPin.HostNode.TempRootDistance)
                    {
                        root.mMaxLeafDistance = linker.OutPin.HostNode.TempRootDistance;
                    }
                    linker.OutPin.HostNode.UpdateNodeDistance(graph, root);
                }
                //else
                //{
                //    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "RenderGraph", $"Input({pin.HostNode.Name}.{pin.Name}) linked null");
                //}
            }
        }

        public virtual Shader.TtGraphicsShadingEnv GetPassShading(Mesh.TtRenderMesh.TtAtom atom = null)
        {
            return null;
        }
        public virtual void OnDrawCall(Shader.TtGraphicsShadingEnv shading, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {

        }
    }

    public abstract class TAuxRenderGraphNode<T> : TtRenderGraphNode 
    {
        [ThreadStatic]
        private static Profiler.TimeScope mRDGTickScope = null;
        public override ref Profiler.TimeScope GetThreadStaticRDGTickScope()
        {
            return ref mRDGTickScope;
        }
    }
}