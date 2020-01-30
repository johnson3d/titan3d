using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Graphics.View;

namespace EngineNS.Graphics
{
    public partial class CGfxCamera : AuxCoreObject<CGfxCamera.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        public float mShadowDistance = 40.0f;
        public float mDefaultFoV = MathHelper.V_PI / 2.5f;

        public class CFrustum
        {
            public enum CONTAIN_TYPE
            {
                CONTAIN_TEST_NOTIMPLEMENT = -2,
                CONTAIN_TEST_INNER = 1,
                CONTAIN_TEST_OUTER = -1,
                CONTAIN_TEST_REFER = 0,
            };
            /*
            frustum plane
	            |--1--|
	            4     2
	            |--3--|
            */
            public enum EFrustumPlane : int
            {
                TOP = 0,
                RIGHT,
                BOTTOM,
                LEFT,
                NEAR,
                FAR,
                NUMBER
            };
            private IntPtr CoreObject;
            public CFrustum(CGfxCamera host, IntPtr self)
            {
                mHost = host;
                CoreObject = self;
            }
            private bool NeedDeleteNative = false;
            public CFrustum(CGfxCamera host)
            {
                mHost = host;
                CoreObject = SDK_v3dxFrustum_NewFrustum();
                NeedDeleteNative = true;
            }
            ~CFrustum()
            {
                if(NeedDeleteNative)
                {
                    if (CoreObject != IntPtr.Zero)
                    {
                        SDK_v3dxFrustum_DeleteFrustum(CoreObject);
                        CoreObject = IntPtr.Zero;
                    }
                }
            }
            private CGfxCamera mHost;
            public CGfxCamera Host
            {
                get { return mHost; }
            }
            public Vector3 TipPos
            {
                get
                {
                    return SDK_v3dxFrustum_GetTipPos(CoreObject);
                }
            }
            public Vector3[] FrustumVectors
            {
                get
                {
                    unsafe
                    {
                        var vecs = stackalloc Vector3[8];
                        SDK_v3dxFrustum_GetVectors(CoreObject, vecs);
                        var retVec = new Vector3[8];
                        for(int i=0; i<8; i++)
                        {
                            retVec[i] = vecs[i];
                        }
                        return retVec;
                    }
                }
            }
            public void GetBoundBox(ref BoundingBox box)
            {
                unsafe
                {
                    fixed(BoundingBox* pBox = &box)
                    {
                        var vecs = stackalloc Vector3[8];
                        SDK_v3dxFrustum_GetVectors(CoreObject, vecs);
                        box.InitEmptyBox();
                        for (int i = 0; i < 8; i++)
                        {
                            box.Merge(ref vecs[i]);
                        }
                    }
                }
            }
            public unsafe void GetPlanes(Plane* pPlanes)
            {
                SDK_v3dxFrustum_GetPlanes(CoreObject, pPlanes);
            }
            public void CopyFrustum(CFrustum dest)
            {
                SDK_v3dxFrustum_CopyFrustum(CoreObject, dest.CoreObject);
            }
            public CONTAIN_TYPE WhichContainType(ref BoundingBox box, bool testInner)
            {
                unsafe
                {
                    fixed (BoundingBox* ptr = &box)
                    {
                        return SDK_v3dxFrustum_WhichContainTypeFast(CoreObject, ptr, testInner ? 1 : 0);
                    }
                }
            }
            public CONTAIN_TYPE WhichContainTypeWithTransform(ref BoundingBox box, ref Matrix matrix, bool testInner)
            {
                unsafe
                {
                    fixed (BoundingBox* ptr = &box)
                    fixed (Matrix* pMatrix = &matrix)
                    {
                        return SDK_v3dxFrustum_WhichContainTypeFastWithTransform(CoreObject, ptr, pMatrix, testInner?1:0);
                    }
                }
            }
            #region SDK
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe CONTAIN_TYPE SDK_v3dxFrustum_WhichContainTypeFast(IntPtr frustum, BoundingBox* box, int testInner);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe CONTAIN_TYPE SDK_v3dxFrustum_WhichContainTypeFastWithTransform(IntPtr frustum, BoundingBox* box, Matrix* matrix, int testInner);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe void SDK_v3dxFrustum_FromInvViewMatrix(IntPtr frustum, Matrix* matrix);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static IntPtr SDK_v3dxFrustum_NewFrustum();
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static void SDK_v3dxFrustum_DeleteFrustum(IntPtr frustum);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe void SDK_v3dxFrustum_CopyFrustum(IntPtr frustum, IntPtr dest);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe Vector3 SDK_v3dxFrustum_GetTipPos(IntPtr frustum);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe void SDK_v3dxFrustum_GetVectors(IntPtr frustum, EngineNS.Vector3* vectors);
            [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public extern static unsafe void SDK_v3dxFrustum_GetPlanes(IntPtr frustum, EngineNS.Plane* planes);
            #endregion
        }

