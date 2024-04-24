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
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.Unserializable)]
        public bool IsScreenSpace
        {
            get { return ReadFlag(ECoreFlags.IsScreenSpace); }
        }

        public void OnDispose()
        {
            CoreSDK.DisposeObject(ref mDrawMesh);
        }

        protected Canvas.TtCanvas mCanvas = new Canvas.TtCanvas();
        protected Graphics.Mesh.UMeshDataProvider mMeshProvider;
        protected Graphics.Mesh.UMeshPrimitives mMesh = null;
        protected Canvas.TtCanvasDrawBatch mDrawBatch = null;
        public Graphics.Pipeline.UCamera RenderCamera;
        public bool BoundingBoxDirty = true;
        struct AABBQueryData
        {
            public BoundingBox AABB;
        }
        AABBQueryData mBoundingBoxData = new AABBQueryData();
        bool CheckBoundingBox(TtUIElement element, ref AABBQueryData data)
        {
            if (!element.Is3D)
                return false;
            element.MergeAABB(ref data.AABB);
            return false;
        }
        public BoundingBox BoundingBox
        {
            get
            {
                if(BoundingBoxDirty)
                {
                    mBoundingBoxData.AABB = BoundingBox.Empty;
                    if (mMesh != null)
                        mBoundingBoxData.AABB = mMesh.mCoreObject.mAABB;

                    QueryElements(CheckBoundingBox, ref mBoundingBoxData);
                    BoundingBoxDirty = false;
                }

                return mBoundingBoxData.AABB;
            }
        }
        //List<TtUIElement> mUIElementWithTransforms;

        [Browsable(false)]
        public override bool MeshDirty
        {
            get => ReadInternalFlag(eInternalFlags.MeshDirty);
            set => WriteInternalFlag(eInternalFlags.MeshDirty, value);
        }
        Graphics.Mesh.TtMesh mDrawMesh;
        public Graphics.Mesh.TtMesh DrawMesh => mDrawMesh;

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
            Matrix mMatrix;
            public ref Matrix Matrix
            {
                get
                {
                    UpdateMatrix();
                    return ref mMatrix;
                }
            }

            Matrix mInvMatrix;
            public ref Matrix InvMatrix
            {
                get
                {
                    UpdateMatrix();
                    return ref mInvMatrix;
                }
            }
            public UInt16 ParentTransformIdx;

            public void SetMatrix(in Matrix mat)
            {
                mMatrix = mat;
                mInvMatrix = Matrix.Invert(in mMatrix);
            }
            public void UpdateMatrix(bool bForce = false)
            {
                if (ELement.RenderTransformDirty || bForce)
                {
                    if(ELement is TtUIHost)
                    {
                        var uiHost = ELement as TtUIHost;
                        if(uiHost.IsScreenSpace)
                        {
                            var camera = uiHost.RenderCamera;
                            float width, height;
                            if(camera.IsOrtho)
                            {
                                width = camera.Width;
                                height = camera.Height;
                            }
                            else
                            {
                                height = (float)(2 * camera.ZNear * Math.Tan(camera.Fov * 0.5f));
                                width = camera.Aspect * height;
                            }
                            var scale = (float)(height / uiHost.DesignRect.Height);
                            mMatrix = Matrix.Scaling(scale) * Matrix.Translate(-width * 0.5f, -height * 0.5f, camera.ZNear);
                            mInvMatrix = Matrix.Invert(in mMatrix);
                            uiHost.RenderTransformDirty = false;
                        }
                        else
                        {
                            if((uiHost.SceneNode != null) &&
                               (uiHost.SceneNode.GetWorld() != null))
                            {
                                mMatrix = uiHost.AbsRenderTransform.ToMatrixWithScale(uiHost.SceneNode.GetWorld().CameraOffset);
                                mInvMatrix = Matrix.Invert(in mMatrix);
                                uiHost.RenderTransformDirty = false;
                            }
                        }
                    }
                    else
                    {
                        var parentData = ELement.RootUIHost.TransformedElements[ParentTransformIdx];
                        parentData.UpdateMatrix();

                        var offset = ELement.RootUIHost.SceneNode.GetWorld().CameraOffset;
                        var absTrans = ELement.RenderTransform;
                        Vector3 localOffset = new Vector3(
                            ELement.DesignRect.Width * ELement.RenderTransformCenter.X, 
                            ELement.RootUIHost.DesignRect.Height - ELement.DesignRect.Height * ELement.RenderTransformCenter.Y, 0.0f);
                        Vector3 pos = new Vector3(absTrans.Position - offset) + localOffset;

                        var invTransMat = Matrix.Translate(-localOffset);
                        var transMat = Matrix.Translate(pos.X, pos.Y, pos.Z);
                        var scaleMat = Matrix.Scaling(absTrans.Scale);
                        var rotMat = Matrix.RotationQuaternion(absTrans.Quat);
                        mMatrix = invTransMat * scaleMat * rotMat * transMat * parentData.Matrix;
                        mInvMatrix = Matrix.Invert(in mMatrix);
                        ELement.RenderTransformDirty = false;
                    }
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

            foreach (var i in mPopupUIHost)
            {
                i.ElementUpdateTransformIndex(mTransformIndex);
            }
            return mTransformIndex;
        }

        protected virtual void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {

        }
        public async Thread.Async.TtTask<Graphics.Mesh.TtMesh> BuildMesh()
        {
            var mesh = await OnBuildMesh();
            foreach (var i in mPopupUIHost)
            {
                await i.BuildMesh();
            }
            return mesh;
        }
        protected async Thread.Async.TtTask<Graphics.Mesh.TtMesh> OnBuildMesh()
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
            //var winSize = WindowSize;
            mCanvas.SetClientClip(mDesignRect.Width, mDesignRect.Height);

            //var subCmd = new EngineNS.Canvas.FSubDrawCmd();

            //var canvasBackground = mCanvas.Background;
            //var canvasForeground = mCanvas.Foregroud;
            //var assistBatch = new Canvas.TtCanvasDrawBatch();
            //assistBatch.SetClientClip(winSize.Width, winSize.Height);

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
            CustomBuildMesh(mDrawBatch);

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
                        clr.SetValue(Color3f.FromColor(brush.Color));
                        //clr.SetValue(Color3f.FromColor(Color.DarkRed));
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

                //mtl.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_PostTranslucent;
                var raster = mtl.Rasterizer;
                raster.CullMode = NxRHI.ECullMode.CMD_NONE;
                mtl.Rasterizer = raster;
                var dsState = mtl.DepthStencil;
                dsState.DepthWriteMask = NxRHI.EDepthWriteMask.DSWM_ZERO;
                mtl.DepthStencil = dsState;
            }
            if(mDrawMesh == null)
            {
                mDrawMesh = new Graphics.Mesh.TtMesh();
                var ok = mDrawMesh.Initialize(mMesh,
                    materials,
                    Rtti.UTypeDescGetter<TtMdfUIMesh>.TypeDesc);
                var mdf = mDrawMesh.MdfQueue as TtMdfUIMesh;
                mdf.UIHost = this;
            }
            else
            {
                mDrawMesh.UpdateMesh(0, mMesh, materials);
            }

            BoundingBoxDirty = true;
            return mDrawMesh;
        }

        public unsafe bool OnLineCheckTriangle(in Vector3 start, in Vector3 end, ref VHitResult result)
        {
            for (int i = mPopupUIHost.Count - 1; i >= 0; i--)
            {
                if(mPopupUIHost[i].OnLineCheckTriangle(in start, in end, ref result) == true)
                {
                    return true;
                }
            }
            return OnMeshLineCheckTriangle(in start, in end, ref result);
        }
        protected unsafe bool OnMeshLineCheckTriangle(in Vector3 start, in Vector3 end, ref VHitResult result)
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
            if (IsScreenSpace)
                return;
            if (mDrawMesh == null)
                return;

            UpdateCameraOffset(param.World);

            param.AddVisibleMesh(mDrawMesh);
            if(param.VisibleNodes != null && SceneNode != null)
            {
                param.VisibleNodes.Add(SceneNode);
            }

            foreach (var i in mPopupUIHost)
            {
                i.GatherVisibleMeshes(param);
            }
        }

        public void UpdateCameraOffset(UWorld world)
        {
            if (world == null)
                return;
            if(world.CameralOffsetSerialId != mCameralOffsetSerialId)
            {
                mCameralOffsetSerialId = world.CameralOffsetSerialId;
                if (mDrawMesh != null)
                    mDrawMesh.UpdateCameraOffset(world);
            }
            foreach (var i in mPopupUIHost)
            {
                i.UpdateCameraOffset(world);
            }
        }

        public void OnHostNodeAbsTransformChanged(TtUINode hostNode, UWorld world)
        {
            if (mDrawMesh == null)
                return;

            mDrawMesh.SetWorldTransform(in hostNode.Placement.AbsTransform, world, false);
            foreach (var i in mPopupUIHost)
            {
                i.OnHostNodeAbsTransformChanged(hostNode, world);
            }
        }
    }
}
