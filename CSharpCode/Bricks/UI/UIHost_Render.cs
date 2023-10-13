using EngineNS.GamePlay;
using EngineNS.GamePlay.Camera;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIHost
    {
        public void OnDispose()
        {
            CoreSDK.DisposeObject(ref mDrawMesh);
        }

        Canvas.TtCanvas mCanvas = new Canvas.TtCanvas();
        Graphics.Mesh.UMeshDataProvider mMeshProvider;
        Graphics.Mesh.UMeshPrimitives mMesh = null;
        Canvas.TtCanvasDrawBatch mDrawBatch = null;
        public Graphics.Pipeline.UCamera RenderCamera;
        public BoundingBox BoundingBox
        {
            get
            {
                if (mMesh != null)
                    return mMesh.mCoreObject.mAABB;
                return BoundingBox.Empty;
            }
        }
        List<TtUIElement> mUIElementWithTransforms;

        [Browsable(false)]
        public override bool MeshDirty
        {
            get => ReadInternalFlag(eInternalFlags.MeshDirty);
            set => WriteInternalFlag(eInternalFlags.MeshDirty, value);
        }
        Graphics.Mesh.UMesh mDrawMesh;
        public Graphics.Mesh.UMesh DrawMesh => mDrawMesh;
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();

        internal class TransformedUIElementData
        {
            TtUIElement mELement;
            public TtUIElement ELement
            {
                get => mELement;
                set
                {
                    mELement = value;
                    UpdateMatrix(true);
                }
            }
            public Matrix Matrix;
            public UInt16 ParentTransformIdx;

            public void UpdateMatrix(bool bForce = false)
            {
                if (ELement.RenderTransformDirty || bForce)
                {
                    if(ELement is TtUIHost)
                    {
                        Matrix = ELement.AbsRenderTransform.ToMatrixWithScale(ELement.RootUIHost.SceneNode.GetWorld().CameraOffset);
                    }
                    else
                    {
                        var parentData = ELement.RootUIHost.TransformedElements[ParentTransformIdx];
                        parentData.UpdateMatrix();

                        var offset = ELement.RootUIHost.SceneNode.GetWorld().CameraOffset;
                        var absTrans = ELement.RenderTransform;
                        Vector3 localOffset = new Vector3(
                            ELement.DesignRect.Width * ELement.RenderTransformCenter.X, 
                            ELement.RootUIHost.WindowSize.Height - ELement.DesignRect.Height * ELement.RenderTransformCenter.Y, 0.0f);
                        Vector3 pos = new Vector3(absTrans.Position - offset) + localOffset;

                        var invTransMat = Matrix.Translate(-localOffset);
                        var transMat = Matrix.Translate(pos.X, pos.Y, pos.Z);
                        var scaleMat = Matrix.Scaling(absTrans.Scale);
                        var rotMat = Matrix.RotationQuaternion(absTrans.Quat);
                        Matrix = invTransMat * scaleMat * rotMat * transMat * parentData.Matrix;
                    }

                    ELement.RenderTransformDirty = false;
                }
            }
        }
        internal List<TransformedUIElementData> TransformedElements = new List<TransformedUIElementData>();
        UInt16 mTransformedUIElementCount = 0;
        public UInt16 TransformedUIElementCount => mTransformedUIElementCount;
        public UInt16 AddTransformedUIElement(TtUIElement element, in UInt16 parentTransformIdx)
        {
            var idx = parentTransformIdx + 1;
            if(TransformedElements.Count > mTransformedUIElementCount)
            {
                TransformedElements[mTransformedUIElementCount].ELement = element;
            }
            else
            {
                var data = new TtUIHost.TransformedUIElementData()
                {
                    ELement = element,
                    ParentTransformIdx = (UInt16)(parentTransformIdx - mTransformIndex),
                };
                data.UpdateMatrix(true);
                TransformedElements.Add(data);
            }
            var retValue = (UInt16)(mTransformedUIElementCount + mTransformIndex);
            mTransformedUIElementCount++;
            return retValue;
        }
        public override UInt16 UpdateTransformIndex(UInt16 parentTransformIdx)
        {
            mTransformedUIElementCount = 0;
            mTransformIndex = parentTransformIdx;
            AddTransformedUIElement(this, parentTransformIdx);

            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child?.UpdateTransformIndex(mTransformIndex);
            }

            return mTransformIndex;
        }

        public async Thread.Async.TtTask<Graphics.Mesh.UMesh> BuildMesh()
        {
            if (!MeshDirty)
                return mDrawMesh;

            if (!IsReadyToDraw())
                return mDrawMesh;

            MeshDirty = false;

            if (mMeshProvider == null)
            {
                mMeshProvider = new Graphics.Mesh.UMeshDataProvider();
                mMesh = new Graphics.Mesh.UMeshPrimitives();
                mMesh.Init("UICookedMesh", 0);
                var builder = mMeshProvider.mCoreObject;
                uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_UV) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_SkinIndex));
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

            //mEdgePath.S_CCW_ArcTo(new Vector2(150.0f, 300.0f), 500.0f);
            //mEdgePath.L_CCW_ArcTo(new Vector2(150.0f, 150.0f), 1.0f);

            mEdgePath.EndPath(canvasBackground);
            canvasBackground.PopPathStyle();

            //var font = UEngine.Instance.FontModule.FontManager.GetFontSDF(RName.GetRName("fonts/simli.fontsdf", RName.ERNameType.Engine), fontSize: 64, 1024, 1024);
            //canvasForeground.PushFont(font);
            //canvasForeground.AddText("abc中国1A，!,", -45, -35, Color4f.FromABGR(Color.LightPink));
            //canvasForeground.PopFont();

            //assistBatch.Backgroud.AddRect(Vector2.Zero, new Vector2(winSize.Width, winSize.Height), 10, Color.White, Canvas.CanvasDrawRectType.Line, ref subCmd);
            //mCanvas.PushBatch(assistBatch);
            if(mDrawBatch == null)
                mDrawBatch = new Canvas.TtCanvasDrawBatch();

            UpdateTransformIndex(0);

            mDrawBatch.Reset();
            var clip = DesignClipRect;
            mDrawBatch.SetPosition(clip.Left, clip.Top);
            mDrawBatch.SetClientClip(clip.Width, clip.Height);
            var count = VisualTreeHelper.GetChildrenCount(this);
            for(int i=0; i< count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child.DrawInternal(mCanvas, mDrawBatch);
            }
            mCanvas.PushBatch(mDrawBatch);

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
                if (brush.Name.StartWith("@Text:"))
                {
                    mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/font_sdf_0.uminst", RName.ERNameType.Engine));
                    var clr = mtl.FindVar("FontColor");
                    if (clr != null)
                    {
                        clr.SetValue(Color3f.FromColor(Color.DarkRed));
                        //mtl.UpdateUniformVars();
                    }
                }
                else if(brush.Name.StartWith("@MatInst:"))
                {
                    var name = brush.Name.c_str().Replace("@MatInst:", "");
                    if (string.IsNullOrEmpty(name) || "DefaultBrush" == name)
                    {
                        mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));
                    }
                    else
                    {
                        mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.ParseFrom(name));
                        if(mtl == null)
                            mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));
                    }
                }
                else
                    mtl = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));

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
                var ok = mDrawMesh.Initialize(mMesh, materials, Rtti.UTypeDescGetter<TtMdfUIMesh>.TypeDesc);
                var mdf = mDrawMesh.MdfQueue as TtMdfUIMesh;
                mdf.UIHost = this;
            }
            else
            {
                mDrawMesh.UpdateMesh(mMesh, materials);
            }

            return mDrawMesh;
        }

        public unsafe bool OnLineCheckTriangle(in Vector3 start, in Vector3 end, ref VHitResult result)
        {
            if (mMeshProvider == null)
                return false;

            fixed(Vector3* pStart = &start)
            fixed(Vector3* pEnd = &end)
            fixed(VHitResult* pResult = &result)
            {
                return mMeshProvider.mCoreObject.IntersectTriangle((Vector3*)0, pStart, pEnd, pResult) != -1;
            }
        }

        protected uint mCameralOffsetSerialId = 0;
        public void GatherVisibleMeshes(UWorld.UVisParameter param)
        {
            if (mDrawMesh == null)
                return;

            if(param.World.CameralOffsetSerialId != mCameralOffsetSerialId)
            {
                mCameralOffsetSerialId = param.World.CameralOffsetSerialId;
                mDrawMesh.UpdateCameraOffset(param.World);
            }

            param.VisibleMeshes.Add(mDrawMesh);
            if(param.VisibleNodes != null && SceneNode != null)
            {
                param.VisibleNodes.Add(SceneNode);
            }
        }

        public void OnHostNodeAbsTransformChanged(TtUINode hostNode, UWorld world)
        {
            if (mDrawMesh == null)
                return;

            mDrawMesh.SetWorldTransform(in hostNode.Placement.AbsTransform, world, false);
        }
    }
}
