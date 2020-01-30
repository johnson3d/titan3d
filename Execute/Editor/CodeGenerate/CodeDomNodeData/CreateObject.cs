using System;
using System.CodeDom;
using System.Collections.Generic;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(CreateObjectConstructionParams))]
    public partial class CreateObject : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class CreateObjectConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Type CreateType { get; set; }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as CreateObjectConstructionParams;
                retVal.CreateType = CreateType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as CreateObjectConstructionParams;
                if (param == null)
                    return false;
                if (CreateType == param.CreateType)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + CreateType.FullName).GetHashCode();
            }
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(CreateType);
            //    att.Write(typeStr);
            //    att.EndWrite();
            //}
            public override void Read(EngineNS.IO.XndNode xndNode)
            {
                var att = xndNode.FindAttrib("ConstructionParams");
                if (att != null)
                {
                    att.BeginRead();
                    switch (att.Version)
                    {
                        case 0:
                            att.Read(out mConstructParam);
                            string typeStr;
                            att.Read(out typeStr);
                            CreateType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        object mDefaultValueObj;

        public void SetPropertyChangedEvent(Action<string, object, object> OnPropertyChangedAction)
        {
            if (mTemplateClassInstance != null)
            {
                mTemplateClassInstance.OnPropertyChangedAction = OnPropertyChangedAction;
            }
        }

        partial void InitConstruction();
        public CreateObject(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var param = csParam as CreateObjectConstructionParams;
            if (param.CreateType != null)
            {
                NodeName = param.CreateType.Name;
            }
            AddLinkPinInfo("ResultLinkHandle", mCtrlResultLinkHandle);

            CreateTemplateClas();
        }

        List<CodeGenerateSystem.Base.CustomPropertyInfo> mClassProperties = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
        public void CreateTemplateClas()
        {
            var param = CSParam as CreateObjectConstructionParams;
            mClassProperties.Clear();
            if(param.CreateType != null)
            {
                var properties = param.CreateType.GetProperties();
                foreach(var property in properties)
                {
                    var att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(property, typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, true);
                    if (att == null)
                        continue;

                    var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo()
                    {
                        PropertyName = property.Name,
                        PropertyType = property.PropertyType,
                        DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(property.PropertyType),
                    };
                    cpInfo.CurrentValue = cpInfo.DefaultValue;
                    bool hasMetaAtt = false;
                    foreach(Attribute customAtt in property.GetCustomAttributes(false))
                    {
                        if (customAtt.GetType().FullName == typeof(EngineNS.Rtti.MetaDataAttribute).FullName)
                            hasMetaAtt = true;
                        cpInfo.PropertyAttributes.Add(customAtt);
                    }
                    if(!hasMetaAtt)
                        cpInfo.PropertyAttributes.Add(new EngineNS.Rtti.MetaDataAttribute());
                    mClassProperties.Add(cpInfo);
                }
                mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(mClassProperties, this, $"CreateObject_{param.CreateType.FullName}", false);
            }
            if (mDefaultValueObj == null)
                mDefaultValueObj = System.Activator.CreateInstance(param.CreateType);
            var classType = mTemplateClassInstance.GetType();
            foreach(var pro in mClassProperties)
            {
                var proInfo = classType.GetProperty(pro.PropertyName);
                if (proInfo == null)
                    continue;
                var srcProInfo = param.CreateType.GetProperty(pro.PropertyName);
                proInfo.SetValue(mTemplateClassInstance, srcProInfo.GetValue(mDefaultValueObj));
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlResultLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as CreateObjectConstructionParams;
            if(param.CreateType != null)
                CollectLinkPinInfo(smParam, "ResultLinkHandle", param.CreateType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            else
                CollectLinkPinInfo(smParam, "ResultLinkHandle", CodeGenerateSystem.Base.enLinkType.Unknow, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }


        public void WriteObjectToCreateobject(Object obj)
        {
            if(obj.GetType().Equals(mTemplateClassInstance.GetType()) == false)
                throw new Exception("Createobject WriteObjectToCreateobject type is not equals!");

            var classType = mTemplateClassInstance.GetType();
            var type = obj.GetType();
            foreach (var pro in mClassProperties)
            {
                var proInfo = classType.GetProperty(pro.PropertyName);
                if (proInfo == null)
                    continue;
                var srcProInfo = type.GetProperty(pro.PropertyName);
                proInfo.SetValue(mTemplateClassInstance, srcProInfo.GetValue(obj));
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("coData");
            att.BeginWrite();
            att.Version = 1;
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("coData");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        if (mTemplateClassInstance != null)
                            att.ReadMetaObject(mTemplateClassInstance);
                        break;
                    case 1:
                        CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                        break;
                }
                att.EndRead();
            }
            await base.Load(xndNode);
        }

        #region 生成代码

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "createItem_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as CreateObjectConstructionParams;
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.CreateType);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as CreateObjectConstructionParams;
            return param.CreateType;
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(GCode_GetValueName(null, context));
        }

        System.CodeDom.CodeVariableDeclarationStatement mVarDec;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var valueName = GCode_GetValueName(null, context);
            await GCode_CodeDom_GenerateCode(context.Method, valueName);
        }

        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeMemberMethod Method, string valueName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var param = CSParam as CreateObjectConstructionParams;
            // 生成创建的代码
            if (mVarDec == null || !Method.Statements.Contains(mVarDec))
            {
                //var valueName = GCode_GetValueName(null, context);
                if (mTemplateClassInstance != null)
                {
                    var type = mTemplateClassInstance.GetType();
                    foreach (var pro in mClassProperties)
                    {
                        var proInfo = type.GetProperty(pro.PropertyName);
                        if (proInfo == null)
                            continue;

                        var defProInfo = param.CreateType.GetProperty(pro.PropertyName);
                        var curValue = proInfo.GetValue(mTemplateClassInstance);
                        var defValue = defProInfo.GetValue(mDefaultValueObj);
                        if (object.Equals(curValue, defValue))
                            continue;
                        if (pro.PropertyType.FullName == typeof(EngineNS.RName).FullName)
                        {
                            var rname = curValue as EngineNS.RName;
                            if (rname == null)
                                Method.Statements.Insert(0, new CodeAssignStatement(new CodeVariableReferenceExpression(valueName + "." + pro.PropertyName), new CodePrimitiveExpression(null)));
                            else
                                Method.Statements.Insert(0, new CodeAssignStatement(new CodeVariableReferenceExpression(valueName + "." + pro.PropertyName),
                                                                                        new CodeSnippetExpression($"EngineNS.CEngine.Instance.FileManager.GetRName(\"{rname.Name}\", {EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(EngineNS.RName.enRNameType))}.{rname.RNameType.ToString()})")));

                        }
                        else if (pro.PropertyType.IsEnum)
                        {
                            Method.Statements.Insert(0, new CodeAssignStatement(new CodeVariableReferenceExpression(valueName + "." + pro.PropertyName), new CodeVariableReferenceExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(pro.PropertyType) + "." + curValue.ToString())));
                        }
                        else
                            Method.Statements.Insert(0, new CodeAssignStatement(new CodeVariableReferenceExpression(valueName + "." + pro.PropertyName), new CodePrimitiveExpression(curValue)));
                    }
                }

                mVarDec = new System.CodeDom.CodeVariableDeclarationStatement(param.CreateType, valueName, new CodeObjectCreateExpression(param.CreateType, new CodeExpression[0]));
                Method.Statements.Insert(0, mVarDec);
            }
        }

        public override bool Pin_UseOrigionParamName(CodeGenerateSystem.Base.LinkPinControl linkElement)
        {
            return true;
        }

        #endregion
    }
}
