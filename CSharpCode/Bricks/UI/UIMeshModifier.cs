using EngineNS.Bricks.CodeBuilder.Backends;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UCoreShaderBinder
    {
        public class TtCBufferPerUIMeshIndexer : NxRHI.UShader.UShaderVarIndexer
        {
            [NxRHI.UShader.UShaderVar(VarType = typeof(Matrix))]
            public NxRHI.FShaderVarDesc AbsTransform;
        }
        public readonly TtCBufferPerUIMeshIndexer CBPerUIMesh = new TtCBufferPerUIMeshIndexer();
    }
}
namespace EngineNS.Graphics.Mesh
{
    public partial class UMeshPrimitives
    {
    }
}

namespace EngineNS.UI
{
    public class TtUIModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public void Dispose()
        {

        }
        public string ModifierNameVS { get => "DoUIModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/modifier/UIModifier.cginc", RName.ERNameType.Engine);
            }
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[]
            {
                EVertexStreamType.VST_Position,
                EVertexStreamType.VST_UV,
                EVertexStreamType.VST_SkinIndex,
            };
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
        public void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {

        }
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {

        }
    }
    public class TtMdfUIMesh : Graphics.Pipeline.Shader.TtMdfQueue1<TtUIModifier>
    {
        public TtUIHost UIHost;
        public NxRHI.UCbView PerUIMeshCBuffer { get; set; }
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            PerUIMeshCBuffer = (mdf as TtMdfUIMesh).PerUIMeshCBuffer;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmdlist, UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmdlist, drawcall, policy, atom);
            unsafe
            {
                var shaderBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                if(PerUIMeshCBuffer == null)
                {
                    if(shaderBinder.CBPerUIMesh.UpdateFieldVar(drawcall.GraphicsEffect, "cbUIMesh"))
                    {
                        PerUIMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(shaderBinder.CBPerUIMesh.Binder.mCoreObject);
                    }
                }

                var binder = drawcall.FindBinder("cbUIMesh");
                if (binder.IsValidPointer == false)
                    return;
                drawcall.BindCBuffer(binder, PerUIMeshCBuffer);

                EngineNS.Canvas.FDrawCmd cmd = new EngineNS.Canvas.FDrawCmd();
                cmd.NativePointer = atom.MaterialMesh.SubMeshes[0].Mesh.mCoreObject.GetAtomExtData((uint)atom.AtomIndex).NativePointer;
                var length = UIHost.TransformedUIElementCount;
                Matrix* absTrans = (Matrix*)PerUIMeshCBuffer.mCoreObject.GetVarPtrToWrite(shaderBinder.CBPerUIMesh.AbsTransform, (uint)length);
                for(var i=0; i<length; i++)
                {
                    var data = UIHost.TransformedElements[i];
                    data.UpdateMatrix();
                    *absTrans = data.Matrix;
                    absTrans++;
                }
                PerUIMeshCBuffer.mCoreObject.FlushWrite(true, UEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);

                var brush = cmd.GetBrush();
                if (brush.Name.StartWith("@Text:"))
                {
                    var srv = brush.GetSrv();
                    if (srv.IsValidPointer)
                    {
                        drawcall.mCoreObject.BindResource(TtNameTable.FontTexture, srv.NativeSuper);
                    }
                    unsafe
                    {
                        if(cmd.IsDirty)
                        {
                            var res = drawcall.mCoreObject.FindGpuResource(TtNameTable.cbPerMaterial);
                            var cbuffer = new NxRHI.ICbView(res.CppPointer);
                            if (cbuffer.IsValidPointer)
                            {
                                var fld = cbuffer.ShaderBinder.FindField("FontColor");
                                if (fld.IsValidPointer)
                                {
                                    var color = cmd.InstanceData.Color.ToColor4Float();
                                    cbuffer.SetValue(fld, &color, sizeof(Color4f), true, UEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
                                    cmd.IsDirty = false;
                                }
                            }
                        }
                    }
                }
                else if (brush.Name.StartWith("@MatInst:"))// == VNameString.FromString("utest/ddd.uminst"))
                {
                    var srv = brush.GetSrv();
                    if (srv.IsValidPointer)
                    {
                        drawcall.mCoreObject.BindResource(TtNameTable.UITexture, srv.NativeSuper);
                        //var matIns = mesh.Atoms[atom].Material as UMaterialInstance;
                    }
                    unsafe
                    {
                        if (brush.IsDirty)
                        {
                            var res = drawcall.mCoreObject.FindGpuResource(TtNameTable.cbPerMaterial);
                            var cbuffer = new NxRHI.ICbView(res.CppPointer);
                            if (cbuffer.IsValidPointer)
                            {
                                var fld = cbuffer.ShaderBinder.FindField("UIColor");
                                if (fld.IsValidPointer)
                                {
                                    var color = brush.Color.ToColor4Float();
                                    cbuffer.SetValue(fld, &color, sizeof(Color4f), true, UEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
                                    brush.IsDirty = false;
                                }
                                //var fld = cbuffer->ShaderBinder.FindField("UIMatrix");
                                //cbuffer->SetMatrix(fld, new Matrix(), true);
                            }
                        }
                    }
                }
            }
        }
    }
}
