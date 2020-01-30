using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ActorControlConstructionParams))]
    public partial class ActorControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class ActorControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Guid ActorId;
            [EngineNS.Rtti.MetaData]
            public string ActorName;
            
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ActorControlConstructionParams;
                retVal.ActorId = ActorId;
                retVal.ActorName = ActorName;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ActorControlConstructionParams;
                if (param == null)
                    return false;
                if (ActorId == param.ActorId)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + ActorId.ToString()).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mActorValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public ActorControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ActorValueLinkHandle", mActorValueLinkHandle, null);

            var param = csParam as ActorControlConstructionParams;
            NodeName = param.ActorName;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ActorValueLinkHandle", typeof(EngineNS.GamePlay.Actor.GActor), CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region 生成代码

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(EngineNS.GamePlay.Actor.GActor).FullName;
        }
        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(EngineNS.GamePlay.Actor.GActor);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as ActorControlConstructionParams;
            return new System.CodeDom.CodeSnippetExpression($"EngineNS.CEngine.Instance.GameInstance.World.FindActor(EngineNS.Rtti.RttiHelper.GuidTryParse(\"{param.ActorId.ToString()}\"))");
        }

        #endregion
    }
}
