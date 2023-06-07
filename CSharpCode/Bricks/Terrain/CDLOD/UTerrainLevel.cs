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
    public class UTerrainLevelData : IDisposable
    {
        public UTerrainLevel Level;
        public UPatch[,] TiledPatch;
        public NxRHI.USrView HeightMapSRV;
        public NxRHI.USrView WaterHMapSRV;
        public NxRHI.USrView NormalMapSRV;
        public NxRHI.USrView MaterialIdMapSRV;
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
            Dispose();
        }
        public void Dispose()
        {
            PhyActor?.AddToScene(null);
            PhyActor = null;

            if (TiledPatch != null)
            {
                foreach (var i in TiledPatch)
                {
                    i.Dispose();
                }
                TiledPatch = null;
            }

            CoreSDK.DisposeObject(ref PlantManager);

            CoreSDK.DisposeObject(ref HeightMapSRV);
            CoreSDK.DisposeObject(ref NormalMapSRV);
            CoreSDK.DisposeObject(ref MaterialIdMapSRV);
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
                BuildLevelDataFromPGC(Level.Node.GetNodeData<UTerrainNode.UTerrainData>());
            }
            else
            {
                await UEngine.Instance.EventPoster.Post((state) =>
                {
                    BuildLevelDataFromPGC(Level.Node.GetNodeData<UTerrainNode.UTerrainData>());
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
            var waterMap = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
            var idMap = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
            var transform = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<FTransform, Procedure.FTransformOperator>>(1, 1, 1));
            var plants = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<Vector2i, Procedure.FInt2Operator>>(1, 1, 1));

            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                if (xnd == null)
                    return false;

                var attr = xnd.RootNode.TryGetAttribute("Desc");
                if (attr.IsValidPointer == false)
                    return false;

                Hash160 hash;
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out hash);
                }
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

                node = xnd.RootNode.TryGetChildNode("WaterMap");
                if (node.IsValidPointer == false)
                {
                    waterMap.LoadXnd(node, Hash160.Emtpy);
                }
                else
                {
                    waterMap = null;
                }

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

                CreateFromBuffer(hMap, norMap, waterMap, idMap, transform, plants);

                node = xnd.RootNode.TryGetChildNode("GrassInfo");
                if (node.IsValidPointer)
                {
                    var nodeCount = node.GetNumOfNode();
                    for(uint i=0; i<nodeCount; i++)
                    {
                        var subNode = node.GetNode(i);
                        if (subNode.IsValidPointer == false)
                            continue;
                        var gAtt = subNode.TryGetAttribute("GrassData");
                        var grassData = new UTerrainGrass();
                        if (!gAtt.IsValidPointer)
                            continue;

                        using (var gReader = gAtt.GetReader(grassData))
                        {
                            IO.ISerializer serializer;
                            gReader.Read(out serializer, null);
                            grassData = (UTerrainGrass)serializer;
                        }

                        var gBuffer = Procedure.UBufferConponent.CreateInstance(Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(1, 1, 1));
                        gBuffer.LoadXnd(subNode, Hash160.Emtpy);

                        UpdateGrass(grassData, gBuffer);
                    }
                }
            }

            return true;
        }

        private void CreateFromBuffer(Procedure.UBufferConponent hMap, Procedure.UBufferConponent norMap, Procedure.UBufferConponent waterMap,
            Procedure.UBufferConponent idMap, Procedure.UBufferConponent transform, Procedure.UBufferConponent plants)
        {
            UpdateHeightMap(hMap);

            UpdateNormalMap(norMap);

            UpdateWaterMap(waterMap);

            UpdateMaterialIdMap(idMap);

            UpdatePlants(transform, plants);
        }

        public void UpdateHeightMap(Procedure.UBufferConponent hMap)
        {
            var patchSide = Level.PatchSide;
            TiledPatch = new UPatch[patchSide, patchSide];
            for (int i = 0; i < patchSide; i++)
            {
                for (int j = 0; j < patchSide; j++)
                {
                    if (TiledPatch[i, j] == null)
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
            HeightMapSRV.SetDebugName("HeightMapSRV");
            HeightMapSRV.SetDebugName("HeightMapSRV");
        }

        public void UpdateWaterMap(Procedure.UBufferConponent waterMap)
        {
            if (waterMap == null)
                return;
            WaterHMapSRV = waterMap.CreateAsHeightMapTexture2D(HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
        }
        
        public void UpdateNormalMap(Procedure.UBufferConponent norMap)
        {
            var norImage = new Bricks.Procedure.UImage2D();
            norImage.Initialize(norMap.Width, norMap.Height,
                norMap as Procedure.USuperBuffer<Vector3, Procedure.FFloat3Operator>,
                null, 0);

            NormalMapSRV = norImage.CreateRGBA8Texture2DAsNormal(); //terrainGen.mResultNormalImage.CreateRGBA8Texture2DAsNormal();
        }
        public void UpdateMaterialIdMap(Procedure.UBufferConponent idMap)
        {
            if (idMap == null)
                return;

            var idMapImage = new Bricks.Procedure.UImage2D();
            idMapImage.Initialize(idMap.Width, idMap.Height,
                idMap as Procedure.USuperBuffer<float, Procedure.FFloatOperator>,
                null,
                null,
                null);

            MaterialIdMapSRV = idMapImage.CreateRGBA8Texture2D(false);
        }
        public void UpdatePlants(Procedure.UBufferConponent transform, Procedure.UBufferConponent plants)
        {
            var terrainGen = Level.Node.TerrainGen;
            var IdMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.UMaterialIdMapNode;
            if (transform != null && plants != null)
            {
                for (int i = 0; i < plants.Width; i++)
                {
                    ref var t = ref transform.GetPixel<FTransform>(i, 0, 0);
                    ref var p = ref plants.GetPixel<Vector2i>(i, 0, 0);

                    var material = IdMapNode.MaterialIdManager.MaterialIdArray[p.X];
                    var plt = material.Plants[p.Y];
                    PlantManager.AddPlant(this.Level.Node, in Level.StartPosition, plt, in t, plants.Width);
                }
            }
        }
        //////////////////////////////////////////////////
        //static bool GAdded = false;
        //////////////////////////////////////////////////
        public void UpdateGrass(Procedure.Node.UGrassNode node)
        {
            for(int i=0; i<node.Inputs.Count; i++)
            {
                var grassData = node.GrassDefines[i].GrassData;
                var buffer = node.GetResultBuffer(i);
                UpdateGrass(grassData, buffer);
            }
        }
        public void UpdateGrass(UTerrainGrass grassData, Procedure.UBufferConponent buffer)
        {
            if (buffer == null)
                return;
            var patchSide = Level.PatchSide;
            float weightMin, weightMax;
            buffer.GetRangeUnsafe<float, Procedure.FFloatOperator>(out weightMin, out weightMax);
            for (int patchIdxY = 0; patchIdxY < patchSide; patchIdxY++)
            {
                for (int patchIdxX = 0; patchIdxX < patchSide; patchIdxX++)
                {
                    if (TiledPatch[patchIdxY, patchIdxX] == null)
                        continue;
                    var patchOffset = new DVector3(
                        patchIdxX * GetTerrainNode().PatchSize + Level.StartPosition.X,
                        Level.StartPosition.Y,
                        patchIdxY * GetTerrainNode().PatchSize + Level.StartPosition.Z);
                    TiledPatch[patchIdxY, patchIdxX].GrassManager.AddGrass(patchOffset, grassData, buffer, weightMin, weightMax);
                }
            }
        }
        //public unsafe void UpdateGrass(Procedure.UBufferConponent grass)
        //{
        //    if (grass == null || grass.Width <= 0)
        //        return;

        //    var terrainGen = Level.Node.TerrainGen;
        //    var idMapNode = terrainGen.AssetGraph.FindFirstNode("MatIdMapping") as Procedure.Node.UMaterialIdMapNode;
        //    var grassAdr = grass.GetSuperPixelAddress(0, 0, 0);
        //    float minScale = float.MaxValue;
        //    float maxScale = float.MinValue;
        //    //////////////////////////////////////////////////
        //    //DVector3 maxPos = new DVector3(double.MinValue, double.MinValue, double.MinValue);
        //    //DVector3 minPos = new DVector3(double.MaxValue, double.MaxValue, double.MaxValue);
        //    //////////////////////////////////////////////////
        //    for (int i=0; i<grass.Width; i++)
        //    {
        //        FGrassTransformData* tData = (FGrassTransformData*)(grassAdr + i * sizeof(FGrassTransformData));
        //        var scale = tData->Transform.Scale.Y;
        //        if(scale < minScale)
        //            minScale = scale;
        //        if(scale > maxScale)
        //            maxScale = scale;
        //        //////////////////////////////////////////////////
        //        //if (maxPos.X < tData->Transform.Position.X)
        //        //    maxPos.X = tData->Transform.Position.X;
        //        //if(maxPos.Y < tData->Transform.Position.Y)
        //        //    maxPos.Y = tData->Transform.Position.Y;
        //        //if(maxPos.Z < tData->Transform.Position.Z)
        //        //    maxPos.Z = tData->Transform.Position.Z;
        //        //if(minPos.X > tData->Transform.Position.X)
        //        //    minPos.X = tData->Transform.Position.X;
        //        //if(minPos.Y > tData->Transform.Position.Y)
        //        //    minPos.Y = tData->Transform.Position.Y;
        //        //if(minPos.Z > tData->Transform.Position.Z)
        //        //    minPos.Z = tData->Transform.Position.Z;
        //        //////////////////////////////////////////////////
        //    }
        //    for(int i=0; i<grass.Width; i++)
        //    {
        //        //////////////////////////////////////////////////
        //        //if (i > 1 || GAdded)
        //        //{
        //        //    GAdded = true;
        //        //    break;
        //        //}
        //        //////////////////////////////////////////////////
        //        ref var p = ref grass.GetPixel<FGrassTransformData>(i, 0, 0);
        //        var levelSize = Level.PatchSide * Level.Node.PatchSize;
        //        if((p.Transform.Position.X < Level.StartPosition.X) || (p.Transform.Position.X > Level.StartPosition.X + levelSize) ||
        //           (p.Transform.Position.Z < Level.StartPosition.Z) || (p.Transform.Position.Z > Level.StartPosition.Z + levelSize))
        //                continue;

        //        var material = idMapNode.MaterialIdManager.MaterialIdArray[(int)p.MaterialIdx];
        //        var grs = material.Grasses[(int)p.GrassIdx];
        //        grs.MinScale = minScale;
        //        grs.MaxScale = maxScale;
        //        // to patch
        //        var patchIdxX = (int)((p.Transform.Position.X - Level.StartPosition.X) / GetTerrainNode().PatchSize);
        //        var patchIdxY = (int)((p.Transform.Position.Z - Level.StartPosition.Z) / GetTerrainNode().PatchSize);

        //        //////////////////////////////////////////////////
        //        //if (patchIdxX != 0 || patchIdxY != 1)
        //        //    continue;
        //        //patchIdxX = 1;
        //        //patchIdxY = 1;
        //        //p.Transform.Position = new DVector3(patchIdxX * Level.Node.PatchSize, 0, patchIdxY * Level.Node.PatchSize) + Level.StartPosition;
        //        //p.Transform.Scale = new Vector3(minScale);
        //        //p.Transform.Quat = Quaternion.Identity;
        //        //if (i == 1)
        //        //{
        //        //    p.Transform.Position += new DVector3(40, 0, 0);
        //        //    p.Transform.Scale = new Vector3(maxScale);
        //        //    p.Transform.Quat = Quaternion.RotationAxis(Vector3.Up, (float)(System.Math.PI * 0.25));
        //        //}
        //        //////////////////////////////////////////////////

        //        if (Level.PatchSide <= patchIdxX || Level.PatchSide <= patchIdxY)
        //            continue;
        //        var patch = TiledPatch[patchIdxY, patchIdxX];
        //        var patchOffset = new DVector3(
        //            patchIdxX * GetTerrainNode().PatchSize + Level.StartPosition.X,
        //            Level.StartPosition.Y,
        //            patchIdxY * GetTerrainNode().PatchSize + Level.StartPosition.Z);
        //        patch.GrassManager.AddGrass(patchOffset, grs, p.Transform, grass.Width);
        //    }
        //}
        protected unsafe void SaveLevelToCache(string file, in Hash160 hash)
        {
            var terrainGen = Level.Node.TerrainGen;
            var hMap = terrainGen.AssetGraph.Root.GetResultBuffer("Height");
            var norMap = terrainGen.AssetGraph.Root.GetResultBuffer("Normal");
            var idMap = terrainGen.AssetGraph.Root.GetResultBuffer("MatId");
            var waterMap = terrainGen.AssetGraph.Root.GetResultBuffer("Water");
            var transform = terrainGen.AssetGraph.Root.GetResultBuffer("Transform");
            var plants = terrainGen.AssetGraph.Root.GetResultBuffer("Plants");

            var grassPin = terrainGen.AssetGraph.Root.FindPinIn("Grass");
            Procedure.Node.UGrassNode linkedGrassNode = null;
            if (grassPin != null)
            {
                linkedGrassNode = terrainGen.AssetGraph.Root.GetInputNode(terrainGen.AssetGraph.Root.ParentGraph as Procedure.UPgcGraph, grassPin) as Procedure.Node.UGrassNode;
            }

            using (var xnd = new IO.TtXndHolder("TrLevel", 0, 0))
            {
                using (var attr = xnd.NewAttribute("Desc", 0, 0))
                {
                    xnd.RootNode.AddAttribute(attr);
                    using (var ar = attr.GetWriter(24))
                    {
                        ar.Write(hash);
                    }
                }

                if (hMap != null)
                {
                    using (var node = xnd.NewNode("HeightMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        hMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (hMap != null)
                {
                    using (var node = xnd.NewNode("HeightMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        hMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (norMap != null)
                {
                    using (var node = xnd.NewNode("NormalMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        norMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (idMap != null)
                {
                    using (var node = xnd.NewNode("MatIdMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        idMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (waterMap != null)
                {
                    using (var node = xnd.NewNode("WaterMap", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        waterMap.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (transform != null)
                {
                    using (var node = xnd.NewNode("PlantTransform", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        transform.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if (plants != null)
                {
                    using (var node = xnd.NewNode("PlantInfo", 0, 0))
                    {
                        xnd.RootNode.AddNode(node);
                        plants.SaveXnd(xnd, node, in Hash160.Emtpy);
                    }
                }

                if(linkedGrassNode != null)
                {
                    using (var node = xnd.NewNode("GrassInfo", 0, 0))
                    {
                        for (int i = 0; i < linkedGrassNode.Inputs.Count; i++)
                        {
                            using (var subNode = xnd.NewNode("data", 0, 0))
                            {
                                var grassData = linkedGrassNode.GrassDefines[i].GrassData;
                                using (var gAtt = xnd.NewAttribute("GrassData", 0, 0))
                                {
                                    using (var gWriter = gAtt.GetWriter(0))
                                    {
                                        IO.SerializerHelper.Write(gWriter, grassData);
                                    }
                                    subNode.AddAttribute(gAtt);
                                }

                                var buffer = linkedGrassNode.GetResultBuffer(i);
                                buffer.SaveXnd(xnd, subNode, in Hash160.Emtpy);

                                node.AddNode(subNode);
                            }
                        }
                        xnd.RootNode.AddNode(node);
                    }
                }

                xnd.SaveXnd(file);
            }   
        }
        protected void BuildLevelDataFromPGC(UTerrainNode.UTerrainData nodeData)
        {
            RName terrainName = nodeData.PgcName;
            if (terrainName == null)
            {
                BuildEmptyLevelData(nodeData.LevelSideX, nodeData.LevelSideZ);
                return;
            }
            var patchSide = Level.PatchSide;
            var terrainGen = Level.Node.TerrainGen;

            var dir = UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "terrain/";
            dir = IO.TtFileManager.CombinePath(dir, terrainName.Name);
            IO.TtFileManager.SureDirectory(dir);
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
            if(noise1!=null)
            {
                noise1.StartPosition = Level.StartPosition;
                noise1.StartPosition.X -= noise1.Border;
                noise1.StartPosition.Z -= noise1.Border;
            }
            //var noise2 = terrainGen.AssetGraph.FindFirstNode("NoisePerlin2") as Procedure.Node.UNoisePerlin;
            //if (noise2 != null)
            //{
            //    noise2.StartPosition = Level.StartPosition;
            //    noise2.StartPosition.X -= noise1.Border;
            //    noise2.StartPosition.Z -= noise1.Border;
            //}

            var trans = terrainGen.AssetGraph.FindFirstNode("Plants") as Procedure.Node.UTransformBuilder;
            if (trans != null)
            {
                trans.FinalRandomSeed = trans.RandomSeed + Level.LevelZ * Level.Node.NumOfLevelX + Level.LevelX;
                trans.GridSize = Level.Node.GridSize;
                trans.Offset = Level.StartPosition;
            }

            var root = terrainGen.AssetGraph.Root;
            terrainGen.Compile(root);
            var hMap = root.GetResultBuffer("Height");
            var norMap = root.GetResultBuffer("Normal") as Procedure.USuperBuffer<Vector3, Procedure.FFloat3Operator>;
            var idMap = root.GetResultBuffer("MatId") as Procedure.USuperBuffer<float, Procedure.FFloatOperator>;
            var waterMap = root.GetResultBuffer("Water") as Procedure.USuperBuffer<float, Procedure.FFloatOperator>;
            var transform = root.GetResultBuffer("Transform") as Procedure.USuperBuffer<FTransform, Procedure.FTransformOperator>;
            var plants = root.GetResultBuffer("Plants") as Procedure.USuperBuffer<Vector2i, Procedure.FInt2Operator>;
            CreateFromBuffer(hMap, norMap, waterMap, idMap, transform, plants);
            var grassPin = root.FindPinIn("Grass");
            if (grassPin != null)
            {
                var tagNode = root.GetInputNode(root.ParentGraph as Procedure.UPgcGraph, grassPin) as Procedure.Node.UGrassNode;
                if(tagNode != null)
                    UpdateGrass(tagNode);
            }
            
            SaveLevelToCache(lvlFile, Level.Node.TerrainGenHash);
            terrainGen.AssetGraph.BufferCache.ResetCache();
        }
        protected void BuildEmptyLevelData(int xSize, int ySize)
        {
            var creator = Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(xSize, ySize, 1);
            var hMap = Procedure.UBufferConponent.CreateInstance(creator);
            var waterMap = Procedure.UBufferConponent.CreateInstance(creator);
            creator = Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<Vector3, Procedure.FFloat3Operator>>(xSize, ySize, 1);
            var norMap = Procedure.UBufferConponent.CreateInstance(creator);
            creator = Procedure.UBufferCreator.CreateInstance<Procedure.USuperBuffer<float, Procedure.FFloatOperator>>(xSize, ySize, 1);
            var idMap = Procedure.UBufferConponent.CreateInstance(creator);

            CreateFromBuffer(hMap, norMap, waterMap, idMap, null, null);
        }
        #region Px & Collide
        public unsafe void InitPhysics(UTerrainLevel level)
        {
            var pc = UEngine.Instance.PhyModule.PhyContext;
            int texSize = level.Node.TexSizePerPatch * level.Node.PatchSide;
            fixed (PhyHeightFieldSample* pPixelData = &PxHeightfieldSamples[0])
            {
                PhyHeightfield = pc.CookHeightfield(texSize, texSize, pPixelData, 0, false);
                var materials = new Bricks.PhysicsCore.UPhyMaterial[1];
                materials[0] = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
                var terrainShape = pc.CreateShapeHeightfield(materials,
                    PhyHeightfield, PxHeightfieldScale, in Vector3.One);
                PhyFilterData SimulationFilterData = new PhyFilterData();
                terrainShape.mCoreObject.SetQueryFilterData(SimulationFilterData);
                terrainShape.mCoreObject.SetSimulationFilterData(SimulationFilterData);
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

            var x2 = xGrid + 1;
            if (x2 >= HeightfieldWidth - 1)
                x2 = xGrid;
            var z2 = zGrid + 1;
            if (z2 >= HeightfieldHeight - 1)
                z2 = zGrid;

            //todo:也许后面要改成和绘制一致的三角形内插
            var h0 = (float)(PxHeightfieldSamples[zGrid * HeightfieldWidth + xGrid].height) * PxHeightfieldScale;
            var h1 = (float)(PxHeightfieldSamples[(z2) * HeightfieldWidth + xGrid].height) * PxHeightfieldScale;
            var h2 = (float)(PxHeightfieldSamples[(z2) * HeightfieldWidth + x2].height) * PxHeightfieldScale;
            var h3 = (float)(PxHeightfieldSamples[zGrid * HeightfieldWidth + x2].height) * PxHeightfieldScale;
            var zT1 = MathHelper.Lerp(h0, h1, zLerp);
            var zT2 = MathHelper.Lerp(h3, h2, zLerp);
            return MathHelper.Lerp(zT1, zT2, xLerp) + (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
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
