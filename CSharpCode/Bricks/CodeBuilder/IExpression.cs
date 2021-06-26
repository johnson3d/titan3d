using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public class IExpression
    {
        public IExpression NextExpr;
    }
    public enum EBinocularOp
    {
        Assign,// =
        Add,// +
        Sub,// -
        Mul,// *
        Div,// /
        Mod,// %
        CmpEqual,// ==
        CmpNotEqual,// !=
        CmpGreate,// >
        CmpLess,// <
        CmpGreateEqual,// >=
        CmpLessEqual,// <=
        And,// &&
        Or,// ||        
        BitAnd,// &
        BitOr,// |   
        GetMember,//. ----> a.b
    }
    public enum EMonocularOp
    {
        Not,// !
        BitXor,// ~
        RIncrease,//i++
        LIncrease,//++i
        RDecrease,//i--
        LDecrease,//--i
    }
    public enum EFlowOp
    {
        If,// if
        Call,
        For,// for        
        Foreach,// foreach
        Continue,// continue
        Break,// break
        Return,// return
    }
    public enum EVisitMode
    {
        Public,
        Protected,
        Private,
        Local,
    }
    public interface IGen
    {
        void GenLines(IExpression src, ICodeGen cgen);
        bool IsFlowControl { get; }
    }
    public interface IOpGen : IGen
    {
        string Gen(OpExpress src, ICodeGen cgen);
    }
    //定义表达式
    [Rtti.Meta]
    public partial class DefineClass : IExpression
    {
        [Rtti.Meta]
        public string ClassName { get; set; } = "NewClass";
        [DefineVar.PropDefTypeEditor(ExcludeValueType = true, ExcludeSealed = true, AssemblyFilter = "EngineCore")]
        [Rtti.Meta]
        public string SuperClassName { get; set; } = null;
        [Rtti.Meta]
        public string NameSpace { get; set; }
        [Rtti.Meta]
        public List<DefineVar> Members { get; } = new List<DefineVar>();
        [Rtti.Meta]
        public List<DefineFunction> Functions { get; } = new List<DefineFunction>();
        public System.Type TryGetType()
        {
            var desc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(GetFullName());
            if (desc == null)
                return null;
            return desc.SystemType;
        }
        public string GetFullName()
        {
            return NameSpace + '.' + ClassName;
        }
        public void Reset()
        {
            ClassName = "NewClass";
            SuperClassName = null;
            NameSpace = null;
            Members.Clear();
            Functions.Clear();
        }
        public DefineVar FindVar(string name)
        {
            foreach(var i in Members)
            {
                if (i.VarName == name)
                    return i;
            }
            var type = TryGetType();
            if (type != null)
            {
                var prop = type.GetProperty(name);
                var result = new DefineVar();
                result.IsLocalVar = false;
                result.DefType = prop.PropertyType.FullName;
                result.VarName = name;
                result.InitValue = null;
                return result;
            }
            return null;
        }
        public DefineFunction FindMethod(string funType)
        {
            foreach (var i in Functions)
            {
                if (i.GetFunctionDeclType() == funType)
                    return i;
            }
            return null;
        }
    }
    [Rtti.Meta]
    public partial class DefineVar : IExpression
    {
        [Rtti.Meta]
        public EVisitMode VisitMode { get; set; } = EVisitMode.Public;
        [Rtti.Meta]
        public bool IsLocalVar { get; set; } = true;
        public class PropDefTypeEditor : EGui.Controls.PropertyGrid.PGTypeEditorAttribute
        {
            public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, EGui.Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
            {
                var sz = new Vector2(0, 0);
                var bindType = EGui.UIEditor.EditableFormData.Instance.CurrentForm?.BindType;
                if (bindType == null)
                    return;
                var props = bindType.SystemType.GetProperties();
                ImGuiAPI.SetNextItemWidth(-1);
                TypeSlt.AssemblyFilter = AssemblyFilter;
                TypeSlt.ExcludeValueType = ExcludeValueType;
                TypeSlt.BaseType = BaseType;
                var typeStr = value as string;
                TypeSlt.SelectedType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeStr);
                if (TypeSlt.OnDraw(-1, 12))
                {
                    foreach (var i in props)
                    {
                        var v = TypeSlt.SelectedType;
                        foreach (var j in pg.TargetObjects)
                        {
                            EGui.Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, v.FullName);
                        }
                    }
                }
            }
        }
        [PropDefTypeEditor()]
        [Rtti.Meta]
        public string DefType { get; set; } = typeof(int).FullName;
        [Rtti.Meta]
        public string VarName { get; set; } = null;
        [Rtti.Meta]
        public string InitValue { get; set; } = null;
    }
    [Rtti.Meta]
    public partial class DefineFunction : IExpression
    {
        public bool IsFunctionDefineChanged
        {
            get;
            set;
        } = false;
        public EVisitMode VisitMode { get; set; } = EVisitMode.Public;
        [DefineVar.PropDefTypeEditor()]
        [Rtti.Meta]
        public string ReturnType { get; set; }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public List<DefineVar> Arguments
        {
            get;
        } = new List<DefineVar>();

        public ExecuteSequence Body = new ExecuteSequence();
        public List<DefineVar> LocalVars = new List<DefineVar>();
        public void AddLocalVar(DefineVar var)
        {
            foreach(var i in LocalVars)
            {
                if (i.VarName == var.VarName)
                    return;
            }
            LocalVars.Add(var);
        }
        public string GetFunctionDeclType()
        {
            var result = $"{ReturnType} {Name}(";
            var args = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i < Arguments.Count - 1)
                    args += Arguments[i].DefType + ",";
                else
                    args += Arguments[i].DefType;
            }
            return result + args + ")";
        }
    }
    //运算表达式
    #region OpExpression
    public class OpExpress : IExpression
    {
    }
    public class OpUseVar : OpExpress
    {
        public bool IsMember;
        public OpUseVar(string n, bool isMember)
        {
            Name = n;
            IsMember = isMember;
        }
        public string Name;
    }
    public class OpUseDefinedVar : OpExpress
    {
        public OpUseDefinedVar(DefineVar v)
        {
            DefVar = v;
        }
        public OpExpress Self;
        public DefineVar DefVar;
    }
    public class OpExecuteAndUseDefinedVar : OpExpress
    {
        public OpExecuteAndUseDefinedVar(IExpression exec, DefineVar v)
        {
            ExecuteExpr = exec;
            DefVar = v;
        }
        public IExpression ExecuteExpr;
        public OpExpress Self;
        public DefineVar DefVar;
    }
    public class BinocularOp : OpExpress
    {
        public BinocularOp()
        {

        }
        public BinocularOp(EBinocularOp o)
        {
            Op = o;
        }
        public EBinocularOp Op;
        public OpExpress Left;
        public OpExpress Right;
        public bool IsCmpOp()
        {
            switch(Op)
            {
                case EBinocularOp.CmpEqual:
                case EBinocularOp.CmpNotEqual:
                case EBinocularOp.CmpGreate:
                case EBinocularOp.CmpGreateEqual:
                case EBinocularOp.CmpLess:
                case EBinocularOp.CmpLessEqual:
                    return true;
                default:
                    return false;
            }
        }
    }
    public class MonocularOp : OpExpress
    {
        public MonocularOp(EMonocularOp o)
        {
            Op = o;
        }
        public EMonocularOp Op;
        public OpExpress Target;
    }
    public class IndexerOp : OpExpress
    {
        public OpExpress Target;
        public List<OpExpress> Arguments = new List<OpExpress>();
    }
    public class ThisVar : OpExpress
    {

    }
    public class ConstVar : OpExpress
    {
        public ConstVar(string n)
        {
            Num = n;
        }
        public Rtti.UTypeDesc VarType;
        public string Num;
    }
    public class CallOp : OpExpress
    {
        public bool IsStatic = false;
        public OpExpress Host;
        public string Name;
        public List<OpExpress> Arguments = new List<OpExpress>();

        public string FunReturnLocalVar;
        public string FunOutLocalVar;
        public ConvertTypeOp ConvertType;
    }
    public class CallDefFunOp : OpExpress
    {
        public OpExpress Host;
        public DefineFunction Function;
        public List<OpExpress> Arguments = new List<OpExpress>();
    }
    #region BoolOp
    public class BoolOp : OpExpress
    {
    }
    public class BoolEqualOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    public class BoolGreateOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    public class BoolGreateEqualOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    public class BoolLessOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    public class BoolLessEqualOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    public class BoolAndOp : BoolOp
    {
        public BoolOp Left;
        public BoolOp Right;
    }
    public class BoolOrOp : BoolOp
    {
        public BoolOp Left;
        public BoolOp Right;
    }
    public class BoolNotOp : BoolOp
    {
        public BoolOp Target;
    }
    #endregion
    public class DefineAndInitVarOp : OpExpress
    {
        public string DefType;
        public string VarName;
        public string VarValue;
    }
    public class ConvertTypeOp : OpExpress
    {
        public string TargetType;
        public OpExpress ObjExpr;
        public bool UseAs;
    }
    public class NewObjectOp : OpExpress
    {
        public string Type;
        public string InitValue;
    }
    public class HardCodeOp : OpExpress
    {
        public string Code;
    }
    #endregion

    #region FlowExpression
    //流程控制表达式
    public class ExecuteSequence : IExpression
    {
        public List<IExpression> Lines = new List<IExpression>();
        public void PushExpr(IExpression expr)
        {
            Lines.Add(expr);
        }
    }
    public class ReturnOp : IExpression
    {
        public OpExpress ReturnExpr;
    }
    public class IfOp : IExpression
    {
        public OpExpress Condition;//Must be BoolOp or Cmp
        public ExecuteSequence TrueExpr;
        public List<IfOp> ElseIfs = new List<IfOp>();//这里塞入的IfOp的ElseExpr将忽略
        public ExecuteSequence ElseExpr;
    }
    public class ForOp : IExpression
    {
        public OpExpress BeginExpr;
        public BoolOp ConditionExpr;
        public OpExpress LoopExpr;
        public ExecuteSequence LoopBody = new ExecuteSequence();
    }
    public class ContinueOp : IExpression
    {

    }
    public class BreakOp : IExpression
    {

    }
    public class AssignOp : BinocularOp
    {
        public AssignOp()
            : base(EBinocularOp.Assign)
        {
            
        }
    }
    #endregion
}

