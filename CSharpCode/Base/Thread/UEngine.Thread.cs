using EngineNS.Graphics.Pipeline;
using EngineNS.IO;
using System;
using System.Collections.Generic;

namespace EngineNS
{
    public partial class UEngine
    {
        public Thread.TtThreadMain ThreadMain
        {
            get;
        } = new Thread.TtThreadMain();

        public Thread.TtThreadRHI ThreadRHI
        {
            get;
        } = new Thread.TtThreadRHI();

        public Thread.TtThreadLogic ThreadLogic
        {
            get;
        } = new Thread.TtThreadLogic();

        public Thread.TtThreadRender ThreadRender
        {
            get;
        } = new Thread.TtThreadRender();

        public Thread.TtThreadAsync ThreadAsync
        {
            get;
        } = new Thread.TtThreadAsync();

        public Thread.TtThreadPhysics ThreadPhysics
        {
            get;
        } = new Thread.TtThreadPhysics();

        public Thread.TtThreadAsyncForEditor ThreadAsyncEditor
        {
            get;
        } = new Thread.TtThreadAsyncForEditor();
        public Thread.ThreadAsyncEditorSlow ThreadAsyncEditorSlow
        {
            get;
        } = new Thread.ThreadAsyncEditorSlow();
        public Thread.Async.TtContextThreadManager EventPoster
        {
            get
            {
                return ContextThreadManager;
            }
        }
        public Thread.TtContextThread GetContext(Thread.Async.EAsyncTarget target)
        {
            Thread.TtContextThread ctx = null;
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
            var ctx = Thread.TtContextThread.CurrentContext;
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
            ThreadLogic.LogicBegin.Set();
            ThreadRender.mRenderBegin.Set();
        }
        public void TryTickRender()
        {
            try
            {
                TickRenderModules();
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        ITickable cur;
                        if (TickableManager.Tickables[i].TryGetTarget(out cur))
                        {
                            cur.TickRender(ElapseTickCountMS);
                        }
                        else
                        {
                            TickableManager.Tickables.RemoveAt(i);
                            i--;
                        }
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
                TickLogicModules();
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        ITickable cur;
                        if (TickableManager.Tickables[i].TryGetTarget(out cur))
                        {
                            cur.TickLogic(ElapseTickCountMS);
                        }
                        else
                        {
                            TickableManager.Tickables.RemoveAt(i);
                            i--;
                        }
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
        public void TickBeginFrame()
        {
            try
            {
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        ITickable cur;
                        if (TickableManager.Tickables[i].TryGetTarget(out cur))
                        {
                            cur.TickBeginFrame(ElapseTickCountMS);
                        }
                        else
                        {
                            TickableManager.Tickables.RemoveAt(i);
                            i--;
                        }
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
                //using (new Profiler.TimeScopeHelper(ScopeDrawSlateWindow))
                //{
                //    DrawSlateWindow();
                //    UEngine.Instance.GfxDevice.SlateApplication?.OnDrawSlate();
                //}
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
        public void TickSync()
        {
            try
            {
                for (int i = 0; i < TickableManager.Tickables.Count; i++)
                {
                    try
                    {
                        ITickable cur;
                        if (TickableManager.Tickables[i].TryGetTarget(out cur))
                        {
                            cur.TickSync(ElapseTickCountMS);
                        }
                        else
                        {
                            TickableManager.Tickables.RemoveAt(i);
                            i--;
                        }
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
                using (new Profiler.TimeScopeHelper(ScopeDrawSlateWindow))
                {
                    DrawSlateWindow();
                    UEngine.Instance.GfxDevice.SlateApplication?.OnDrawSlate();
                }
                if (this.PlayMode != EPlayMode.Game)
                {
                    UEngine.Instance.AssetMetaManager.EditorCheckShowIconTimeout();
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            GfxDevice?.TickSync(this);
        }

        public static async System.Threading.Tasks.Task RunCoroutine<T>(IAsyncEnumerable<T> enumerable)
        {
            await foreach (var it in enumerable)
            {

            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeDrawSlateWindow = Profiler.TimeScopeManager.GetTimeScope(typeof(UEngine), nameof(DrawSlateWindow));
        partial void DrawSlateWindow();
    }
}