using EngineNS.Bricks.Animation.Notify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.AnimNode
{
    public delegate void AxisNameChange(string newName);
    [Rtti.MetaClass]
    public class BlendAxis : IO.Serializer.Serializer
    {
        public event AxisNameChange OnAxisNameChange;
        string mAxisName = "None";
        [Rtti.MetaData]
        public string AxisName
        {
            get => mAxisName;
            set
            {
                mAxisName = value;
                OnAxisNameChange?.Invoke(value);
            }
        }
        [Rtti.MetaData]
        public float Min { get; set; }
        [Rtti.MetaData]
        public float Max { get; set; }
        [Rtti.MetaData]
        public int GridNum { get; set; }
        public BlendAxis()
        {

        }
        public BlendAxis(string axisName = "None", float min = 0.0f, float max = 100.0f, int gridNum = 4)
        {
            AxisName = axisName;
            Min = min;
            Max = max;
            GridNum = gridNum;
        }
        public float Range
        {
            get { return Max - Min; }
        }
        public float GridSize
        {
            get { return Range / (float)GridNum; }
        }
    }
    public struct AnimationSampleData
    {
        public int AnimationIndex;
        public AnimationClip Animation;
        public float TotalWeight;
        public float Time;
        public float PreviousTime;
        public float SamplePlayRate;

        //FMarkerTickRecord MarkerTickRecord;
        //List<float> PerBoneBlendData;
    }
    //BlendSpace中 动作和所在的位置
    [Rtti.MetaClass]
   
    public class AnimationSample : IO.Serializer.Serializer
    {
        public event EventHandler OnAnimationChanged;
        AnimationClip mAnimation = null;
        public AnimationClip Animation
        {
            get => mAnimation;
            set
            {
                mAnimation = value;
                mAnimationName = value.Name;
                OnAnimationChanged?.Invoke(this, new EventArgs());
            }
        }
        [Rtti.MetaData]
        public Vector3 Value { get; set; }
        RName mAnimationName = RName.EmptyName;
        [Rtti.MetaData]
        [EngineNS.Editor.Editor_PackDataAttribute]
        public RName AnimationName
        {
            get => mAnimationName;
            set
            {
                mAnimationName = value;
                var clip = AnimationClip.CreateSync(mAnimationName);
                if (clip == null)
                    return;
                Animation = clip;

            }
        }
        public AnimationSample(AnimationClip animation, Vector3 value)
        {
            mAnimation = animation;
            mAnimationName = animation.Name;
            Value = value;
        }
        public AnimationSample()
        {
        }
    }
    [Rtti.MetaClass]
    public class GridElement : IO.Serializer.Serializer
    {
        public static int MaxVertices = 3;
        [Rtti.MetaData]
        public List<int> Indices { get; set; } = new List<int>() { -1, -1, -1 };
        [Rtti.MetaData]
        public List<float> Weights { get; set; } = new List<float>() { 0, 0, 0 };
        public GridElement()
        {

        }
        public GridElement(int count)
        {
            Indices = new List<int>();
            Weights = new List<float>();
            for (int i = 0; i < count; ++i)
            {
                Indices.Add(-1);
                Weights.Add(0);
            }
        }
    }
    //BlendSpace Grid里每个元素和权重
    public struct GridElementSample
    {
        public GridElement GridElement;
        public float BlendWeight;
    }
    [Rtti.MetaClass]
   
    public class BlendSpace : IO.Serializer.Serializer
    {
        protected Pose.CGfxSkeletonPose mPose = null;
        public virtual Pose.CGfxSkeletonPose Pose
        {
            get => mPose;
            set
            {
                mPose = value;
                for (int i = 0; i < mSamples.Count; i++)
                {
                    mSamples[i].Animation?.Bind(value.Clone());
                }
            }
        }
        protected Vector3 mInput = Vector3.Zero;
        public Vector3 Input
        {
            get => mInput;
            set
            {
                if (mInput == value)
                    return;
                mInput = value;
                EvaluateNewSamples();
            }
        }
        protected List<BlendAxis> mBlendAxises = new List<BlendAxis>() { null, null, null };
        [Rtti.MetaData]
        public List<BlendAxis> BlendAxises
        {
            get => mBlendAxises;
            protected set => mBlendAxises = value;
        }
        protected List<GridElement> mGridElements = new List<GridElement>();
        [Rtti.MetaData]
        public List<GridElement> GridElements
        {
            get => mGridElements;
            protected set => mGridElements = value;
        }
        protected List<AnimationSample> mSamples = new List<AnimationSample>();
        [Rtti.MetaData]
       
        public List<AnimationSample> Samples
        {
            get => mSamples;
            protected set => mSamples = value;
        }
        public AnimationSample GetAnimationSample(RName name)
        {
           var temp = mSamples.Find((sample) => { return sample.AnimationName == name; });
            return temp;
        }
        public AnimationSample GetAnimationSample(int index)
        {
            if (mSamples.Count < index)
                return null;
            return mSamples[index];
        }
        List<CGfxNotify> mNotifies = new List<CGfxNotify>();
        public List<CGfxNotify> Notifies
        {
            get
            {
                if (mNotifies.Count == 0)
                {
                    for (int i = 0; i < mSamples.Count; ++i)
                    {
                        if (mSamples[i].Animation != null)
                        {
                            mNotifies.AddRange(mSamples[i].Animation.Notifies);
                        }
                    }
                }
                return mNotifies;
            }
        }
        public void AttachNotifyEvent(int sampleIndex,int notifyIndex, NotifyHandle notifyHandle)
        {
            mSamples[sampleIndex].Animation.AttachNotifyEvent(notifyIndex, notifyHandle);
        }
        public string GetElementProperty(ElementPropertyType elementPropertyType)
        {
            if (mSamples.Count > 0)
            {
                return mSamples[0].Animation.GetElementProperty(elementPropertyType);
            }
            return "";
        }
        protected BlendSpace()
        {

        }
        public float Duration
        {
            get => DurationInMilliSecond * 0.001f;
            set => DurationInMilliSecond = (long)(value * 1000);
        }
        public long DurationInMilliSecond
        {
            get;
            protected set;
        }
        public uint KeyFrames
        {
            get;
            protected set;
        }
        public float PlayRate { get; set; } = 1.0f;
        public float Fps
        {
            get;
            protected set;
        }
        public float CurrentTime
        {
            get => mCurrentTimeInMilliSecond * 0.001f;
            set
            {
                mCurrentTimeInMilliSecond = (long)(value * 1000);
            }
        }
        long mCurrentTimeInMilliSecond = 0;
        public long CurrentTimeInMilliSecond
        {
            get => mCurrentTimeInMilliSecond;
            set
            {
                mCurrentTimeInMilliSecond = value;
            }
        }
        public float PlayPercent { get; set; } = 0.0f;
        RName mSkeletonAsset = RName.EmptyName;
        [Rtti.MetaData]
        public RName SkeletonAsset
        {
            get => mSkeletonAsset;
            set => mSkeletonAsset = value;
        }
        [Rtti.MetaData]
        public RName Name { get; set; } = RName.EmptyName;
        //播放了多长时间，包括循环
        protected long mLastTime = 0;
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
        protected bool mIsLoop = true;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsLoop
        {
            get => mIsLoop;
            set => mIsLoop = value;
        }
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
        Int64 beforeTime = 0;
        protected void TimeAdvance(float elapseTimeSecond)
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
            PlayPercent = CurrentTime / Duration;
        }
        public List<AnimationSampleData> CurrentSamples { get; set; } = new List<AnimationSampleData>();
        protected void EvaluateNewSamples()
        {
            lock (CurrentSamples)
            {
                CurrentSamples.Clear();
                GetSamplesFromBlendInput(mInput,CurrentSamples);
                float newDuration = 0;
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    newDuration += CurrentSamples[i].Animation.Duration * CurrentSamples[i].TotalWeight;
                }
                float scale = 1;
                if (Duration > 0)
                    scale = newDuration / Duration;
                Duration = newDuration;
                CurrentTime = CurrentTime * scale;
                mLastTime = mCurrentTimeInMilliSecond;
                LoopTimes = 0;
                KeyFrames = (uint)(Duration * 30);
            }
        }

        public virtual void Tick()
        {
            if (Pause)
                return;
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                TimeAdvance(CEngine.Instance.EngineElapseTimeSecond);
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(PlayPercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public virtual void Tick(float elapseTimeSecond)
        {
            if (Pause)
                return;
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                TimeAdvance(elapseTimeSecond);
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(PlayPercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public virtual void Evaluate(float playpercent)
        {
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(playpercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public virtual void TickFroEditor(float playpercent)
        {
            lock (CurrentSamples)
            {
                if (CurrentSamples == null || CurrentSamples.Count == 0)
                {
                    EvaluateNewSamples();
                }
                for (int i = 0; i < CurrentSamples.Count; ++i)
                {
                    CurrentSamples[i].Animation.Seek(playpercent * CurrentSamples[i].Animation.Duration);
                }
                BlendSamples(CurrentSamples);
            }
        }
        public virtual void TickNofity(GamePlay.Component.GComponent component)
        {
            for (int i = 0; i < CurrentSamples.Count; ++i)
            {
                CurrentSamples[i].Animation.TickNofity(component);
            }
        }
        protected virtual void BlendSamples(List<AnimationSampleData> samples)
        {
            if (samples == null)
                return;
            if (samples.Count == 1)
            {
                Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, samples[0].Animation.BindingSkeletonPose);
            }
            if (samples.Count == 2)
            {
                Animation.Runtime.CGfxAnimationRuntime.BlendPose(Pose, samples[0].Animation.BindingSkeletonPose, samples[1].Animation.BindingSkeletonPose, samples[1].TotalWeight);
            }
            if (samples.Count > 2)
            {
                float totalWeigth = samples[0].TotalWeight + samples[1].TotalWeight;
                float bWeight = samples[1].TotalWeight / totalWeigth;
                Animation.Runtime.CGfxAnimationRuntime.BlendPose(Pose, samples[0].Animation.BindingSkeletonPose, samples[1].Animation.BindingSkeletonPose, bWeight);
                for (int i = 2; i < samples.Count; ++i)
                {
                    totalWeigth += samples[i].TotalWeight;
                    bWeight = samples[i].TotalWeight / totalWeigth;
                    Animation.Runtime.CGfxAnimationRuntime.BlendPose(Pose, Pose, samples[i].Animation.BindingSkeletonPose, bWeight);
                }
            }
        }
        //add to blendSpace and copy Clone Pose
        public virtual AnimationSample AddSample(AnimationClip sample, Vector3 value)
        {
            //valid sample
            var animSample = new AnimationSample(sample, value);
            sample.Bind(Pose.Clone());
            mSamples.Add(animSample);
            animSample.OnAnimationChanged += AnimSample_OnAnimationChanged;
            ResampleData();
            return animSample;
        }

        private void AnimSample_OnAnimationChanged(object sender, EventArgs e)
        {
            //StretchAnimationClips();
        }


        protected virtual void ResampleData()
        {

        }
        public bool RemoveSample(int index)
        {
            if (mSamples.Count < index)
                return false;
            mSamples[index].OnAnimationChanged -= AnimSample_OnAnimationChanged;
            mSamples.RemoveAt(index);
            ResampleData();
            return true;
        }
        public void ReFresh()
        {
            ResampleData();
        }
        protected virtual void GetRawSamplesFromBlendInput(Vector3 input, List<GridElementSample> gridElementSamples)
        {
            return;
        }
        protected Vector3 GriddingInput(Vector3 input)
        {
            Vector3 MinBlendInput = new Vector3(mBlendAxises[0].Min, mBlendAxises[1].Min, mBlendAxises[2].Min);
            Vector3 MaxBlendInput = new Vector3(mBlendAxises[0].Max, mBlendAxises[1].Max, mBlendAxises[2].Max);
            Vector3 GridSize = new Vector3(mBlendAxises[0].GridSize, mBlendAxises[1].GridSize, mBlendAxises[2].GridSize);

            Vector3 griddingInput;
            griddingInput.X = MathHelper.Clamp(input.X, MinBlendInput.X, MaxBlendInput.X);
            griddingInput.Y = MathHelper.Clamp<float>(input.Y, MinBlendInput.Y, MaxBlendInput.Y);
            griddingInput.Z = MathHelper.Clamp<float>(input.Z, MinBlendInput.Z, MaxBlendInput.Z);

            griddingInput -= MinBlendInput;
            griddingInput.X /= GridSize.X;
            griddingInput.Y /= GridSize.Y;
            griddingInput.Z /= GridSize.Z;
            return griddingInput;
        }
        protected void FillTheGrid(int[] sortedIndex2SampleIndex, List<GridElement> gridElements)
        {
            mGridElements.Clear();
            for (int i = 0; i < gridElements.Count; ++i)
            {
                var element = gridElements[i];
                var newElement = new GridElement(3);
                float totalWeight = 0;
                for (int vertexIndex = 0; vertexIndex < GridElement.MaxVertices; ++vertexIndex)
                {
                    int sortedIndex = element.Indices[vertexIndex];
                    if (sortedIndex != -1 && sortedIndex2SampleIndex[sortedIndex] != -1)
                    {
                        newElement.Indices[vertexIndex] = sortedIndex2SampleIndex[sortedIndex];
                    }
                    else
                    {
                        newElement.Indices[vertexIndex] = -1;
                    }

                    if (newElement.Indices[vertexIndex] == -1)
                    {
                        newElement.Weights[vertexIndex] = 0.0f;
                    }
                    else
                    {
                        newElement.Weights[vertexIndex] = element.Weights[vertexIndex];
                        totalWeight += element.Weights[vertexIndex];
                    }
                }
                if (totalWeight > 0.0f)
                {
                    for (int j = 0; j < GridElement.MaxVertices; ++j)
                    {
                        newElement.Weights[j] /= totalWeight;
                    }
                }

                mGridElements.Add(newElement);
            }
        }
        int outSamplesSearchIndex = 0;
        bool OutSamplesFindIndex(AnimationSampleData sample)
        {
            return sample.AnimationIndex == outSamplesSearchIndex;
        }
        int OutSamplesSort(AnimationSampleData a, AnimationSampleData b)
        {
            if (b.TotalWeight < a.TotalWeight)
                return 1;
            if (b.TotalWeight == a.TotalWeight)
                return 0;
            else
                return -1;
        }
        List<GridElementSample> mGridElementSamples = new List<GridElementSample>();
        protected void GetSamplesFromBlendInput(Vector3 input, List<AnimationSampleData> outSamples)
        {
            if (mSamples.Count == 0)
                return;
            mGridElementSamples.Clear();
            GetRawSamplesFromBlendInput(input, mGridElementSamples);
            for (int i = 0; i < mGridElementSamples.Count; ++i)
            {
                var gridElementSample = mGridElementSamples[i];
                var gridWeight = gridElementSample.BlendWeight;
                var gridElement = gridElementSample.GridElement;
                for (int index = 0; index < GridElement.MaxVertices; ++index)
                {
                    var animationIndex = gridElement.Indices[index];
                    outSamplesSearchIndex = animationIndex;
                    if (animationIndex != -1)
                    {
                        var result = outSamples.FindIndex(OutSamplesFindIndex);
                        AnimationSampleData animationData;
                        if (result == -1)
                        {
                            animationData = new AnimationSampleData();
                            animationData.TotalWeight += gridElement.Weights[index] * gridWeight;
                            animationData.Animation = mSamples[animationIndex].Animation;
                            animationData.AnimationIndex = animationIndex;
                            outSamples.Add(animationData);
                        }
                        else
                        {
                            animationData = outSamples[result];
                            animationData.TotalWeight += gridElement.Weights[index] * gridWeight;
                            animationData.Animation = mSamples[animationIndex].Animation;
                            animationData.AnimationIndex = animationIndex;
                            outSamples[result] = animationData;
                        }

                    }
                }
            }

            outSamples.Sort(OutSamplesSort);
            float totalWeight = 0;
            for (int i = 0; i < outSamples.Count; ++i)
            {
                totalWeight += outSamples[i].TotalWeight;
                if (outSamples[i].TotalWeight < 0.000001f)
                {
                    outSamples.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < outSamples.Count; ++i)
            {
                var sample = outSamples[i];
                sample.TotalWeight /= totalWeight;
                outSamples[i] = sample;
            }
        }

        public virtual void Save(string absFileName)
        {
            var xnd = IO.XndHolder.NewXNDHolder();
            SaveXnd(xnd.Node);
            IO.XndHolder.SaveXND(absFileName, xnd);
        }
        public virtual void Save()
        {
            Save(Name.Address);
        }
        protected void SaveXnd(IO.XndNode node)
        {
            var versionAtt = node.AddAttrib("Version");
            versionAtt.BeginWrite();
            versionAtt.Write(0);
            versionAtt.EndWrite();
            var att = node.AddAttrib("BlendSpace");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
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
        public virtual bool SyncLoad(CRenderContext rc, RName name)
        {
            using (var xnd = IO.XndHolder.SyncLoadXND(name.Address))
            {
                if (xnd == null)
                    return false;
                Name = name;
                var attrib = xnd.Node.FindAttrib("Version");
                int version = -1;
                if (attrib != null)
                {
                    attrib.BeginRead();
                    attrib.Read(out version);
                    attrib.EndRead();
                    var att = xnd.Node.FindAttrib("BlendSpace");
                    if (att != null)
                    {
                        att.BeginRead();
                        att.ReadMetaObject(this);
                        att.EndRead();
                    }
                }
                else
                {
                    OldLoadXnd(xnd.Node);
                }
            }
            return true;
        }
        protected void OldLoadXnd(IO.XndNode node)
        {
            var attrib = node.FindAttrib("SkeletonAsset");
            if (attrib != null)
            {
                string temp = "";
                attrib.BeginRead();
                attrib.Read(out temp);
                attrib.EndRead();
                mSkeletonAsset = RName.GetRName(temp);
            }
            var sampleNode = node.FindNode("Samples");
            if (sampleNode != null)
            {
                var sampleAtts = sampleNode.GetAttribs();
                for (int i = 0; i < sampleAtts.Count; ++i)
                {
                    AnimationSample sample;
                    if (LoadAnimationSample(sampleAtts[i], out sample))
                    {
                        mSamples.Add(sample);
                    }
                }
            }

            var blendAxisNode = node.FindNode("BlendAsixes");
            if (blendAxisNode != null)
            {
                var blendAxisAtts = blendAxisNode.GetAttribs();
                for (int i = 0; i < blendAxisAtts.Count; ++i)
                {
                    BlendAxis axis;
                    if (LoadBlendAxis(blendAxisAtts[i], out axis))
                    {
                        mBlendAxises[i] = axis;
                    }
                }

            }

            var gridElementsNode = node.FindNode("GridElements");
            if (gridElementsNode != null)
            {
                var nodes = gridElementsNode.GetNodes();
                for (int i = 0; i < nodes.Count; ++i)
                {
                    GridElement gridElement;
                    if (LoadGridElement(nodes[i], out gridElement))
                    {
                        mGridElements.Add(gridElement);
                    }
                }
            }
        }

        protected void OldSaveXnd(IO.XndNode node)
        {
            var versionAtt = node.AddAttrib("Version");
            versionAtt.BeginWrite();
            versionAtt.Write(0);
            versionAtt.EndWrite();
            var att = node.AddAttrib("SkeletonAsset");
            att.BeginWrite();
            att.Write(mSkeletonAsset.Name);
            att.EndWrite();
            var sampleNode = node.AddNode("Samples", 0, 0);
            for (int i = 0; i < mSamples.Count; ++i)
            {
                SaveAnimationSample(sampleNode, mSamples[i]);
            }

            var blendAxisNode = node.AddNode("BlendAsixes", 0, 0);
            for (int i = 0; i < mBlendAxises.Count; ++i)
            {
                SaveBlendAxis(blendAxisNode, mBlendAxises[i]);
            }

            var gridElementsNode = node.AddNode("GridElements", 0, 0);
            for (int i = 0; i < mGridElements.Count; ++i)
            {
                SaveGridElement(gridElementsNode, mGridElements[i]);
            }
        }
        void SaveAnimationSample(IO.XndNode xndNode, AnimationSample sample)
        {
            var att = xndNode.AddAttrib("Sample");
            att.BeginWrite();
            att.Write(sample.AnimationName.Name);
            att.Write(sample.Value);
            att.EndWrite();
        }
        bool LoadAnimationSample(IO.XndAttrib att, out AnimationSample sample)
        {
            sample = new AnimationSample();
            if (att != null)
            {
                att.BeginRead();
                string name;
                att.Read(out name);
                sample.AnimationName = RName.GetRName(name);
                Vector3 tempValue = Vector3.Zero;
                att.Read(out tempValue);
                sample.Value = tempValue;
                att.EndRead();
                if (Pose != null)
                    sample.Animation.Bind(Pose.Clone());
                return true;
            }
            return false;
        }
        void SaveGridElement(IO.XndNode xndNode, GridElement gridElement)
        {
            var node = xndNode.AddNode("GridElement", 0, 0);
            var indicesNode = node.AddNode("Indices", 0, 0);
            for (int i = 0; i < gridElement.Indices.Count; ++i)
            {
                var att = indicesNode.AddAttrib("Index");
                att.BeginWrite();
                att.Write(gridElement.Indices[i]);
                att.EndWrite();
            }
            var weightsNode = node.AddNode("Weights", 0, 0);
            for (int i = 0; i < gridElement.Weights.Count; ++i)
            {
                var att = weightsNode.AddAttrib("Weight");
                att.BeginWrite();
                att.Write(gridElement.Weights[i]);
                att.EndWrite();
            }
        }
        bool LoadGridElement(IO.XndNode node, out GridElement gridElement)
        {
            gridElement = new GridElement();
            if (node != null)
            {
                var indicesNode = node.FindNode("Indices");
                var atts = indicesNode.GetAttribs();
                for (int i = 0; i < atts.Count; ++i)
                {
                    var indexAtt = atts[i];
                    indexAtt.BeginRead();
                    int index = -1;
                    indexAtt.Read(out index);
                    gridElement.Indices[i] = index;
                    indexAtt.EndRead();
                }
                var weightsNode = node.FindNode("Weights");
                atts = weightsNode.GetAttribs();
                for (int i = 0; i < atts.Count; ++i)
                {
                    var weightAtt = atts[i];
                    weightAtt.BeginRead();
                    float weight = 0;
                    weightAtt.Read(out weight);
                    gridElement.Weights[i] = weight;
                    weightAtt.EndRead();
                }
                return true;
            }
            else
                return false;

        }
        void SaveBlendAxis(IO.XndNode xndNode, BlendAxis blendAxis)
        {
            var att = xndNode.AddAttrib("BlendAxis");
            att.BeginWrite();
            att.Write(blendAxis.AxisName);
            att.Write(blendAxis.Min);
            att.Write(blendAxis.Max);
            att.Write(blendAxis.GridNum);
            att.EndWrite();
        }
        bool LoadBlendAxis(IO.XndAttrib att, out BlendAxis blendAxis)
        {
            blendAxis = new BlendAxis();
            if (att != null)
            {
                att.BeginRead();
                var axisName = "";
                att.Read(out axisName);
                blendAxis.AxisName = axisName;
                float temp = 0;
                att.Read(out temp);
                blendAxis.Min = temp;
                att.Read(out temp);
                blendAxis.Max = temp;
                int intTemp = 0;
                att.Read(out intTemp);
                blendAxis.GridNum = intTemp;
                att.EndRead();
                return true;
            }
            return false;
        }
    }
}
