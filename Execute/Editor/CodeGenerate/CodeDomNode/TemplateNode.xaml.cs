using CSUtility.Support;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234236 上有介绍

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("参数.获取模板", "模板节点")]
    public partial class TemplateNode : CodeGenerateSystem.Base.UsefulMember
    {
        public List<CodeGenerateSystem.Base.UsefulMemberHostData> GetUsefulMembers()
        {
            List<CodeGenerateSystem.Base.UsefulMemberHostData> retValue = new List<CodeGenerateSystem.Base.UsefulMemberHostData>();
            if (mReturnType == null)
                return retValue;
            var memberData = new CodeGenerateSystem.Base.UsefulMemberHostData()
            {
                ClassTypeFullName = mReturnType.FullName,
                HostControl = this,
                LinkObject = mReturnLinkInfo,
            };

            retValue.Add(memberData);        
            return retValue;
        }

        public List<CodeGenerateSystem.Base.UsefulMemberHostData> GetUsefulMembers(CodeGenerateSystem.Base.LinkControl linkCtrl)
        {
            List<CodeGenerateSystem.Base.UsefulMemberHostData> retValue = new List<CodeGenerateSystem.Base.UsefulMemberHostData>();

            if (linkCtrl == returnLink)
            {
                if (mReturnType == null)
                    return retValue;
                var memberData = new CodeGenerateSystem.Base.UsefulMemberHostData()
                {
                    ClassTypeFullName = mReturnType.FullName,
                    HostControl = this,
                    LinkObject = mReturnLinkInfo,
                };

                retValue.Add(memberData);
            }
            return retValue;
        }

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;        

        partial void Init_WPF()
        {
            ComboBox_Templates.ItemsSource = mTemplateTypes;
            ComboBox_Templates.DisplayMemberPath = "FullName";
        }

        partial void InitConstruction()
        {
            this.InitializeComponent();
            mCtrlreturnLink = returnLink;
            mCtrlparamLink = paramLink;
            SetDragObject(RectangleTitle);
        }

        partial void RefreshValueFromTemplateClass2PropertyInfoList()
        {
            if (mTemplateClassInstance == null)
                return;

            var tempClassType = mTemplateClassInstance.GetType();
            foreach(var proInfo in mCustomPropertyInfos)
            {
                var pro = tempClassType.GetProperty(proInfo.PropertyName);
                if (pro == null)
                    continue;

                proInfo.CurrentValue = pro.GetValue(mTemplateClassInstance);
            }
        }
        partial void RefreshValueFromPropertyInfoList2TemplateClass()
        {
            if (mTemplateClassInstance == null)
                return;

            var tempClassValue = mTemplateClassInstance.GetType();
            foreach(var proInfo in mCustomPropertyInfos)
            {
                var pro = tempClassValue.GetProperty(proInfo.PropertyName);
                if (pro == null)
                    continue;

                pro.SetValue(mTemplateClassInstance, proInfo.CurrentValue);
            }
        }

        private void ComboBox_Templates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mReturnType = ComboBox_Templates.SelectedItem as Type;
            if (mReturnType == null)
                return;

            mParamType = CSUtility.Data.DataTemplateManagerAssist.Instance.GetDataTemplateIDType(mReturnType);
            mParanLinkInfo.LinkType = CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromCommonType(mParamType);

            StrParams = mReturnType.FullName;

            this.HostNodesContainer.RefreshNodeProperty(this, CodeGenerateSystem.Base.ENodeHandleType.AddNodeControl);

            SetParameters();
        }


        protected void CreateTemplateClassInstance()
        {
            if (mCustomPropertyInfos.Count == 0)
                return;

            var classType = CodeGenerateSystem.Base.PropertyClassGenerator.CreateTypeFromCustomPropertys(mCustomPropertyInfos);
            mTemplateClassInstance = System.Activator.CreateInstance(classType) as CodeGenerateSystem.Base.GeneratorClassBase;
            var field = mTemplateClassInstance.GetType().GetField("HostNode");
            if (field != null)
                field.SetValue(mTemplateClassInstance, this);
            foreach (var property in mTemplateClassInstance.GetType().GetProperties())
            {
                property.SetValue(mTemplateClassInstance, CodeGenerateSystem.Program.GetDefaultValueFromType(property.PropertyType));
            }
        }
        public override object GetShowPropertyObject()
        {
            if (mTemplateClassInstance == null)
                CreateTemplateClassInstance();
            return mTemplateClassInstance;
        }
    }
}
