using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UCoreShaderBinder
    {
        public unsafe void UpdateIndex(NxRHI.UShaderEffect effect)
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
        public class UCBufferPerFrameIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc Time;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeFracSecond; 
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeSin;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeCos;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ElapsedTime;
        }
        public readonly UCBufferPerFrameIndexer CBPerFrame = new UCBufferPerFrameIndexer();
        public class UCBufferPerViewIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc ViewportPos;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc mDirLightSpecularIntensity;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc mDirLightShadingSSS;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mDirLightingAmbient;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mDirLightingDiffuse;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mDirLightingSpecular;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc mGroundLightColor;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc gCsmNum;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc mSkyLightColor;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogStart;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogHorizontalRange;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogCeil;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]            
            public NxRHI.FShaderVarDesc FogVerticalRange;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogDensity;

            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gDirLightColor_Intensity;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gDirLightDirection_Leak;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gViewportSizeAndRcp;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gDepthBiasAndZFarRcp;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gFadeParam;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gShadowMapSizeAndRcp;

            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc gViewer2ShadowMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc gViewer2ShadowMtxArray;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gCsmDistanceArray;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gShadowTransitionScaleArray;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc gSunPosNDC;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc gAoParam;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gShadowTransitionScale;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gShadowDistance;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gEnvMapMaxMipLevel;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gEyeEnvMapMaxMipLevel;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc PointLightPos_RadiusInv;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PointLightColor_Intensity;
        }
        public readonly UCBufferPerViewIndexer CBPerViewport = new UCBufferPerViewIndexer();
        public class UCBufferPerCameraIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc CameraViewMatrix;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc CameraViewInverse;
            
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrjMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrjInvMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc ViewPrjMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc ViewPrjInvMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PreFrameViewPrjMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPrjMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPrjInvMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterViewPrjMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterViewPrjInvMtx;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPreFrameViewPrjMtx;

            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraPosition;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc gZNear;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraLookAt;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc gZFar;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraDirection;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraRight;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraUp;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraOffset;
            
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesX;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesY;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesZ;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesW;

            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4), NumElement = 4)]
            public NxRHI.FShaderVarDesc CornerRays;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4), NumElement = 6)]
            public NxRHI.FShaderVarDesc ClipPlanes;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc ClipMinPoint;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc ClipMaxPoint;
        }
        public readonly UCBufferPerCameraIndexer CBPerCamera = new UCBufferPerCameraIndexer();
        public class UCBufferPerMeshIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldMatrix;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldMatrixInverse;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PreWorldMatrix;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc HitProxyId;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ActorId;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraPositionInModel;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PickedID;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc PointLightIndices;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc PointLightNum;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ObjectFLags_2Bit;
        }
        public readonly UCBufferPerMeshIndexer CBPerMesh = new UCBufferPerMeshIndexer();
        public class UCBufferPreFramePerMeshIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            
        }
        public class UCBufferPerGpuSceneIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc TileNum;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc LightNum;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc PixelNum;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMiddleGrey;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMinLuminance;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMaxLuminance;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc Exposure;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc EyeAdapterTimeRange;
        }
        public readonly UCBufferPerGpuSceneIndexer CBPerGpuScene = new UCBufferPerGpuSceneIndexer();

        public class UCBufferPerSkinMeshIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc AbsBonePos;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Quaternion))]
            public NxRHI.FShaderVarDesc AbsBoneQuat;
        }
        public readonly UCBufferPerSkinMeshIndexer CBPerSkinMesh = new UCBufferPerSkinMeshIndexer();

        public class UCBufferPerTerrainIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc EyeCenter;// = 1.0f;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc GridSize;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchSize;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TexUVScale;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MaterialIdUVStep;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc DiffuseUVStep;
            [NxRHI.UShader.UShaderVar(VarType = null)]
            public NxRHI.FShaderVarDesc MorphLODs;
        }
        public readonly UCBufferPerTerrainIndexer CBPerTerrain = new UCBufferPerTerrainIndexer();

        public class UCBufferPerTerrainPatchIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc StartPosition;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CurrentLOD;// = 1.0f;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc TexUVOffset;
        }
        public readonly UCBufferPerTerrainPatchIndexer CBPerTerrainPatch = new UCBufferPerTerrainPatchIndexer();

        public class UCBufferPerParticlIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleRandomPoolSize;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_IndexCountPerInstance;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_StartIndexLocation;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_BaseVertexLocation;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_StartInstanceLocation;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleMaxSize;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ParticleElapsedTime;
            [NxRHI.UShader.UShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ParticleStartSecond;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc OnTimer;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleRandomSeed;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc AllocatorCapacity;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CurAliveCapacity;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc BackendAliveCapacity;
            [NxRHI.UShader.UShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleCapacity;
            [NxRHI.UShader.UShaderVar(VarType = typeof(Bricks.Particle.FParticleEmitter))]
            public NxRHI.FShaderVarDesc SystemData; 
        }
        public readonly UCBufferPerParticlIndexer CBPerParticle = new UCBufferPerParticlIndexer();

        public class UShaderResourceIndexer : NxRHI.UShader.UShaderBinderIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.UEffectBinder cbPerViewport;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.UEffectBinder cbPerFrame;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.UEffectBinder cbPerCamera;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.UEffectBinder cbPerMesh;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.UEffectBinder cbPreFramePerMesh;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.UBuffer))]
            public NxRHI.UEffectBinder cbPerMaterial;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.USrView))]
            public NxRHI.UEffectBinder gEnvMap;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.USrView))]
            public NxRHI.UEffectBinder gShadowMap;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.USampler))]
            public NxRHI.UEffectBinder Samp_gEnvMap;
            [NxRHI.UShader.UShaderVar(VarType = typeof(NxRHI.USampler))]
            public NxRHI.UEffectBinder Samp_gShadowMap;
        }
        public readonly UShaderResourceIndexer CBufferCreator = new UShaderResourceIndexer();
    }
    partial class UGfxDevice
    {
        public readonly UCoreShaderBinder CoreShaderBinder = new UCoreShaderBinder();
    }
}
