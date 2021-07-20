using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    [Rtti.Meta]
    public partial class HLSLMethod
    {
        [Rtti.Meta]
        [UserCallNode(CallNodeType = typeof(SampleLevel2DNode))]
        public static Vector4 SampleLevel2D(Var.Texture2D texture, Var.SamplerState sampler, Vector2 uv, float level, out Vector3 rgb)
        {
            rgb = new Vector3();
            return new Vector4();
        }
        [Rtti.Meta]
        public static void Clamp(float x, float min, float max, out float ret)
        {
            ret = 0;
        }
        [Rtti.Meta]
        public static void SinCos(float x, out float sin, out float cos)
        {
            sin = (float)Math.Sin(x);
            cos = (float)Math.Cos(x);
        }
        [Rtti.Meta]
        public static void Max(float v1, float v2, out float ret)
        {
            ret = Math.Max(v1, v2);
        }
        [Rtti.Meta]
        public static void Min(float v1, float v2, out float ret)
        {
            ret = Math.Min(v1, v2);
        }
    }
    public class UserCallNodeAttribute : Attribute
    {
        public Type CallNodeType;
    }
    public class CallNode : IBaseNode
    {
        public EGui.Controls.NodeGraph.PinOut Result = null;
        public List<EGui.Controls.NodeGraph.PinIn> Arguments = new List<EGui.Controls.NodeGraph.PinIn>();
        public List<EGui.Controls.NodeGraph.PinOut> OutArguments = new List<EGui.Controls.NodeGraph.PinOut>();
        public Rtti.UClassMeta.MethodMeta Method;
        [Rtti.Meta]
        public string MethodDeclString
        {
            get
            {
                if (Method == null)
                    return null;
                return Method.GetMethodDeclareString();
            }
            set
            {
                var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(typeof(HLSLMethod).FullName);
                Method = meta.GetMethod(value);
                this.Initialize(Method);
            }
        }
        public static CallNode NewMethodNode(Rtti.UClassMeta.MethodMeta m)
        {
            var result = new CallNode();
            result.Initialize(m);
            return result;
        }
        public CallNode()
        {
            Icon = UShaderEditorStyles.Instance.FunctionIcon;
            TitleImage.Color = UShaderEditorStyles.Instance.FunctionTitleColor;
            Background.Color = UShaderEditorStyles.Instance.FunctionBGColor;
        }
        internal void Initialize(Rtti.UClassMeta.MethodMeta m)
        {
            Method = m;
            Name = Method.Method.Name;

            if (Method.Method.ReturnType != typeof(void))
            {
                Result = new EGui.Controls.NodeGraph.PinOut();
                Result.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                Result.Name = "Result";
                AddPinOut(Result);
            }

            Arguments.Clear();
            OutArguments.Clear();
            foreach (var i in Method.Parameters)
            {
                var pin = new EGui.Controls.NodeGraph.PinIn();
                pin.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                pin.Link.CanLinks.Add("Value");
                pin.Name = i.ParamInfo.Name;

                Arguments.Add(pin);
                AddPinIn(pin);
                if (i.ParamInfo.IsOut)
                {
                    var pinOut = new EGui.Controls.NodeGraph.PinOut();
                    pinOut.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                    pin.Link.CanLinks.Add("Value");
                    pinOut.Name = i.ParamInfo.Name;
                    OutArguments.Add(pinOut);
                    AddPinOut(pinOut);
                }
                //else if (i.ParamInfo.ParameterType.IsByRef)
                //{
                //    Arguments.Add(pin);
                //    AddPinIn(pin);

                //    var pinOut = new EGui.Controls.NodeGraph.PinOut();
                //    pinOut.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                //    pin.Link.CanLinks.Add("Value");
                //    pinOut.Name = i.ParamInfo.Name;
                //    OutArguments.Add(pinOut);
                //    AddPinOut(pinOut);
                //}
                //else
                //{
                //    Arguments.Add(pin);
                //    AddPinIn(pin);
                //}
            }
        }
        public override void OnMouseStayPin(EGui.Controls.NodeGraph.NodePin pin)
        {
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as System.Type;
                    if (cvtType != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{cvtType.FullName}");
                        return;
                    }
                }
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.Method.ReturnType.FullName}");
                return;
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    var inPin = pin as EGui.Controls.NodeGraph.PinIn;
                    var paramMeta = GetInPinParamMeta(inPin);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParamInfo.ParameterType.FullName}");
                    }
                    return;
                }
            }
            for (int i = 0; i < OutArguments.Count; i++)
            {
                if (pin == OutArguments[i])
                {
                    var paramMeta = Method.FindParameter(pin.Name);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParamInfo.ParameterType.FullName}");
                    }
                    return;
                }
            }
        }
        public Rtti.UClassMeta.MethodMeta.ParamMeta GetInPinParamMeta(EGui.Controls.NodeGraph.PinIn pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    return Method.GetParameter(i);
                }
            }
            return null;
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as System.Type;
                    if (cvtType != null)
                        return cvtType;
                }
                return Method.Method.ReturnType;
            }
            foreach (var i in OutArguments)
            {
                if (pin == i)
                {
                    foreach (var j in Method.Parameters)
                    {
                        if (j.ParamInfo.Name == i.Name && j.ParamInfo.IsOut)
                        {
                            return j.ParamInfo.ParameterType.GetElementType();
                        }
                    }
                }
            }
            return null;
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as IBaseNode;
            if (nodeExpr == null)
                return true;

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i])
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    return ICodeGen.CanConvert(testType, Method.GetParameter(i).ParamInfo.ParameterType);
                }
            }
            return true;
        }
        public override void PreGenExpr()
        {
            Executed = false;
        }
        bool Executed = false;
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            if (Executed)
            {
                if (oPin == Result)
                {
                    var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                    return new OpUseVar(mth_ret_temp_name, false);
                }
                else
                {
                    var parameters = Method.Method.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].Name == oPin.Name)
                        {
                            System.Diagnostics.Debug.Assert(parameters[i].IsOut);
                            var mth_outarg_temp_name = $"tmp_o_{parameters[i].Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                            return new OpUseVar(mth_outarg_temp_name, false);
                        }
                    }
                }
                System.Diagnostics.Debug.Assert(false);
            }
            Executed = true;

            ConvertTypeOp cvtExpr = null;
            DefineVar retVar = null;

            if (Method.Method.ReturnType != typeof(void))
            {
                var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                retVar = new DefineVar();
                retVar.IsLocalVar = true;
                retVar.DefType = cGen.GetTypeString(Method.Method.ReturnType);
                retVar.VarName = mth_ret_temp_name;
                retVar.InitValue = cGen.GetDefaultValue(Method.Method.ReturnType);
                funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);

                if (Result != null && Result.Tag != null && ((Result.Tag as System.Type) != Method.Method.ReturnType))
                {
                    var cvtTargetType = (Result.Tag as System.Type);
                    retVar.DefType = cvtTargetType.FullName;
                    cvtExpr = new ConvertTypeOp();
                    cvtExpr.TargetType = retVar.DefType;
                }
            }
            
            var callExpr = GetExpr_Impl(funGraph, cGen) as CallOp;

            if (retVar != null)
            {
                callExpr.FunReturnLocalVar = retVar.VarName;
            }
            if (cvtExpr != null)
            {
                callExpr.ConvertType = cvtExpr;
            }

            if (oPin == Result)
            {
                var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                callExpr.FunOutLocalVar = mth_ret_temp_name;
            }
            else
            {
                var parameters = Method.Method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].Name == oPin.Name)
                    {
                        System.Diagnostics.Debug.Assert(parameters[i].IsOut);
                        var mth_outarg_temp_name = $"tmp_o_{parameters[i].Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                        callExpr.FunOutLocalVar = mth_outarg_temp_name;
                    }
                }
            }

            return callExpr;
        }
        private IExpression GetExpr_Impl(UMaterialGraph funGraph, ICodeGen cGen)
        {
            CallOp CallExpr = new CallOp();
            var links = new List<EGui.Controls.NodeGraph.PinLinker>();
            
            {
                //这里要处理Static名字获取
                //CallExpr.Host = selfExpr;
                CallExpr.IsStatic = true;
                CallExpr.Host = new HardCodeOp() { Code = "" };
                CallExpr.Name = Method.Method.Name;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (Method.Parameters[i].ParamInfo.IsOut)
                {
                    var mth_outarg_temp_name = $"tmp_o_{Method.Parameters[i].ParamInfo.Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                    var retVar = new DefineVar();
                    retVar.IsLocalVar = true;
                    retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType.GetElementType());
                    retVar.VarName = mth_outarg_temp_name;
                    retVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParamInfo.ParameterType.GetElementType());

                    funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);
                    CallExpr.Arguments.Add(new OpUseDefinedVar(retVar));
                    continue;
                }

                links.Clear();
                links = new List<EGui.Controls.NodeGraph.PinLinker>();
                funGraph.FindInLinker(Arguments[i], links);
                OpExpress argExpr = null;
                if (links.Count == 1)
                {
                    var argNode = links[0].OutNode as IBaseNode;
                    argExpr = argNode.GetExpr(funGraph, cGen, links[0].Out, true) as OpExpress;
                }
                else if (links.Count == 0)
                {
                    argExpr = OnNoneLinkedParameter(funGraph, cGen, i);
                }
                else
                {
                    throw new GraphException(this, Arguments[i], $"Arg error:{Arguments[i].Name}");
                }
                CallExpr.Arguments.Add(argExpr);
            }

            return CallExpr;
        }
        protected virtual OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            var mth_arg_temp_name = $"t_{Method.Parameters[i].ParamInfo.Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
            var retVar = new DefineVar();
            retVar.IsLocalVar = true;
            retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
            retVar.VarName = mth_arg_temp_name;
            retVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParamInfo.ParameterType);
            funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);

            return new OpUseDefinedVar(retVar);
        }
    }

    public class SampleLevel2DNode : CallNode
    {
        public SampleLevel2DNode()
        {
            PreviewWidth = 100;
            TextureVarName = $"Texture_{(uint)NodeId.GetHashCode()}";
        }
        ~SampleLevel2DNode()
        {
            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
                SnapshotPtr = IntPtr.Zero;
            }
        }
        [Rtti.Meta]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (SnapshotPtr != IntPtr.Zero)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                    handle.Free();
                    SnapshotPtr = IntPtr.Zero;
                }
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        private RHI.CShaderResourceView TextureSRV;
        IntPtr SnapshotPtr;
        public unsafe override void OnPreviewDraw(ref Vector2 prevStart, ref Vector2 prevEnd)
        {
            if (TextureSRV == null)
                return;

            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());

            if (SnapshotPtr == IntPtr.Zero)
            {
                SnapshotPtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(TextureSRV));
            }

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage(SnapshotPtr.ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        protected override OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            if (Method.Parameters[i].ParamInfo.Name == "texture")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = TextureVarName;
                return new OpUseDefinedVar(retVar);
            }
            else if (Method.Parameters[i].ParamInfo.Name == "uv")
            {
                var retVar = new DefineVar();
                retVar.IsLocalVar = false;
                retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
                retVar.VarName = "input.vUV";
                return new OpUseDefinedVar(retVar);
            }

            return base.OnNoneLinkedParameter(funGraph, cGen, i);
        }
    }
}