namespace EngineNS.UTest
{
    [UTest]
    public class UTest_Codom
    {
        public int A;
        public int Func(int a)
        {
            A = a;
            return a;
        }
        public void Func2()
        {
            if(Func(1)==1)
            {
                return;
            }
        }
        public void UnitTestEntrance()
        {
            var kls = new Bricks.CodeBuilder.DefineClass() { ClassName = "UTest_Codom", SuperClassName = null };
            var mb_A = new Bricks.CodeBuilder.DefineVar() { IsLocalVar = false,  DefType = typeof(int).FullName, VarName = "A" };

            var fun_Func = new Bricks.CodeBuilder.DefineFunction() { ReturnType = typeof(int).FullName, Name = "Func" };
            fun_Func.Arguments.Add(new Bricks.CodeBuilder.DefineVar() { DefType = typeof(int).FullName, VarName = "a" });

            var fun_Func2 = new Bricks.CodeBuilder.DefineFunction() { ReturnType = typeof(void).FullName, Name = "Func2" };
            
            kls.Members.Add(mb_A);
            kls.Functions.Add(fun_Func);
            kls.Functions.Add(fun_Func2);

            fun_Func.Body.Lines.Add(new Bricks.CodeBuilder.BinocularOp(Bricks.CodeBuilder.EBinocularOp.Assign)
            {
                Left = new Bricks.CodeBuilder.OpUseDefinedVar(mb_A),
                Right = new Bricks.CodeBuilder.OpUseDefinedVar(fun_Func.Arguments[0]),//new Bricks.CodeBuilder.OpUseVar("a", false)//
            });
            
            fun_Func.Body.Lines.Add(new Bricks.CodeBuilder.ReturnOp()
            {
                ReturnExpr = new Bricks.CodeBuilder.OpUseDefinedVar(fun_Func.Arguments[0]),//new Bricks.CodeBuilder.OpUseVar("a", false)
            });

            var call_func = new Bricks.CodeBuilder.CallDefFunOp()
            {
                Host = new Bricks.CodeBuilder.ThisVar(),
                Function = fun_Func,
            };
            call_func.Arguments.Add(new Bricks.CodeBuilder.ConstVar("1"));

            var call_func2 = new Bricks.CodeBuilder.CallDefFunOp()
            {
                Host = new Bricks.CodeBuilder.ThisVar(),
                Function = fun_Func,
            };
            call_func2.Arguments.Add(new Bricks.CodeBuilder.ConstVar("2"));

            var ifExpr = new Bricks.CodeBuilder.IfOp()
            {
                Condition = new Bricks.CodeBuilder.BoolEqualOp()
                {
                    Left = call_func,
                    Right = new Bricks.CodeBuilder.ConstVar("1"),
                },
                TrueExpr = new Bricks.CodeBuilder.ExecuteSequence(),
            };
            ifExpr.TrueExpr.Lines.Add(new Bricks.CodeBuilder.ReturnOp());
            var elseifExpr2 = new Bricks.CodeBuilder.IfOp()
            {
                Condition = new Bricks.CodeBuilder.BoolEqualOp()
                {
                    Left = call_func2,
                    Right = new Bricks.CodeBuilder.ConstVar("2"),
                },
                TrueExpr = new Bricks.CodeBuilder.ExecuteSequence(),
            };
            elseifExpr2.TrueExpr.Lines.Add(new Bricks.CodeBuilder.ReturnOp());
            ifExpr.ElseIfs.Add(elseifExpr2);
            fun_Func2.Body.Lines.Add(ifExpr);

            var forExpr = new Bricks.CodeBuilder.ForOp();
            forExpr.BeginExpr = new Bricks.CodeBuilder.DefineAndInitVarOp()
            {
                DefType = typeof(int).FullName,
                VarName = "i",
                VarValue = "0",
            };

            forExpr.ConditionExpr = new Bricks.CodeBuilder.BoolLessOp()
            {
                Left = new Bricks.CodeBuilder.OpUseVar("i", false),
                Right = new Bricks.CodeBuilder.ConstVar("5"),
            };

            forExpr.LoopExpr = new Bricks.CodeBuilder.MonocularOp(Bricks.CodeBuilder.EMonocularOp.RDecrease)
            {
                Target = new Bricks.CodeBuilder.OpUseVar("i", false),
            };

            forExpr.LoopBody = new Bricks.CodeBuilder.ExecuteSequence();

            fun_Func2.Body.Lines.Add(forExpr);

            var gen = new Bricks.CodeBuilder.CSharp.CSGen();
            gen.BuildClassCode(kls);
        }
    }
}
