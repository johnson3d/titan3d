using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class Monocular : UNodeBase
    {
        public PinIn Left { get; set; } = new PinIn();
        public PinOut Result { get; set; } = new PinOut()
        {
            MultiLinks = true,
        };
    }
    public partial class TypeConverterVar : Monocular//, UEditableValue.IValueEditNotify
    {
        public static TypeConverterVar NewTypeConverterVar(Rtti.TtClassMeta src, Rtti.TtClassMeta tar)
        {
            if (tar.CanConvertTo(src) == false)
                return null;

            var result = new TypeConverterVar();
            result.SrcType = src;
            result.TarType = tar;
            //var typeSlt = result.ToType.EditValue as UTypeSelectorEValue;
            //typeSlt.Selector.BaseType = src.ClassType;
            //typeSlt.Selector.SelectedType = tar.ClassType;
            //typeSlt.Value = tar.ClassType;
            //result.OnValueChanged(typeSlt);
            result.Name = "As " + tar.ClassMetaName;

            return result;
        }
        public Rtti.TtClassMeta SrcType;
        public Rtti.TtClassMeta TarType;
        //public PinIn ToType { get; set; } = new PinIn();
        public TypeConverterVar()
        {
            Left.Name = " Src";
            //ToType.Name = " Type";
            Result.Name = "Target ";

            Left.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            //ToType.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            Result.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            Left.LinkDesc.CanLinks.Add("Value");
            //ToType.LinkDesc.CanLinks.Add("Dummy");
            Result.LinkDesc.CanLinks.Add("Value");

            //ToType.EditValue = UEditableValue.CreateEditableValue(this, typeof(System.Type), ToType);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            Name = "As";

            AddPinIn(Left);
            //AddPinIn(ToType);
            AddPinOut(Result);
        }
        [Rtti.Meta]
        public class TSaveData
        {
            [Rtti.Meta]
            public string SrcType { get; set; }
            [Rtti.Meta]
            public string TarType { get; set; }
        }

        [Rtti.Meta]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.SrcType = SrcType?.ClassMetaName;
                tmp.TarType = TarType?.ClassMetaName;
                return tmp;
            }
            set
            {
                SrcType = Rtti.TtClassMetaManager.Instance.GetMeta(value.SrcType);
                if (SrcType == null)
                    SrcType = Rtti.TtClassMetaManager.Instance.GetMetaFromFullName(typeof(object).FullName); 
                TarType = Rtti.TtClassMetaManager.Instance.GetMeta(value.TarType);
                if (TarType == null)
                    TarType = SrcType;
                //var typeSlt = ToType.EditValue as UTypeSelectorEValue;
                //typeSlt.Selector.BaseType = SrcType.ClassType;
                //typeSlt.Selector.SelectedType = TarType.ClassType;
                //typeSlt.Value = TarType.ClassType;
                //OnValueChanged(typeSlt);
                Name = "As " + TarType.ClassMetaName;
            }
        }
        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    var links = new List<UPinLinker>();
        //    funGraph.FindInLinker(Left, links);
        //    if (links.Count != 1)
        //    {
        //        throw new GraphException(this, Left, "Please link SourceObject pin");
        //    }
        //    var srcNode = links[0].OutNode as UNodeExpr;
        //    var srcExpr = srcNode.GetExpr(funGraph, cGen, true) as OpExpress;

        //    var cvtExpr = new ConvertTypeOp();
        //    cvtExpr.ObjExpr = srcExpr;
        //    cvtExpr.TargetType = TarType.ClassType.FullName;
        //    if (TarType.ClassType.SystemType.IsValueType == false)
        //        cvtExpr.UseAs = true;
        //    return cvtExpr;
        //}
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin != Result)
                return null;
            var exp = new TtCastExpression()
            {
                SourceType = new TtTypeReference(SrcType.ClassType),
                TargetType = new TtTypeReference(TarType.ClassType),
                Expression = data.NodeGraph.GetOppositePinExpression(Left, ref data),
            };
            return exp;
        }
        //public void OnValueChanged(UEditableValue ev)
        //{
        //    TarType = Rtti.UClassMetaManager.Instance.GetMeta(ev.Value as Rtti.TtTypeDesc);
        //}
        public override void OnLoadLinker(UPinLinker linker)
        {
            base.OnLoadLinker(linker);
        }
        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            if (stayPin == Left)
            {
                if (SrcType != null)
                    EGui.Controls.CtrlUtility.DrawHelper($"VarType:{SrcType.ClassType.FullName}");
            }
            else if (stayPin == Result)
            {
                if (TarType != null)
                    EGui.Controls.CtrlUtility.DrawHelper($"VarType:{TarType.ClassType.FullName}");
            }
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (TarType == null)
                return null;
            return TarType.ClassType;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            return true;
        }
        //public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        //{
        //    base.OnLinkedFrom(iPin, OutNode, oPin);

        //    if (iPin == Left)
        //    {
        //        var nodeExpr = OutNode as UNodeBase;
        //        if (nodeExpr == null)
        //            return;

        //        var typeSlt = ToType.EditValue as UTypeSelectorEValue;

        //        var newType = Rtti.UClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeStr(nodeExpr.GetOutPinType(oPin)));
        //        if (newType != null && !newType.CanConvertFrom(TarType))
        //        {//类型改变，且不能转换到目标
        //            this.ParentGraph.RemoveLinkedOut(this.Result);
        //            SrcType = newType;
        //            typeSlt.Selector.BaseType = SrcType.ClassType;
        //            typeSlt.Selector.SelectedType = SrcType.ClassType;
        //            typeSlt.Value = SrcType.ClassType;
        //            OnValueChanged(typeSlt);
        //        }
        //        else
        //        {
        //            SrcType = newType;
        //            typeSlt.Selector.BaseType = SrcType.ClassType;
        //        }
        //    }
        //}
    }
}
