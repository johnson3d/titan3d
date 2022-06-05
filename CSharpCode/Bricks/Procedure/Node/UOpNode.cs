using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public partial class UOpNode : UPgcNodeBase
    {
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = this.ParentGraph as UPgcGraph;
            var result = UBufferCreator.CreateInstance(
                DefaultBufferCreator.BufferType, 
                DefaultBufferCreator.XSize,
                DefaultBufferCreator.YSize,
                DefaultBufferCreator.ZSize);
            if (result.XSize == -1)
            {
                result.XSize = graph.DefaultCreator.XSize;
            }
            if (result.YSize == -1)
            {
                result.YSize = graph.DefaultCreator.YSize;
            }
            if (result.ZSize == -1)
            {
                result.ZSize = graph.DefaultCreator.ZSize;
            }
            return result;
        }
    }
    
    [Bricks.CodeBuilder.ContextMenu("Perlin", "Float1\\Perlin", UPgcGraph.PgcEditorKeyword)]
    public partial class UNoisePerlin : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UNoisePerlin()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", DefaultBufferCreator);
        }
        int mOctaves = 3;
        [Rtti.Meta]
        public int Octaves 
        {
            get => mOctaves;
            set
            {
                mOctaves = value;
                //UpdatePerlin();
            }
        }
        float mFreq = 3.5f;
        [Rtti.Meta]
        public float Freq 
        {
            get => mFreq;
            set
            {
                mFreq = value;
                //UpdatePerlin();
            }
        }
        float mAmptitude = 10.0f;
        [Rtti.Meta]
        public float Amptitude 
        {
            get => mAmptitude;
            set
            {
                mAmptitude = value;
                //UpdatePerlin();
            }
        }
        int mSeed = (int)Support.Time.GetTickCount();
        [Rtti.Meta]
        public int Seed 
        { 
            get => mSeed;
            set
            {
                mSeed = value;
                //UpdatePerlin();
            }
        }
        int mSamplerSize = 1024;
        [Rtti.Meta]
        public int SamplerSize
        {
            get
            {
                return mSamplerSize;
            }
            set
            {
                mSamplerSize = value;
                //UpdatePerlin();
            }
        }

        [Rtti.Meta]
        public float GridSize { get; set; } = 1.0f;
        [Rtti.Meta]
        public float Border { get; set; } = 3.0f;
        protected Support.CPerlin mPerlin;
        protected void UpdatePerlin()
        {
            mPerlin = new Support.CPerlin(Octaves, Freq, Amptitude, Seed, SamplerSize);
        }
        public DVector3 StartPosition = DVector3.Zero;
        public override bool InitProcedure(UPgcGraph graph)
        {
            UpdatePerlin();
            return true;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            if (TryLoadOutBufferFromCache(graph, ResultPin))
                return true;

            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            float XScale = 1.0f * GridSize / (resultComp.Width - 1);
            float YScale = 1.0f * GridSize / (resultComp.Height - 1);
            //for (int i = 0; i < resultComp.Height; i++)
            //{
            //    for (int j = 0; j < resultComp.Width; j++)
            //    {
            //        var value = (float)mPerlin.mCoreObject.Get(StartPosition.X + GridSize * j + XScale * j, StartPosition.Z + GridSize * i + YScale * i);
            //        resultComp.SetSuperPixelAddress(j, i, 0, &value);
            //    }
            //}
            resultComp.DispatchPixels((result, x, y, z) =>
            {
                var value = (float)mPerlin.mCoreObject.Get(StartPosition.X + GridSize * x + XScale * x, StartPosition.Z + GridSize * y + YScale * y);
                resultComp.SetSuperPixelAddress(x, y, 0, &value);
            }, true);

            this.SaveOutBufferToCache(graph, ResultPin);
            return true;
        }

        public override Hash160 GetOutBufferHash(PinOut pin)
        {
            if (pin == ResultPin)
            {
                string hashStr = $"{StartPosition.ToString()}_{Octaves}_{Freq}_{Amptitude}_{Seed}_{SamplerSize}_{GridSize}_{Border}";
                return Hash160.CreateHash160(hashStr);
            }
            return Hash160.Emtpy;
        }
        public override void OnAfterProcedure(UPgcGraph graph)
        {
            
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Bezier", "Function\\Bezier", UPgcGraph.PgcEditorKeyword)]
    public partial class UBezier : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public EGui.Controls.BezierControl mBezierCtrl = new EGui.Controls.BezierControl();
        public EGui.Controls.BezierControl BezierCtrl
        {
            get => mBezierCtrl;
        }
        [Rtti.Meta]
        public Vector2 GraphSize
        {
            get
            {
                return PrevSize;
            }
            set
            {
                PrevSize = value;
                this.OnPositionChanged();
            }
        }

        [Rtti.Meta]
        public float MinX
        {
            get => mBezierCtrl.MinX;
            set
            {
                mBezierCtrl.MinX = value;
            }
        }
        [Rtti.Meta]
        public float MaxX
        {
            get => mBezierCtrl.MaxX;
            set
            {
                mBezierCtrl.MaxX = value;
            }
        }
        [Rtti.Meta]
        public float RangeX
        {
            get => MaxX - MinX;
        }

        [Rtti.Meta]
        public float MinY
        {
            get => mBezierCtrl.MinY;
            set
            {
                mBezierCtrl.MinY = value;
            }
        }
        [Rtti.Meta]
        public float MaxY
        {
            get => mBezierCtrl.MaxY;
            set
            {
                mBezierCtrl.MaxY = value;
            }
        }
        public float RangeY
        {
            get => MaxY - MinY;
        }
        [Rtti.Meta]
        public List<BezierPointBase> BzPoints
        {
            get => mBezierCtrl.BezierPoints;
            set
            {
                mBezierCtrl.BezierPoints = value;
            }
        }
        public bool LockLinkedControlPoint
        {
            get => mBezierCtrl.LockLinkedControlPoint;
            set
            {
                mBezierCtrl.LockLinkedControlPoint = value;
            }
        }
        public UBezier()
        {
            PrevSize = new Vector2(200, 60);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", DefaultBufferCreator, "Bezier");

            mBezierCtrl.Initialize(MinX, MinY, MaxX, MaxY);
        }
        public override Hash160 GetOutBufferHash(PinOut pin)
        {
            if (pin == ResultPin)
            {
                string hashStr = "";
                hashStr += MinX;
                hashStr += MaxX;
                hashStr += MinY;
                hashStr += MaxX;
                foreach (var i in BzPoints)
                {
                    hashStr += i.ToString();
                }
                return Hash160.CreateHash160(hashStr);
            }
            return Hash160.Emtpy;
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            //var ctrlPos = ParentGraph.CanvasToViewport(in prevStart);
            //ctrlPos -= ImGuiAPI.GetWindowPos();
            //ImGuiAPI.SetCursorPos(in ctrlPos);

            //ImGuiAPI.InvisibleButton("canvas", prevEnd - prevStart, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight);
            mBezierCtrl.OnDrawCanvas(in prevStart, prevEnd - prevStart);
        }
        [Rtti.Meta]
        public float GetY(float x)
        {
            var vzValue = BezierCalculate.ValueOnBezier(BzPoints, x);
            return vzValue.Y;
        }
    }

    /*
     0 1 2
     3 c 4
     5 6 7
     */
    [Bricks.CodeBuilder.ContextMenu("CalcNormal", "Function\\CalcNormal", UPgcGraph.PgcEditorKeyword)]
    public partial class UCalcNormal : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HFieldPin { get; set; } = new PinIn(); 
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UCalcNormal()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HFieldPin, "Height", DefaultInputDesc);
            AddOutput(XPin, "X", DefaultBufferCreator);
            AddOutput(YPin, "Y", DefaultBufferCreator);
            AddOutput(ZPin, "Z", DefaultBufferCreator);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (XPin == pin || YPin == pin || ZPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HFieldPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        [Rtti.Meta]
        public float GridSize { get; set; } = 1.0f;
        //public float HeightRange;
        public override bool OnProcedure(UPgcGraph graph)
        {
            var heightFiels = graph.BufferCache.FindBuffer(HFieldPin);
            var xResult = graph.BufferCache.FindBuffer(XPin);
            var yResult = graph.BufferCache.FindBuffer(YPin);
            var zResult = graph.BufferCache.FindBuffer(ZPin);
            float minHeight, maxHeight;
            heightFiels.GetRangeUnsafe<float, FFloatOperator>(out minHeight, out maxHeight);
            //float range = HeightRange;// maxHeight - minHeight;
            for (int i = 1; i < heightFiels.Height - 1; i++)
            {
                for (int j = 1; j < heightFiels.Width - 1; j++)
                {
                    #region old
                    //float h1 = heightFiels.GetPixel(j, i);
                    //float h2 = heightFiels.GetPixel(j + 1, i);
                    //float h3 = heightFiels.GetPixel(j, i + 1);
                    //float h4 = heightFiels.GetPixel(j + 1, i + 1);

                    //var V1 = Vector3.Normalize(new Vector3(1.0f, 0.0f, h2 - h1));//正方形边
                    //var V2 = Vector3.Normalize(new Vector3(1.0f, 0.0f, h4 - h3));//正方形边
                    //var V3 = Vector3.Normalize(new Vector3(1.0f, -1.0f, h4 - h1));//斜边
                    //var V4 = Vector3.Normalize(new Vector3(-1.0f, -1.0f, h3 - h2));//斜边

                    //var Normal1 = Vector3.Cross(in V4, in V1);
                    //var Normal2 = Vector3.Cross(in V3, in V1);
                    //var Normal3 = Vector3.Cross(in V3, in V2);
                    //var Normal4 = Vector3.Cross(in V4, in V2);

                    //var n = Vector3.Normalize(Normal1 + Normal2 + Normal3 + Normal4);

                    //{
                    //    var hc = (heightFiels.GetPixel(j, i) - minHeight);
                    //    var h0 = (heightFiels.GetPixel(j - 1, i + 1) - minHeight);
                    //    var h1 = (heightFiels.GetPixel(j, i + 1) - minHeight);
                    //    var h2 = (heightFiels.GetPixel(j + 1, i + 1) - minHeight);
                    //    var h3 = (heightFiels.GetPixel(j - 1, i) - minHeight);
                    //    var h4 = (heightFiels.GetPixel(j + 1, i) - minHeight);
                    //    var h5 = (heightFiels.GetPixel(j - 1, i - 1) - minHeight);
                    //    var h6 = (heightFiels.GetPixel(j, i - 1) - minHeight);
                    //    var h7 = (heightFiels.GetPixel(j + 1, i - 1) - minHeight);

                    //    hc /= range;
                    //    h0 /= range;
                    //    h1 /= range;
                    //    h2 /= range;
                    //    h3 /= range;
                    //    h4 /= range;
                    //    h5 /= range;
                    //    h6 /= range;
                    //    h7 /= range;

                    //    var vc = new Vector3(0, hc, 0);
                    //    var v0 = new Vector3(-1, h0, 1);
                    //    var v1 = new Vector3(0, h1, 1);
                    //    var v2 = new Vector3(1, h2, 1);
                    //    var v3 = new Vector3(-1, h3, 0);
                    //    var v4 = new Vector3(1, h4, 0);
                    //    var v5 = new Vector3(-1, h5, -1);
                    //    var v6 = new Vector3(0, h6, -1);
                    //    var v7 = new Vector3(1, h7, -1);

                    //    var n1 = Vector3.Cross(vc - v1, v0 - v1);
                    //    var n2 = Vector3.Cross(v2 - v1, vc - v1);
                    //    var n3 = Vector3.Cross(vc - v4, v2 - v4);
                    //    var n4 = Vector3.Cross(v7 - v4, vc - v4);
                    //    var n5 = Vector3.Cross(vc - v6, v7 - v6);
                    //    var n6 = Vector3.Cross(v5 - v6, vc - v6);
                    //    var n7 = Vector3.Cross(vc - v3, v5 - v3);
                    //    var n8 = Vector3.Cross(v0 - v3, vc - v3);

                    //    n1.Normalize();
                    //    n2.Normalize();
                    //    n3.Normalize();
                    //    n4.Normalize();
                    //    n5.Normalize();
                    //    n6.Normalize();
                    //    n7.Normalize();
                    //    n8.Normalize();

                    //    var n = n1 + n2 + n3 + n4 + n6 + n6 + n7 + n8;
                    //    n /= 8.0f;
                    //    n.Normalize();
                    //}
                    #endregion

                    float altInfo = heightFiels.GetPixel<float>(j, i);
                    float v_du = heightFiels.GetPixel<float>(j + 1, i);
                    float v_dv = heightFiels.GetPixel<float>(j, i + 1);

                    var A = new Vector3(GridSize, (v_du - altInfo), 0);
                    var B = new Vector3(0, (v_dv - altInfo), -GridSize);

                    var n = Vector3.Cross(A, B);
                    n = Vector3.Normalize(n);

                    xResult.SetPixel(j, i, n.X);
                    yResult.SetPixel(j, i, n.Y);
                    zResult.SetPixel(j, i, n.Z);
                }
            }
            //for (int i = 1; i < heightFiels.Height - 1; i++)
            //{
            //    for (int j = 1; j < heightFiels.Width - 1; j++)
            //    {
            //        var xV = xResult.GetPixel(i, i);
            //        var yV = yResult.GetPixel(i, i);
            //        var zV = zResult.GetPixel(i, i);
            //    }
            //}
            heightFiels.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Normalize3D", "Function\\Normalize3D", UPgcGraph.PgcEditorKeyword)]
    public partial class UNormalize3D : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InYPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InZPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UNormalize3D()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXPin, "InX", DefaultInputDesc);
            AddInput(InYPin, "InY", DefaultInputDesc);
            AddInput(InZPin, "InZ", DefaultInputDesc);
            AddOutput(XPin, "X", DefaultBufferCreator);
            AddOutput(YPin, "Y", DefaultBufferCreator);
            AddOutput(ZPin, "Z", DefaultBufferCreator);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (XPin == pin || YPin == pin || ZPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InXPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xSrc = graph.BufferCache.FindBuffer(InXPin);
            var ySrc = graph.BufferCache.FindBuffer(InYPin);
            var zSrc = graph.BufferCache.FindBuffer(InZPin);

            if (xSrc.Width != ySrc.Width || xSrc.Width != zSrc.Width ||
                xSrc.Height != ySrc.Height || xSrc.Height != zSrc.Height)
            {
                return false;
            }

            var xResult = graph.BufferCache.FindBuffer(XPin);
            var yResult = graph.BufferCache.FindBuffer(YPin);
            var zResult = graph.BufferCache.FindBuffer(ZPin);
            for (int i = 1; i < xSrc.Height - 1; i++)
            {
                for (int j = 1; j < xSrc.Width - 1; j++)
                {
                    Vector3 nor;
                    nor.X = xSrc.GetPixel<float>(j, i);
                    nor.Y = ySrc.GetPixel<float>(j, i);
                    nor.Z = zSrc.GetPixel<float>(j, i);
                    nor.Normalize();

                    xResult.SetPixel(j, i, nor.X);
                    yResult.SetPixel(j, i, nor.Y);
                    zResult.SetPixel(j, i, nor.Z);
                }
            }
            xSrc.LifeCount--;
            ySrc.LifeCount--;
            zSrc.LifeCount--;

            return true;
        }
    }
    
    [Bricks.CodeBuilder.ContextMenu("RootNode", "RootNode", UPgcGraph.PgcEditorKeyword)]
    public class UEndingNode : UOpNode
    {
        public UEndingNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            //PinNumber = 4;
        }
        [Rtti.Meta]
        public int PinNumber
        {
            get
            {
                return this.Inputs.Count;
            }
            set
            {
                if (value > Inputs.Count)
                {
                    var delta = value - this.Inputs.Count;
                    for (int i = 0; i < delta; i++)
                    {
                        var inPin = new PinIn();
                        var inputDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
                        var num = Inputs.Count;
                        AddInput(inPin, $"in{num}", inputDesc);

                        var outPin = new PinOut();
                        AddOutput(outPin, $"out{num}", inputDesc);
                    }

                    OnPositionChanged();
                }
                else
                {
                    foreach (var i in Inputs)
                    {
                        this.ParentGraph.RemoveLinkedIn(i);
                    }
                    foreach (var i in Outputs)
                    {
                        this.ParentGraph.RemoveLinkedOut(i);
                    }
                    this.Inputs.Clear();
                    this.Outputs.Clear();
                    for (int i = 0; i < value; i++)
                    {
                        var inPin = new PinIn();
                        var inputDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
                        AddInput(inPin, $"in{i}", inputDesc);

                        var outPin = new PinOut();
                        AddOutput(outPin, $"out{i}", inputDesc);
                    }

                    OnPositionChanged();
                }
            }
        }
        public class UPinInfoEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            int PinIndex = 0;
            string CanLinkType = "value";
            public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                var node = info.ObjectInstance as UEndingNode;
                ImGuiAPI.PushID("PinInfoEditor");
                ImGuiAPI.InputInt("Pin", ref PinIndex, 0, 0, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                ImGuiAPI.InputText("Type", ref CanLinkType);
                if (ImGuiAPI.Button("OK"))
                {
                    if (PinIndex > 0 && PinIndex < node.Inputs.Count)
                    {
                        node.Inputs[PinIndex].LinkDesc.CanLinks.Clear();
                        node.Inputs[PinIndex].LinkDesc.CanLinks.Add(CanLinkType);
                    }
                }
                
                ImGuiAPI.PopID();
                return false;
            }
        }
        [UPinInfoEditorAttribute]
        public int PinInfoEditor
        {
            get { return 0; }
        }
        public override bool IsMatchLinkedPin(UBufferCreator input, UBufferCreator output)
        {
            return true;
        }
        public override void OnLoadLinker(UPinLinker linker)
        {
            base.OnLoadLinker(linker);

            var input = linker.OutPin.Tag as UBufferCreator;
            (linker.InPin.Tag as UBufferCreator).BufferType = input.BufferType;
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            var input = oPin.Tag as UBufferCreator;
            (iPin.Tag as UBufferCreator).BufferType = input.BufferType;
        }
        public override UBufferConponent GetResultBuffer(int index)
        {
            if (index < 0 || index >= Inputs.Count)
                return null;
            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(Inputs[index]);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            //for (int i = 0; i < Inputs.Count; i++)
            //{
            //    var buffer = GetResultBuffer(i);
            //    if (buffer != null)
            //    {
            //        buffer.LifeCount--;
            //    }
            //}
            return true;
        }
    }
}