        public CGfxCamera()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxCamera");
            mFrustum = new CFrustum(this, SDK_GfxCamera_GetFrustum(mCoreObject));
            mCullingFrustum = new CFrustum(this);

            unsafe
            {
                mCameraData = new CameraDataSet(SDK_GfxCamera_GetLogicData(mCoreObject));
                mCameraRenderData = new CameraDataSet(SDK_GfxCamera_GetRenderData(mCoreObject));
            }
        }
        public string DebugName;
        private CFrustum mFrustum;
        bool mLockCulling = false;
        public bool LockCulling
        {
            get { return mLockCulling; }
            set
            {
                mLockCulling = value;
                if(mLockCulling)
                {
                    mFrustum.CopyFrustum(mCullingFrustum);
                }
            }
        }


        CFrustum mCullingFrustum;
        public CFrustum CullingFrustum
        {
            get
            {
                if(LockCulling==false)
                {
                    return mFrustum;
                }
                return mCullingFrustum;
            }
        }

        public override void Cleanup()
        {
            mSceneView = null;
            mCBuffer = null;
            base.Cleanup();
        }

        CConstantBuffer mCBuffer;
        public CConstantBuffer CBuffer
        {
            get { return mCBuffer; }
        }


        public CGfxRenderLayer[] mSceneRenderLayer;
        public List<Mesh.CGfxMesh>  mVisibleSceneMeshArray = new List<Mesh.CGfxMesh>();
        public List<Mesh.CGfxMesh> mVisibleSceneMeshArray_Shadow = new List<Mesh.CGfxMesh>();

        //at the end of the frame,we need to clear all render layer data;
        public void ClearAllRenderLayerData()
        {
            for (int i = 0; i < mSceneRenderLayer.Length; i++)
            {
                mSceneRenderLayer[i].Clear();
            }

            mVisibleSceneMeshArray.Clear();
            mVisibleSceneMeshArray_Shadow.Clear();
            
        }

        public void ClearSpecRenderLayerData(ERenderLayer render_layer)
        {
            mSceneRenderLayer[(int)render_layer].Clear();
        }

        public void PushVisibleSceneMesh2RenderLayer()
        {
            for (int Idx_VisibleMesh = 0; Idx_VisibleMesh < mVisibleSceneMeshArray.Count; Idx_VisibleMesh++)
            {
                for (UInt32 Idx_MtlMesh = 0; Idx_MtlMesh < mVisibleSceneMeshArray[Idx_VisibleMesh].MtlMeshArray.Length; Idx_MtlMesh++)
                {
                    var MtlMesh = mVisibleSceneMeshArray[Idx_VisibleMesh].MtlMeshArray[Idx_MtlMesh];
                    
                    if (MtlMesh == null || MtlMesh.Visible == false)
                    {
                        continue;
                    }

                    if (MtlMesh.MtlInst.mRenderLayer == ERenderLayer.RL_Num)
                    {
                        MtlMesh.MtlInst.mRenderLayer = ERenderLayer.RL_Num - 1;
                    }

                    if (MtlMesh.MtlInst.mRenderLayer == ERenderLayer.RL_Shadow)
                    {
                        continue;
                    }

                    this.mSceneRenderLayer[(int)MtlMesh.MtlInst.mRenderLayer].AddRenderLayerAtom(MtlMesh);
                }
            }
        }

