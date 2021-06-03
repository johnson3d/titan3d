using System;
using System.Collections.Generic;

namespace EngineNS
{
    public partial class UEngine
    {
        public Thread.ThreadMain ThreadMain
        {
            get;
        } = new Thread.ThreadMain();

        public Thread.ThreadRHI ThreadRHI
        {
            get;
        } = new Thread.ThreadRHI();

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
        public Thread.Async.UContextThreadManager EventPoster
        {
            get
            {
                return ContextThreadManager;
            }
        }
        public Thread.ContextThread GetContext(Thread.Async.EAsyncTarget target)
        {
            Thread.ContextThread ctx = null;
            switch (target)
            {
                case Thread.Async.EAsyncTarget.AsyncIO:
                    ctx = this.ThreadAsync;
                    break;
                case Thread.Async.EAsyncTarget.Physics:
                    ctx = this.ThreadPhysics;
                    break;
                case Thread.Async.EAsyncTarget.Logic:
                    ctx = this.ThreadLogic;
                    break;
                case Thread.Async.EAsyncTarget.Render:
                    ctx = this.ThreadRHI;
                    break;
                case Thread.Async.EAsyncTarget.Main:
                    ctx = this.ThreadMain;
                    break;
                case Thread.Async.EAsyncTarget.AsyncEditor:
                    ctx = this.ThreadAsyncEditor;
                    break;
                case Thread.Async.EAsyncTarget.AsyncEditorSlow:
                    ctx = this.ThreadAsyncEditorSlow;
                    break;
            }
            return ctx;
        }
        public bool IsThread(Thread.Async.EAsyncTarget target)
        {
            var ctx = Thread.ContextThread.CurrentContext;
            switch (target)
            {
                case Thread.Async.EAsyncTarget.AsyncIO:
                    return ctx == this.ThreadAsync;
                case Thread.Async.EAsyncTarget.Physics:
                    return ctx == this.ThreadPhysics;
                case Thread.Async.EAsyncTarget.Logic:
                    return ctx == this.ThreadLogic;
                case Thread.Async.EAsyncTarget.Render:
                    return ctx == this.ThreadRHI;
                case Thread.Async.EAsyncTarget.Main:
                    return ctx == this.ThreadMain;
                case Thread.Async.EAsyncTarget.AsyncEditor:
                    return ctx == this.ThreadAsyncEditor;
                case Thread.Async.EAsyncTarget.AsyncEditorSlow:
                    return ctx == this.ThreadAsyncEditorSlow;
                case Thread.Async.EAsyncTarget.TPools:
                    break;
            }
            return false;
        }
        private void StartSystemThreads()
        {
            ThreadRHI.FromCurrent("RHI");
            ThreadMain.FromCurrent("Main");

            ThreadLogic.StartThread("Logic", null);
            ThreadRender.StartThread("Render", null);
            ThreadAsync.StartThread("AsyncIO", null);
            ThreadPhysics.StartThread("Physics", null);

            if (PlayMode != EPlayMode.Game)
            {
                ThreadAsyncEditor.StartThread("AsyncEditor", null);
                ThreadAsyncEditorSlow.StartThread("AsyncEditorSlow", null);
            }

            EventPoster.StartPools(Config.NumOfThreadPool);
        }
        private void StopSystemThreads()
        {
            ThreadRender.StopThread(null);
            ThreadLogic.StopThread(null);
            ThreadAsync.StopThread(null);
            ThreadPhysics.StopThread(null);
            if (PlayMode != EPlayMode.Game)
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
            if (PlayMode != EPlayMode.Game)
                ThreadAsyncEditor.StopThread(null);

            EventPoster.StopPools();
        }
        private void ResumeSystemThreads()
        {
            ThreadRender.StartThread("Render", null);
            ThreadLogic.StartThread("Logic", null);
            ThreadAsync.StartThread("AsyncIO", null);
            ThreadPhysics.StartThread("Physics", null);
            if (PlayMode != EPlayMode.Game)
                ThreadAsyncEditor.StartThread("AsyncEditor", null);

            EventPoster.StartPools(Config.NumOfThreadPool);
        }
        public void StartFrame()
        {
            ThreadLogic.mLogicBegin.Set();
            ThreadRender.mRenderBegin.Set();
        }
        public void TryTickRender()
        {
            try
            {
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        TickableManager.Tickables[i].TickRender(ElapseTickCount);
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public void TryTickLogic()
        {
            try
            {
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        TickableManager.Tickables[i].TickLogic(ElapseTickCount);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        public void TickSync()
        {
            try
            {
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        TickableManager.Tickables[i].TickSync(ElapseTickCount);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }

            try
            {
                DrawSlateWindow();
                if (this.PlayMode != EPlayMode.Game)
                {
                    UEngine.Instance.AssetMetaManager.EditorCheckShowIconTimeout();
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
        partial void DrawSlateWindow();
    }
}