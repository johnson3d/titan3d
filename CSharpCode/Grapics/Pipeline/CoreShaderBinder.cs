using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public partial class TtCoreShaderBinder
    {
        public unsafe static void InitializeCoreBinder(NxRHI.TtNativeGraphicsEffect effect)
        {
            NxRHI.TtShader.TtCommonShaderResourceIndexer.Instance.UpdateBindResouce(effect);
            TtPerFrameCBufferVarIndexer.Instance.UpdateFieldVar(effect.mCoreObject, "cbPerFrame");
            TtPerViewCBufferVarIndexer.Instance.UpdateFieldVar(effect.mCoreObject, "cbPerViewport");
            TtPerCameraCBufferVarIndexer.Instance.UpdateFieldVar(effect.mCoreObject, "cbPerCamera");
            TtPerMeshCBufferVarIndexer.Instance.UpdateFieldVar(effect.mCoreObject, "cbPerMesh");
            TtPerGpuSceneCBufferVarIndexer.Instance.UpdateFieldVar(effect.mCoreObject, "cbPerGpuScene");
            TtPerSkinMeshCBufferVarIndexer.Instance.UpdateFieldVar(effect.mCoreObject, "cbSkinMesh");
        }
        public static void FinalCleanup()
        {
            NxRHI.TtShader.TtCBufferVarIndexer.FinalCleanup();
            NxRHI.TtShader.TtShaderBinderIndexer.FinalCleanup();
        }
        public class TtPerFrameCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerFrameCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc Time;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeFracSecond; 
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeSin;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TimeCos;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ElapsedTime;
        }
        public class TtPerViewCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerViewCBufferVarIndexer>
        {
            public TtPerViewCBufferVarIndexer()
            {

            }
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc ViewportPos;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Graphics.Pipeline.Shader.FDirLight))]
            public NxRHI.FShaderVarDesc DirLight;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CsmNum;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc FogStart;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogHorizontalRange;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogCeil;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]            
            public NxRHI.FShaderVarDesc FogVerticalRange;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc FogDensity;

            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc ViewportSizeAndRcp;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc DepthBiasAndZFarRcp;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc FadeParam;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ShadowMapSizeAndRcp;

            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc Viewer2ShadowMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc Viewer2ShadowMtxArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc CsmDistanceArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ShadowTransitionScaleArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc SunPosNDC;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc AoParam;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ShadowTransitionScale;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ShadowDistance;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc EnvMapMaxMipLevel;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc EyeEnvMapMaxMipLevel;
        }
        public class TtPerCameraCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerCameraCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc CameraViewMatrix;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc CameraViewInverse;
            
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrjMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PrjInvMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc ViewPrjMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc ViewPrjInvMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PreFrameViewPrjMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPrjMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPrjInvMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterViewPrjMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterViewPrjInvMtx;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc JitterPreFrameViewPrjMtx;

            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraPosition;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ZNear;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraLookAt;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ZFar;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraDirection;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraRight;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraUp;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraOffset;
            
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesX;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesY;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesZ;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ClipPlanesW;

            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4), NumElement = 4)]
            public NxRHI.FShaderVarDesc CornerRays;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4), NumElement = 6)]
            public NxRHI.FShaderVarDesc ClipPlanes;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc ClipMinPoint;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc ClipMaxPoint;
        }
        public class TtPerMeshCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerMeshCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldMatrix;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc WorldMatrixInverse;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc PreWorldMatrix;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc HitProxyId;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc ActorId;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc CameraPositionInModel;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PickedID;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4))]
            public NxRHI.FShaderVarDesc PointLightIndices;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc PointLightNum;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ObjectFLags_2Bit;
        }
        public class TtPerGpuSceneCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerGpuSceneCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc TileNum;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc LightNum;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc PixelNum;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMiddleGrey;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMinLuminance;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc HdrMaxLuminance;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc Exposure;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc EyeAdapterTimeRange;
        }

        public class TtPerSkinMeshCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerSkinMeshCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector4), NumElement = 360)]
            public NxRHI.FShaderVarDesc AbsBonePos;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Quaternion), NumElement = 360)]
            public NxRHI.FShaderVarDesc AbsBoneQuat;
        }

        public class TtPerTerrainCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerTerrainCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc EyeCenter;// = 1.0f;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc GridSize;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc PatchSize;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc TexUVScale;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc MaterialIdUVStep;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc DiffuseUVStep;
            [NxRHI.TtShader.TtShaderVar(VarType = null)]
            public NxRHI.FShaderVarDesc MorphLODs;
        }
        
        public class TtPerTerrainPatchCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerTerrainPatchCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector3))]
            public NxRHI.FShaderVarDesc StartPosition;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CurrentLOD;// = 1.0f;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Vector2))]
            public NxRHI.FShaderVarDesc TexUVOffset;
        }

        public class TtPerParticleCBufferVarIndexer : NxRHI.TtShader.AuxCBufferVarIndexer<TtPerParticleCBufferVarIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleRandomPoolSize;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_IndexCountPerInstance;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_StartIndexLocation;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_BaseVertexLocation;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc Draw_StartInstanceLocation;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleMaxSize;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ParticleElapsedTime;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(float))]
            public NxRHI.FShaderVarDesc ParticleStartSecond;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc OnTimerState;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleRandomSeed;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc AllocatorCapacity;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc CurAliveCapacity;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc BackendAliveCapacity;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(uint))]
            public NxRHI.FShaderVarDesc ParticleCapacity;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(Bricks.Particle.FParticleEmitter))]
            public NxRHI.FShaderVarDesc EmitterData; 
        }
    }
}
