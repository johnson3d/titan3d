using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CConstantBuffer : AuxPtrType<IConstantBuffer>
    {
        public class UShaderTypeAttribute : Attribute
        {
            public System.Type ShaderType;
        }

        #region Viewport CBuffer
        public static Rtti.UTypeDesc PerViewportType = Rtti.UTypeDescGetter<PerViewport>.TypeDesc;
        public class PerViewport
        {
            public int ViewportPos; // leftup position
            
            public int mDirLightSpecularIntensity;
            public int mDirLightingAmbient;
            public int mDirLightingDiffuse;
            public int mDirLightingSpecular;
            public int mDirLightShadingSSS;
            
            public int FogStart;// = 30;
            public int FogHorizontalRange;// = 170;
            public int FogCeil;// = 70;
            public int FogVerticalRange;// = 150;
            public int FogDensity;// = 1.57;
            public int mSkyLightColor;
            public int mGroundLightColor;
            public int gEnvMapMaxMipLevel;
            public int gEyeEnvMapMaxMipLevel;

            public int gDirLightColor_Intensity;
            public int gDirLightDirection_Leak;
            
            public int gViewportSizeAndRcp;
            
            public int gDepthBiasAndZFarRcp;
            public int gFadeParam;
            public int gShadowMapSizeAndRcp;
            public int gViewer2ShadowMtx;
            public int gViewer2ShadowMtxArrayEditor;
            public int gCsmDistanceArray;
            public int gShadowTransitionScaleArrayEditor;
            public int gShadowTransitionScale;
            public int gShadowDistance;
            public int gCsmNum;
            
            public int gSunPosNDC;
            public int gAoParam;//radius_platform_bias_dark;
        }
        public static PerViewport mPerViewportIndexer;
        public PerViewport PerViewportIndexer
        {
            get
            {
                if (mPerViewportIndexer == null)
                {
                    unsafe
                    {
                        var name = this.mCoreObject.GetName();
                        if(name == "cbPerViewport")
                        {
                            mPerViewportIndexer = Rtti.UTypeDescManager.CreateInstance(PerViewportType.SystemType) as PerViewport;
                            UpdateVarIndex(mPerViewportIndexer);
                        }
                    }
                }
                return mPerViewportIndexer;
            }
        }
        #endregion

        #region Camera CBuffer
        public static Type PerCameraType = typeof(PerCamera);
        public class PerCamera
        {
            public int CameraViewMatrix;
            public int CameraViewInverse;
            public int ViewPrjMtx;
            public int ViewPrjInvMtx;
            public int PrjMtx;
            public int PrjInvMtx;

            public int CameraPosition;
            public int gZNear;

            public int CameraLookAt;
            public int gZFar;

            public int CameraDirection;
            public int pad0;

            public int CameraRight;
            public int pad1;

            public int CameraUp;
            public int pad2;
        }
        public static PerCamera mPerCameraIndexer;
        public PerCamera PerCameraIndexer
        {
            get
            {
                if (mPerCameraIndexer == null)
                {
                    unsafe
                    {
                        var name = this.mCoreObject.GetName();
                        if (name == "cbPerCamera")
                        {
                            mPerCameraIndexer = Rtti.UTypeDescManager.CreateInstance(PerCameraType) as PerCamera;
                            UpdateVarIndex(mPerCameraIndexer);
                        }
                    }
                }
                return mPerCameraIndexer;
            }
        }
        #endregion

        #region Frame CBuffer
        public static Type PerFrameType = typeof(PerFrame);
        public class PerFrame
        {
            [UShaderType(ShaderType = typeof(float))]
            public int Time;// = 1.0f;
            [UShaderType(ShaderType = typeof(float))]
            public int TimeSin;// = 1.0f;
            [UShaderType(ShaderType = typeof(float))]
            public int TimeCos;// = 1.0f;
        }
        public static PerFrame mPerFrameIndexer;
        public PerFrame PerFrameIndexer
        {
            get
            {
                if (mPerFrameIndexer == null)
                {
                    unsafe
                    {
                        var name = this.mCoreObject.GetName();
                        if (name == "cbPerFrame")
                        {
                            mPerFrameIndexer = Rtti.UTypeDescManager.CreateInstance(PerFrameType) as PerFrame;
                            UpdateVarIndex(mPerFrameIndexer);
                        }
                    }
                }
                return mPerFrameIndexer;
            }
        }
        #endregion

        #region Mesh CBuffer
        public static Type PerMeshType = typeof(PerMesh);
        public class PerMesh
        {
            public int WorldMatrix;
            public int WorldMatrixInverse;

            public int HitProxyId;
            public int ActorId;

            public int CameraPositionInModel;
            public int PickedID;

            public int PointLightNum;
            public int PointLightIndices;

            public int AbsBonePos;
            public int AbsBoneQuat;
        }
        public static PerMesh mPerMeshIndexer;
        public PerMesh PerMeshIndexer
        {
            get
            {
                if (mPerMeshIndexer == null)
                {
                    unsafe
                    {
                        var name = this.mCoreObject.GetName();
                        if (name == "cbPerMesh")
                        {
                            mPerMeshIndexer = Rtti.UTypeDescManager.CreateInstance(PerMeshType) as PerMesh;
                            UpdateVarIndex(mPerMeshIndexer);
                        }
                    }
                }
                return mPerMeshIndexer;
            }
        }
        #endregion

        public void UpdateVarIndex(object tab)
        {
            var members = tab.GetType().GetFields();
            foreach (var i in members)
            {
                var index = mCoreObject.FindVar(i.Name);
                i.SetValue(tab, index);
            }
        }

        #region SetVar
        public void SetValue<T>(int index, ref T value, uint elem = 0) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &value)
                {
                    mCoreObject.SetVarValuePtr(index, p, sizeof(T), elem);
                }
            }
        }
        public void SetMatrix(int index, ref Matrix value, uint elem = 0)
        {
            unsafe
            {
                var tm = Matrix.Transpose(ref value);
                mCoreObject.SetVarValuePtr(index, &tm, sizeof(Matrix), elem);
            }
        }
        #endregion
    }
}
