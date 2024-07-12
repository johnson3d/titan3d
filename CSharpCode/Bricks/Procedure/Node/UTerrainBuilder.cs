using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("MaterialIdMap", "Terrain\\MaterialIdMap", UPgcGraph.PgcEditorKeyword)]
    public partial class UMaterialIdMapNode : UPgcNodeBase
    {
        [System.ComponentModel.Browsable(false)]
        public PinOut IdMapPin { get; set; } = new PinOut();
        public UMaterialIdMapNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(IdMapPin, "Self", null, "IdMap");
        }
        [Rtti.Meta]
        public Terrain.CDLOD.UTerrainMaterialIdManager MaterialIdManager { get; } = new Terrain.CDLOD.UTerrainMaterialIdManager();
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            foreach (var i in MaterialIdManager.MaterialIdArray)
            {
                ameta.AddReferenceAsset(i.TexDiffuse);
                ameta.AddReferenceAsset(i.TexNormal);
            }
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return null;
        }
        public override Hash160 GetOutBufferHash(PinOut pin)
        {
            if (pin == IdMapPin)
            {
                string hashStr = "";
                foreach (var i in MaterialIdManager.MaterialIdArray)
                {
                    hashStr += i.ToString();
                }
                return Hash160.CreateHash160(hashStr);
            }
            return Hash160.Emtpy;
        }
        public override bool InitProcedure(UPgcGraph graph)
        {
            base.InitProcedure(graph);
            if (MaterialIdManager.MaterialIdArray.Count == 0)
            {
                var dft = new Terrain.CDLOD.UTerrainMaterialId();
                dft.TexDiffuse = UEngine.Instance.GfxDevice.TextureManager.DefaultTexture.AssetName;
                MaterialIdManager.MaterialIdArray.Add(dft);
            }
            foreach (var i in MaterialIdManager.MaterialIdArray)
            {
                i.UpdateTotalPlantDensity();
            }
            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            return true;
        }
        public async System.Threading.Tasks.Task SureMaterialResources()
        {
            //MaterialIdManager.BuildSRV(UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            foreach (var i in MaterialIdManager.MaterialIdArray)
            {
                foreach (var j in i.Plants)
                {
                    await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(j.MeshName);
                }
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("HeightMapping", "Terrain\\HeightMapping", UPgcGraph.PgcEditorKeyword)]
    public partial class UHeightMappingNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn BezierPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn IdMapPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UHeightMappingNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", Float1Desc);
            AddInput(BezierPin, "Bezier", null, "Bezier");
            AddInput(IdMapPin, "IdMap", Float1Desc, "IdMap");
            AddOutput(ResultPin, "Result", OutputFloat1Desc);
        }
        [Rtti.Meta]
        public int SamplerNum { get; set; } = 20;
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HeightPin);
                if (buffer != null)
                {
                    OutputFloat1Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat1Desc;
                }
            }
            return null;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var bzNode = GetInputNode(graph, BezierPin) as UBezier;
            var idMapNode = GetInputNode(graph, IdMapPin) as UMaterialIdMapNode;
            var heightComp = graph.BufferCache.FindBuffer(HeightPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);

            var heightMapHash = heightComp.CalcPixelHash();
            var bzHash = bzNode.GetOutBufferHash(graph.FindInLinkerSingle(BezierPin).OutPin);
            var idMapHash = idMapNode.GetOutBufferHash(graph.FindInLinkerSingle(IdMapPin).OutPin);
            var testHash = Hash160.CreateHash160(heightMapHash.ToString() + bzHash.ToString() + idMapHash.ToString());
            if (this.TryLoadOutBufferFromCache(graph, ResultPin, in testHash))
            {
                return true;
            }

            var rangeX = (bzNode.MaxX - bzNode.MinX);
            var rangeY = (bzNode.MaxY - bzNode.MinY);

            var randObj = new Support.URandom();
            randObj.mCoreObject.SetSeed(20);
            resultComp.DispatchPixels((result, x, y, z) =>
            {
                var height = heightComp.GetPixel<float>(x, y, z);
                var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, height);// - bzNode.MinX);// ((double)j) * rangeX / (double)resultComp.Width);

                var norValue = (vzValue.Y - bzNode.MinY) / rangeY;
                norValue = MathHelper.Clamp(norValue, 0, 1);
                int idx = (int)((float)idMapNode.MaterialIdManager.MaterialIdArray.Count * norValue);
                if (idx >= idMapNode.MaterialIdManager.MaterialIdArray.Count)
                    idx = idMapNode.MaterialIdManager.MaterialIdArray.Count - 1;

                var MatId = idMapNode.MaterialIdManager.MaterialIdArray[idx];
                var counter = Sampler(bzNode, idMapNode, height, MatId.TransitionRange, SamplerNum);

                int total = 0;
                foreach (var i in counter)
                {
                    total += i.Num;
                }
                var rdValue = randObj.mCoreObject.NextValue16Bit() % total;
                total = 0;
                foreach (var i in counter)
                {
                    total += i.Num;
                    if (rdValue <= total)
                    {
                        result.SetPixel(x, y, z, (float)i.Id);
                        return;
                    }
                }

                //var rdValue = randObj.GetProbability()
                //float value = idx;
                result.SetPixel(x, y, z, (float)idx);
            }, true);

            this.SaveOutBufferToCache(graph, ResultPin, testHash);
            heightComp.LifeCount--;
            //MaterialIdManager.BuildSRV(UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            return true;
        }
        class MatIdCounter
        {
            public int Id;
            public int Num;
        }

        private List<MatIdCounter> Sampler(UBezier bzNode, UMaterialIdMapNode madIdNode, float height, float range, int Nums = 100)
        {
            var result = new List<MatIdCounter>(Nums);
            float step = range / Nums;
            var rangeY = (bzNode.MaxY - bzNode.MinY);
            height = height - range;
            for (int i = 0; i < Nums; i++)
            {
                var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, height + i * step);
                var norValue = (vzValue.Y - bzNode.MinY) / rangeY;
                norValue = MathHelper.Clamp(norValue, 0, 1);
                int idx = (int)((float)madIdNode.MaterialIdManager.MaterialIdArray.Count * norValue);
                if (idx >= madIdNode.MaterialIdManager.MaterialIdArray.Count)
                    idx = madIdNode.MaterialIdManager.MaterialIdArray.Count - 1;

                Add(idx, result);
            }
            result.Sort((x, y) =>
            {
                return x.Num.CompareTo(y.Num);
            });
            return result;
        }
        private void Add(int idx, List<MatIdCounter> lst)
        {
            foreach (var i in lst)
            {
                if (i.Id == idx)
                {
                    i.Num++;
                    return;
                }
            }
            var t = new MatIdCounter();
            t.Id = idx;
            t.Num = 1;
            lst.Add(t);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("HeightmapPreview", "Terrain\\HeightmapPreview", UPgcGraph.PgcEditorKeyword)]
    public partial class UHeightmapPreviewNode : UPgcNodeBase
    {
        [System.ComponentModel.Browsable(false)]
        public PinIn HMapPin { get; set; } = new PinIn();
        [System.ComponentModel.Browsable(false)]
        public PinOut HMapOutPin { get; set; } = new PinOut();
        [System.ComponentModel.Browsable(false)]
        public PinIn NormPin { get; set; } = new PinIn();
        [System.ComponentModel.Browsable(false)]
        public PinIn MatIdPin { get; set; } = new PinIn();
        [System.ComponentModel.Browsable(false)]
        public PinIn WaterPin { get; set; } = new PinIn();
        public UBufferCreator NormalBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UHeightmapPreviewNode()
        {
            PrevSize = new Vector2(100, 60);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HMapPin, "Height", Float1Desc);
            AddInput(NormPin, "Normal", NormalBufferCreator);
            AddInput(MatIdPin, "MatId", Float1Desc);
            AddInput(WaterPin, "Water", Float1Desc);

            AddOutput(HMapOutPin, "Height", Float1Desc);
            HMapOutPin.RefInput = HMapPin;
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return null;
        }
        public override Hash160 GetOutBufferHash(PinOut pin)
        {
            return Hash160.Emtpy;
        }
        public override bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            return true;
        }
        public Terrain.CDLOD.UTerrainSystem.EShowMode ShowMode
        {
            get
            {
                if (PreviewTerrainNode == null)
                    return Terrain.CDLOD.UTerrainSystem.EShowMode.Normal;
                return PreviewTerrainNode.Terrain.ShowMode;
            }
            set
            {
                if (PreviewTerrainNode == null)
                    return;
                PreviewTerrainNode.Terrain.ShowMode = value;
            }
        }

        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            //var ctrlPos = ParentGraph.CanvasToViewport(in prevStart);
            var ctrlPos = prevStart;
            ctrlPos -= ImGuiAPI.GetWindowPos();
            ImGuiAPI.SetCursorPos(in ctrlPos);
            ImGuiAPI.PushID($"{this.NodeId.ToString()}");
            //if (ImGuiAPI.Button("Preview"))
            //{
            //    var task = DoPreviewMesh();
            //}
            ImGuiAPI.PopID();
        }
        //Graphics.Mesh.UMeshPrimitives TerrainMesh;
        //Graphics.Mesh.TtMesh PreviewMesh = new Graphics.Mesh.TtMesh();
        Bricks.Terrain.CDLOD.UTerrainNode PreviewTerrainNode;
        public override async System.Threading.Tasks.Task DoPreviewMesh()
        {
            var graph = this.ParentGraph as UPgcGraph;

            //graph.Compile(this);

            var viewport = graph.GraphEditor.PreviewViewport;
            //if (TerrainMesh == null)
            {
                //var mdfType = Rtti.UTypeDesc.TypeOf(typeof(Terrain.CDLOD.UTerrainMdfQueuePermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>));

                //var Material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/SysDft.material", RName.ERNameType.Engine));
                //TerrainMesh = Graphics.Mesh.UMeshDataProvider.MakeGridForTerrain(256, 256).ToMesh();
                //var materials = new Graphics.Pipeline.Shader.UMaterial[1];
                //materials[0] = Material;
                //PreviewMesh.Initialize(TerrainMesh, materials, mdfType);

                //var trMdfQueue = PreviewMesh.MdfQueue as Terrain.CDLOD.UTerrainMdfQueue;
                //trMdfQueue.Dimension = 256;
                //var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, 
                //    new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), PreviewMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                //meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                //meshNode.NodeData.Name = "PreviewHeightMap";
                ////meshNode.IsAcceptShadow = false;
                ////meshNode.IsCastShadow = true;
            }

            if (PreviewTerrainNode == null)
            {
                //graph.Compile(this);

                graph.GraphEditor.PreviewRoot.ClearChildren();

                var terrainNode = new Bricks.Terrain.CDLOD.UTerrainNode();
                var terrainData = new Bricks.Terrain.CDLOD.UTerrainNode.UTerrainData();
                terrainData.NumOfLevelX = 1;
                terrainData.NumOfLevelZ = 1;
                terrainData.LODRangeFloat.Clear();
                terrainData.LODRangeFloat.Add(2048.0f);
                terrainData.LODRangeFloat.Add(4096.0f);
                terrainData.Name = "TerrainGen";
                terrainData.PgcName = graph.GraphEditor.PreviewPGC;
                await terrainNode.InitializeNode(viewport.World, terrainData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                terrainNode.Parent = graph.GraphEditor.PreviewRoot;
                terrainNode.Placement.Position = DVector3.Zero;
                terrainNode.IsCastShadow = false;
                terrainNode.IsAcceptShadow = false;
                terrainNode.SetActiveCenter(in DVector3.Zero);
                PreviewTerrainNode = terrainNode;
                PreviewTerrainNode.Terrain.ShowMode = Terrain.CDLOD.UTerrainSystem.EShowMode.Both;
            }
            else
            {
                graph.GraphEditor.PreviewRoot.ClearChildren();
                PreviewTerrainNode.Parent = graph.GraphEditor.PreviewRoot;
            }

            PreviewTerrainNode.UpdateAABB();
            var aabb = PreviewTerrainNode.AABB.ToSingleAABB();
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector();
            sphere.Radius = radius;
            graph.GraphEditor.PreviewViewport.RenderPolicy.DefaultCamera.AutoZoom(in sphere);

            var hMap = graph.BufferCache.FindBuffer(HMapPin);
            if (hMap == null)
                return;
            var level = PreviewTerrainNode.GetLevel(in DVector3.Zero);
            if (level.LevelData != null)
            {
                level.LevelData.UpdateHeightMap(hMap);

                var norMap = graph.BufferCache.FindBuffer(NormPin);
                if (norMap != null)
                {
                    level.LevelData.UpdateNormalMap(norMap);
                }

                var matIdMap = graph.BufferCache.FindBuffer(MatIdPin);
                if (matIdMap != null)
                {
                    level.LevelData.UpdateMaterialIdMap(matIdMap);
                }

                var waterMap = graph.BufferCache.FindBuffer(WaterPin);
                if (waterMap != null)
                {
                    if (hMap != null)
                        level.LevelData.UpdateAABB(hMap, waterMap);
                    PreviewTerrainNode.Terrain.IsShowWater = true;
                    level.LevelData.UpdateWaterMap(waterMap);
                }
                else
                {
                    PreviewTerrainNode.Terrain.IsShowWater = false;
                }

                //var HeightMapSRV = level.LevelData.HeightMapSRV;
                //float HeightMapMinHeight;
                //float HeightMapMaxHeight;
                //hMap.GetRangeUnsafe<float, Procedure.FFloatOperator>(out HeightMapMinHeight, out HeightMapMaxHeight);
                //float HeightfieldMidHeight = (HeightMapMinHeight + HeightMapMaxHeight) * 0.5f;
                //HeightMapSRV = hMap.CreateAsHeightMapTexture2D(HeightMapMinHeight, HeightMapMaxHeight, EPixelFormat.PXF_R16_FLOAT);
                //level.LevelData.HeightMapSRV = HeightMapSRV;
            }
        }
    }

    //https://archive.gamedev.net/archive/reference/articles/article2065.html
    //http://pcg.wikidot.com/pcg-algorithm%3afractal-river-basins
    //http://vterrain.org/Water/
    //https://www.researchgate.net/figure/Wireframe-Representation-of-Modeled-Terrain_fig2_220720125
    //http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/
    //https://zhuanlan.zhihu.com/p/95917609?native.theme=1
    [Bricks.CodeBuilder.ContextMenu("Water", "Terrain\\Water", UPgcGraph.PgcEditorKeyword)]
    public partial class UWaterNode : UPgcNodeBase
    {
        [System.ComponentModel.Browsable(false)]
        public PinIn HMapPin { get; set; } = new PinIn();
        [System.ComponentModel.Browsable(false)]
        public PinIn NormPin { get; set; } = new PinIn();
        [System.ComponentModel.Browsable(false)]
        public PinOut DeepPin { get; set; } = new PinOut();
        [System.ComponentModel.Browsable(false)]
        public PinOut WaterPin { get; set; } = new PinOut();
        [System.ComponentModel.Browsable(false)]
        public PinOut VelocityPin { get; set; } = new PinOut();
        public UBufferCreator NormalBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator VelocityBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloat1Creator { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UWaterNode()
        {
            PrevSize = new Vector2(100, 60);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HMapPin, "Height", OutputFloat1Creator);
            AddInput(NormPin, "Normal", NormalBufferCreator);
            AddOutput(DeepPin, "Deep", OutputFloat1Creator);
            AddOutput(WaterPin, "Water", OutputFloat1Creator);
            AddOutput(VelocityPin, "Velocity", VelocityBufferCreator);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (DeepPin == pin || WaterPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HMapPin);
                if (buffer != null)
                {
                    OutputFloat1Creator.SetSize(buffer.BufferCreator);
                    return OutputFloat1Creator;
                }
            }
            else if (VelocityPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HMapPin);
                if (buffer != null)
                {
                    VelocityBufferCreator.SetSize(buffer.BufferCreator);
                    return VelocityBufferCreator;
                }
            }
            return null;
        }
        [Rtti.Meta]
        public float WaterPerPixel { get; set; } = 1.0f;
        [Rtti.Meta]
        public int SimStep { get; set; } = 1;
        [Rtti.Meta]
        public int SimCount { get; set; } = 1;
        [Rtti.Meta]
        public int NoneWaterLimit { get; set; } = 0;
        private struct FPixelWater
        {
            public Vector2i MoveTarget;
            public float Water;
        }
        private class UProcedureProfiler
        {
            public int NumOfLostWater = 0;
            public int NumOfClearWater = 0;
            public int NumOfDry = 0;
            public int NumOfLowWater = 0;
        }

        private Vector2i DoPixel(int x, int y, UBufferComponent curHMap, FPixelWater[,] PixelWaters, UProcedureProfiler profiler)
        {
            ref var center = ref curHMap.GetPixel<Vector2>(x, y, 0);
            if (center.Y <= 0)
            {
                center.Y = 0;
                profiler.NumOfDry++;
                return Vector2i.MinusOne;
            }
            ref var slt = ref PixelWaters[y, x];
            if (slt.MoveTarget.X == -1 || slt.MoveTarget.Y == -1)
            {
                profiler.NumOfLowWater++;
                return Vector2i.MinusOne;
            }
            profiler.NumOfLostWater++;
            ref var target = ref curHMap.GetPixel<Vector2>(slt.MoveTarget.X, slt.MoveTarget.Y, 0);
            var delta = (center.X + center.Y) - (target.X + target.Y);
            var mov = delta * 0.5f;
            if (mov > center.Y)
            {
                target.Y += center.Y;
                center.Y = 0;
                profiler.NumOfClearWater++;
            }
            else
            {
                target.Y += mov;
                center.Y -= mov;
            }
            //if (target.X + target.Y + center.Y < center.X)
            //{
            //    target.Y += center.Y;
            //    center.Y = 0;
            //    profiler.NumOfClearWater++;
            //}
            //else
            //{
            //    var delta = center.X - (target.X + target.Y);
            //    target.Y += delta;
            //    center.Y -= delta;
            //    center.Y /= 2.0f;
            //    target.Y += center.Y;
            //}
            return slt.MoveTarget;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var height = graph.BufferCache.FindBuffer(HMapPin);
            var deep = graph.BufferCache.FindBuffer(DeepPin);
            var water = graph.BufferCache.FindBuffer(WaterPin);
            var velocity = graph.BufferCache.FindBuffer(VelocityPin);

            var opMapCreator = UBufferCreator.CreateInstance<USuperBuffer<Vector2, FFloat2Operator>>(height.Width, height.Height, height.Depth); ;
            var curHMap = UBufferComponent.CreateInstance(opMapCreator);            
            curHMap.DispatchPixels((result, x, y, z) =>
            {
                Vector2 tmp;
                tmp.X = height.GetFloat1(x, y, z);
                tmp.Y = WaterPerPixel;
                result.SetFloat2(x, y, z, in tmp);
            }, true);

            for (int sim = 0; sim < SimCount; sim++)
            {
                var profiler = new UProcedureProfiler();
                var PixelWaters = new FPixelWater[height.Width, height.Height];
                curHMap.DispatchPixels((result, x, y, z) =>
                {
                    ref var center = ref curHMap.GetPixel<Vector2>(x, y, 0);
                    float maxDelta = 0;
                    int selectIndex = -1;
                    unsafe
                    {
                        var neighbours = stackalloc Vector2i[8];
                        {
                            neighbours[0].X = x - 1;
                            neighbours[0].Y = y - 1;
                            neighbours[1].X = x;
                            neighbours[1].Y = y - 1;
                            neighbours[2].X = x + 1;
                            neighbours[2].Y = y - 1;

                            neighbours[3].X = x - 1;
                            neighbours[3].Y = y;
                            neighbours[4].X = x + 1;
                            neighbours[4].Y = y;

                            neighbours[5].X = x - 1;
                            neighbours[5].Y = y + 1;
                            neighbours[6].X = x;
                            neighbours[6].Y = y + 1;
                            neighbours[7].X = x + 1;
                            neighbours[7].Y = y + 1;
                        }
                        var centerH = center.X + center.Y;
                        for (int i = 0; i < 8; i++)
                        {
                            if (neighbours[i].X < 0 || neighbours[i].Y < 0)
                                continue;
                            if (neighbours[i].X >= result.Width || neighbours[i].Y >= result.Height)
                                continue;

                            ref var v = ref curHMap.GetPixel<Vector2>(neighbours[i].X, neighbours[i].Y, 0);

                            var h = centerH - (v.X + v.Y);
                            if (h > maxDelta)
                            {
                                maxDelta = h;
                                selectIndex = i;
                            }
                        }

                        if (selectIndex >= 0)
                        {
                            PixelWaters[y, x].MoveTarget = neighbours[selectIndex];

                            ref var target = ref curHMap.GetPixel<Vector2>(neighbours[selectIndex].X, neighbours[selectIndex].Y, 0);
                            var delta = (center.X + center.Y) - (target.X + target.Y);
                            var mov = delta * 0.5f;
                            if (mov > center.Y)
                            {
                                PixelWaters[y, x].Water = center.Y;
                            }
                            else
                            {
                                PixelWaters[y, x].Water = mov;
                            }
                        }
                        else
                        {
                            PixelWaters[y, x].MoveTarget = new Vector2i(-1, -1);
                            PixelWaters[y, x].Water = 0;
                        }
                    }
                }, true);

                //curHMap.DispatchPixels((result, x, y, z) =>
                //{
                //    ref var slt = ref PixelWaters[y, x];
                //    if (slt.Water == 0)
                //        return;
                //    ref var target = ref curHMap.GetPixel<Vector2>(slt.MoveTarget.X, slt.MoveTarget.Y, 0);
                //    ref var src = ref curHMap.GetPixel<Vector2>(x, y, 0);
                //    target.Y += slt.Water;
                //    src.Y -= slt.Water;
                //}, false);

                curHMap.DispatchPixels((result, x, y, z) =>
                {
                    Vector2i cur = new Vector2i(x, y);
                    for (int i = 0; i < SimStep; i++)
                    {
                        cur = DoPixel(cur.X, cur.Y, curHMap, PixelWaters, profiler);
                        if (cur.X == -1 || cur.Y == -1)
                            break;
                    }
                }, false);

                System.Diagnostics.Debug.WriteLine($"SimCount = {sim}; LostWater = {profiler.NumOfLostWater}; ClearWater = {profiler.NumOfClearWater}; Dry = {profiler.NumOfDry}; Low = {profiler.NumOfLowWater};");

                if (profiler.NumOfClearWater < NoneWaterLimit)
                {
                    System.Diagnostics.Debug.WriteLine($"Break: {profiler.NumOfClearWater} < {NoneWaterLimit}");
                    break;
                }
            }

            deep.DispatchPixels((result, x, y, z) =>
            {
                ref var center = ref curHMap.GetPixel<Vector2>(x, y, 0);
                if (center.Y > 0.01f)
                {
                    result.SetFloat1(x, y, z, center.Y);
                    water.SetFloat1(x, y, z, center.X + center.Y);
                }
                else
                {
                    result.SetFloat1(x, y, z, 0);
                    water.SetFloat1(x, y, z, float.MinValue);
                }
            }, true);
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Grass", "Terrain\\Grass", UPgcGraph.PgcEditorKeyword)]
    public partial class UGrassNode : UPgcNodeBase
    {
        public class UGrassNodeDefine
        {
            internal UGrassNode HostNode;
            public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as UGrassNodeDefine;
                    var NumOfPin = nodeDef.HostNode.GrassDefines.Count;
                    if (ImGuiAPI.InputInt("NumOfPin", (int*)&NumOfPin, 0, 0, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
                    {
                        if (NumOfPin <= 0)
                            NumOfPin = 1;
                        var oldInputs = nodeDef.HostNode.GrassDefines;
                        nodeDef.HostNode.GrassDefines = new List<UGrassPinDefine>();
                        for (int i = 0; i < NumOfPin; i++)
                        {
                            if (i < oldInputs.Count)
                            {
                                nodeDef.HostNode.GrassDefines.Add(oldInputs[i]);
                            }
                            else
                            {
                                nodeDef.HostNode.GrassDefines.Add(new UGrassPinDefine());
                            }
                        }
                    }
                    if (ImGuiAPI.Button("UpdatePins"))
                    {
                        nodeDef.HostNode.UpdateInputs();
                    }
                    return false;
                }
            }
        }
        [UGrassNodeDefine.UValueEditor]
        public UGrassNodeDefine NodeDefine
        {
            get;
        } = new UGrassNodeDefine();

        [System.ComponentModel.Browsable(false)]
        public PinOut Grass { get; set; } = new PinOut();

        public class UGrassPinDefine : IO.BaseSerializer
        {
            [Rtti.Meta]
            public string Name { get; set; } = "UserPin";
            [Rtti.Meta]
            public string TypeValue { get; set; } = "Value";
            [Rtti.Meta]
            public Terrain.CDLOD.UTerrainGrass GrassData { get; set; } = new Terrain.CDLOD.UTerrainGrass();
        }

        List<UGrassPinDefine> mGrassDefines = new List<UGrassPinDefine>();
        UBufferCreator mInBufferCreator = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>();
        [Rtti.Meta]
        public List<UGrassPinDefine> GrassDefines
        {
            get => mGrassDefines;
            set
            {
                mGrassDefines = value;
                UpdateInputs();
            }
        }

        public UGrassNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(Grass, "Grass", null, "Grass");

            NodeDefine.HostNode = this;
            UpdateInputs();
        }

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return null;
        }

        public void UpdateInputs()
        {
            //for(int i=0; i<Inputs.Count; i++)
            //{
            //    ParentGraph.RemoveLinkedIn(Inputs[i]);
            //}
            //Inputs.Clear();
            
            for(int i=0; i<GrassDefines.Count; i++)
            {
                if(i >= Inputs.Count)
                {
                    var pin = new PinIn();
                    AddInput(pin, GrassDefines[i].Name, mInBufferCreator, GrassDefines[i].TypeValue);
                }
                else
                {
                    if(!Inputs[i].LinkDesc.CanLinks.Contains(GrassDefines[i].TypeValue))
                    {
                        ParentGraph.RemoveLinkedIn(Inputs[i]);
                        Inputs[i].LinkDesc.CanLinks.Clear();
                        Inputs[i].LinkDesc.CanLinks.Add(GrassDefines[i].TypeValue);
                    }
                    if(Inputs[i].Name != GrassDefines[i].Name)
                    {
                        Inputs[i].Name = GrassDefines[i].Name;
                    }
                }
            }
            OnPositionChanged();
        }
        public override UBufferComponent GetResultBuffer(int index)
        {
            if (index < 0 || index >= Inputs.Count)
                return null;
            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(Inputs[index]);
        }
    }
}
