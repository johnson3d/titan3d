using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.Animation.Notify
{
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    public class McNotify
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnHandleNoify(CGfxNotify sender)
        {

        }
    }
    public delegate void NotifyHandle(CGfxNotify sender);
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Rtti.MetaClass]

    public class CGfxNotify : IO.Serializer.Serializer
    {
        Int64 mNotifyTime = 0;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Rtti.MetaData]
        [ReadOnly(true)]
        public Int64 NotifyTime
        {
            get => mNotifyTime;
            set => mNotifyTime = value;
        }
        string mNotifyName;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Rtti.MetaData]
        [ReadOnly(true)]
        public string NotifyName
        {
            get => mNotifyName;
            set => mNotifyName = value;
        }
        Guid mId = Guid.NewGuid();
        [Rtti.MetaData]
        [Browsable(false)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Guid ID
        {
            get => mId;
            set => mId = value;
        }
        public event NotifyHandle OnNotify;
        public virtual void TickLogic(GamePlay.Component.GComponent component)
        {
            //OnNotify?.Invoke(this);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnEditor_Selected(AnimNode.AnimationClip anim, GamePlay.Actor.GActor actorEd)
        {

        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnEditor_UnSelected(AnimNode.AnimationClip anim, GamePlay.Actor.GActor actorEd)
        {

        }
        public bool Firing(Int64 beforeTime, Int64 afterTime)
        {
            if (mNotifyTime > beforeTime && mNotifyTime <= afterTime)
            {
                return true;
            }
            return false;
        }
        public virtual bool Notify(GamePlay.Component.GComponent component, Int64 beforeTime, Int64 afterTime)
        {
            if (mNotifyTime > beforeTime && mNotifyTime <= afterTime)
            {
                McNotifyGetter?.Get()?.OnHandleNoify(this);
                OnNotify?.Invoke(this);
                return true;
            }
            return false;
        }
        public virtual void Notifying()
        {
            McNotifyGetter?.Get()?.OnHandleNoify(this);
            OnNotify?.Invoke(this);
        }
        public virtual bool EditorNotify(Int64 beforeTime, Int64 afterTime)
        {
            if (mNotifyTime > beforeTime && mNotifyTime <= afterTime)
            {
                McNotifyGetter?.Get()?.OnHandleNoify(this);
                OnNotify?.Invoke(this);
                return true;
            }
            else if (mNotifyTime <= beforeTime && mNotifyTime > afterTime)
            {
                McNotifyGetter?.Get()?.OnHandleNoify(this);
                OnNotify?.Invoke(this);
                return true;
            }
            return false;
        }

        protected RName mNotifyMacross;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McNotify))]
        [Editor.Editor_PackData()]
        [Rtti.MetaData]
        public RName NotifyMacross
        {
            get { return mNotifyMacross; }
            set
            {
                if (mNotifyMacross == value)
                    return;
                mNotifyMacross = value;
                mMcNotifyGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<McNotify>(value);
            }
        }
        protected Macross.MacrossGetter<McNotify> mMcNotifyGetter;
        [System.ComponentModel.Browsable(false)]
        public Macross.MacrossGetter<McNotify> McNotifyGetter
        {
            get { return mMcNotifyGetter; }
        }
        public CGfxNotify Clone()
        {
            var notify = Activator.CreateInstance(Rtti.RttiHelper.GetTypeFromTypeFullName(this.GetType().FullName)) as CGfxNotify;
            Rtti.MetaClass.CopyData(this, notify);
            return notify;
        }
    }
}
