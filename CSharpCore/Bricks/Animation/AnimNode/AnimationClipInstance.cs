using EngineNS.Bricks.Animation.AnimElement;
using EngineNS.Bricks.Animation.Notify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.AnimNode
{
    public enum ClipWarpMode
    {
        Loop,
        Clamp,
    }
    [Rtti.MetaClass]
    public class AnimationClipInstance
    {
        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }
        [Rtti.MetaData]
        public RName Name { get; set; }
        public uint KeyFrames { get; set; }
        [Rtti.MetaData]
        public float SampleRate { get; set; }
        [Rtti.MetaData]
        public float Duration { get; set; } = 0.0f;
        [Rtti.MetaData]
        public List<ElementProperty> ElementProperties { get; set; } = new List<ElementProperty>();
        public string GetElementProperty(ElementPropertyType elementPropertyType)
        {
            for (int i = 0; i < ElementProperties.Count; ++i)
            {
                if (ElementProperties[i].ElementPropertyType == elementPropertyType)
                    return ElementProperties[i].Value;
            }
            return "";
        }
        #region Notifies
        protected List<CGfxNotify> mNotifies = new List<CGfxNotify>();
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CGfxNotify FindNotifyByName(string name)
        {
            for (int i = 0; i < mNotifies.Count; i++)
            {
                if (mNotifies[i].NotifyName == name)
                    return mNotifies[i];
            }
            return null;
        }

        [Rtti.MetaClass]
        public class NotifyWrapper : IO.Serializer.Serializer
        {
            [Rtti.MetaData]
            public List<CGfxNotify> Notifies
            {
                get;
                set;
            } = new List<CGfxNotify>();
        }
        public List<CGfxNotify> Notifies
        {
            get => mNotifies;
            set => mNotifies = value;
        }
        public void AttachNotifyEvent(int index, NotifyHandle notifyHandle)
        {
            if (index >= mNotifies.Count)
            {
                System.Diagnostics.Debug.WriteLine("NotifyAttach :Out of Range!!");
                return;
            }
            mNotifies[index].OnNotify += notifyHandle;
        }
        public List<CGfxNotify> CloneNotifies()
        {
            List<CGfxNotify> notifies = new List<CGfxNotify>();
            for (int i = 0; i < Notifies.Count; ++i)
            {
                notifies.Add(Notifies[i].Clone());
            }
            return notifies;
        }
        #endregion

        Dictionary<uint, AnimElement.CGfxAnimationElement> mElements = new Dictionary<uint, AnimElement.CGfxAnimationElement>();
        public Dictionary<uint, AnimElement.CGfxAnimationElement> Elements
        {
            get { return mElements; }
            set { mElements = value; }
        }
        public bool TryGetValue(uint key, out AnimElement.CGfxAnimationElement element)
        {
            return mElements.TryGetValue(key, out element);
        }
        public void Add(AnimElement.CGfxAnimationElement element)
        {
            mElements.Add(element.Desc.NameHash, element);
            if (KeyFrames < element.GetKeyCount())
            {
                KeyFrames = element.GetKeyCount();
            }
        }
        CGfxAnimationElement CreateAnimationElement(AnimationElementType eType)
        {
            CGfxAnimationElement result = null;
            switch (eType)
            {
                case AnimationElementType.Default:
                    {
                        result = new CGfxAnimationElement();
                    }
                    break;
                case AnimationElementType.Bone:
                    {
                        result = new CGfxBoneAnimationElement();
                    }
                    break;
                case AnimationElementType.Skeleton:
                    {
                        result = new CGfxSkeletonAnimationElement();
                    }
                    break;
            }
            return result;
        }
        public bool SyncLoad(CRenderContext rc, RName name)
        {
            if (name == null || name == RName.EmptyName)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
                return false;
            }
            if (name.IsExtension(CEngineDesc.AnimationClipExtension) == false)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
                return false;
            }
            using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
            using (var notifyXnd = IO.XndHolder.SyncLoadXND(name.Address + CEngineDesc.AnimationClipNotifyExtension))
            {
                if (xnd == null)
                    return false;
                Name = name;
                var att = xnd.Node.FindAttrib("Clip");
                att.BeginRead();
                att.ReadMetaObject(this);
                att.EndRead();
                var elementsNode = xnd.Node.FindNode("Elements");
                if (elementsNode != null)
                {
                    AnimationElementType elementType;
                    var nodes = elementsNode.GetNodes();
                    for (int i = 0; i < nodes.Count; ++i)
                    {
                        var elementTypeAtt = nodes[i].FindAttrib("ElementType");
                        elementTypeAtt.BeginRead();
                        elementTypeAtt.Read(out elementType);
                        elementTypeAtt.EndRead();
                        var element = CreateAnimationElement(elementType);
                        element.SyncLoad(rc, nodes[i]);
                        Add(element);
                    }
                }
                if(notifyXnd != null)
                {
                    var wrapper = new NotifyWrapper();
                    var notifyAtt = notifyXnd.Node.FindAttrib("Notify");
                    notifyAtt.BeginRead();
                    notifyAtt.ReadMetaObject(wrapper);
                    notifyAtt.EndRead();
                    Notifies = wrapper.Notifies;
                }
                EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
                return true;
            }
        }
        public async Task<bool> Load(CRenderContext rc, RName name)
        {
            await CEngine.Instance.EventPoster.Post(() =>
            {
                SyncLoad(rc, name);
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return true;
        }
        public void SaveAs(RName name)
        {
            if (name.IsExtension(CEngineDesc.AnimationClipExtension) == false)
                return;
            Name = name;
            Save(name.Address);
        }
        public void Save(string absFile)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            var att = xnd.Node.AddAttrib("Clip");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();

            var elementsNode = xnd.Node.AddNode("Elements", 0, 0);
            using (var it = mElements.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var animElement = it.Current.Value;
                    var animElementNode = elementsNode.AddNode(animElement.Desc.Name, 0, 0);
                    var typeAtt = animElementNode.AddAttrib("ElementType");
                    typeAtt.BeginWrite();
                    typeAtt.Write(animElement.ElementType);
                    typeAtt.EndWrite();
                    animElement.Save(animElementNode);
                }
            }
            IO.XndHolder.SaveXND(absFile, xnd);
            var wrapper = new NotifyWrapper();
            wrapper.Notifies = this.Notifies;
            var notifyXnd = IO.XndHolder.NewXNDHolder();
            var notifyAtt = notifyXnd.Node.AddAttrib("Notify");
            notifyAtt.BeginWrite();
            notifyAtt.WriteMetaObject(wrapper);
            notifyAtt.EndWrite();
            IO.XndHolder.SaveXND(absFile + CEngineDesc.AnimationClipNotifyExtension, notifyXnd);
        }
    }
    public class AnimationInstanceManager
    {
        public Dictionary<RName, AnimationClipInstance> AnimationClipInstanceDic
        {
            get;
        } = new Dictionary<RName, AnimationClipInstance>();
        public async Task<AnimationClipInstance> GetAnimationClipInstance(CRenderContext rc, RName name)
        {
            if (name.IsExtension(CEngineDesc.AnimationClipExtension) == false)
                return null;
            AnimationClipInstance first;
            bool finded = false;
            lock (AnimationClipInstanceDic)
            {
                if (AnimationClipInstanceDic.TryGetValue(name, out first) == false)
                {
                    first = new AnimationClipInstance();
                    AnimationClipInstanceDic.Add(name, first);
                }
                else
                    finded = true;
            }
            if (finded)
            {
                var context = await first.AwaitLoad();
                if (context != null && context.Result == null)
                    return null;
                return first;
            }

            if (false == await first.Load(rc, name/*, senv*/))
                return null;
            return first;
        }
        public AnimationClipInstance GetAnimationClipInstanceSync(CRenderContext rc, RName name)
        {
            if (name.IsExtension(CEngineDesc.AnimationClipExtension) == false)
                return null;
            AnimationClipInstance first;
            lock (AnimationClipInstanceDic)
            {
                if (AnimationClipInstanceDic.TryGetValue(name, out first) == false)
                {
                    first = new AnimationClipInstance();
                    AnimationClipInstanceDic.Add(name, first);
                }
                else
                    return first;
            }

            if (false == first.SyncLoad(rc, name/*, senv*/))
                return null;
            return first;
        }
        public void Remove(RName name)
        {
            if (AnimationClipInstanceDic.ContainsKey(name))
            {
                AnimationClipInstanceDic.Remove(name);
            }
        }
    }
}
namespace EngineNS
{
    public partial class CEngine
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Bricks.Animation.AnimNode.AnimationInstanceManager AnimationInstanceManager
        {
            get;
        } = new Bricks.Animation.AnimNode.AnimationInstanceManager();
    }
}