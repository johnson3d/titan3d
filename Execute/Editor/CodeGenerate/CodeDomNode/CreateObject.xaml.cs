using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Arithmetic.xaml
    /// </summary>
    public partial class CreateObject
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlResultLinkHandle = ResultLink;
        }

        protected override void CollectionErrorMsg()
        {
            var param = CSParam as CreateObjectConstructionParams;
            if (param.CreateType == null)
            {
                HasError = true;
                ErrorDescription = "创建类型丢失!";
            }
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        public void SetPropertyValue(string name, object value)
        {
            var classType = mTemplateClassInstance.GetType();
            var proInfo = classType.GetProperty(name);
            if (proInfo != null)
            {
                proInfo.SetValue(mTemplateClassInstance, value);
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as CreateObject;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            return copyedNode;
        }
    }
}
