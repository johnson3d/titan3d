using EngineNS.UI;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Editor
{
    public partial class EditorUIHost : TtUIHost
    {
        public float PathWidth = 10;
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();
        TtCanvasBrush mDrawBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/uimat_inst_default.uminst:Engine",
            Color = Color.White,
        };
        public TtCanvasBrush DrawBrush => mDrawBrush;
        TtUIEditor mHostEditor = null;
        public TtUIEditor HostEditor => mHostEditor;
        public EditorUIHost(TtUIEditor editor)
        {
            mHostEditor = editor;
        }

        protected override void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {
            var canvas = mCanvas.Background; //batch.Middleground;

            if (mEdgePath == null)
            {
                mEdgePath = new TtPath();
            }
            mEdgePathStyle.PathWidth = PathWidth;
            mEdgePathStyle.FillArea = false;
            mEdgePathStyle.StrokeMode = EngineNS.Canvas.EPathStrokeMode.Stroke_Dash;
            canvas.PushPathStyle(mEdgePathStyle);
            canvas.PushTransformIndex(mTransformIndex);
            canvas.PushBrush(mDrawBrush);
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

            mEdgePath.EndPath(canvas);
            canvas.PopBrush();
            canvas.PopTransformIndex();
            canvas.PopPathStyle();

            mDrawBrush.IsDirty = true;
        }

        public override bool CanAddChild(Rtti.UTypeDesc childType)
        {
            if (Children.Count > 0)
                return false;
            return true;
        }
    }

    public partial class SelectedDecorator : TtUIHost
    {
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();
        TtCanvasBrush mDrawBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/uimat_inst_default.uminst:Engine",
            Color = Color.White,
        };
        public TtCanvasBrush DrawBrush => mDrawBrush;

        public List<TtUIElement> SelectedElements;

        protected override void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {
            var canvas = mCanvas.Background;

            if(mEdgePath == null)
            {
                mEdgePath = new TtPath();
            }
            mEdgePathStyle.PathWidth = 2;
            mEdgePathStyle.FillArea = false;
            mEdgePathStyle.StrokeMode = EngineNS.Canvas.EPathStrokeMode.Stroke_Dash;
            canvas.PushPathStyle(mEdgePathStyle);
            canvas.PushTransformIndex(mTransformIndex);
            canvas.PushBrush(mDrawBrush);

            for(int i=0; i<SelectedElements.Count; i++)
            {
                var element = SelectedElements[i];
                canvas.PushMatrix(in element.RootUIHost.TransformedElements[element.TransformIndex].Matrix);
                mEdgePath.BeginPath();

                var rect = element.PreviousArrangeRect;
                var start = new Vector2(rect.Left, rect.Top);
                mEdgePath.MoveTo(in start);
                var tr = new Vector2(rect.Right, rect.Top);
                mEdgePath.LineTo(in tr);
                var br = new Vector2(rect.Right, rect.Bottom);
                mEdgePath.LineTo(in br);
                var bl = new Vector2(rect.Left, rect.Bottom);
                mEdgePath.LineTo(in bl);
                mEdgePath.LineTo(in start);

                mEdgePath.EndPath(canvas);
                canvas.PopMatrix();
            }

            canvas.PopBrush();
            canvas.PopTransformIndex();
            canvas.PopPathStyle();

            mDrawBrush.IsDirty = true;
        }
    }
}
