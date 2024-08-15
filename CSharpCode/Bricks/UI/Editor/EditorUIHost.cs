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
            Color = Color4b.White,
        };
        TtCanvasBrush mGridBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/grid_matins.uminst:Engine",
        };

        Vector2 mSnapTile = Vector2.One;
        public Vector2 SnapTile
        {
            get => mSnapTile;
            set
            {
                mSnapTile = value;
                var anyValue = new Support.UAnyValue();
                anyValue.SetValue(value);
                mGridBrush.SetValue("SnapTile", in anyValue);
            }
        }
        void CalculateSnapTile()
        {
            SnapTile = new Vector2(WindowSize.Width / GridSize.X * mViewScale, WindowSize.Height / GridSize.Y * mViewScale);
        }

        Vector2 mUVOffset = Vector2.Zero;
        public Vector2 UVOffset
        {
            get => mUVOffset;
            set
            {
                var anyValue = new Support.UAnyValue();
                anyValue.SetValue(value);
                mGridBrush.SetValue("UVOffset", in anyValue);
                mUVOffset = value;
            }
        }
        Vector2 mGridSize = new Vector2(500, 500);
        public Vector2 GridSize
        {
            get => mGridSize;
            set
            {
                mGridSize = value;
                CalculateSnapTile();
            }
        }
        float mViewScale = 1.0f;
        public float ViewScale
        {
            get => mViewScale;
            set
            {
                mViewScale = value;
                CalculateSnapTile();
            }
        }

        public TtCanvasBrush DrawBrush => mDrawBrush;
        TtUIEditor mHostEditor = null;
        public TtUIEditor HostEditor => mHostEditor;
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
                    CalculateSnapTile();
                    MeshDirty = true;
                }
                else
                {
                    mWindowSize = value;
                    DesignWindowSize = value;
                }
            }
        }
        SizeF mDesignWindowSize;
        public SizeF DesignWindowSize
        {
            get => mDesignWindowSize;
            set
            {
                if (mDesignWindowSize.Equals(in value))
                    return;

                mDesignWindowSize = value;
                SizeF tagDesignSize;
                mDPIScale = TtEngine.Instance.UIManager.Config.GetDPIScaleAndDesignSize(mDesignWindowSize.Width, mDesignWindowSize.Height, out tagDesignSize);
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
            DesignRect = TtEngine.Instance.UIManager.Config.DefaultDesignRect;
            DesignWindowSize = TtEngine.Instance.UIManager.Config.DefaultDesignRect.Size;
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
        //        TtEngine.Instance.SourceControlModule.AddFile(fileName);
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
        //    var material = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/gridline.material", RName.ERNameType.Engine));
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

        protected override void ResetCanvas()
        {
            mCanvas.Reset();
            if (IsScreenSpace)
                mCanvas.SetClientClip(mWindowSize.Width, mWindowSize.Height);
            else
                mCanvas.SetClientClip(mDesignClipRect.Width, mDesignClipRect.Height);
        }

        EngineNS.Canvas.FSubDrawCmd mDrawCmd = new EngineNS.Canvas.FSubDrawCmd();
        protected override void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {
            var canvas = mCanvas.Background; //batch.Middleground;

            if(IsScreenSpace)
            {
                canvas.PushBrush(mGridBrush);
                var winRect = new RectangleF(0, 0, WindowSize.Width, WindowSize.Height);
                canvas.AddRectFill(in winRect, Vector4.Zero, Color4b.White, ref mDrawCmd);
                canvas.PopBrush();
                mGridBrush.IsDirty = true;
            }

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

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            return mDesignWindowSize;
        }
    }

    public partial class SelectedDecorator : TtUIHost
    {
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();
        TtCanvasBrush mDrawBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/uimat_inst_default.uminst:Engine",
            Color = Color4b.White,
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
