using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.HLSL
{
    [Obsolete]
    public class UHLSLGen : ICodeGen
    {
        public DefineClassGen mDefineClassGen = new DefineClassGen();
        public DefineVarGen mDefineVarGen = new DefineVarGen();
        public DefineFunctionGen mDefineFunctionGen = new DefineFunctionGen();
        public ExecuteSequenceGen mExecuteSequenceGen = new ExecuteSequenceGen();
        public OpUseVarGen mOpUseVarGen = new OpUseVarGen();
        public OpUseDefinedVarGen mOpUseDefinedVarGen = new OpUseDefinedVarGen();
        public OpExecuteAndUseDefinedVarGen mOpExecuteAndUseDefinedVarGen = new OpExecuteAndUseDefinedVarGen();
        public BinocularOpGen mBinocularOpGen = new BinocularOpGen();
        public MonocularOpGen mMonocularOpGen = new MonocularOpGen();
        public CallOpGen mCallOpGen = new CallOpGen();
        public CallDefFunOpGen mCallDefFunOpGen = new CallDefFunOpGen();
        public ReturnOpGen mReturnOpGen = new ReturnOpGen();
        public IfOpGen mIfOpGen = new IfOpGen();
        public IndexerOpGen mIndexerOpGen = new IndexerOpGen();
        public ThisVarGen mThisVarGen = new ThisVarGen();
        public ConstVarGen mConstVarGen = new ConstVarGen();
        public BoolEqualOpGen mBoolEqualOpGen = new BoolEqualOpGen();
        public BoolGreateOpGen mBoolGreateOpGen = new BoolGreateOpGen();
        public BoolGreateEqualOpGen mBoolGreateEqualOpGen = new BoolGreateEqualOpGen();
        public BoolLessOpGen mBoolLessOpGen = new BoolLessOpGen();
        public BoolLessEqualOpGen mBoolLessEqualOpGen = new BoolLessEqualOpGen();
        public BoolAndOpGen mBoolAndOpGen = new BoolAndOpGen();
        public BoolOrOpGen mBoolOrOpGen = new BoolOrOpGen();
        public BoolNotOpGen mBoolNotOpGen = new BoolNotOpGen();
        public DefineAndInitVarOpGen mDefineAndInitVarOpGen = new DefineAndInitVarOpGen();
        public ConvertTypeOpGen mConvertTypeOpGen = new ConvertTypeOpGen();
        public ForOpGen mForOpGen = new ForOpGen();
        public ContinueOpGen mContinueOpGen = new ContinueOpGen();
        public BreakOpGen mBreakOpGen = new BreakOpGen();
        public AssignOpGen mAssignOpGen = new AssignOpGen();
        public NewObjectOpGen mNewObjectOpGen = new NewObjectOpGen();
        public HardCodeOpGen mHardCodeOpGen = new HardCodeOpGen();
        public DefaultValueOpGen mDefaultValueOpGen = new DefaultValueOpGen();
        public VariableReferenceOpGen mArgumentReferenceOpGen = new VariableReferenceOpGen();
        public override IGen GetGen(Type exprType)
        {
            if (exprType == typeof(DefineClass))
            {
                return mDefineClassGen;
            }
            else if (exprType == typeof(DefineVar))
            {
                return mDefineVarGen;
            }
            else if (exprType == typeof(DefineFunction))
            {
                return mDefineFunctionGen;
            }
            else if (exprType == typeof(ExecuteSequence))
            {
                return mExecuteSequenceGen;
            }
            else if (exprType == typeof(OpUseVar))
            {
                return mOpUseVarGen;
            }
            else if (exprType == typeof(OpUseDefinedVar))
            {
                return mOpUseDefinedVarGen;
            }
            else if (exprType == typeof(OpExecuteAndUseDefinedVar))
            {
                return mOpExecuteAndUseDefinedVarGen;
            }
            else if (exprType == typeof(BinocularOp))
            {
                return mBinocularOpGen;
            }
            else if (exprType == typeof(MonocularOp))
            {
                return mMonocularOpGen;
            }
            else if (exprType == typeof(CallOp))
            {
                return mCallOpGen;
            }
            else if (exprType == typeof(CallDefFunOp))
            {
                return mCallDefFunOpGen;
            }
            else if (exprType == typeof(ReturnOp))
            {
                return mReturnOpGen;
            }
            else if (exprType == typeof(IfOp))
            {
                return mIfOpGen;
            }
            else if (exprType == typeof(IndexerOp))
            {
                return mIndexerOpGen;
            }
            else if (exprType == typeof(ThisVar))
            {
                return mThisVarGen;
            }
            else if (exprType == typeof(ConstVar))
            {
                return mConstVarGen;
            }
            else if (exprType == typeof(BoolEqualOp))
            {
                return mBoolEqualOpGen;
            }
            else if (exprType == typeof(BoolGreateOp))
            {
                return mBoolGreateOpGen;
            }
            else if (exprType == typeof(BoolGreateEqualOp))
            {
                return mBoolGreateEqualOpGen;
            }
            else if (exprType == typeof(BoolLessOp))
            {
                return mBoolLessOpGen;
            }
            else if (exprType == typeof(BoolLessEqualOp))
            {
                return mBoolLessEqualOpGen;
            }
            else if (exprType == typeof(BoolAndOp))
            {
                return mBoolAndOpGen;
            }
            else if (exprType == typeof(BoolOrOp))
            {
                return mBoolOrOpGen;
            }
            else if (exprType == typeof(BoolNotOp))
            {
                return mBoolNotOpGen;
            }
            else if (exprType == typeof(DefineAndInitVarOp))
            {
                return mDefineAndInitVarOpGen;
            }
            else if (exprType == typeof(ConvertTypeOp))
            {
                return mConvertTypeOpGen;
            }
            else if (exprType == typeof(ForOp))
            {
                return mForOpGen;
            }
            else if (exprType == typeof(ContinueOp))
            {
                return mContinueOpGen;
            }
            else if (exprType == typeof(BreakOp))
            {
                return mBreakOpGen;
            }
            else if (exprType == typeof(AssignOp))
            {
                return mAssignOpGen;
            }
            else if (exprType == typeof(NewObjectOp))
            {
                return mNewObjectOpGen;
            }
            else if (exprType == typeof(HardCodeOp))
            {
                return mHardCodeOpGen;
            }
            return null;
        }
        public override string GetTypeString(Rtti.UTypeDesc t)
        {
            if (t.IsEqual(typeof(float)))
                return "float";
            else if (t.IsEqual(typeof(Vector2)))
                return "float2";
            else if (t.IsEqual(typeof(Vector3)))
                return "float3";
            else if (t.IsEqual(typeof(Vector4)))
                return "float4";
            else if (t.IsEqual(typeof(Matrix)))
                return "matrix";
            else if (t.IsEqual(typeof(int)))
                return "int";
            else if (t.IsEqual(typeof(Int32_2)))
                return "int2";
            else if (t.IsEqual(typeof(Int32_3)))
                return "int3";
            else if (t.IsEqual(typeof(Int32_4)))
                return "int4";
            else if (t.IsEqual(typeof(uint)))
                return "uint";
            else if (t.IsEqual(typeof(UInt32_2)))
                return "uint2";
            else if (t.IsEqual(typeof(UInt32_3)))
                return "uint3";
            else if (t.IsEqual(typeof(UInt32_4)))
                return "uint4";
            else if (t.IsEqual(typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.Texture2D)))
                return "Texture2D";
            else if (t.IsEqual(typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.Texture2DArray)))
                return "Texture2DArray";
            else if (t.IsEqual(typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.SamplerState)))
                return "SamplerState";
            return t.FullName;
        }
        public override string GetDefaultValue(System.Type t)
        {
            if (t == typeof(sbyte) ||
                t == typeof(Int16) ||
                t == typeof(Int32) ||
                t == typeof(Int64) ||
                t == typeof(byte) ||
                t == typeof(UInt16) ||
                t == typeof(UInt32) ||
                t == typeof(UInt64) ||
                t == typeof(float) ||
                t == typeof(double) ||
                t == typeof(Vector2) ||
                t == typeof(Vector3) ||
                t == typeof(Vector4) ||
                t == typeof(Matrix))
            {
                return "0";
            }
            else if (t == typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.Texture2D))
            {
                return "gDefaultTextue2D";
            }
            else if (t == typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.SamplerState))
            {
                return "gDefaultSamplerState";
            }
            else if (t == typeof(string))
            {
                return "";
            }
            else if (t.IsValueType)
            {
                return $"new {t.FullName}()";
            }
            else
            {
                return "0";
            }
        }
        public static string GetVisitMode(EVisitMode mode)
        {
            return "";
        }
        public static string GetOp(EBinocularOp op)
        {
            switch (op)
            {
                case EBinocularOp.Assign:
                    return "=";
                case EBinocularOp.Add:
                    return "+";
                case EBinocularOp.Sub:
                    return "-";
                case EBinocularOp.Mul:
                    return "*";
                case EBinocularOp.Div:
                    return "/";
                case EBinocularOp.Mod:
                    return "%";
                case EBinocularOp.CmpEqual:
                    return "==";
                case EBinocularOp.CmpNotEqual:
                    return "!=";
                case EBinocularOp.CmpGreate:
                    return ">";
                case EBinocularOp.CmpLess:
                    return "<";
                case EBinocularOp.CmpGreateEqual:
                    return ">=";
                case EBinocularOp.CmpLessEqual:
                    return "<=";
                case EBinocularOp.And:
                    return "&&";
                case EBinocularOp.Or:
                    return "||";
                case EBinocularOp.BitAnd:
                    return "&";
                case EBinocularOp.BitOr:
                    return "|";
                case EBinocularOp.GetMember:
                    return ".";
                default:
                    return op.ToString();
            }
        }
        public static string GetOp(EMonocularOp op)
        {
            switch (op)
            {
                case EMonocularOp.Not:
                    return "!";
                case EMonocularOp.BitXor:
                    return "~";
                case EMonocularOp.RIncrease:
                case EMonocularOp.LIncrease:
                    return "++";
                case EMonocularOp.RDecrease:
                case EMonocularOp.LDecrease:
                    return "--";
                default:
                    return null;
            }
        }
    }
    [Obsolete]
    public class ExprGen : IGen
    {
        public virtual void GenLines(IExpression src, ICodeGen cgen)
        {

        }
        public virtual bool IsFlowControl { get => false; }
    }
    [Obsolete]
    public class DefineClassGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (DefineClass)src;
            if (string.IsNullOrEmpty(expr.NameSpace) == false)
            {
                cgen.AddLine($"namespace {expr.NameSpace}");
                cgen.PushBrackets();
            }
            if (string.IsNullOrEmpty(expr.SuperClassName))
                cgen.AddLine($"public class {expr.ClassName}");
            else
                cgen.AddLine($"public class {expr.ClassName} : {expr.SuperClassName}");
            cgen.PushBrackets();
            {
                foreach (var i in expr.Members)
                {
                    csGen.mDefineVarGen.GenLines(i, cgen);
                }
                foreach (var i in expr.Functions)
                {
                    csGen.mDefineFunctionGen.GenLines(i, cgen);
                }
            }
            cgen.PopBrackets();
            if (string.IsNullOrEmpty(expr.NameSpace) == false)
            {
                cgen.PopBrackets();
            }
        }
    }
    [Obsolete]
    public class DefineVarGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var expr = (DefineVar)src;
            cgen.AddLine($"{UHLSLGen.GetVisitMode(expr.VisitMode)} {expr.DefType} {expr.VarName};");
        }
        public string GenAsArgument(DefineVar expr, UHLSLGen cgen)
        {
            return $"{expr.DefType} {expr.VarName}";
        }
    }
    [Obsolete]
    public class DefineFunctionGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (DefineFunction)src;
            if (expr.ReturnType == "System.Void")
            {
                cgen.AppendCode($"{UHLSLGen.GetVisitMode(expr.VisitMode)} void {expr.Name}(", true, false);
            }
            else
            {
                cgen.AppendCode($"{UHLSLGen.GetVisitMode(expr.VisitMode)} {expr.ReturnType} {expr.Name}(", true, false);
            }
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                cgen.AppendCode(csGen.mDefineVarGen.GenAsArgument(expr.Arguments[i], csGen), false, false);
                if (i < expr.Arguments.Count - 1)
                {
                    cgen.AppendCode(",", false, false);
                }
            }
            cgen.AppendCode(")", false, true);
            cgen.PushBrackets();
            {
                foreach (var i in expr.LocalVars)
                {
                    if (i.InitValue != null)
                        cgen.AddLine($"{i.DefType} {i.VarName} = ({i.DefType}){i.InitValue};");
                    else
                        cgen.AddLine($"{i.DefType} {i.VarName};");
                }
                var bodyExpr = csGen.GetGen(expr.Body.GetType());
                bodyExpr.GenLines(expr.Body, cgen);
            }
            cgen.PopBrackets();
        }
    }
    [Obsolete]
    public class ExecuteSequenceGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ExecuteSequence)src;
            foreach (var i in expr.Lines)
            {
                var lexpr = csGen.GetGen(i.GetType());
                //if (lexpr is IOpGen)
                //{
                //    cgen.AddLine($"{((IOpGen)lexpr).Gen((OpExpress)i, cgen)};");
                //}
                //else
                {
                    lexpr.GenLines(i, cgen);
                }
            }
        }
    }
    [Obsolete]
    public class OpUseVarGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (OpUseVar)src;
            if (expr.IsMember)
                return $"this.{expr.Name}";
            else
                return $"{expr.Name}";
        }
    }
    [Obsolete]
    public class OpUseDefinedVarGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (OpUseDefinedVar)src;

            if (expr.DefVar.IsLocalVar)
                return expr.DefVar.VarName;

            if (expr.Self != null)
            {
                var selfGen = csGen.GetGen(expr.Self.GetType()) as IOpGen;
                var strSelf = selfGen.Gen(expr.Self, cgen);
                return $"{strSelf}.{expr.DefVar.VarName}";
            }
            else
                return expr.DefVar.VarName;
        }
    }
    [Obsolete]
    public class OpExecuteAndUseDefinedVarGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (OpExecuteAndUseDefinedVar)src;
            var execGen = csGen.GetGen(expr.ExecuteExpr.GetType());
            execGen.GenLines(expr.ExecuteExpr, cgen);

            if (expr.DefVar.IsLocalVar)
                return expr.DefVar.VarName;

            if (expr.Self != null)
            {
                var selfGen = csGen.GetGen(expr.Self.GetType()) as IOpGen;
                var strSelf = selfGen.Gen(expr.Self, cgen);
                return $"{strSelf}.{expr.DefVar.VarName}";
            }
            else
                return expr.DefVar.VarName;
        }
    }
    [Obsolete]
    public class BinocularOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BinocularOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            var strOp = UHLSLGen.GetOp(expr.Op);

            if (strOp == ".")
                return $"({strLeft}){strOp}{strRight}";
            else
                return $"({strLeft}) {strOp} ({strRight})";
        }
    }
    [Obsolete]
    public class MonocularOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (MonocularOp)src;

            var tarGen = csGen.GetGen(expr.Target.GetType()) as IOpGen;
            var strTar = tarGen.Gen(expr.Target, cgen);

            var strOp = UHLSLGen.GetOp(expr.Op);

            if (expr.Op == EMonocularOp.RIncrease ||
                expr.Op == EMonocularOp.RDecrease)
            {
                return $"({strTar}){strOp}";
            }
            else
            {
                return $"{strOp}({strTar})";
            }
        }
    }
    [Obsolete]
    public class CallOpGen : ExprGen, IOpGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (CallOp)src;

            var hostGen = csGen.GetGen(expr.Host.GetType()) as IOpGen;
            var strHost = hostGen.Gen(expr.Host, cgen);
            var strName = expr.Name;

            string callArgs = "";
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var argGen = csGen.GetGen(expr.Arguments[i].GetType()) as IOpGen;
                var str = argGen.Gen(expr.Arguments[i], cgen);
                callArgs += str;
                if (i < expr.Arguments.Count - 1)
                    callArgs += ",";
            }

            string opCode;
            if (string.IsNullOrEmpty(strHost))
                opCode = $"{strName}({callArgs})";
            else
                opCode = $"{strHost}.{strName}({callArgs})";

            if (!string.IsNullOrEmpty(expr.FunReturnLocalVar))
            {
                if (expr.ConvertType != null)
                    cgen.AddLine($"{expr.FunReturnLocalVar} = ({expr.ConvertType.TargetType}){opCode};");
                else
                    cgen.AddLine($"{expr.FunReturnLocalVar} = {opCode};");
            }
            else
            {
                cgen.AddLine($"{opCode};");
            }

            if (expr.NextExpr != null)
            {
                var nextGen = csGen.GetGen(expr.NextExpr.GetType());
                nextGen.GenLines(expr.NextExpr, cgen);
            }
        }
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            GenLines(src, cgen);
            var csGen = (UHLSLGen)cgen;
            var expr = (CallOp)src;
            return expr.FunOutLocalVar;
        }
    }
    [Obsolete]
    public class CallDefFunOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (CallDefFunOp)src;

            var hostGen = csGen.GetGen(expr.Host.GetType()) as IOpGen;

            var strHost = hostGen.Gen(expr.Host, cgen);
            var strName = expr.Function.Name;

            string callArgs = "";
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var argGen = csGen.GetGen(expr.Arguments[i].GetType()) as IOpGen;
                var str = argGen.Gen(expr.Arguments[i], cgen);
                callArgs += str;
                if (i < expr.Arguments.Count - 1)
                    callArgs += ",";
            }
            if (string.IsNullOrEmpty(strHost))
                return $"{strName}({callArgs})";
            else
                return $"{strHost}.{strName}({callArgs})";
        }
    }
    [Obsolete]
    public class ReturnOpGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ReturnOp)src;

            var returnGen = csGen.GetGen(expr.ReturnExpr?.GetType()) as IOpGen;

            var strReturn = returnGen?.Gen(expr.ReturnExpr, cgen);

            if (string.IsNullOrEmpty(strReturn))
                cgen.AddLine($"return;");
            else
                cgen.AddLine($"return {strReturn};");
        }
        public override bool IsFlowControl { get => true; }
    }
    [Obsolete]
    public class IfOpGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (IfOp)src;

            var conditionGen = csGen.GetGen(expr.Condition.GetType()) as IOpGen;
            var trueGen = csGen.GetGen(expr.TrueExpr?.GetType());
            var falseGen = csGen.GetGen(expr.ElseExpr?.GetType());

            var strCondition = conditionGen.Gen(expr.Condition, cgen);

            cgen.AddLine($"if( {strCondition} )");
            cgen.PushBrackets();
            {
                if (expr.TrueExpr != null)
                    trueGen.GenLines(expr.TrueExpr, cgen);
            }
            cgen.PopBrackets();
            foreach (var i in expr.ElseIfs)
            {
                conditionGen = csGen.GetGen(i.Condition.GetType()) as IOpGen;
                strCondition = conditionGen.Gen(i.Condition, cgen);
                var trueGen1 = csGen.GetGen(i.TrueExpr?.GetType());
                cgen.AddLine($"else if( {strCondition} )");
                cgen.PushBrackets();
                {
                    if (i.TrueExpr != null)
                        trueGen1.GenLines(i.TrueExpr, cgen);
                }
                cgen.PopBrackets();
            }
            if (expr.ElseExpr != null)
            {
                cgen.AddLine($"else");
                cgen.PushBrackets();
                {
                    falseGen.GenLines(expr.ElseExpr, cgen);
                }
                cgen.PopBrackets();
            }
            if (expr.NextExpr != null)
            {
                var nextGen = csGen.GetGen(expr.NextExpr.GetType());
                nextGen.GenLines(expr.NextExpr, cgen);
            }
        }
        public override bool IsFlowControl { get => true; }
    }
    [Obsolete]
    public class ForOpGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ForOp)src;

            var beginGen = csGen.GetGen(expr.BeginExpr.GetType()) as IOpGen;
            var condiGen = csGen.GetGen(expr.ConditionExpr.GetType()) as IOpGen;
            var loopGen = csGen.GetGen(expr.LoopExpr.GetType()) as IOpGen;

            var strBegin = beginGen.Gen(expr.BeginExpr, csGen);
            var strCondi = condiGen.Gen(expr.ConditionExpr, csGen);
            var strLoop = loopGen.Gen(expr.LoopExpr, csGen);

            csGen.AddLine($"for( {strBegin}; {strCondi}; {strLoop} )");
            csGen.PushBrackets();
            {
                var loopBodyGen = csGen.GetGen(expr.LoopBody.GetType());
                loopBodyGen.GenLines(expr.LoopBody, cgen);
            }
            csGen.PopBrackets();
        }
    }
    [Obsolete]
    public class ContinueOpGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ContinueOp)src;

            csGen.AddLine($"continue");
        }
    }
    [Obsolete]
    public class BreakOpGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BreakOp)src;

            csGen.AddLine($"break");
        }
    }
    [Obsolete]
    public class IndexerOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (IndexerOp)src;

            var tarGen = csGen.GetGen(expr.Target.GetType()) as IOpGen;
            string args = "";
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                var at = csGen.GetGen(expr.Arguments[i].GetType()) as IOpGen;
                args += at.Gen(expr.Arguments[i], cgen);
                if (i < expr.Arguments.Count - 1)
                {
                    args += ",";
                }
            }
            return $"({tarGen})[{args}]";
        }
    }
    [Obsolete]
    public class ThisVarGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ThisVar)src;
            return "this";
        }
    }
    [Obsolete]
    public class ConstVarGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ConstVar)src;
            return expr.Num;
        }
    }
    [Obsolete]
    public class BoolEqualOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolEqualOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} == {strRight}";
        }
    }
    [Obsolete]
    public class BoolGreateOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolGreateOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} > {strRight}";
        }
    }
    [Obsolete]
    public class BoolGreateEqualOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolGreateEqualOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} >= {strRight}";
        }
    }
    [Obsolete]
    public class BoolLessOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolLessOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} < {strRight}";
        }
    }
    [Obsolete]
    public class BoolLessEqualOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolLessEqualOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} <= {strRight}";
        }
    }
    [Obsolete]
    public class BoolAndOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolAndOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} && {strRight}";
        }
    }
    [Obsolete]
    public class BoolOrOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolOrOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            return $"{strLeft} || {strRight}";
        }
    }
    [Obsolete]
    public class BoolNotOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BoolNotOp)src;

            var tarGen = csGen.GetGen(expr.Target.GetType()) as IOpGen;

            var strTar = tarGen.Gen(expr.Target, cgen);

            return $"!({strTar})";
        }
    }
    [Obsolete]
    public class DefineAndInitVarOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (DefineAndInitVarOp)src;
            return $"{expr.DefType} {expr.VarName} = {expr.VarValue}";
        }
    }
    [Obsolete]
    public class ConvertTypeOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (ConvertTypeOp)src;

            var objGen = csGen.GetGen(expr.ObjExpr.GetType()) as IOpGen;
            var strObj = objGen.Gen(expr.ObjExpr, cgen);

            if (expr.UseAs)
            {
                return $"(({strObj}) as {expr.TargetType})";
            }
            else
            {
                return $"(({expr.TargetType})({strObj}))";
            }
        }
    }
    [Obsolete]
    public class NewObjectOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (NewObjectOp)src;

            if (expr.InitValue == "null")
                return "null";
            switch (expr.Type)
            {
                case "System.SByte":
                case "System.Byte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Single":
                case "System.Double":
                    {
                        if (!string.IsNullOrEmpty(expr.InitValue))
                            return $"({expr.Type}){expr.InitValue}";
                        return "0";
                    }
                case "System.String":
                    {
                        if (!string.IsNullOrEmpty(expr.InitValue))
                            return expr.InitValue;
                        return "";
                    }
                case "System.Type":
                    {
                        if (!string.IsNullOrEmpty(expr.InitValue))
                            return $"typeof({expr.InitValue})";
                        return "null";
                    }
                default:
                    {
                        return $"new {expr.Type}()";
                    }
            }
        }
    }
    [Obsolete]
    public class DefaultValueOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var expr = (DefaultValueOp)src;
            switch(expr.Type)
            {
                case "float":
                case "System.Single":
                    return "0";
                case "flost2":
                case "EngineNS.Vector2":
                    return "float2(0,0)";
                case "float3":
                case "EngineNS.Vector3":
                    return "float3(0,0,0)";
                case "float4":
                case "EngineNS.Vector4":
                    return "float4(0,0,0,0)";
                default:
                    throw new InvalidOperationException($"{expr.Type} is invalid type in default value");
            }
        }
    }
    [Obsolete]
    public class VariableReferenceOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var varRefOp = (VariableReferenceOp)src;
            return varRefOp.VariableName;
        }
    }
    [Obsolete]
    public class AssignOpGen : ExprGen
    {
        public override void GenLines(IExpression src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (BinocularOp)src;

            var leftGen = csGen.GetGen(expr.Left.GetType()) as IOpGen;
            var rightGen = csGen.GetGen(expr.Right.GetType()) as IOpGen;

            var strLeft = leftGen.Gen(expr.Left, cgen);
            var strRight = rightGen.Gen(expr.Right, cgen);

            var strOp = UHLSLGen.GetOp(expr.Op);

            cgen.AddLine($"({strLeft}) {strOp} ({strRight});");
            if (expr.NextExpr != null)
            {
                var nextGen = csGen.GetGen(expr.NextExpr.GetType());
                nextGen.GenLines(expr.NextExpr, cgen);
            }
        }
    }

    [Obsolete]
    public class HardCodeOpGen : ExprGen, IOpGen
    {
        public string Gen(OpExpress src, ICodeGen cgen)
        {
            var csGen = (UHLSLGen)cgen;
            var expr = (HardCodeOp)src;

            return expr.Code;
        }
    }

}
