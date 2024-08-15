using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    [Editor_UIControl("Controls.Image", "Image", "")]
    public partial class TtImage : TtUIElement
    {
        TtBrush mUIBrush;
        [BindProperty, Rtti.Meta]
        public TtBrush UIBrush
        {
            get => mUIBrush;
            set
            {
                OnValueChange(value, mUIBrush);
                mUIBrush.HostElement = this;
                mUIBrush = value;
            }
        }

        public TtImage()
        {
            mUIBrush = new TtBrush();
            mUIBrush.HostElement = this;
            _ = Initialize();
        }

        async Thread.Async.TtTask Initialize()
        {
            // test only ////////////////////////////////////
            //mDrawBrush.Name = "utest/ddd.uminst";
            //var texture = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/ground_01.srv"));
            //mDrawBrush.SetSrv(texture);
            /////////////////////////////////////////////////
        }

        public override bool IsReadyToDraw()
        {
            return mUIBrush.IsReadyToDraw();
        }
        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            //mBrush.Name = "utest/ddd.uminst";
            //var texture = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/groundsnow.srv"));
            //mBrush.mCoreObject.SetSrv(texture.mCoreObject);
            //batch.Middleground.PushBrush(mBrush);
            mUIBrush.Draw(this, in mDesignClipRect, in mCurFinalRect, batch);
        }
    }
}
