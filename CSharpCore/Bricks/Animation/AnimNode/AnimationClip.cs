using EngineNS.Bricks.Animation.AnimElement;
using EngineNS.Bricks.Animation.Binding;
using EngineNS.Bricks.Animation.Notify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.AnimNode
{
    public enum ElementPropertyType
    {
        EPT_Property,
        EPT_Skeleton,
        EPT_InValid,
    }
    [Rtti.MetaClass]
    public class ElementProperty : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        public ElementPropertyType ElementPropertyType { get; set; } = ElementPropertyType.EPT_InValid;
        [Rtti.MetaData]
        public string Value { get; set; } = null;
        public ElementProperty()
        {

        }
        public ElementProperty(ElementPropertyType elementPropertyType, string value)
        {
            ElementPropertyType = elementPropertyType;
            Value = value;
        }
    }

    public class AnimationClip : IAnimation
    {
        public async static Task<AnimationClip> Create(RName name)
        {
            var instance = await CEngine.Instance.AnimationInstanceManager.GetAnimationClipInstance(CEngine.Instance.RenderContext, name);
            if (instance != null)
            {
                return new AnimationClip() { ClipInstance = instance };
            }
            return null;
        }
        public static AnimationClip CreateSync(RName name)
        {
            if (name == null)
                return null;
            var instance = CEngine.Instance.AnimationInstanceManager.GetAnimationClipInstanceSync(CEngine.Instance.RenderContext, name);
            if (instance != null)
            {
                return new AnimationClip() { ClipInstance = instance };
            }
            return null;
        }
        #region IAnimation

        public RName Name
        {
            get
            {
                if (ClipInstance == null)
                    return RName.EmptyName;
                return ClipInstance.Name;
            }
        }
        public uint KeyFrames
        {
            get
            {
                if (ClipInstance == null)
                    return 0;
                return ClipInstance.KeyFrames;
            }
        }

        public float SampleRate
        {
            get
            {
                if (ClipInstance == null)
                    return 0;
                return ClipInstance.SampleRate;
            }
        }

        public float Duration
        {
            get
            {
                if (ClipInstance == null)
                    return 0;
                return ClipInstance.Duration;
            }
        }
        public float CurrentTime { get => mCurrentTimeInMilliSecond * 0.001f; }
        public long DurationInMilliSecond
        {
            get => (long)(Duration * 1000);
        }
        long mCurrentTimeInMilliSecond = 0;
        public long CurrentTimeInMilliSecond
        {
            get => mCurrentTimeInMilliSecond;
            set
            {
                mCurrentTimeInMilliSecond = value;
                PlayPercent = CurrentTime / Duration;
            }
        }
        public float PlayPercent { get; set; } = 0.0f;
        public float PlayRate { get; set; } = 1.0f;
        #endregion

        protected bool mPause = false;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Pause
        {
            get => mPause;
            set => mPause = value;
        }
        protected bool mFinish = false;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Finish
        {
            get => mFinish;
            set => mFinish = value;
        }
        protected bool mIsLoop = true;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsLoop
        {
            get => mIsLoop;
            set => mIsLoop = value;
        }
        public string GetElementProperty(ElementPropertyType elementPropertyType)
        {
            return ClipInstance.GetElementProperty(elementPropertyType);
        }
        protected uint mCurrentLoopTimes = 0;
        public uint CurrentLoopTimes
        {
            get => mCurrentLoopTimes;
            set => mCurrentLoopTimes = value;
        }
        protected uint mLoopTimes = 0;
        public uint LoopTimes
        {
            get => mLoopTimes;
            set => mLoopTimes = value;
        }
        List<CGfxNotify> mNotifies = new List<CGfxNotify>();
        public List<CGfxNotify> Notifies
        {
            get => mNotifies;
            set => mNotifies = value;
        }
        public List<CGfxNotify> InstanceNotifies
        {
            get => ClipInstance?.Notifies;
        }
        public void AddInstanceNotify(CGfxNotify notify)
        {
            ClipInstance?.Notifies.Add(notify);
        }
        public void RemoveInstanceNotify(CGfxNotify notify)
        {
            ClipInstance?.Notifies.Remove(notify);
        }
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
        public void AttachNotifyEvent(int index, NotifyHandle notifyHandle)
        {
            if (index >= mNotifies.Count)
            {
                System.Diagnostics.Debug.WriteLine("NotifyAttach :Out of Range!!");
                return;
            }
            mNotifies[index].OnNotify += notifyHandle;
        }
        //播放了多长时间，包括循环
        protected long mLastTime = 0;
        public AnimationBindingPose BindingPose { get; set; }
        AnimationClipInstance mClipInstance = null;
        public AnimationClipInstance ClipInstance
        {
            get => mClipInstance;
            set
            {
                mClipInstance = value;
                if (value != null)
                {
                    Notifies = mClipInstance.CloneNotifies();
                }
            }
        }

        public void StretchTime(float timeSecond)
        {
            if (timeSecond == 0)
                return;
            PlayRate = Duration / timeSecond;
        }
        void TimeAdvance(float elapseTimeSecond)
        {
            mLastTime += (long)((elapseTimeSecond * PlayRate) * 1000);
            CurrentLoopTimes = (uint)(mLastTime / (DurationInMilliSecond + 1));
            beforeTime = CurrentTimeInMilliSecond;
            CurrentTimeInMilliSecond = mLastTime % (DurationInMilliSecond + 1);
            if (IsLoop == false && beforeTime > CurrentTimeInMilliSecond)
            {
                CurrentTimeInMilliSecond = DurationInMilliSecond;
                Finish = true;
            }
        }
        void TimeClamp(float timeInSecond)
        {
            mLastTime = (long)((timeInSecond * PlayRate) * 1000);
            mLastTime = MathHelper.Clamp<long>(mLastTime, 0, DurationInMilliSecond);
            CurrentLoopTimes = (uint)(mLastTime / (DurationInMilliSecond + 1));
            beforeTime = CurrentTimeInMilliSecond;
            CurrentTimeInMilliSecond = mLastTime % (DurationInMilliSecond + 1);
            if (IsLoop == false && beforeTime > CurrentTimeInMilliSecond)
            {
                CurrentTimeInMilliSecond = DurationInMilliSecond;
                Finish = true;
            }
        }

        public void Reset()
        {
            mLastTime = 0;
            CurrentLoopTimes = 0;
            CurrentTimeInMilliSecond = 0;
            beforeTime = 0;
            Finish = false;
        }
        public void Tick()
        {
            if (Pause)
                return;
            TimeAdvance(CEngine.Instance.EngineElapseTimeSecond);
            AdvanceAnim();
        }
        public void TickNofity(GamePlay.Component.GComponent component)
        {
            //notifies
            for (int i = 0; i < Notifies.Count; ++i)
            {
                Notifies[i].TickLogic(component);
            }
            for (int i = 0; i < Notifies.Count; ++i)
            {
                Notifies[i].Notify(component, beforeTime, mCurrentTimeInMilliSecond);
            }
        }
        public void Seek(float timeInSecond)
        {
            if (BindingPose == null)
                return;
            TimeClamp(timeInSecond);
            AdvanceAnim();
        }
        public void SeekForEditor(float timeInSecond)
        {
            if (BindingPose == null)
                return;
            TimeClamp(timeInSecond);
            AdvanceAnim();
        }
        public void Jump(float timeInSecond)
        {
            CurrentTimeInMilliSecond = (long)(timeInSecond * 1000);
            mLastTime = CurrentTimeInMilliSecond;
            CurrentLoopTimes = 0;
            beforeTime = CurrentTimeInMilliSecond;
            Finish = false;
            Seek(timeInSecond);
        }
        void AdvanceAnim()
        {
            Evaluate(CurrentTime, BindingPose);

        }
        Int64 beforeTime = 0;
        internal void Evaluate(float timeInSecond, AnimationBindingPose pose)
        {
            for (int i = 0; i < pose.BindingElements.Count; ++i)
            {
                var eleHash = pose.BindingElements[i].AnimationElementHash;
                AnimElement.CGfxAnimationElement element = null;
                if (ClipInstance.TryGetValue(eleHash, out element))
                {
                    element.Evaluate(timeInSecond, pose.BindingElements[i]);
                    //element.Evaluate(manualTime, pose.BindingElements[i]);
                }
            }
            pose.Flush();
        }
        internal void Evaluate(float timeInSecond)
        {
            for (int i = 0; i < BindingPose.BindingElements.Count; ++i)
            {
                var eleHash = BindingPose.BindingElements[i].AnimationElementHash;
                AnimElement.CGfxAnimationElement element = null;
                if (ClipInstance.TryGetValue(eleHash, out element))
                {
                    element.Evaluate(timeInSecond, BindingPose.BindingElements[i]);
                    //element.Evaluate(manualTime, pose.BindingElements[i]);
                }
            }
            BindingPose.Flush();
        }
        internal void Evaluate(float timeInSecond, ClipWarpMode mode)
        {
            if (mode == ClipWarpMode.Loop)
                timeInSecond = (timeInSecond * 1000 % (DurationInMilliSecond + 1)) * 0.001f;
            for (int i = 0; i < BindingPose.BindingElements.Count; ++i)
            {
                var eleHash = BindingPose.BindingElements[i].AnimationElementHash;
                AnimElement.CGfxAnimationElement element = null;
                if (ClipInstance.TryGetValue(eleHash, out element))
                {
                    element.Evaluate(timeInSecond, BindingPose.BindingElements[i]);
                    //element.Evaluate(manualTime, pose.BindingElements[i]);
                }
            }
            BindingPose.Flush();
        }

        public AnimationBindingPose Bind(GamePlay.Actor.GActor objToBind)
        {
            AnimationBindingPose bp = new AnimationBindingPose();
            using (var it = ClipInstance.Elements.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var element = it.Current.Value;
                    if (element.ElementType == AnimationElementType.Default)
                    {
                        var bindingObj = new ObjectAnimationBinding();
                        bindingObj.BindingPath = element.Desc.Path;
                        bindingObj.AnimationElementHash = element.Desc.NameHash;
                        bindingObj.RootObject = objToBind;
                        bindingObj.BindedProperty = PropertyChainCache.GetObjectProperty(bindingObj, objToBind);
                        bp.Add(bindingObj);
                    }
                    else if (element.ElementType == AnimationElementType.Skeleton)
                    {
                        var meshCom = objToBind.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                        if (meshCom != null)
                        {
                            var skinModifier = meshCom.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                            var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, RName.GetRName(skinModifier.SkeletonAsset));
                            if (skeleton == null)
                                skeleton = skinModifier.SkinSkeleton;
                            var skeletonBinding = SkeletonAnimationBinding.Bind(skinModifier.AnimationPose, (AnimElement.CGfxSkeletonAnimationElement)element);
                            skeletonBinding.BindingPath = element.Desc.Path;
                            skeletonBinding.AnimationElementHash = element.Desc.NameHash;
                            //skeletonBinding.ske = PropertyChainCache.GetObjectProperty(skeletonBinding.BindingPath, objToBind);
                            //过场动画的话，actor上面会有一个专门的管理和播放动画的组件，用来实现动画过渡，融合等，可支持宏图控制
                            //每根骨绑定
                            bp.Add(skeletonBinding);
                        }
                    }
                }
            }
            BindingPose = bp;
            return bp;
        }
        public Pose.CGfxSkeletonPose BindingSkeletonPose { get; set; } = null;
        public AnimationBindingPose Bind(Pose.CGfxSkeletonPose pose)
        {
            BindingSkeletonPose = pose;
            AnimationBindingPose bp = new AnimationBindingPose();
            using (var it = ClipInstance.Elements.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var element = it.Current.Value;
                    if (element.ElementType == AnimationElementType.Default)
                    {

                    }
                    else if (element.ElementType == AnimationElementType.Skeleton)
                    {

                        var skeletonBinding = SkeletonAnimationBinding.Bind(pose, (AnimElement.CGfxSkeletonAnimationElement)element);
                        skeletonBinding.BindingPath = element.Desc.Path;
                        skeletonBinding.AnimationElementHash = element.Desc.NameHash;
                        //skeletonBinding.ske = PropertyChainCache.GetObjectProperty(skeletonBinding.BindingPath, objToBind);
                        //过场动画的话，actor上面会有一个专门的管理和播放动画的组件，用来实现动画过渡，融合等，可支持宏图控制
                        //每根骨绑定
                        bp.Add(skeletonBinding);

                    }
                }
            }
            BindingPose = bp;
            return bp;

        }
        public Pose.CGfxSkeletonPose GetAnimationSkeletonPose(float timeSecond)
        {
            for (int i = 0; i < BindingPose.BindingElements.Count; ++i)
            {
                var eleHash = BindingPose.BindingElements[i].AnimationElementHash;
                AnimElement.CGfxAnimationElement element = null;
                if (ClipInstance.Elements.TryGetValue(eleHash, out element))
                {
                    if (element.ElementType == AnimationElementType.Skeleton)
                    {
                        var binding = BindingPose.BindingElements[i] as SkeletonAnimationBinding;
                        element.Evaluate(timeSecond, binding);
                        BindingPose.Flush();
                        return binding.Pose.Clone();
                    }
                    //element.Evaluate(manualTime, pose.BindingElements[i]);
                }
            }
            return null;
        }
        //public Pose.CGfxAnimationPoseProxy GenerateAnimationPoseProxy()
        //{
        //    //根据 clip 生成 poseProxy
        //    //poseProxy 包括 skeleton，bone,object,等等属性的集合。
        //    //每个可动画实体都有PoseProxy,
        //}
        public GamePlay.Actor.GActor GetActor(string name)
        {
            return null;
        }
        public AnimationBindingPose Bind(GamePlay.Actor.GActor[] objsToBind)
        {
            return null;
        }
        #region IAnimation SaveLoad
        public void Save()
        {
            SaveAs(Name);
        }
        public void SaveAs(RName name)
        {
            ClipInstance?.SaveAs(name);
        }
        public void Save(string absFile)
        {
            ClipInstance?.Save(absFile);
        }
        #endregion
    }
}

