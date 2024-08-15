using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UCoreShaderBinder
    {
        public unsafe void UpdateIndex(NxRHI.TtShaderEffect effect)
        {
            //effect.GraphicsEffect.
            CBufferCreator.UpdateBindResouce(effect);
            var binder = effect.FindBinder("cbPerFrame");
            if (binder != null)
                CBPerFrame.UpdateFieldVar(binder.GetShaderBinder());
            binder = effect.FindBinder("cbPerViewport");
            if (binder != null)
                CBPerViewport.UpdateFieldVar(binder.GetShaderBinder());
            binder = effect.FindBinder("cbPerCamera");
            if (binder != null)
                CBPerCamera.UpdateFieldVar(binder.GetShaderBinder());
            binder = effect.FindBinder("cbPerMesh");
            if (binder != null)
                CBPerMesh.UpdateFieldVar(binder.GetShaderBinder());
            binder = effect.FindBinder("cbPerGpuScene");
            if (binder != null)
                CBPerGpuScene.UpdateFieldVar(binder.GetShaderBinder());
        }
        public class UCBufferPerFrameIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc Time;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeFracSecond; 
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeSin;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeCos;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ElapsedTime;
        }
        public readonly UCBufferPerFrameIndexer CBPerFrame = new UCBufferPerFrameIndexer();
        public class UCBufferPerViewIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc ViewportPos;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc mDirLightSpecularIntensity;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc mDirLightShadingSSS;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mDirLightingAmbient;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mDirLightingDiffuse;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mDirLightingSpecular;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc mGroundLightColor;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc gCsmNum;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mSkyLightColor;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogStart;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogHorizontalRange;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogCeil;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]            
            public NxRHI.FShaderVarDesc FogVerticalRange;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogDensity;

            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gDirLightColor_Intensity;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gDirLightDirection_Leak;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gViewportSizeAndRcp;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gDepthBiasAndZFarRcp;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gFadeParam;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gShadowMapSizeAndRcp;

            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc gViewer2ShadowMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc gViewer2ShadowMtxArray;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gCsmDistanceArray;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gShadowTransitionScaleArray;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gSunPosNDC;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gAoParam;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gShadowTransitionScale;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gShadowDistance;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gEnvMapMaxMipLevel;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gEyeEnvMapMaxMipLevel;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc PointLightPos_RadiusInv;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PointLightColor_Intensity;
        }
        public readonly UCBufferPerViewIndexer CBPerViewport = new UCBufferPerViewIndexer();
        public class UCBufferPerCameraIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc CameraViewMatrix;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc CameraViewInverse;
            
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrjMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrjInvMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc ViewPrjMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc ViewPrjInvMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PreFrameViewPrjMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPrjMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPrjInvMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterViewPrjMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterViewPrjInvMtx;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPreFrameViewPrjMtx;

            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraPosition;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc gZNear;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraLookAt;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gZFar;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraDirection;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraRight;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraUp;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraOffset;
            
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesX;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesY;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesZ;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesW;

            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4), NumElement = 4)]
            public NxRHI.FShaderVarDesc CornerRays;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4), NumElement = 6)]
            public NxRHI.FShaderVarDesc ClipPlanes;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc ClipMinPoint;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc ClipMaxPoint;
        }
        public readonly UCBufferPerCameraIndexer CBPerCamera = new UCBufferPerCameraIndexer();
        public class UCBufferPerMeshIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldMatrix;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldMatrixInverse;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PreWorldMatrix;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc HitProxyId;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ActorId;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraPositionInModel;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PickedID;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc PointLightIndices;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc PointLightNum;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ObjectFLags_2Bit;
        }
        public readonly UCBufferPerMeshIndexer CBPerMesh = new UCBufferPerMeshIndexer();
        public class UCBufferPreFramePerMeshIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            
        }
        public class UCBufferPerGpuSceneIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc TileNum;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc LightNum;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc PixelNum;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMiddleGrey;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMinLuminance;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMaxLuminance;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc Exposure;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc EyeAdapterTimeRange;
        }
        public readonly UCBufferPerGpuSceneIndexer CBPerGpuScene = new UCBufferPerGpuSceneIndexer();

        public class UCBufferPerSkinMeshIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc AbsBonePos;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Quaternion))]
            public NxRHI.FShaderVarDesc AbsBoneQuat;
        }
        public readonly UCBufferPerSkinMeshIndexer CBPerSkinMesh = new UCBufferPerSkinMeshIndexer();

        public class UCBufferPerTerrainIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc EyeCenter;// = 1.0f;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc GridSize;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchSize;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TexUVScale;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MaterialIdUVStep;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc DiffuseUVStep;
            [NxRHI.TtShader.UShaderVar(VarType = null)]
            public NxRHI.FShaderVarDesc MorphLODs;
        }
        public readonly UCBufferPerTerrainIndexer CBPerTerrain = new UCBufferPerTerrainIndexer();

        public class UCBufferPerTerrainPatchIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc StartPosition;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CurrentLOD;// = 1.0f;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc TexUVOffset;
        }
        public readonly UCBufferPerTerrainPatchIndexer CBPerTerrainPatch = new UCBufferPerTerrainPatchIndexer();

        public class UCBufferPerParticlIndexer : NxRHI.TtShader.UShaderVarIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleRandomPoolSize;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_IndexCountPerInstance;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_StartIndexLocation;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_BaseVertexLocation;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_StartInstanceLocation;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleMaxSize;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ParticleElapsedTime;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ParticleStartSecond;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc OnTimerState;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleRandomSeed;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc AllocatorCapacity;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CurAliveCapacity;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc BackendAliveCapacity;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleCapacity;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(Bricks.Particle.FParticleEmitter))]
            public NxRHI.FShaderVarDesc EmitterData; 
        }
        public readonly UCBufferPerParticlIndexer CBPerParticle = new UCBufferPerParticlIndexer();

        public class UShaderResourceIndexer : NxRHI.TtShader.UShaderBinderIndexer
        {
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.TtEffectBinder cbPerViewport;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.TtEffectBinder cbPerFrame;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.TtEffectBinder cbPerCamera;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.TtEffectBinder cbPerMesh;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.TtEffectBinder cbPreFramePerMesh;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.TtEffectBinder cbPerMaterial;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.USrView))]
            public NxRHI.TtEffectBinder gEnvMap;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.USrView))]
            public NxRHI.TtEffectBinder gShadowMap;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.USampler))]
            public NxRHI.TtEffectBinder Samp_gEnvMap;
            [NxRHI.TtShader.UShaderVar(VarType = typeof(NxRHI.USampler))]
            public NxRHI.TtEffectBinder Samp_gShadowMap;
        }
        public readonly UShaderResourceIndexer CBufferCreator = new UShaderResourceIndexer();
    }
    partial class UGfxDevice
    {
        public readonly UCoreShaderBinder CoreShaderBinder = new UCoreShaderBinder();
    }
}
