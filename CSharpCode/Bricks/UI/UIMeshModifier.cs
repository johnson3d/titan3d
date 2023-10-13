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
    public class TtMdfUIMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public TtUIHost UIHost;
        public NxRHI.UCbView PerUIMeshCBuffer { get; set; }
        public TtMdfUIMesh()
        {
            UpdateShaderCode();
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[]
            {
                EVertexStreamType.VST_Position,
                EVertexStreamType.VST_UV,
                EVertexStreamType.VST_SkinIndex,
            };
        }
        public override void CopyFrom(UMdfQueue mdf)
        {
            base.CopyFrom(mdf);
            PerUIMeshCBuffer = (mdf as TtMdfUIMesh).PerUIMeshCBuffer;
        }
        protected override string GetBaseBuilder(UHLSLCodeGenerator codeBuilder)
        {
            var codeString = "";
            var mdfSourceName = RName.GetRName("shaders/modifier/UIModifier.cginc", RName.ERNameType.Engine);
            codeBuilder.AddLine($"#include \"{mdfSourceName.Address}\"", ref codeString);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                codeBuilder.AddLine("DoUIModifierVS(output, input);", ref codeString);
            }
            codeBuilder.PopSegment(ref codeString);
            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            var code = EngineNS.Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(mdfSourceName);
            codeBuilder.AddLine($"//Hash for {mdfSourceName}:{UniHash32.APHash(code.SourceCode.TextCode)}", ref codeString);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = codeString;
            return codeString;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmdlist, URenderPolicy.EShadingType shadingType, UGraphicDraw drawcall, URenderPolicy policy, UMesh mesh, int atom)
        {
            base.OnDrawCall(cmdlist, shadingType, drawcall, policy, mesh, atom);
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
                cmd.NativePointer = mesh.MaterialMesh.Mesh.mCoreObject.GetAtomExtData((uint)atom).NativePointer;
                var length = UIHost.TransformedUIElementCount;
                Matrix* absTrans = (Matrix*)PerUIMeshCBuffer.mCoreObject.GetVarPtrToWrite(shaderBinder.CBPerUIMesh.AbsTransform, (uint)length);
                for(var i=0; i<length; i++)
                {
                    var data = UIHost.TransformedElements[i];
                    data.UpdateMatrix();
                    *absTrans = data.Matrix;
                    absTrans++;
                }

                var brush = cmd.GetBrush();
                if (brush.Name.StartWith("@Text:"))
                {
                    var srv = brush.GetSrv();
                    if (srv.IsValidPointer)
                    {
                        drawcall.mCoreObject.BindResource(VNameString.FromString("FontTexture"), srv.NativeSuper);
                    }
                }
                else if (brush.Name.StartWith("@MatInst:"))// == VNameString.FromString("utest/ddd.uminst"))
                {
                    var srv = brush.GetSrv();
                    if (srv.IsValidPointer)
                    {
                        drawcall.mCoreObject.BindResource(VNameString.FromString("UITexture"), srv.NativeSuper);
                        //var matIns = mesh.Atoms[atom].Material as UMaterialInstance;
                    }
                    unsafe
                    {
                        if (brush.IsDirty)
                        {
                            var res = drawcall.mCoreObject.FindGpuResource(VNameString.FromString("cbPerMaterial"));
                            var cbuffer = new NxRHI.ICbView(res.CppPointer);
                            if (cbuffer.IsValidPointer)
                            {
                                var fld = cbuffer.ShaderBinder.FindField("UIColor");
                                if (fld.IsValidPointer)
                                {
                                    var color = brush.Color.ToColor4Float();
                                    cbuffer.SetValue(fld, &color, sizeof(Color4f));
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
        public override Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            if (features.Contains("UMdf_NoShadow"))
            {
                return Rtti.UTypeDescGetter<TtMdfUIMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc;
            }
            else
            {
                return Rtti.UTypeDescGetter<TtMdfUIMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow>>.TypeDesc;
            }
        }
    }

    public class TtMdfUIMeshPermutation<PermutationType> : TtMdfUIMesh
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            var codeString = GetBaseBuilder(codeBuilder);

            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1", ref codeString);
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {

            }

            SourceCode.TextCode = codeString;
        }
    }
}
