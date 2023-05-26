using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Bricks.Terrain.CDLOD;
using NPOI.Util;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("TransformUnpack", "Transform\\Unpack", UPgcGraph.PgcEditorKeyword)]
    public class UTransformUnpackNodes : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InTransform { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut PosPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ScalePin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut QuatPin { get; set; } = new PinOut();

        public PinOut SinglePosPin { get; set; } = new PinOut();

        public UBufferCreator InputTransDesc = UBufferCreator.CreateInstance<USuperBuffer<FTransform, FTransformOperator>>(-1, -1, -1);
        public UBufferCreator OutputDouble3Desc = UBufferCreator.CreateInstance<USuperBuffer<DVector3, FDouble3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloat4Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(-1, -1, -1);
        public UTransformUnpackNodes()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InTransform, "Trans", InputTransDesc);
            AddOutput(PosPin, "DPos", OutputDouble3Desc);
            AddOutput(ScalePin, "Scale", OutputFloat3Desc);
            AddOutput(QuatPin, "Quat", OutputFloat4Desc);
            AddOutput(SinglePosPin, "SPos", OutputFloat3Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = ParentGraph as UPgcGraph;
            if (PosPin == pin)
            {
                var buffer = graph.BufferCache.FindBuffer(InTransform);
                if (buffer != null)
                {
                    OutputDouble3Desc.SetSize(buffer.BufferCreator);
                    return OutputDouble3Desc;
                }
            }
            else if (ScalePin == pin)
            {
                var buffer = graph.BufferCache.FindBuffer(InTransform);
                if (buffer != null)
                {
                    OutputFloat3Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat3Desc;
                }
            }
            else if (QuatPin == pin)
            {
                var buffer = graph.BufferCache.FindBuffer(InTransform);
                if (buffer != null)
                {
                    OutputFloat4Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat4Desc;
                }
            }
            return null;
        }
        public override unsafe bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var trans = graph.BufferCache.FindBuffer(InTransform);
            var posResult = graph.BufferCache.FindBuffer(PosPin);
            var scaleResult = graph.BufferCache.FindBuffer(ScalePin);
            var quatResult = graph.BufferCache.FindBuffer(QuatPin);
            var singlePosResult = graph.BufferCache.FindBuffer(SinglePosPin);

            for (int i = 0; i < trans.Depth; i++)
            {
                for (int j = 0; j < trans.Height; j++)
                {
                    for (int k = 0; k < trans.Width; k++)
                    {
                        var xyz = trans.GetPixel<FTransform>(k, j, i);
                        posResult.SetPixel(k, j, i, xyz.Position);
                        posResult.SetPixel(k, j, i, xyz.Scale);
                        quatResult.SetPixel(k, j, i, xyz.Quat);
                        singlePosResult.SetPixel(k, j, i, xyz.Position.ToSingleVector3());
                    }
                }
            }

            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Transformpack", "Transform\\Pack", UPgcGraph.PgcEditorKeyword)]
    public class UTransformPackNodes : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutTransform { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn PosPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn ScalePin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn QuatPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn SinglePosPin { get; set; } = new PinIn();

        public UBufferCreator InputDouble3Desc = UBufferCreator.CreateInstance<USuperBuffer<DVector3, FDouble3Operator>>(-1, -1, -1);
        public UBufferCreator InputFloat3Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator InputFloat4Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector4, FFloat4Operator>>(-1, -1, -1);
        public UBufferCreator OutputTransDesc = UBufferCreator.CreateInstance<USuperBuffer<FTransform, FTransformOperator>>(-1, -1, -1);
        public UTransformPackNodes()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(PosPin, "DPos", InputDouble3Desc);
            AddInput(ScalePin, "Scale", InputFloat3Desc);
            AddInput(QuatPin, "Quat", InputFloat4Desc);
            AddInput(SinglePosPin, "SPos", InputFloat3Desc);
            AddOutput(OutTransform, "Trans", OutputTransDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = ParentGraph as UPgcGraph;
            if (pin == OutTransform)
            {
                var buffer = graph.BufferCache.FindBuffer(PosPin);
                if (buffer != null)
                {
                    OutputTransDesc.SetSize(buffer.BufferCreator);
                    return OutputTransDesc;
                }
            }
            return null;
        }
        public override unsafe bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var trans = graph.BufferCache.FindBuffer(OutTransform);
            var posResult = graph.BufferCache.FindBuffer(PosPin);
            var scaleResult = graph.BufferCache.FindBuffer(ScalePin);
            var quatResult = graph.BufferCache.FindBuffer(QuatPin);
            var singlePosResult = graph.BufferCache.FindBuffer(SinglePosPin);

            for (int i = 0; i < trans.Depth; i++)
            {
                for (int j = 0; j < trans.Height; j++)
                {
                    for (int k = 0; k < trans.Width; k++)
                    {
                        FTransform tmp = new FTransform();
                        if (posResult != null)
                        {
                            tmp.Position = posResult.GetPixel<DVector3>(k, j, i);
                        }
                        else if (singlePosResult != null)
                        {
                            tmp.Position = posResult.GetPixel<Vector3>(k, j, i).AsDVector();
                        }
                        tmp.Scale = scaleResult.GetPixel<Vector3>(k, j, i);
                        tmp.Quat = quatResult.GetPixel<Quaternion>(k, j, i);

                        trans.SetPixel(k, j, i, in tmp);
                    }
                }
            }

            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("TransformBuilder", "Transform\\Builder", UPgcGraph.PgcEditorKeyword)]
    public class UTransformBuilder : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn NormPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn MatIdPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn IdMapPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutTransform { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutPlant { get; set; } = new PinOut();
        //[EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        //public PinOut OutGrass { get; set; } = new PinOut();
        public UBufferCreator InputHeightDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator InputNormDesc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator InputMatIdDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator InputHMappinDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputTransDesc = UBufferCreator.CreateInstance<USuperBuffer<FTransform, FTransformOperator>>(0, 0, 0);
        public UBufferCreator OutputPlantDesc = UBufferCreator.CreateInstance<USuperBuffer<Vector2i, FInt2Operator>>(0, 0, 0);
        //public UBufferCreator OutputGrassDesc = UBufferCreator.CreateInstance<USuperBuffer<FGrassTransformData, FGrassTransformDataOperator>>(0, 0, 0);
        public UTransformBuilder()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", InputHeightDesc);
            AddInput(NormPin, "Norm", InputNormDesc);
            AddInput(MatIdPin, "MatId", InputMatIdDesc);
            AddInput(IdMapPin, "IdMap", InputHMappinDesc, "IdMap");
            AddOutput(OutTransform, "Trans", OutputTransDesc);
            AddOutput(OutPlant, "Plant", OutputPlantDesc);
            //AddOutput(OutGrass, "Grass", OutputGrassDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = ParentGraph as UPgcGraph;
            if (pin == OutTransform)
            {
                return OutputTransDesc;
            }
            else if (pin == OutPlant)
            {
                return OutputPlantDesc;
            }
            //else if(pin == OutGrass)
            //{
            //    return OutputGrassDesc;
            //}
            return null;
        }
        public override unsafe bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        [Rtti.Meta]
        public DVector3 Offset { get; set; } = DVector3.Zero;
        [Rtti.Meta]
        public float GridSize { get; set; } = 1.0f;
        [Rtti.Meta]
        public int RandomSeed { get; set; } = 60;
        public int FinalRandomSeed { get; set; }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var trans = graph.BufferCache.FindBuffer(OutTransform);
            var plants = graph.BufferCache.FindBuffer(OutPlant);
            //var grasses = graph.BufferCache.FindBuffer(OutGrass);

            var heightResult = graph.BufferCache.FindBuffer(HeightPin);
            var normResult = graph.BufferCache.FindBuffer(NormPin);
            var matIdResult = graph.BufferCache.FindBuffer(MatIdPin);
            var idMapNode = this.GetInputNode(graph, IdMapPin) as UMaterialIdMapNode;

            trans.ResizePixels(0, 1, 1);
            plants.ResizePixels(0, 1, 1);
            //grasses.ResizePixels(0, 1, 1);
            var randObj = new Support.URandom();
            randObj.mCoreObject.SetSeed(FinalRandomSeed);
            for (int i = 0; i < heightResult.Depth; i++)
            {
                for (int j = 0; j < heightResult.Height; j++)
                {
                    for (int k = 0; k < heightResult.Width; k++)
                    {
                        var id = (int)matIdResult.GetFloat1(k, j, i);
                        id = id % idMapNode.MaterialIdManager.MaterialIdArray.Count;
                        var trMtlDesc = idMapNode.MaterialIdManager.MaterialIdArray[id];

                        var height = heightResult.GetFloat1(k, j, i);
                        var nor = normResult.GetFloat3(k, j, i);
                        var x = Offset.X + (float)k * GridSize;
                        var z = Offset.Z + (float)j * GridSize;
                        var y = Offset.Y + height;

                        // plant
                        if (trMtlDesc.Plants.Count > 0)
                        {
                            var iPlant = trMtlDesc.GetRandomPlant(randObj.GetNextInt32());
                            if (iPlant >= 0)
                            {
                                var plantDesc = trMtlDesc.Plants[iPlant];

                                FTransform tmp = FTransform.Identity;
                                Vector2i plt = Vector2i.Zero;
                                plt.X = id;
                                plt.Y = iPlant;

                                tmp.Position = new DVector3(x, y, z);
                                var scale = MathHelper.Lerp(plantDesc.MinScale, plantDesc.MaxScale, randObj.GetUnit());
                                tmp.Scale = new Vector3(scale);
                                tmp.Quat = Quaternion.RotationAxis(in Vector3.Up, MathHelper.TWO_PI * randObj.GetUnit());
                                trans.AddPixel(in tmp);
                                plants.AddPixel(in plt);
                            }
                        }

                        //// grass
                        //if(trMtlDesc.Grasses.Count > 0)
                        //{
                        //    var iGrass = trMtlDesc.GetRandomGrass(randObj.GetNextInt32());
                        //    if(iGrass >= 0)
                        //    {
                        //        var grassDesc = trMtlDesc.Grasses[iGrass];

                        //        var tmp = Terrain.CDLOD.FGrassTransformData.Create();
                        //        tmp.Transform.Position = new DVector3(x, y, z);
                        //        var scale = CoreDefine.Lerp(grassDesc.MinScale, grassDesc.MaxScale, randObj.GetUnit());
                        //        tmp.Transform.Scale = new Vector3(scale);
                        //        tmp.Transform.Quat = Quaternion.RotationAxis(in Vector3.Up, CoreDefine.TWO_PI * randObj.GetUnit());
                        //        tmp.MaterialIdx = id;
                        //        tmp.GrassIdx = iGrass;
                        //        grasses.AddPixel(in tmp);
                        //    }
                        //}
                    }
                }
            }

            return true;
        }
    }

    //jacobi iteration:https://zhuanlan.zhihu.com/p/30965284
}
