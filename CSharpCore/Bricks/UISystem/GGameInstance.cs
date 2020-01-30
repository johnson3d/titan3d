using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay
{
    public class GameUIContext
    {
        public UISystem.UIHost UIHost
        {
            get
            {
                return CEngine.Instance.GameInstance.RenderPolicy.BaseSceneView?.UIHost;
            }
        }
    }
    public class UIContextProcesser : CEngineAutoMemberProcessor
    {
        public override async Task<object> CreateObject()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var context = new GameUIContext();
            return context;
        }
        public override async Task OnGameInit(object obj, uint width, uint height)
        {
            await base.OnGameInit(obj, width, height);
        }
        public override void Cleanup(object obj)
        {
            var context = obj as GameUIContext;
            context.UIHost.Cleanup();
            context.UIHost.ClearChildren();
            base.Cleanup(obj);
        }
    }

    public partial class GGameInstance
    {
        [CEngineAutoMemberAttribute(typeof(UIContextProcesser))]
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public GameUIContext UIContext
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public UISystem.UIHost UIHost
        {
            get => UIContext.UIHost;
        }

        protected void OnDrawUI(CCommandList cmd, Graphics.View.CGfxScreenView view)
        {
            if(UIHost != null)
                UIHost.Draw(CEngine.Instance.RenderContext, cmd, view);
        }

        partial void WindowsSizeChanged_UIProcess(UInt32 width, UInt32 height)
        {
            if(UIHost != null)
                UIHost.WindowSize = new SizeF(width, height);
        }
    }
}