        public void PushVisibleMesh2ShadowLayer(CFrustum cam_frustum)
        {
            for (int Idx_VisibleShadowMesh = 0; Idx_VisibleShadowMesh < mVisibleSceneMeshArray_Shadow.Count; Idx_VisibleShadowMesh++)
            {
                if (cam_frustum.WhichContainType(ref mVisibleSceneMeshArray_Shadow[Idx_VisibleShadowMesh].HostActor.Placement.ActorAABB, false) == CFrustum.CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                {
                    continue;
                }

                for (UInt32 Idx_MtlMesh = 0; Idx_MtlMesh < mVisibleSceneMeshArray_Shadow[Idx_VisibleShadowMesh].MtlMeshArray.Length; Idx_MtlMesh++)
                {
                    var MtlMesh = this.mVisibleSceneMeshArray_Shadow[Idx_VisibleShadowMesh].MtlMeshArray[Idx_MtlMesh];
                    if (MtlMesh == null || MtlMesh.Visible == false)
                    {
                        continue;
                    }
                    if (MtlMesh.MtlInst.mRenderLayer == ERenderLayer.RL_Opaque || MtlMesh.MtlInst.mRenderLayer == ERenderLayer.RL_CustomOpaque /*||
                        MtlMesh.MtlInst.mRenderLayer == ERenderLayer.RL_CustomTranslucent*/)
                    {
                        this.mSceneRenderLayer[(int)ERenderLayer.RL_Shadow].AddRenderLayerAtom(MtlMesh);
                    }
                }
            }
        }


        public bool Init(CRenderContext rc, bool autoFlush)
        {
            mSceneRenderLayer = new CGfxRenderLayer[(int)ERenderLayer.RL_Num];
            for (int i = 0; i < mSceneRenderLayer.Length; i++)
            {
                mSceneRenderLayer[i] = new CGfxRenderLayer();
            }
            
            var shaderProgram = CEngine.Instance.EffectManager.DefaultEffect.ShaderProgram;
            CEngine.Instance.EffectManager.DefaultEffect.PreUse((succsessed) =>
            {
                var cbIndex = CEngine.Instance.EffectManager.DefaultEffect.CacheData.CBID_Camera;
                mCBuffer = rc.CreateConstantBuffer(shaderProgram, cbIndex);
                if (mCBuffer == null)
                {
#if DEBUG
                    System.Diagnostics.Debug.Assert(false);
#endif
                }

                //mCBuffer.AutoFlush = autoFlush;
                SDK_GfxCamera_BindConstBuffer(CoreObject, mCBuffer.CoreObject);
            });
            return true;
        }

        public void BeforeFrame()
        {
            SDK_GfxCamera_ExecuteLookAtLH(CoreObject);
        }
        public void SwapBuffer(bool bImm = false)
        {   
            SDK_GfxCamera_UpdateConstBufferData(CoreObject, CEngine.Instance.RenderContext.CoreObject, vBOOL.FromBoolean(bImm));
        }

        CGfxSceneView mSceneView;
        public CGfxSceneView SceneView
        {
            get { return mSceneView; }
        }

        bool mIsPerspective = false;
        public bool IsPerspective
        {
            get { return mIsPerspective; }
        }
        float mCameraWidth;
        public float CameraWidth
        {
            get { return mCameraWidth; }
        }
        float mCameraHeight;
        public float CameraHeight
        {
            get { return mCameraHeight; }
        }
        public void PerspectiveFovLH(float fov, float width, float height, float zMin = -1, float zMax = -1)
        {
            if (zMin < 0)
                zMin = this.ZNear;
            if (zMax < 0)
                zMax = this.ZFar;

            //zMax =  ClampZFar(zMin, zMax);

            mCameraWidth = width;
            mCameraHeight = height;
            SDK_GfxCamera_PerspectiveFovLH(CoreObject, fov, width, height, zMin, zMax);
            
            mIsPerspective = true;

            mFoV = fov;
            mAspect = width / height;
            mZNear = zMin;
            mZFar = zMax;
    }
        public void MakeOrtho(float w, float h, float zn, float zf)
        {
            if (w <= 0.0f || h <= 0.0f)
            {
                w = 1.0f;
                h = 1.0f;
            }

            if (zn < 0.0f || zf < 0.0f)
            {
                zn = ZNear;
                zf = ZFar;
            }
            
            mCameraWidth = w;
            mCameraHeight = h;
            SDK_GfxCamera_MakeOrtho(CoreObject, w, h, zn, zf);
            mIsPerspective = false;
            
            mAspect = w / h;
            mZNear = zn;
            mZFar = zf;
        }

        public void DoOrthoProjectionForShadow(float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY)
        {
            if (w <= 0.0f || h <= 0.0f)
            {
                w = 1.0f;
                h = 1.0f;
            }

            if (znear < 0.0f || zfar < 0.0f)
            {
                znear = ZNear;
                zfar = ZFar;
            }

            mCameraWidth = w;
            mCameraHeight = h;
            SDK_GfxCamera_DoOrthoProjectionForShadow(CoreObject, w, h, znear, zfar, TexelOffsetNdcX, TexelOffsetNdcY);
            mIsPerspective = false;

            mAspect = w / h;
            mZNear = znear;
            mZFar = zfar;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void LookAtLH(Vector3 eye, Vector3 lookAt, Vector3 up, bool bImm = false)
        {
            unsafe
            {
                SDK_GfxCamera_LookAtLH(CoreObject, &eye, &lookAt, &up, vBOOL.FromBoolean(true));
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void LookPosAndDir(Vector3 eye, Vector3 dir, bool bImm = false)
        {
            unsafe
            {
                var lookAt = eye + dir * 10.0f;
                fixed (Vector3* pUp = &Vector3.UnitY)
                {
                    SDK_GfxCamera_LookAtLH(CoreObject, &eye, &lookAt, pUp, vBOOL.FromBoolean(bImm));
                }
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool GetPickRay(ref Vector3 PickRay, float x, float y, float sw, float sh)
        {
            if(this.SceneView!=null)
            {
                if (sw == 0)
                {
                    sw = this.SceneView.Viewport.Width;
                }
                if (sh == 0)
                {
                    sh = this.SceneView.Viewport.Height;
                }
            }
            unsafe
            {
                fixed (Vector3* p = &PickRay)
                {
                    return SDK_GfxCamera_GetPickRay(CoreObject, p, x, y, sw, sh);
                }
            }
        }
        public void SetSceneView(CRenderContext rc, CGfxSceneView vp)
        {
            mSceneView = vp;
        }

        private float mFoV = EngineNS.MathHelper.V_PI / 2.0f;
        public float FoV
        {
            get
            {
                //return SDK_GfxCamera_GetFov(CoreObject);
                return mFoV;
            }
            set { mFoV = value; }
        }

        private float mAspect;
        public float Aspect
        {
            get { return mAspect; }
            set { mAspect = value; }
        }

        private float mZNear = 0.1f;
        public float ZNear
        {
            get
            {
                //return SDK_GfxCamera_GetZNear(CoreObject);
                return mZNear;
            }
            set { mZNear = value; }
        }

        private float ClampZFar(float znear, float zfar)
        {
            float Zf = 0.0f;
            if (zfar > 500.0f)
            {
                Zf = 500.0f;
            }
            else
            {
                Zf = zfar;
            }

            if (Zf <= znear)
            {
                Zf = znear + 1.0f;
            }
            else
            {
                //do nothing;
            }

            return Zf;
        }

        private float mZFar = 200.0f;
        public float ZFar
        {
            get
            {
                //return SDK_GfxCamera_GetZFar(CoreObject);
                return mZFar;
            }
            set
            {
                mZFar = value;
                //mZFar = ClampZFar(mZNear, mZFar);
            }
        }

        public float GetScreenSizeInWorld(EngineNS.Vector3 worldPos, float screenSize)
        {
            if (mSceneView == null)
                return 0;
            var fov = FoV;
            if (fov == 0)
            {
                return screenSize * (mCameraWidth / mSceneView.Viewport.Width) * 1000;
            }
            else
            {
                var dis = (CameraData.Position - worldPos).Length();
                var worldFullScreenSize = (float)System.Math.Tan(fov * 0.5f) * dis * 2;
                return worldFullScreenSize * screenSize;
            }
        }
        public class CameraDataSet
        {
            public struct CameraData
            {
                public Vector3 Position;
                public Vector3 LookAt;
                public Vector3 Direction;
                public Vector3 Right;
                public Vector3 Up;
                public Matrix ViewMatrix;
                public Matrix ViewInverse;
                public Matrix ProjectionMatrix;
                public Matrix ProjectionInverse;
                public Matrix ViewProjection;
                public Matrix ViewProjectionInverse;
                public Matrix ViewPortOffsetMatrix;
                public Matrix ToViewPortMatrix;
            }
            public unsafe CameraDataSet(CameraData* camera)
            {
                Camera = camera;
            }
            private unsafe CameraData* Camera;
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Vector3 Position
            {
                get
                {
                    unsafe
                    {
                        return Camera->Position;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Vector3 LookAt
            {
                get
                {
                    unsafe
                    {
                        return Camera->LookAt;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Vector3 Direction
            {
                get
                {
                    unsafe
                    {
                        return Camera->Direction;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Vector3 Right
            {
                get
                {
                    unsafe
                    {
                        return Camera->Right;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Vector3 Up
            {
                get
                {
                    unsafe
                    {
                        return Camera->Up;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ViewMatrix
            {
                get
                {
                    unsafe
                    {
                        return Camera->ViewMatrix;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ViewInverse
            {
                get
                {
                    unsafe
                    {
                        return Camera->ViewInverse;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ProjectionMatrix
            {
                get
                {
                    unsafe
                    {
                        return Camera->ProjectionMatrix;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ProjectionInverse
            {
                get
                {
                    unsafe
                    {
                        return Camera->ProjectionInverse;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ViewProjection
            {
                get
                {
                    unsafe
                    {
                        return Camera->ViewProjection;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ViewProjectionInverse
            {
                get
                {
                    unsafe
                    {
                        return Camera->ViewProjectionInverse;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Matrix ToViewPortMatrix
            {
                get
                {
                    unsafe
                    {
                        return Camera->ToViewPortMatrix;
                    }
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public Vector3 Trans2ViewPort(ref Vector3 pos)
            {
                unsafe
                {
                    Vector3 result;
                    Vector3.TransformCoordinate(ref pos, ref Camera->ToViewPortMatrix, out result);
                    return result;
                }
            }
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
            public float Trans2ViewPortWithW(ref Vector3 pos, out Vector3 result)
            {
                unsafe
                {
                    return Vector3.TransformCoordinate(ref pos, ref Camera->ToViewPortMatrix, out result);
                }
            }
        }

        CameraDataSet mCameraData;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public CameraDataSet CameraData
        {
            get
            {
                return mCameraData;
            }
        }
        CameraDataSet mCameraRenderData;
        public CameraDataSet CameraRenderData
        {
            get
            {
                return mCameraRenderData;
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_GfxCamera_GetFrustum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCamera_PerspectiveFovLH(NativePointer self, float fov, float width, float height, float zMin, float zMax);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCamera_MakeOrtho(NativePointer self, float w, float h, float zn, float zf);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCamera_DoOrthoProjectionForShadow(NativePointer self, float w, float h, float znear, float zfar, float TexelOffsetNdcX, float TexelOffsetNdcY);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxCamera_LookAtLH(NativePointer self, Vector3* eye, Vector3* lookAt, Vector3* up, vBOOL Imm);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCamera_ExecuteLookAtLH(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCamera_UpdateConstBufferData(NativePointer self, CRenderContext.NativePointer rc, vBOOL bImm);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxCamera_GetPickRay(NativePointer self, Vector3* pvPickRay, float x, float y, float sw, float sh);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCamera_BindConstBuffer(NativePointer self, CConstantBuffer.NativePointer cb);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxCamera_GetFov(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxCamera_GetZNear(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxCamera_GetZFar(NativePointer self);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxCamera_GetPosition(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxCamera_GetLookAt(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxCamera_GetDirection(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxCamera_GetRight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxCamera_GetUp(NativePointer self);        
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetViewMatrix(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetViewInverse(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetProjectionMatrix(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetProjectionInverse(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetViewProjection(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetViewProjectionInverse(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Matrix SDK_GfxCamera_GetToViewPortMatrix(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static CameraDataSet.CameraData* SDK_GfxCamera_GetLogicData(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static CameraDataSet.CameraData* SDK_GfxCamera_GetRenderData(NativePointer self);
        #endregion
    }
}
