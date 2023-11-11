using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    [Obsolete]
    public class IExpression
    {
        public IExpression NextExpr;
    }
    [Obsolete]
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
    [Obsolete]
    public enum EMonocularOp
    {
        Not,// !
        BitXor,// ~
        RIncrease,//i++
        LIncrease,//++i
        RDecrease,//i--
        LDecrease,//--i
    }
    [Obsolete]
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
    [Obsolete]
    public enum EVisitMode
    {
        Public,
        Protected,
        Private,
        Local,
    }
    [Obsolete]
    public interface IGen
    {
        void GenLines(IExpression src, ICodeGen cgen);
        bool IsFlowControl { get; }
    }
    [Obsolete]
    public interface IOpGen : IGen
    {
        string Gen(OpExpress src, ICodeGen cgen);
    }
    //定义表达式
    [Rtti.Meta]
    [Obsolete]
    public partial class DefineAttribute : IExpression
    {
        [Rtti.Meta]
        public string AttributeName { get; set; }
        [Rtti.Meta]
        public string NameSpace { get; set; }
        [Rtti.Meta]
        public List<DefineVar> Members
        {
            get;
            set;
        } = new List<DefineVar>();

        public string GetFullName()
        {
            return NameSpace + '.' + AttributeName;
        }

    }
    [Rtti.Meta]
    [Obsolete]
    public partial class DefineClass : IExpression
    {
        [Rtti.Meta]
        public string ClassName { get; set; } = "NewClass";
        //[DefineVar.PropDefTypeEditor(typeof(void), ExcludeValueType = true, ExcludeSealed = true, AssemblyFilter = "EngineCore")]
        //[EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true)]
        [ReadOnly(true)]
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
        public Rtti.UTypeDesc TryGetTypeDesc()
        {
            return Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(GetFullName());
        }
        public Rtti.UClassMeta TryGetTypeMeta()
        {
            var desc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(GetFullName());
            if (desc == null)
                return null;

            return Rtti.TtClassMetaManager.Instance.GetMeta(desc);
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
    [Obsolete]
    public partial class DefineVar : IExpression
    {
        [Rtti.Meta]
        public EVisitMode VisitMode { get; set; } = EVisitMode.Public;
        [Rtti.Meta]
        public bool IsLocalVar { get; set; } = true;
        public class PropDefTypeEditor : EGui.Controls.PropertyGrid.PGTypeEditorAttribute
        {
            public PropDefTypeEditor(System.Type baseType, bool allowVoid)
                : base(baseType)
            {
                AllowVoidType = allowVoid;
            }
            public bool AllowVoidType = false;
            public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;

                ImGuiAPI.SetNextItemWidth(-1);
                TypeSlt.AssemblyFilter = AssemblyFilter;
                TypeSlt.FilterMode = FilterMode;
                TypeSlt.FilterMode |= EGui.Controls.UTypeSelector.EFilterMode.ExcludeNoMeta;// .SearchFromMetas = true;
                TypeSlt.BaseType = BaseType;
                var typeStr = info.Value as string;
                TypeSlt.SelectedType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeStr);
                int maxItem = TypeSlt.ShowTypes.Count;
                if (maxItem > 12)
                    maxItem = 12;
                if (TypeSlt.OnDraw(-1, maxItem, (type)=>
                    {
                        if (!AllowVoidType)
                        {
                            if (type.SystemType == typeof(void))
                                return true;
                        }
                        return false;
                    }))
                {
                    var v = TypeSlt.SelectedType;
                    newValue = v.FullName;
                    return true;
                }
                return false;
            }
        }
        //[PropDefTypeEditor(typeof(void))]
        [PropDefTypeEditor(null, false)]
        [Rtti.Meta]
        public string DefType { get; set; } = typeof(int).FullName;
        [Rtti.Meta]
        public string VarName { get; set; } = null;
        [Rtti.Meta]
        public string InitValue { get; set; } = null;
    }
    [Rtti.Meta]
    [Obsolete]
    public class DefineFunctionParam : DefineVar
    {
        public enum enOpType
        {
            normal,
            In,
            Out,
            Ref,
        }
        [Rtti.Meta]
        public enOpType OpType { get; set; } = enOpType.normal;
        [Rtti.Meta]
        public bool IsParamArray { get; set; } = false;
    }
    [Rtti.Meta]
    [Obsolete]
    public partial class DefineFunction : IExpression
    {
        [Browsable(false)]
        public bool IsFunctionDefineChanged
        {
            get;
            set;
        } = false;

        [Rtti.Meta, Browsable(false)]
        public List<DefineAttribute> Attributes
        {
            get;
            set;
        } = new List<DefineAttribute>()
        {
            new DefineAttribute()
            {
                AttributeName = "MetaAttribute",
                NameSpace = "EngineNS.Rtti",
            }
        };
        [Rtti.Meta]
        public EVisitMode VisitMode { get; set; } = EVisitMode.Public;
        //[DefineVar.PropDefTypeEditor(typeof(void))]
        [DefineVar.PropDefTypeEditor(null, true)]
        [Rtti.Meta]
        public string ReturnType { get; set; }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta, ReadOnly(true)]
        public List<DefineFunctionParam> Arguments
        {
            get;
            set;
        } = new List<DefineFunctionParam>();

        [Rtti.Meta, Browsable(false)]
        public bool IsOverride { get; set; } = false;

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
    [Obsolete]
    public class OpExpress : IExpression
    {
    }
    [Obsolete]
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
    [Obsolete]
    public class OpUseDefinedVar : OpExpress
    {
        public OpUseDefinedVar(DefineVar v)
        {
            DefVar = v;
        }
        public OpExpress Self;
        public DefineVar DefVar;
    }
    [Obsolete]
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
    [Obsolete]
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
    [Obsolete]
    public class MonocularOp : OpExpress
    {
        public MonocularOp(EMonocularOp o)
        {
            Op = o;
        }
        public EMonocularOp Op;
        public OpExpress Target;
    }
    [Obsolete]
    public class IndexerOp : OpExpress
    {
        public OpExpress Target;
        public List<OpExpress> Arguments = new List<OpExpress>();
    }
    [Obsolete]
    public class ThisVar : OpExpress
    {

    }
    [Obsolete]
    public class ConstVar : OpExpress
    {
        public ConstVar(string n)
        {
            Num = n;
        }
        public Rtti.UTypeDesc VarType;
        public string Num;
    }
    [Obsolete]
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
    [Obsolete]
    public class CallDefFunOp : OpExpress
    {
        public OpExpress Host;
        public DefineFunction Function;
        public List<OpExpress> Arguments = new List<OpExpress>();
    }
    #region BoolOp
    [Obsolete]
    public class BoolOp : OpExpress
    {
    }
    [Obsolete]
    public class BoolEqualOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    [Obsolete]
    public class BoolGreateOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    [Obsolete]
    public class BoolGreateEqualOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    [Obsolete]
    public class BoolLessOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    [Obsolete]
    public class BoolLessEqualOp : BoolOp
    {
        public OpExpress Left;
        public OpExpress Right;
    }
    [Obsolete]
    public class BoolAndOp : BoolOp
    {
        public BoolOp Left;
        public BoolOp Right;
    }
    [Obsolete]
    public class BoolOrOp : BoolOp
    {
        public BoolOp Left;
        public BoolOp Right;
    }
    [Obsolete]
    public class BoolNotOp : BoolOp
    {
        public BoolOp Target;
    }
    #endregion
    [Obsolete]
    public class DefineAndInitVarOp : OpExpress
    {
        public string DefType;
        public string VarName;
        public string VarValue;
    }
    [Obsolete]
    public class ConvertTypeOp : OpExpress
    {
        public string TargetType;
        public OpExpress ObjExpr;
        public bool UseAs;
    }
    [Obsolete]
    public class NewObjectOp : OpExpress
    {
        public string Type;
        public string InitValue;
    }
    [Obsolete]
    public class HardCodeOp : OpExpress
    {
        public string Code;
    }
    // 获取默认值
    [Obsolete]
    public class DefaultValueOp : OpExpress
    {
        public string Type;
        public string ValueName;
    }
    [Obsolete]
    public class VariableReferenceOp : OpExpress
    {
        public enum eReferenceType
        {
            None,
            In,
            Out,
            Ref,
        }
        public eReferenceType ReferenceType = eReferenceType.None;
        public string VariableName;
    }
    #endregion

    #region FlowExpression
    //流程控制表达式
    [Obsolete]
    public class ExecuteSequence : IExpression
    {
        public List<IExpression> Lines = new List<IExpression>();
        public void PushExpr(IExpression expr)
        {
            Lines.Add(expr);
        }
    }
    [Obsolete]
    public class ReturnOp : IExpression
    {
        public OpExpress ReturnExpr;
    }
    [Obsolete]
    public class IfOp : IExpression
    {
        public OpExpress Condition;//Must be BoolOp or Cmp
        public ExecuteSequence TrueExpr;
        public List<IfOp> ElseIfs = new List<IfOp>();//这里塞入的IfOp的ElseExpr将忽略
        public ExecuteSequence ElseExpr;
    }
    [Obsolete]
    public class ForOp : IExpression
    {
        public OpExpress BeginExpr;
        public BoolOp ConditionExpr;
        public OpExpress LoopExpr;
        public ExecuteSequence LoopBody = new ExecuteSequence();
    }
    [Obsolete]
    public class ContinueOp : IExpression
    {

    }
    [Obsolete]
    public class BreakOp : IExpression
    {

    }
    [Obsolete]
    public class AssignOp : BinocularOp
    {
        public AssignOp()
            : base(EBinocularOp.Assign)
        {
            
        }
    }
    #endregion
}
