using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIHost
    {
        Canvas.TtCanvas mCanvas = new Canvas.TtCanvas();
        Graphics.Mesh.UMeshDataProvider mMeshProvider;
        Graphics.Mesh.UMeshPrimitives mMesh = null;
        List<Canvas.TtCanvasDrawBatch> mDrawBatchs = new List<Canvas.TtCanvasDrawBatch>();
        bool mMeshDirty = false;
        public bool MeshDirty
        {
            get => mMeshDirty;
            set => mMeshDirty = value;
        }
        Graphics.Mesh.UMesh mDrawMesh;
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();

        public async Thread.Async.TtTask<Graphics.Mesh.UMesh> BuildMesh()
        {
            if (!mMeshDirty)
                return mDrawMesh;
            mMeshDirty = false;

            if (mMeshProvider == null)
            {
                mMeshProvider = new Graphics.Mesh.UMeshDataProvider();
                mMesh = new Graphics.Mesh.UMeshPrimitives();
                mMesh.Init("UICookedMesh", 0);
                var builder = mMeshProvider.mCoreObject;
                uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_UV));
                builder.Init(streams, false, 0);
            }

            mCanvas.Reset();
            var winSize = WindowSize;
            mCanvas.SetClientClip(winSize.Width, winSize.Height);

            //var subCmd = new EngineNS.Canvas.FSubDrawCmd();

            var canvasBackground = mCanvas.Background;
            var canvasForeground = mCanvas.Foregroud;
            //var assistBatch = new Canvas.TtCanvasDrawBatch();
            //assistBatch.SetClientClip(winSize.Width, winSize.Height);

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
            mEdgePath.EndPath(canvasBackground);
            canvasBackground.PopPathStyle();

            var font = UEngine.Instance.FontModule.FontManager.GetFontSDF(RName.GetRName("fonts/simli.fontsdf", RName.ERNameType.Engine), fontSize: 64, 1024, 1024);
            canvasForeground.PushFont(font);
            canvasForeground.AddText("abc中国1A，!,", -45, -35, Color4f.FromABGR(Color.LightPink));
            canvasForeground.PopFont();

            //assistBatch.Backgroud.AddRect(Vector2.Zero, new Vector2(winSize.Width, winSize.Height), 10, Color.White, Canvas.CanvasDrawRectType.Line, ref subCmd);
            //mCanvas.PushBatch(assistBatch);

            for(int i=0; i<Children.Count; i++)
            {
                Canvas.TtCanvasDrawBatch drawBatch;
                if (mDrawBatchs.Count <= i)
                {
                    drawBatch = new Canvas.TtCanvasDrawBatch();
                    mDrawBatchs.Add(drawBatch);
                }
                else
                    drawBatch = mDrawBatchs[i];
                drawBatch.Reset();
                var clip = Children[i].DesignClipRect;
                drawBatch.SetPosition(clip.Left, clip.Top);
                drawBatch.SetClientClip(clip.Width, clip.Height);
                Children[i].Draw(mCanvas, drawBatch);
                mCanvas.PushBatch(drawBatch);
            }

            mCanvas.BuildMesh(mMeshProvider);
            mMeshProvider.ToMesh(mMesh);
            mMesh.AssetName = RName.GetRName("@UI");
            var materials = ListExtra.CreateList<Graphics.Pipeline.Shader.UMaterial>((int)mMesh.NumAtom);
            for (int i = 0; i < materials.Count; i++)
            {
                Graphics.Pipeline.Shader.UMaterial mtl = null;
                EngineNS.Canvas.FDrawCmd cmd = new EngineNS.Canvas.FDrawCmd();
                cmd.NativePointer = mMeshProvider.GetAtomExtData((uint)i).NativePointer;
                var brush = cmd.GetBrush();
                if (brush.Name == VNameString.FromString("Text"))
                {
                    mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/font_sdf_0.uminst", RName.ERNameType.Engine));
                    var clr = mtl.FindVar("FontColor");
                    if (clr != null)
                    {
                        clr.SetValue(Color3f.FromColor(Color.DarkRed));
                        //mtl.UpdateUniformVars();
                    }
                }
                else
                {
                    var name = brush.Name.c_str();
                    if ("DefaultBrush" == name)
                    {
                        mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));
                    }
                    else
                    {
                        mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName(name));
                    }
                }
                materials[i] = mtl;

                mtl.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_PostTranslucent;
                var raster = mtl.Rasterizer;
                raster.CullMode = NxRHI.ECullMode.CMD_NONE;
                mtl.Rasterizer = raster;
                var dsState = mtl.DepthStencil;
                dsState.DepthWriteMask = NxRHI.EDepthWriteMask.DSWM_ZERO;
                mtl.DepthStencil = dsState;
            }
            if(mDrawMesh == null)
            {
                mDrawMesh = new Graphics.Mesh.UMesh();
                var ok = mDrawMesh.Initialize(mMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                var mdf = mDrawMesh.MdfQueue as Graphics.Mesh.UMdfStaticMesh;
                mdf.OnDrawCallCallback = static (shadingType, drawcall, policy, mDrawMesh, atom) =>
                {
                    EngineNS.Canvas.FDrawCmd cmd = new EngineNS.Canvas.FDrawCmd();
                    cmd.NativePointer = mDrawMesh.MaterialMesh.Mesh.mCoreObject.GetAtomExtData((uint)atom).NativePointer;
                    var brush = cmd.GetBrush();
                    if (brush.Name == VNameString.FromString("Text"))
                    {
                        var srv = cmd.GetBrush().GetSrv();
                        if (srv.IsValidPointer)
                        {
                            drawcall.mCoreObject.BindResource(VNameString.FromString("FontTexture"), srv.NativeSuper);
                        }
                    }
                    else if (brush.Name == VNameString.FromString("utest/ddd.uminst"))
                    {
                        var srv = cmd.GetBrush().GetSrv();
                        if (srv.IsValidPointer)
                        {
                            drawcall.mCoreObject.BindResource(VNameString.FromString("Texture_3680116513"), srv.NativeSuper);
                        }
                    }
                };
            }
            else
            {
                mDrawMesh.UpdateMesh(mMesh, materials);
            }

            return mDrawMesh;
        }
    }
}
