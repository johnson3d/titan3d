using EngineNS.UI;
using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Editor
{
    public partial class EditorUIHost : TtUIHost
    {
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();

        protected override void CustomBuildMesh()
        {
            var canvasBackground = mCanvas.Background;

            if (mEdgePath == null)
            {
                mEdgePath = new TtPath();
            }
            mEdgePathStyle.PathWidth = 10;
            mEdgePathStyle.FillArea = false;
            mEdgePathStyle.StrokeMode = EngineNS.Canvas.EPathStrokeMode.Stroke_Dash;
            canvasBackground.PushPathStyle(mEdgePathStyle);
            mEdgePath.BeginPath();
            var start = new Vector2(mDesignRect.Left, mDesignRect.Top);
            mEdgePath.MoveTo(in start);
            var tr = new Vector2(mDesignRect.Right, mDesignRect.Top);
            mEdgePath.LineTo(in tr);
            var br = new Vector2(mDesignRect.Right, mDesignRect.Bottom);
            mEdgePath.LineTo(in br);
            var bl = new Vector2(mDesignRect.Left, mDesignRect.Bottom);
            mEdgePath.LineTo(in bl);
            mEdgePath.LineTo(in start);

            //mEdgePath.S_CCW_ArcTo(new Vector2(150.0f, 300.0f), 500.0f);
            //mEdgePath.L_CCW_ArcTo(new Vector2(150.0f, 150.0f), 1.0f);

            mEdgePath.EndPath(canvasBackground);
            canvasBackground.PopPathStyle();
        }
    }
}
