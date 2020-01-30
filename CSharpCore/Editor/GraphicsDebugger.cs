using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class GraphicsDebugger
    {
        public enum EDebugTarget
        {
            Game,
            Editor,
        }
        EDebugTarget mTarget = EDebugTarget.Editor;
        public EDebugTarget Target
        {
            get
            {
                return mTarget;
            }
            set
            {
                mTarget = value;
                if (mTarget == EDebugTarget.Game && CEngine.Instance.GameInstance == null)
                {
                    mTarget = EDebugTarget.Editor;
                }
            }
        }
        int mCurrentDrawCallStep;
        public int CurrentDrawCallStep
        {
            get
            {
                return mCurrentDrawCallStep;
            }
            set
            {
                mCurrentDrawCallStep = value;
                switch (Target)
                {
                    case EDebugTarget.Editor:
                        {
                            var rpolicy = CEngine.Instance.GameEditorInstance.RenderPolicy as Graphics.CGfxRenderPolicy;
                            rpolicy.DPLimitter = value;
                        }
                        break;
                    case EDebugTarget.Game:
                        {
                            var rpolicy = CEngine.Instance.GameInstance.RenderPolicy;
                            rpolicy.DPLimitter = value;
                        }
                        break;
                }
            }
        }

        public CPass CurrentPass
        {
            get
            {
                switch (Target)
                {
                    case EDebugTarget.Editor:
                        {
                            var rpolicy = CEngine.Instance.GameEditorInstance.RenderPolicy as Graphics.CGfxRenderPolicy;
                            return rpolicy.LatestPass;
                        }
                    case EDebugTarget.Game:
                        {
                            var rpolicy = CEngine.Instance.GameInstance.RenderPolicy;
                            return rpolicy.LatestPass;
                        }
                }
                return null;
            }
        }

        public int StartDrawCallStep()
        {
            EngineNS.Graphics.CGfxRenderPolicy.GraphicsDebug = true;
            var editor = (CEditorInstance)CEngine.Instance.GameEditorInstance;
            var camera = editor.GetMainViewCamera();
            if (camera != null)
            {
                if(camera.LockCulling==false)
                    camera.LockCulling = true;
                if(CEngine.Instance.GameInstance!=null)
                    CEngine.Instance.GameInstance.GameCamera.LockCulling = true;
            }
                
            switch (Target)
            {
                case EDebugTarget.Editor:
                    {
                        var rpolicy = CEngine.Instance.GameEditorInstance.RenderPolicy as Graphics.CGfxRenderPolicy;
                        if(rpolicy.DPLimitter==int.MaxValue)
                            return rpolicy.DrawCall;
                        rpolicy.DPLimitter = int.MaxValue;
                    }
                    break;
                case EDebugTarget.Game:
                    {
                        if(CEngine.Instance.GameInstance==null)
                        {
                            Target = EDebugTarget.Editor;
                            return int.MaxValue;
                        }
                        var rpolicy = CEngine.Instance.GameInstance.RenderPolicy;
                        if (rpolicy.DPLimitter == int.MaxValue)
                            return rpolicy.DrawCall;
                        rpolicy.DPLimitter = int.MaxValue;
                    }
                    break;
                default:
                    break;
            }
            return int.MaxValue;
        }
        public void EndDrawCallStep()
        {
            EngineNS.Graphics.CGfxRenderPolicy.GraphicsDebug = false;
            var editor = (CEditorInstance)CEngine.Instance.GameEditorInstance;
            var camera = editor.GetMainViewCamera();
            if (camera != null)
            {
                camera.LockCulling = false;
                if (CEngine.Instance.GameInstance != null)
                    CEngine.Instance.GameInstance.GameCamera.LockCulling = false;
            }
            editor.RenderPolicy.ClearLatestPass();
            switch (Target)
            {
                case EDebugTarget.Editor:
                    {
                        var rpolicy = CEngine.Instance.GameEditorInstance.RenderPolicy as Graphics.CGfxRenderPolicy;
                        rpolicy.DPLimitter = int.MaxValue;
                    }
                    break;
                case EDebugTarget.Game:
                    {
                        if (CEngine.Instance.GameInstance != null)
                        {
                            var rpolicy = CEngine.Instance.GameInstance.RenderPolicy;
                            rpolicy.DPLimitter = int.MaxValue;
                            rpolicy.ClearLatestPass();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        Editor.GraphicsDebugger mGraphicDebugger = new Editor.GraphicsDebugger();
        public Editor.GraphicsDebugger GraphicDebugger
        {
            get => mGraphicDebugger;
        }
    }
}
