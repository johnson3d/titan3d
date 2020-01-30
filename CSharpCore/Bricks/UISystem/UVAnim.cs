using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.IO.Serializer;

namespace EngineNS.UISystem
{
    [Rtti.MetaClass]
   
    public partial class UVAnim : IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        
        public void Cleanup()
        {
            mTextureObject?.Cleanup();
            mMaterialInstanceObject?.Cleanup();
        }

        string mTextureShaderVarName = "texture";
        [Rtti.MetaData]
        public string TextureShaderVarName
        {
            get => mTextureShaderVarName;
            set
            {
                mTextureShaderVarName = value;
                OnPropertyChanged("TextureShaderVarName");
            }
        }

        RName mTextureRName;
        [Rtti.MetaData]
        [EngineNS.Editor.Editor_PackDataAttribute]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Texture)]
        public RName TextureRName
        {
            get => mTextureRName;
            set
            {
                mTextureRName = value;

                var rc = CEngine.Instance.RenderContext;
                if (mTextureObject != null)
                    mTextureObject.Cleanup();
                if(mTextureRName != null && mTextureRName != RName.EmptyName)
                {
                    mTextureObject = CEngine.Instance.TextureManager.GetShaderRView(rc, mTextureRName);
                    var noUse = CalculateWH(rc);
                }

                OnPropertyChanged("TextureRName");
            }
        }

        async Task CalculateWH(CRenderContext rc)
        {
            var textureObject = await CEngine.Instance.TextureManager.AwaitTextureValid(rc, TextureRName);
            var desc = textureObject.TxPicDesc;
            mPixelWidth = (UInt32)desc.Width;
            mPixelHeight = (UInt32)desc.Height;
        }
        public async Task WaitTextureValid(CRenderContext rc)
        {
            var textureObject = await CEngine.Instance.TextureManager.AwaitTextureValid(rc, TextureRName);
        }

        UInt32 mPixelWidth = UInt32.MaxValue;
        public UInt32 PixelWidth => mPixelWidth;
        UInt32 mPixelHeight = UInt32.MaxValue;
        public UInt32 PixelHeight => mPixelHeight;

        CShaderResourceView mTextureObject;
        [Browsable(false)]
        public CShaderResourceView TextureObject => mTextureObject;

        RName mMaterialInstanceRName;
        [Rtti.MetaData]
        [EngineNS.Editor.Editor_PackDataAttribute]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance)]
        public RName MaterialInstanceRName
        {
            get => mMaterialInstanceRName;
            set
            {
                mMaterialInstanceRName = value;
                var noUse = UpdateMaterialInstanceObject();
                OnPropertyChanged("MaterialInstanceRName");
            }
        }
        async Task UpdateMaterialInstanceObject()
        {
            var rc = CEngine.Instance.RenderContext;
            if (mMaterialInstanceObject != null)
                mMaterialInstanceObject.Cleanup();
            mMaterialInstanceObject = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, mMaterialInstanceRName);
        }
        
        Graphics.CGfxMaterialInstance mMaterialInstanceObject;
        [Browsable(false)]
       
        public Graphics.CGfxMaterialInstance MaterialInstanceObject => mMaterialInstanceObject;

        int mVersion = 0;
        [Rtti.MetaData]
        public int Version
        {
            get => mVersion;
            protected set
            {
                mVersion = value;
                OnPropertyChanged("Version");
            }
        }

        // 帧动画播放持续时间(毫秒)
        float mDuration = 1000.0f;
        [Rtti.MetaData]
        public float Duration
        {
            get => mDuration;
            set
            {
                mDuration = value;
                OnPropertyChanged("Duration");
            }
        }

        #region FramesOperation

        int mPreFrameIndex = -1;
        int mCurrentPlayedTimes = 0;
        public int CurrentPlayedTimes
        {
            get => mCurrentPlayedTimes;
        }

        int mPlayTimes = 0;
        [Rtti.MetaData]
        public int PlayTimes
        {
            get => mPlayTimes;
            set
            {
                mPlayTimes = value;
                ResetUVAnimFramePlay();
                OnPropertyChanged("PlayTimes");
            }
        }

        public void ResetUVAnimFramePlay()
        {
            mCurrentPlayedTimes = 0;
            mPreFrameIndex = -1;
            mFrameStartTime = EngineNS.Support.Time.GetTickCount();
        }

        List<UVFrame> mFrames = new List<UVFrame>();
        [Rtti.MetaData]
        [Browsable(false)]
        public List<UVFrame> Frames
        {
            get => mFrames;
            set
            {
                mFrames = value;
                for(int i=0; i<mFrames.Count; i++)
                {
                    mFrames[i].ParentAnim = this;
                }
                OnPropertyChanged("Frames");
            }
        }
        public UVFrame GetUVFrame(float millisecondTime, out bool frameChanged)
        {
            frameChanged = false;
            int index = 0;
            if (mFrames.Count != 1 && mDuration != 0)
                index = GetFrameIndex(millisecondTime);
            if (index != mPreFrameIndex)
                frameChanged = true;

            if (mPreFrameIndex > index)
            {
                mCurrentPlayedTimes++;
                if(PlayTimes > 0 && mCurrentPlayedTimes >= PlayTimes)
                {
                    return mFrames[mFrames.Count - 1];
                }
            }
            mPreFrameIndex = index;
            return mFrames[index];
        }
        float mFrameStartTime = 0;
        int GetFrameIndex(float millisecondTime)
        {
            millisecondTime -= mFrameStartTime;
            var fRemainder = (float)System.Math.IEEERemainder(millisecondTime, mDuration) / mDuration;
            if (fRemainder < 0)
                fRemainder = 1 + fRemainder;
            return (int)(fRemainder * Frames.Count);
        }
        public UVFrame AddFrame()
        {
            var frame = new UVFrame();
            frame.ParentAnim = this;
            Frames.Add(frame);
            return frame;
        }
        public void DelFrame(int idx)
        {
            // 至少有一帧
            if (Frames.Count <= 1)
                return;
            if (idx < 0 || idx >= Frames.Count)
                return;
            Frames.RemoveAt(idx);
        }

        #endregion

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }

        public async Task<bool> LoadUVAnimAsync(CRenderContext rc, RName name)
        {
            IO.XndHolder xnd = null;

            var loadOK = await CEngine.Instance.EventPoster.Post(() =>
            {
                using (xnd = IO.XndHolder.SyncLoadXND(name.Address))
                {
                    if (xnd == null)
                        return false;
                    var att = xnd.Node.FindAttrib("data");
                    if (att != null)
                    {
                        att.BeginRead();
                        att.ReadMetaObject(this);
                        att.EndRead();
                    }
                }
                return true;
            });
            if(false == loadOK)
            {
                Thread.Async.TaskLoader.Release(ref WaitContext, null);
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "UVAnim", $"LoadUVAnimAsync {name.Address} Failed");
                return false;
            }

            await CEngine.Instance.TextureManager.AwaitTextureValid(rc, TextureRName);
            for(int i=0; i<mFrames.Count; i++)
            {
                await mFrames[i].UpdateScale9Infos();
            }

            Thread.Async.TaskLoader.Release(ref WaitContext, this);
            return true;
        }

        public void Save2Xnd(RName name)
        {
            Save2Xnd(name.Address);
        }
        public void Save2Xnd(string absFileName)
        {
            Version++;
            var xnd = IO.XndHolder.NewXNDHolder();
            var att = xnd.Node.AddAttrib("data");
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
            IO.XndHolder.SaveXND(absFileName, xnd);
        }

        UVAnim mTemplateUVAnim = null;
        [Browsable(false)]
        public UVAnim TemplateUVAnim
        {
            get => mTemplateUVAnim;
        }
        public void CopyFromTemplate(UVAnim template)
        {
            if (template == null)
                return;
            TextureRName = template.TextureRName;
            MaterialInstanceRName = template.MaterialInstanceRName;
            Version = template.Version;
            Duration = template.Duration;
            Frames.Clear();
            for(int i=0; i<template.Frames.Count; i++)
            {
                var srcFrame = template.Frames[i];
                var copyedFrame = srcFrame.CloneObject() as UVFrame;
                copyedFrame.ParentAnim = this;
                //var noUse = copyedFrame.UpdateScale9Infos();
                Frames.Add(copyedFrame);
            }
            mTemplateUVAnim = template;
        }
        public void CheckAndAutoReferenceFromTemplateUVAnim()
        {
            if (mTemplateUVAnim == null)
                return;

            if (mTemplateUVAnim.Version != Version)
                CopyFromTemplate(mTemplateUVAnim);
        }
    }
}
