using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainLevel
    {
        public UTerrainNode Node;
        public int LevelX = 0;
        public int LevelZ = 0;
        public DVector3 StartPosition;
        public int PatchSide
        {
            get
            {
                return Node.PatchSide;
            }
        }
        public int TexSizePerPatch
        {
            get
            {
                return Node.TexSizePerPatch;
            }
        }
        public UTerrainLevelData LevelData
        {
            get;
            internal set;
        }
        public void ReleaseLevel(ULevelStreaming streaming)
        {
            streaming.PushUnloadLevel(this);
        }
        public void CreateLevel(int levelX, int levelZ, int patchSide, UTerrainNode node)
        {
            Node = node;
            LevelX = levelX;
            LevelZ = levelZ;

            var patchSize = node.PatchSize;
            StartPosition.X = ((double)patchSide * patchSize) * LevelX;
            StartPosition.Z = ((double)patchSide * patchSize) * LevelZ;
        }
        public void LoadLevelData(ULevelStreaming streaming, bool bForce)
        {
            streaming.PushStreamingLevel(this, bForce);
        }
    }
    public class UTerrainLevelData
    {
        public UTerrainLevel Level;
        public UPatch[,] TiledPatch;
        public RHI.CShaderResourceView HeightMapSRV;
        public RHI.CShaderResourceView NormalMapSRV;
        public float HeightMapMinHeight;
        public float HeightMapMaxHeight;
        
        public int HeightfieldWidth;
        public int HeightfieldHeight;
        public PhyHeightFieldSample[] PxHeightfieldSamples;
        public float PxHeightfieldScale;
        public Bricks.PhysicsCore.UPhyHeightfield PhyHeightfield { get; protected set; }
        public Bricks.PhysicsCore.UPhyActor PhyActor { get; protected set; }
        public float RemainUnloadTime = 10.0f;
        ~UTerrainLevelData()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            PhyActor?.AddToScene(null);
            PhyActor = null;

            if (TiledPatch != null)
            {
                foreach (var i in TiledPatch)
                {
                    i.Cleanup();
                }
                TiledPatch = null;
            }

            HeightMapSRV?.Dispose();
            HeightMapSRV = null;
            NormalMapSRV?.Dispose();
            NormalMapSRV = null;
        }
        public UTerrainNode GetTerrainNode()
        {
            return Level.Node;
        }
        public async System.Threading.Tasks.Task CreateLevelData(UTerrainLevel level, bool bForce)
        {
            Level = level;
            var patchSide = level.PatchSide;
            var terrainGen = level.Node.TerrainGen;

            Action action = () =>
            {
                var oriImage = new Bricks.Procedure.Buffer2D.Image();
                oriImage.Initialize(level.Node.TexSizePerPatch * patchSide, level.Node.TexSizePerPatch * patchSide, 0, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);
                terrainGen.SetStartPosition(in level.StartPosition);
                terrainGen.Process(oriImage, Bricks.Procedure.Buffer2D.Image.EImageComponent.X);

                var hMap = terrainGen.mResultImage;

                TiledPatch = new UPatch[patchSide, patchSide];
                for (int i = 0; i < patchSide; i++)
                {
                    for (int j = 0; j < patchSide; j++)
                    {
                        TiledPatch[i, j] = new UPatch();
                        TiledPatch[i, j].Initialize(this, j, i, hMap);
                    }
                }

                hMap.CompX.GetRange(out HeightMapMinHeight, out HeightMapMaxHeight);
                float HeightfieldMidHeight = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
                PxHeightfieldScale = 0.1f;//0.1f精度为分米
                HeightfieldWidth = hMap.CompX.Width;
                HeightfieldHeight = hMap.CompX.Height;
                PxHeightfieldSamples = new PhyHeightFieldSample[hMap.CompX.Width * hMap.CompX.Height];
                for (int i = 0; i < HeightfieldHeight; i++)
                {
                    for (int j = 0; j < HeightfieldWidth; j++)
                    {
                        float height = hMap.CompX.GetPixel(i, j);
                        float localHeight = height - HeightfieldMidHeight;
                        PxHeightfieldSamples[i * HeightfieldWidth + j].height = (short)(localHeight / PxHeightfieldScale);//(short)((height * (float)short.MaxValue) / maxHeight);
                        PxHeightfieldSamples[i * HeightfieldWidth + j].materialIndex0 = 0;
                        PxHeightfieldSamples[i * HeightfieldWidth + j].materialIndex1 = 0;
                    }
                }

                InitPhysics(level);

                HeightMapSRV = terrainGen.mResultImage.CompX.CreateAsTexture2D(HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
                NormalMapSRV = terrainGen.mResultNormalImage.CreateRGBA8Texture2DAsNormal();
            };
            if (bForce)
            {
                action();
            }
            else
            {
                await UEngine.Instance.EventPoster.Post(() =>
                {
                    action();
                    return true;
                }, Thread.Async.EAsyncTarget.AsyncIO);
            }

            {
                //HeightMapSRV = terrainGen.mResultImage.CompX.CreateAsTexture2D(HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
                //NormalMapSRV = terrainGen.mResultNormalImage.CreateRGBA8Texture2DAsNormal();
            }

            OnParentSceneChanged(null, Level.Node.ParentScene);
        }
        #region Px & Collide
        public unsafe void InitPhysics(UTerrainLevel level)
        {
            var pc = UEngine.Instance.PhyModue.PhyContext;
            int texSize = level.Node.TexSizePerPatch * level.Node.PatchSide;
            fixed (PhyHeightFieldSample* pPixelData = &PxHeightfieldSamples[0])
            {
                PhyHeightfield = pc.CookHeightfield(texSize, texSize, pPixelData, 0, false);
                var materials = new Bricks.PhysicsCore.UPhyMaterial[1];
                materials[0] = UEngine.Instance.PhyModue.PhyContext.PhyMaterialManager.DefaultMaterial;
                var terrainShape = pc.CreateShapeHeightfield(materials,
                    PhyHeightfield, PxHeightfieldScale, in Vector3.UnitXYZ);

                PhyActor = pc.CreateActor(EPhyActorType.PAT_Static, in Level.Node.Placement.AbsTransform.mPosition, in Quaternion.Identity);
                Vector3 shapeCenter;
                shapeCenter.X = (float)Level.LevelX * Level.Node.LevelSize;
                shapeCenter.Z = (float)Level.LevelZ * Level.Node.LevelSize;
                shapeCenter.Y = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
                PhyActor.AddShape(terrainShape, in shapeCenter, in Quaternion.Identity);

                //var planeShape = pc.CreateShapePlane(materials[0]);
                //PhyActor.AddShape(planeShape, in StartPosition, in Quaternion.Identity);
            }
        }
        public float GetAltitude(float x, float z, float gridSize)
        {
            //这里因为physx的heightfield奇怪的拧巴了一下 
            var xGrid = (int)(z / gridSize);
            var zGrid = (int)(x / gridSize);
            if (xGrid < 0 || xGrid >= HeightfieldWidth || zGrid < 0 || zGrid >= HeightfieldHeight)
                return float.MinValue;

            var xLerp = z % gridSize;
            var zLerp = x % gridSize;

            //todo:也许后面要改成和绘制一致的三角形内插
            var h0 = (float)(PxHeightfieldSamples[zGrid * HeightfieldWidth + xGrid].height) * PxHeightfieldScale;
            var h1 = (float)(PxHeightfieldSamples[(zGrid + 1) * HeightfieldWidth + xGrid].height) * PxHeightfieldScale;
            var h2 = (float)(PxHeightfieldSamples[(zGrid + 1) * HeightfieldWidth + xGrid + 1].height) * PxHeightfieldScale;
            var h3 = (float)(PxHeightfieldSamples[zGrid * HeightfieldWidth + xGrid + 1].height) * PxHeightfieldScale;
            var zT1 = CoreDefine.FloatLerp(h0, h1, zLerp);
            var zT2 = CoreDefine.FloatLerp(h3, h2, zLerp);
            return CoreDefine.FloatLerp(zT1, zT2, xLerp) + (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
        }
        #endregion
        public void SetAcceptShadow(bool value)
        {
            foreach (var i in TiledPatch)
            {
                i.SetAcceptShadow(value);
            }
        }
        public void OnAbsTransformChanged(UTerrainNode node, GamePlay.UWorld world)
        {
            foreach (var i in TiledPatch)
            {
                i.OnAbsTransformChanged(node, world);
            }
        }
        public void UpdateCameraOffset(GamePlay.UWorld world)
        {
            foreach (var i in TiledPatch)
            {
                i.UpdateCameraOffset(world);
            }
        }
        public unsafe void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            foreach (var i in TiledPatch)
            {
                if (rp.CullCamera.WhichContainTypeFast(rp.World, i.AABB, false) == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                    continue;
                i.OnGatherVisibleMeshes(rp);
            }
        }
        public void Tick(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            foreach (var i in TiledPatch)
            {
                i.Tick(world, policy);
            }
        }
        public void OnParentSceneChanged(GamePlay.Scene.UScene prev, GamePlay.Scene.UScene cur)
        {
            if (PhyActor == null)
                return;
            //if (UEngine.Instance != null)
            {
                //if (UEngine.Instance.PlayMode != EPlayMode.Editor || UEngine.Instance.PlayMode != EPlayMode.Cook)
                {
                    if (cur != null)
                    {
                        PhyActor.AddToScene(cur.PxSceneMB.PxScene);
                    }
                    else
                    {
                        PhyActor.AddToScene(null);
                    }
                }
            }
        }

        public void FrustumCull(GamePlay.UWorld.UVisParameter rp, List<UPatch> patches)
        {
            foreach (var i in TiledPatch)
            {
                if (rp.CullCamera.WhichContainTypeFast(rp.World, i.AABB, false) == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                    continue;

                patches.Add(i);
            }
        }
        #region CDLOD
        public void SetLODLevel(int level)
        {
            foreach(var i in TiledPatch)
            {
                i.CurrentLOD = level;
            }
        }
        public unsafe double SphereCover(in DBoundingSphere sphere, int level, int maxLevel)
        {
            double maxDist = 0;
            DVector3* boxCorner = stackalloc DVector3[8];
            foreach (var i in TiledPatch)
            {
                if (i.CurrentLOD == maxLevel)
                {
                    if (DBoundingBox.Intersects(in i.AABB, sphere))
                    {
                        i.CurrentLOD = level;
                        i.AABB.GetCornersUnsafe(boxCorner);
                        for (int k = 0; k < 8; k++)
                        {
                            double d = DVector3.DistanceSquared(in boxCorner[k], in sphere.Center);
                            if (d >= maxDist)
                            {
                                maxDist = d;
                            }
                        }
                    }
                }
            }

            return maxDist;
        }
        #endregion
    }
}
