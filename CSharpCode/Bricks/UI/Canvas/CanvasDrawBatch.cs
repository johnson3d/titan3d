using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace EngineNS.UI.Canvas
{
    public class TtCanvasDrawBatch : AuxPtrType<EngineNS.Canvas.FCanvasDrawBatch>
    {
        public TtCanvasDrawCmdList Backgroud { get; set; }
        public TtCanvasDrawCmdList Middleground { get; set; }
        public TtCanvasDrawCmdList Foregroud { get; set; }

        public List<TtUIElement> UIElements = new List<TtUIElement>();

        public TtCanvasDrawBatch()
        {
            mCoreObject = EngineNS.Canvas.FCanvasDrawBatch.CreateInstance();
            Backgroud = new TtCanvasDrawCmdList(mCoreObject.GetBackgroud());
            Middleground = new TtCanvasDrawCmdList(mCoreObject.GetMiddleground());
            Foregroud = new TtCanvasDrawCmdList(mCoreObject.GetForegroud());
        }

        public TtCanvasDrawBatch(EngineNS.Canvas.FCanvasDrawBatch self)
        {
            mCoreObject = self;

            Backgroud = new TtCanvasDrawCmdList(mCoreObject.GetBackgroud());
            Middleground = new TtCanvasDrawCmdList(mCoreObject.GetMiddleground());
            Foregroud = new TtCanvasDrawCmdList(mCoreObject.GetForegroud());
        }

        public void SetClientClip(float x, float y)
        {
            mCoreObject.SetClientClip(x, y);
        }

        public void SetPosition(float x, float y)
        {
            mCoreObject.SetPosition(x, y);
        }

        //public void SetTransformMatrix(UInt16 index, in Matrix matrix)
        //{
        //    mCoreObject.SetTransformMatrix(index, in matrix);
        //}

        public void Reset()
        {
            mCoreObject.Reset();
        }

    }
}
