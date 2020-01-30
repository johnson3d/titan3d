using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem
{
    public class UIRender
    {
        public virtual void Cleanup()
        {

        }

        public void DrawImage(CCommandList cmd, Graphics.View.CGfxScreenView view, Graphics.Mesh.CGfxImage2D image, int zOrder)
        {
            var pass = image.GetPass();
            pass.ViewPort = view.Viewport;
            pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
            pass.ShadingEnv.BindResources(image.Mesh, pass);
            cmd.PushPass(pass);
        }
    }

    public class UIRenderProcesser : CEngineAutoMemberProcessor
    {
        public override async Task<object> CreateObject()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new UIRender();
        }
        public override void Tick(object obj)
        {
        }
        public override void Cleanup(object obj)
        {
            var ur = obj as UIRender;
            ur?.Cleanup();
        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(EngineNS.UISystem.UIRenderProcesser))]
        public EngineNS.UISystem.UIRender UIRender
        {
            get;
            set;
        }
    }
}
