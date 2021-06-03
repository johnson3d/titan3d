using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Base
{
    public interface IAnimatableClassDesc : ISyncXndSaveLoad
    {
        public Rtti.UTypeDesc ClassType { get; }
        public VNameString TypeStr { get; set; }
        public VNameString Name { get; set; }
        public List<IAnimatablePropertyDesc> Properties { get;}
    }
    public interface IAnimatablePropertyDesc : ISyncXndSaveLoad
    {
        public Rtti.UTypeDesc ClassType { get; }
        public VNameString TypeStr { get; set; }
        public VNameString Name { get; set; }
        public int CurveIndex { get; set; }
    }
    public class AnimHierarchy : IAsyncXndSaveLoad
    {
        public IAnimatableClassDesc Value { get; set; }
        public AnimHierarchy Root { get; set; }
        public AnimHierarchy Parent { get; set; }
        public List<AnimHierarchy> Children { get; set; } = new List<AnimHierarchy>();
        public async Task<bool> LoadXnd(CXndHolder xndHolder, XndNode parentNode)
        {
            await UEngine.Instance.EventPoster.Post(async () =>
            {
                using (var hierarchyNode = parentNode.TryGetChildNode("AnimHierarchyNode"))
                {
                    if (!hierarchyNode.IsValidPointer)
                    {
                        return false;
                    }
                    using (var att = parentNode.TryGetAttribute("ValueType"))
                    {
                        if (!att.IsValidPointer)
                        {
                            return false;
                        }
                        unsafe
                        {
                            VNameString typeStr = *VNameString.CreateInstance();
                            att.BeginRead();
                            att.Read(ref typeStr);
                            att.EndRead();
                            Value = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(typeStr.ToString())) as IAnimatableClassDesc;
                            Value.LoadXnd(xndHolder, hierarchyNode);
                        }
                    }
                    using (var att = parentNode.TryGetAttribute("ChildCount"))
                    {
                        if (!att.IsValidPointer)
                        {
                            return false;
                        }
                        int childrenCount = 0;
                        att.BeginRead();
                        att.Read(ref childrenCount);
                        att.EndRead();
                        for (int i = 0; i < childrenCount; ++i)
                        {
                            AnimHierarchy node = new AnimHierarchy();
                            await node.LoadXnd(xndHolder, hierarchyNode);
                            Children.Add(node);
                        }
                    }
                }
                await Thread.AsyncDummyClass.DummyFunc();
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }

        public async Task<bool> SaveXnd(CXndHolder xndHolder, XndNode parentNode)
        {
            await UEngine.Instance.EventPoster.Post(async () =>
            {

                using (var hierarchyNode = xndHolder.NewNode("AnimHierarchyNode", 1, 0))
                {
                    using (var countAtt = xndHolder.NewAttribute("ValueType", 1, 0))
                    {
                        countAtt.BeginWrite(10);
                        countAtt.Write(Value.TypeStr);
                        countAtt.EndWrite();
                    }
                    Value.SaveXnd(xndHolder, hierarchyNode);
                    using (var countAtt = xndHolder.NewAttribute("ChildCount", 1, 0))
                    {
                        countAtt.BeginWrite(10);
                        countAtt.Write(Children.Count);
                        countAtt.EndWrite();
                    }
                    for (int i = 0; i < Children.Count; ++i)
                    {
                        await SaveXnd(xndHolder, hierarchyNode);
                    }
                    unsafe
                    {
                        parentNode.AddNode(hierarchyNode);
                    }
                }
                await Thread.AsyncDummyClass.DummyFunc();
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }

    }
}
