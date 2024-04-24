using EngineNS.Bricks.CodeBuilder;
using EngineNS.Rtti;
using EngineNS.Thread.Async;
using EngineNS.UI;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Editor
{
    public class EditorOnlyData : IO.BaseSerializer
    {
    }

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
        TtCanvasBrush mGridBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/grid_matins.uminst:Engine",
        };
        Vector2 mGridUVMin = Vector2.Zero;
        public Vector2 GridUVMin
        {
            get => mGridUVMin;
            set
            {
                var anyValue = new Support.UAnyValue();
                anyValue.SetVector2(value);
                mGridBrush.SetValue("UVMin", in anyValue);
                mGridUVMin = value;
            }
        }
        Vector2 mGridUVMax = Vector2.One;
        public Vector2 GridUVMax
        {
            get => mGridUVMax;
            set
            {
                var anyValue = new Support.UAnyValue();
                anyValue.SetVector2(value);
                mGridBrush.SetValue("UVMax", in anyValue);
                mGridUVMax = value;
            }
        }

        public TtCanvasBrush DrawBrush => mDrawBrush;
        TtUIEditor mHostEditor = null;
        public TtUIEditor HostEditor => mHostEditor;
        float mViewScale = 1.0f;
        SizeF mWindowSize;
        public override SizeF WindowSize
        {
            get => mWindowSize;
            set
            {
                if (mWindowSize.Equals(in value))
                    return;

                if (IsScreenSpace)
                {
                    mWindowSize = value;

                }
                else
                {
                    mWindowSize = value;
                    SizeF tagDesignSize;
                    mDPIScale = UEngine.Instance.UIManager.Config.GetDPIScaleAndDesignSize(mWindowSize.Width, mWindowSize.Height, out tagDesignSize);
                    var newRect = new RectangleF(0, 0, tagDesignSize.Width, tagDesignSize.Height);
                    SetDesignRect(in newRect, true);
                    var childrenCount = VisualTreeHelper.GetChildrenCount(this);
                    for (int i = 0; i < childrenCount; i++)
                    {
                        var child = VisualTreeHelper.GetChild(this, i);
                        child?.UpdateLayout();
                    }
                }
            }
        }
        FTransform mScreenSpaceTransform;
        public override ref FTransform AbsRenderTransform
        {
            get
            {
                if(IsScreenSpace)
                {
                    return ref mScreenSpaceTransform;
                }
                else
                {
                    return ref base.AbsRenderTransform;
                }
            }
        }

        //EditorOnlyData mEditorOnlyData;
        //public EditorOnlyData EditorOnlyData
        //{
        //    get
        //    {
        //        if (mEditorOnlyData == null)
        //            mEditorOnlyData = new EditorOnlyData();
        //        return mEditorOnlyData;
        //    }
        //}

        public EditorUIHost(TtUIEditor editor)
        {
            mHostEditor = editor;
            GridUVMin = Vector2.Zero;
            GridUVMax = Vector2.One;
        }

        //public void SaveEditorOnlyData(RName asset)
        //{
        //    var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(EditorOnlyData.GetType());
        //    using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
        //    {
        //        using (var attr = xnd.NewAttribute("EditorOnlyData", 0, 0))
        //        {
        //            using (var ar = attr.GetWriter(512))
        //            {
        //                ar.Write(EditorOnlyData);
        //            }
        //            xnd.RootNode.AddAttribute(attr);
        //        }
        //        var fileName = asset.Address + "/EditorOnlyData.dat";
        //        xnd.SaveXnd(fileName);
        //        UEngine.Instance.SourceControlModule.AddFile(fileName);
        //    }
        //}
        //public void LoadEditorOnlyData(RName asset)
        //{
        //    using (var xnd = IO.TtXndHolder.LoadXnd(asset.Address + "/EditorOnlyData.dat"))
        //    {
        //        if (xnd == null)
        //            return;

        //        var attr = xnd.RootNode.TryGetAttribute("EditorOnlyData");
        //        if (attr.NativePointer == IntPtr.Zero)
        //            return;

        //        using (var ar = attr.GetReader(null))
        //        {
        //            try
        //            {
        //                ar.ReadObject(out mEditorOnlyData);
        //            }
        //            catch (Exception ex)
        //            {
        //                Profiler.Log.WriteException(ex);
        //            }
        //        }
        //    }
        //}

        //async TtTask Init2DGrid()
        //{
        //    var material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/gridline.material", RName.ERNameType.Engine));
        //    var materialInstance = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(material);
        //    materialInstance.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_PostTranslucent;
        //    unsafe
        //    {
        //        var rsState = materialInstance.Rasterizer;
        //        rsState.CullMode = NxRHI.ECullMode.CMD_NONE;
        //        materialInstance.Rasterizer = rsState;

        //        var blend0 = materialInstance.Blend;
        //        blend0.RenderTarget[0].BlendEnable = 1;
        //        materialInstance.Blend = blend0;

        //        var dsState = materialInstance.DepthStencil;
        //        dsState.DepthWriteMask = 0;
        //        materialInstance.DepthStencil = dsState;
        //    }
        //    if (materialInstance.UsedSamplerStates.Count > 0)
        //    {
        //        var samp = materialInstance.UsedSamplerStates[0].Value;
        //        //samp.Filter = NxRHI.ESamplerFilter.SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
        //        samp.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
        //        samp.m_MaxLOD = float.MaxValue;

        //        materialInstance.UsedSamplerStates[0].Value = samp;
        //    }
        //    var gridColor = materialInstance.FindVar("GridColor");
        //    if (gridColor != null)
        //    {
        //        gridColor.SetValue(new Vector4(0.6f, 0.6f, 0.6f, 1));
        //    }
        //}

        protected override void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {
            var canvas = mCanvas.Background; //batch.Middleground;

            canvas.PushBrush(mGridBrush);

            canvas.PopBrush();

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
