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

        public UTerrainMaterialIdManager GetMaterialIdManager()
        {
            return Node?.TerrainMaterialIdManager;
        }
    }
    public class UTerrainLevelData
    {
        public UTerrainLevel Level;
        public UPatch[,] TiledPatch;
        public RHI.CShaderResourceView HeightMapSRV;
        public RHI.CShaderResourceView NormalMapSRV;
        public RHI.CShaderResourceView MaterialIdMapSRV;
        public float HeightMapMinHeight;
        public float HeightMapMaxHeight;
        
        public int HeightfieldWidth;
        public int HeightfieldHeight;
        public PhyHeightFieldSample[] PxHeightfieldSamples;
        public float PxHeightfieldScale;
        public Bricks.PhysicsCore.UPhyHeightfield PhyHeightfield { get; protected set; }
        public Bricks.PhysicsCore.UPhyActor PhyActor { get; protected set; }
        public float RemainUnloadTime = 10.0f;
        public UTerainPlantManager PlantManager = new UTerainPlantManager();
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
            MaterialIdMapSRV?.Dispose();
            MaterialIdMapSRV = null;
        }
        public UTerrainNode GetTerrainNode()
        {
            return Level.Node;
        }
        public async System.Threading.Tasks.Task CreateLevelData(UTerrainLevel level, bool bForce)
        {
            Level = level;
            
            if (bForce)
            {
                BuildLevelDataFromPGC(Level.Node.GetNodeData<UTerrainNode.UTerrainData>().PgcName);
            }
            else
            {
                await UEngine.Instance.EventPoster.Post(() =>
                {
                    BuildLevelDataFromPGC(Level.Node.GetNodeData<UTerrainNode.UTerrainData>().PgcName);
                    return true;
                }, Thread.Async.EAsyncTarget.AsyncIO);
            }

            OnParentSceneChanged(null, Level.Node.ParentScene);
        }
        //bool bTryLoad = false;
        protected bool LoadLevelFromCache(string file, in Hash160 testHash)
        {
            //if (bTryLoad == false)
            //    return false;
            var patchSide = Level.PatchSide;
            var terrainGen = Level.Node.TerrainGen;
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.UMaterialIdMapNode;
            IdMapNode.InitProcedure(terrainGen.AssetGraph);

            var hMap = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
            var norMap = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<Vector3, Procedure.FFloat3Operator>>(1, 1, 1));
            var idMap = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
            var transform = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<FTransform, Procedure.FTransformOperator>>(1, 1, 1));
            var plants = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<Int32_2, Procedure.FInt2Operator>>(1, 1, 1));

            using (var xnd = IO.CXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return false;

                var attr = xnd.RootNode.TryGetAttribute("Desc");
                if (attr.IsValidPointer == false)
                    return false;

                var ar = attr.GetReader(null);
                Hash160 hash;
                ar.Read(out hash);
                attr.ReleaseReader(ref ar);
                if (hash != testHash)
                    return false;

                var node = xnd.RootNode.TryGetChildNode("HeightMap");
                if (node.IsValidPointer == false)
                    return false;
                hMap.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("NormalMap");
                if (node.IsValidPointer == false)
                    return false;
                norMap.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("MatIdMap");
                if (node.IsValidPointer == false)
                    return false;
                idMap.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("PlantTransform");
                if (node.IsValidPointer == false)
                    return false;
                transform.LoadXnd(node, Hash160.Emtpy);

                node = xnd.RootNode.TryGetChildNode("PlantInfo");
                if (node.IsValidPointer == false)
                    return false;
                plants.LoadXnd(node, Hash160.Emtpy);
            }

            CreateFromBuffer(hMap, norMap, idMap, transform, plants);
            return true;
        }

        private void CreateFromBuffer(Procedure.UBufferConponent hMap, Procedure.UBufferConponent norMap, 
            Procedure.UBufferConponent idMap, Procedure.UBufferConponent transform, Procedure.UBufferConponent plants)
        {
            var patchSide = Level.PatchSide;
            var terrainGen = Level.Node.TerrainGen;
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.UMaterialIdMapNode;

            TiledPatch = new UPatch[patchSide, patchSide];
            for (int i = 0; i < patchSide; i++)
            {
                for (int j = 0; j < patchSide; j++)
                {
                    TiledPatch[i, j] = new UPatch();
                    TiledPatch[i, j].Initialize(this, j, i, hMap);
                }
            }

            hMap.GetRangeUnsafe<float, Procedure.FFloatOperator>(out HeightMapMinHeight, out HeightMapMaxHeight);
            float HeightfieldMidHeight = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
            PxHeightfieldScale = 0.1f;//0.1f精度为分米
            HeightfieldWidth = hMap.Width;
            HeightfieldHeight = hMap.Height;
            PxHeightfieldSamples = new PhyHeightFieldSample[hMap.Width * hMap.Height];
            for (int i = 0; i < HeightfieldHeight; i++)
            {
                for (int j = 0; j < HeightfieldWidth; j++)
                {
                    float height = hMap.GetPixel<float>(i, j);
                    float localHeight = height - HeightfieldMidHeight;
                    PxHeightfieldSamples[i * HeightfieldWidth + j].height = (short)(localHeight / PxHeightfieldScale);//(short)((height * (float)short.MaxValue) / maxHeight);
                    PxHeightfieldSamples[i * HeightfieldWidth + j].materialIndex0 = 0;
                    PxHeightfieldSamples[i * HeightfieldWidth + j].materialIndex1 = 0;
                }
            }

            InitPhysics(Level);

            HeightMapSRV = hMap.CreateAsHeightMapTexture2D(HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
            var norImage = new Bricks.Procedure.UImage2D();
            norImage.Initialize(norMap.Width, norMap.Height,
                norMap as Procedure.USuperBuffer<Vector3, Procedure.FFloat3Operator>,
                null, 0);

            var idMapImage = new Bricks.Procedure.UImage2D();
            idMapImage.Initialize(idMap.Width, idMap.Height,
                idMap as Procedure.USuperBuffer<float, Procedure.FFloatOperator>,
                null,
                null,
                null);

            NormalMapSRV = norImage.CreateRGBA8Texture2DAsNormal(); //terrainGen.mResultNormalImage.CreateRGBA8Texture2DAsNormal();
            MaterialIdMapSRV = idMapImage.CreateRGBA8Texture2D(false);

            if (transform != null && plants != null)
            {
                for (int i = 0; i < plants.Width; i++)
                {
                    ref var t = ref transform.GetPixel<FTransform>(i, 0, 0);
                    ref var p = ref plants.GetPixel<Int32_2>(i, 0, 0);

                    var material = IdMapNode.MaterialIdManager.MaterialIdArray[p.X];
                    var plt = material.Plants[p.Y];
                    PlantManager.AddPlant(this.Level.Node, in Level.StartPosition, plt, in t, plants.Width);
                }
            }
        }
        protected unsafe void SaveLevelToCache(string file, in Hash160 hash)
        {
            var terrainGen = Level.Node.TerrainGen;
            var hMap = terrainGen.AssetGraph.Root.GetResultBuffer(0);
            var norMap = terrainGen.AssetGraph.Root.GetResultBuffer(1);
            var idMap = terrainGen.AssetGraph.Root.GetResultBuffer(2);
            var transform = terrainGen.AssetGraph.Root.GetResultBuffer(3);
            var plants = terrainGen.AssetGraph.Root.GetResultBuffer(4);

            using (var xnd = new IO.CXndHolder("TrLevel", 0, 0))
            {
                using (var attr = xnd.NewAttribute("Desc", 0, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    var ar = attr.GetWriter(24);
                    ar.Write(hash);
                    attr.ReleaseWriter(ref ar);
                }

                using (var node = xnd.NewNode("HeightMap", 0, 0))
                {
                    xnd.RootNode.AddNode(node);
                    hMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                }

                using (var node = xnd.NewNode("NormalMap", 0, 0))
                {
                    xnd.RootNode.AddNode(node);
                    norMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                }

                using (var node = xnd.NewNode("MatIdMap", 0, 0))
                {
                    xnd.RootNode.AddNode(node);
                    idMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                }

                using (var node = xnd.NewNode("PlantTransform", 0, 0))
                {
                    xnd.RootNode.AddNode(node);
                    transform.SaveXnd(xnd, node, in Hash160.Emtpy);
                }

                using (var node = xnd.NewNode("PlantInfo", 0, 0))
                {
                    xnd.RootNode.AddNode(node);
                    plants.SaveXnd(xnd, node, in Hash160.Emtpy);
                }

                xnd.SaveXnd(file);
            }   
        }
        protected void BuildLevelDataFromPGC(RName terrainName)
        {
            var patchSide = Level.PatchSide;
            var terrainGen = Level.Node.TerrainGen;

            var dir = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Cache) + "terrain/";
            dir = IO.FileManager.CombinePath(dir, terrainName.Name);
            IO.FileManager.SureDirectory(dir);
            var lvlFile = $"{dir}/X{Level.LevelX}_Y{Level.LevelZ}.trlvl";
            if (terrainName != null)
            {
                if (LoadLevelFromCache(lvlFile, Level.Node.TerrainGenHash) == true)
                {
                    return;
                }
            }
            
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.UMaterialIdMapNode;
            //if (IdMapNode != null)
            //    await IdMapNode.SureMaterialResources();

            var noise1 = terrainGen.AssetGraph.FindFirstNode("NoisePerlin1") as Procedure.Node.UNoisePerlin;
            var noise2 = terrainGen.AssetGraph.FindFirstNode("NoisePerlin2") as Procedure.Node.UNoisePerlin;
            noise1.StartPosition = Level.StartPosition;
            noise2.StartPosition = Level.StartPosition;

            //make border guard 2 + 1024 + 3
            noise1.StartPosition.X -= noise1.Border;
            noise1.StartPosition.Z -= noise1.Border;

            noise2.StartPosition.X -= noise1.Border;
            noise2.StartPosition.Z -= noise1.Border;

            var trans = terrainGen.AssetGraph.FindFirstNode("Plants") as Procedure.Node.UTransformBuilder;
            if (trans != null)
            {
                trans.FinalRandomSeed = trans.RandomSeed + Level.LevelZ * Level.Node.NumOfLevelX + Level.LevelX;
                trans.GridSize = Level.Node.GridSize;
                trans.Offset = Level.StartPosition;
            }

            terrainGen.Compile();
            var hMap = terrainGen.AssetGraph.Root.GetResultBuffer(0);
            var norMap = terrainGen.AssetGraph.Root.GetResultBuffer(1) as Procedure.USuperBuffer<Vector3, Procedure.FFloat3Operator>;
            var idMap = terrainGen.AssetGraph.Root.GetResultBuffer(2) as Procedure.USuperBuffer<float, Procedure.FFloatOperator>;
            var transform = terrainGen.AssetGraph.Root.GetResultBuffer(3) as Procedure.USuperBuffer<FTransform, Procedure.FTransformOperator>;
            var plants = terrainGen.AssetGraph.Root.GetResultBuffer(4) as Procedure.USuperBuffer<Int32_2, Procedure.FInt2Operator>;
            CreateFromBuffer(hMap, norMap, idMap, transform, plants);
            
            SaveLevelToCache(lvlFile, Level.Node.TerrainGenHash);
            terrainGen.AssetGraph.BufferCache.ResetCache();
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
