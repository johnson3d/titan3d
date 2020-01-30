using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial class CEngine
    {
		//from App main
        public Thread.ThreadMain ThreadMain
        {
            get;
        } = new Thread.ThreadMain();

        public Thread.ThreadRHI ThreadRHI
        {
            get;
        } = new Thread.ThreadRHI();

        public Thread.ThreadEditor ThreadEditor
        {
            get;
        } = new Thread.ThreadEditor(); 

        public Thread.ThreadLogic ThreadLogic
        {
            get;
        } = new Thread.ThreadLogic();

        public Thread.ThreadRender ThreadRender
        {
            get;
        } = new Thread.ThreadRender(); 

        public Thread.ThreadAsync ThreadAsync
        {
            get;
        } = new Thread.ThreadAsync();

        public Thread.ThreadPhysics ThreadPhysics
        {
            get;
        } = new Thread.ThreadPhysics();

        public Thread.ThreadAsyncForEditor ThreadAsyncEditor
        {
            get;
        } = new Thread.ThreadAsyncForEditor();
        public Thread.ThreadAsyncEditorSlow ThreadAsyncEditorSlow
        {
            get;
        } = new Thread.ThreadAsyncEditorSlow();

        private void StartSystemThreads()
        {
            ThreadRHI.FromCurrent("RHI");
            ThreadMain.FromCurrent("Main");
            ThreadEditor.FromCurrent("Editor");

            ThreadLogic.StartThread("Logic", null);
            ThreadRender.StartThread("Render", null);
            ThreadAsync.StartThread("AsyncIO", null);
            ThreadPhysics.StartThread("Physics", null);

            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                ThreadAsyncEditor.StartThread("AsyncEditor", null);
                ThreadAsyncEditorSlow.StartThread("AsyncEditorSlow", null);
            }

            EventPoster.StartPools(Desc.ThreadPoolCount);
        }
        private void StopSystemThreads()
        {
            //CEngine.Instance.ThreadMain.FrameBegin();

            ThreadRender.StopThread(null);
            ThreadLogic.StopThread(null);
            ThreadAsync.StopThread(null);
            ThreadPhysics.StopThread(null);
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                ThreadAsyncEditor.StopThread(null);
                ThreadAsyncEditorSlow.StopThread(null);
            }

            EventPoster.StopPools();
        }
        private void PauseSystemThreads()
        {
            ThreadRender.StopThread(null);
            ThreadLogic.StopThread(null);
            ThreadAsync.StopThread(null);
            ThreadPhysics.StopThread(null);
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
                ThreadAsyncEditor.StopThread(null);

            EventPoster.StopPools();
        }
        private void ResumeSystemThreads()
        {
            ThreadRender.StartThread("Render", null);
            ThreadLogic.StartThread("Logic", null);
            ThreadAsync.StartThread("AsyncIO", null);
            ThreadPhysics.StartThread("Physics", null);
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
                ThreadAsyncEditor.StartThread("AsyncEditor", null);

            EventPoster.StartPools(Desc.ThreadPoolCount);
        }
        //public System.Threading.AutoResetEvent mLogicBeginEvent = new System.Threading.AutoResetEvent(false);
        //public System.Threading.AutoResetEvent mLogicEndEvent = new System.Threading.AutoResetEvent(false);
        public void StartFrame()
        {
            CEngine.Instance.ThreadLogic.mLogicBegin.Set();
            CEngine.Instance.ThreadRender.mRenderBegin.Set();
        }
    }
}
