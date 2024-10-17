using EngineNS.Canvas;
using EngineNS.Support;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace EngineNS.UI.Canvas
{
    public class TtCanvas : AuxPtrType<EngineNS.Canvas.FCanvas>
    {
        public TtCanvas()
        {
            mCoreObject = EngineNS.Canvas.FCanvas.CreateInstance();
            Background = new TtCanvasDrawCmdList(mCoreObject.Background);
            Foregroud = new TtCanvasDrawCmdList(mCoreObject.Foreground);
        }

        public TtCanvas(EngineNS.Canvas.FCanvas self)
        {
            mCoreObject = self;
            Background = new TtCanvasDrawCmdList(mCoreObject.Background);
            Foregroud = new TtCanvasDrawCmdList(mCoreObject.Foreground);
        }

        public TtCanvasDrawCmdList Background;
        public TtCanvasDrawCmdList Foregroud;

        public void SetClientClip(float x, float y)
        {
            mCoreObject.SetClientClip(x, y);
        }
        public void PushBatch(TtCanvasDrawBatch batch)
        {
            mCoreObject.PushBatch(batch.mCoreObject);
        }
        public void Reset()
        {
            mCoreObject.Reset();
        }
        public void BuildMesh(Graphics.Mesh.TtMeshDataProvider mesh)
        {
            mCoreObject.BuildMesh(mesh.mCoreObject);
        }
        //public static async System.Threading.Tasks.Task<Graphics.Mesh.TtMesh> TestCreate()
        //{
        //    var TestCanvas = new EngineNS.UI.Canvas.TtCanvas();
        //    Graphics.Mesh.UMeshDataProvider MeshProvider = new Graphics.Mesh.UMeshDataProvider();

        //    var builder = MeshProvider.mCoreObject;
        //    uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
        //        (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
        //        (1 << (int)NxRHI.EVertexStreamType.VST_UV));
        //    builder.Init(streams, false, 0);
        //    {
        //        TestCanvas.SetClientClip(800, 600);
        //        var TestCanvasDrawBatch = new EngineNS.UI.Canvas.TtCanvasDrawBatch();
        //        TestCanvasDrawBatch.SetPosition(50, 50);
        //        TestCanvasDrawBatch.SetClientClip(800, 600);

        //        var outCmd = new EngineNS.Canvas.FSubDrawCmd();
        //        {
        //            var brush = new TtCanvasBrush();
        //            brush.Name = "utest/ddd.uminst";
        //            var texture = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/ground_01.srv"));
        //            brush.mCoreObject.SetSrv(texture.mCoreObject);
        //            TestCanvasDrawBatch.Middleground.AddImage(brush.mCoreObject, 0, 0, 800, 600, Color.DarkRed, ref outCmd);
        //        }
        //        TestCanvasDrawBatch.Middleground.AddLine(new Vector2(1, 1), new Vector2(799, 599), 80.0f, Color.DarkRed, ref outCmd);

        //        //var font = TtEngine.Instance.FontModule.FontManager.GetFontSDF(RName.GetRName("fonts/roboto-regular.fontsdf", RName.ERNameType.Engine), 0, 1024, 512);
        //        //var font = TtEngine.Instance.FontModule.FontManager.GetFontSDF(RName.GetRName("fonts/dengl.fontsdf", RName.ERNameType.Engine), 0, 1024, 512);
        //        var font = TtEngine.Instance.FontModule.FontManager.GetFontSDF(RName.GetRName("fonts/simli.fontsdf", RName.ERNameType.Engine), fontSize: 64, 1024, 1024);
        //        TestCanvasDrawBatch.Middleground.PushFont(font);
        //        TestCanvasDrawBatch.Middleground.AddText("abc中国1A，!,", 30, 30, Color4f.FromABGR(Color.LightPink));
        //        TestCanvasDrawBatch.Middleground.PopFont();

        //        {
        //            var brush = new TtCanvasBrush();
        //            brush.Name = "utest/ddd.uminst";
        //            var texture = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/groundsnow.srv"));
        //            brush.mCoreObject.SetSrv(texture.mCoreObject);
        //            TestCanvasDrawBatch.Middleground.PushBrush(brush);
                    
        //            var pathStyle = new TtPathStyle();
        //            pathStyle.StrokeMode = EPathStrokeMode.Stroke_Dash;
        //            pathStyle.JoinMode = EPathJoinMode.Join_Miter;
        //            pathStyle.CapMode = EPathCapMode.Cap_Round;
        //            pathStyle.ResetStrokePattern();
        //            pathStyle.PushStrokePattern(new float[] { 20, 40});
        //            TestCanvasDrawBatch.Middleground.PushPathStyle(pathStyle);
        //            var path = new TtPath();
        //            path.BeginPath();
        //            path.MoveTo(new Vector2(150.0f, 150.0f));
        //            path.LineTo(new Vector2(250.0f, 300.0f));
        //            path.S_CCW_ArcTo(new Vector2(150.0f, 300.0f), 1.0f);
        //            path.L_CCW_ArcTo(new Vector2(150.0f, 150.0f), 1.0f);
        //            path.EndPath(TestCanvasDrawBatch.Middleground);
        //            TestCanvasDrawBatch.Middleground.PopPathStyle();

        //            TestCanvasDrawBatch.Middleground.PopBrush();
        //        }

        //        TestCanvas.PushBatch(TestCanvasDrawBatch);
        //    }

        //    TestCanvas.BuildMesh(MeshProvider);
        //    var Mesh = new Graphics.Mesh.UMeshPrimitives("", 0);
        //    MeshProvider.ToMesh(Mesh);
        //    Mesh.AssetName = RName.GetRName("@UI");

        //    var materials = new Graphics.Pipeline.Shader.UMaterial[Mesh.NumAtom];
        //    for (int i = 0; i < materials.Length; i++)
        //    {
        //        Graphics.Pipeline.Shader.UMaterial mtl = null;
        //        EngineNS.Canvas.FDrawCmd cmd = new FDrawCmd();
        //        cmd.NativePointer = MeshProvider.GetAtomExtData((uint)i).NativePointer;
        //        var brush = cmd.GetBrush();
        //        if (brush.Name.StartWith("@Text:"))
        //        {
        //            mtl = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/font_sdf_0.uminst", RName.ERNameType.Engine));
        //            var clr = mtl.FindVar("FontColor");
        //            if (clr != null)
        //            {
        //                clr.SetValue(Color3f.FromColor(Color.DarkRed));
        //                //mtl.UpdateUniformVars();
        //            }
        //        }
        //        else
        //        {
        //            var name = brush.Name.c_str();
        //            if ("DefaultBrush" == name)
        //            {
        //                mtl = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));
        //            }
        //            else
        //            {
        //                mtl = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName(name));
        //            }
                    
        //            mtl.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
        //            var raster = mtl.Rasterizer;
        //            raster.CullMode = NxRHI.ECullMode.CMD_NONE;
        //            mtl.Rasterizer = raster;
        //            var dsState = mtl.DepthStencil;
        //            dsState.DepthWriteMask = NxRHI.EDepthWriteMask.DSWM_ZERO;
        //            mtl.DepthStencil = dsState;
        //        }
        //        materials[i] = mtl;
        //    }
        //    var mesh = new Graphics.Mesh.TtMesh();
        //    var ok = mesh.Initialize(Mesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
        //    var mdf = mesh.MdfQueue as Graphics.Mesh.UMdfStaticMesh;
        //    mdf.OnDrawCallCallback = static (drawcall, policy, atom) =>
        //    {
        //        EngineNS.Canvas.FDrawCmd cmd = new EngineNS.Canvas.FDrawCmd();
        //        cmd.NativePointer = atom.GetAtomExtData().NativePointer;
        //        var brush = cmd.GetBrush();
        //        if (brush.Name.StartWith("@Text:"))
        //        {
        //            var srv = cmd.GetBrush().GetSrv();
        //            if (srv.IsValidPointer)
        //            {
        //                drawcall.mCoreObject.BindResource(TtNameTable.FontTexture, srv.NativeSuper);
        //            }
        //        }
        //        else if (brush.Name == VNameString.FromString("utest/ddd.uminst"))
        //        {
        //            var srv = cmd.GetBrush().GetSrv();
        //            if (srv.IsValidPointer)
        //            {
        //                drawcall.mCoreObject.BindResource(VNameString.FromString("Texture_3680116513"), srv.NativeSuper);
        //            }
        //        }
        //    };

        //    ///test code
        //    //var api = new OpenAI_API.OpenAIAPI("YOUR_API_KEY");
        //    //var result = await api.Completions.GetCompletion("One Two Three One Two");
        //    //Console.WriteLine(result);

        //    TestCanvas.Dispose();
        //    return mesh;
        //}
    }

    public class TtCanvasBrush : AuxPtrType<EngineNS.Canvas.ICanvasBrush>
    { 
        public TtCanvasBrush() 
        {
            mCoreObject = EngineNS.Canvas.ICanvasBrush.CreateInstance();
        }
        public TtCanvasBrush(ICanvasBrush brush)
        {
            mCoreObject = brush;
        }
        public string Name
        {
            get
            {
                return mCoreObject.Name.Text;
            }
            set
            {
                mCoreObject.Name = VNameString.FromString(value);
            }
        }
        public Color4b Color
        {
            get => mCoreObject.Color;
            set => mCoreObject.SetColor(value);
        }

        public bool IsDirty
        {
            get => mCoreObject.IsDirty;
            set => mCoreObject.IsDirty = value;
        }

        public void SetSrv(NxRHI.TtSrView texture)
        {
            if (texture == null)
                return;
            mCoreObject.SetSrv(texture.mCoreObject);
        }
        public void SetUV(in Vector2 uv0, in Vector2 uv1)
        {
            mCoreObject.SetUV(in uv0, in uv1);
        }
        public void SetValue(string name, in TtAnyValue value)
        {
            mCoreObject.SetValue(name, in value);
        }
        public bool GetValue(string name, ref TtAnyValue value)
        {
            return mCoreObject.GetValue(name, ref value);
        }
        public void SetValuesToCbView(in NxRHI.ICbView cbuffer, in NxRHI.FCbvUpdater updater)
        {
            mCoreObject.SetValuesToCbView(cbuffer, updater);
        }
    }
}
