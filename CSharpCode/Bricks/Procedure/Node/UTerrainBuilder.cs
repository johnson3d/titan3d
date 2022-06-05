using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("MaterialIdMap", "Terrain\\MaterialIdMap", UPgcGraph.PgcEditorKeyword)]
    public partial class UMaterialIdMapNode : UOpNode
    {
        public PinOut IdMapPin { get; set; } = new PinOut();
        public UMaterialIdMapNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(IdMapPin, "Self", DefaultBufferCreator, "IdMap");
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
                i.UpdateTotalDensity();
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
    public partial class UHeightMappingNode : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn BezierPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn IdMapPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UHeightMappingNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", DefaultInputDesc);
            AddInput(BezierPin, "Bezier", DefaultInputDesc, "Bezier");
            AddInput(IdMapPin, "IdMap", DefaultBufferCreator, "IdMap");
            AddOutput(ResultPin, "Result", DefaultBufferCreator);
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
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
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
                norValue = CoreDefine.Clamp(norValue, 0, 1);
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
                norValue = CoreDefine.Clamp(norValue, 0, 1);
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
}
