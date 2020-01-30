using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class EnumConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public Type EnumType { get; set; }
        [EngineNS.Rtti.MetaData]
        public string EnumCurrentValue { get; set; }
        [EngineNS.Rtti.MetaData]
        public List<Int64> FlagEnumValues { get; set; } = new List<Int64>();

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as EnumConstructParam;
            retVal.EnumType = EnumType;
            retVal.EnumCurrentValue = EnumCurrentValue;
            retVal.FlagEnumValues = FlagEnumValues;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(EnumConstructParam))]
    public partial class EnumValue : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();

        object mSelectedItem = null;
        public object SelectedItem
        {
            get { return mSelectedItem; }
            set
            {
                mSelectedItem = value;
                var enumParam = CSParam as EnumConstructParam;
                if (value == null)
                    enumParam.EnumCurrentValue = "";
                else
                    enumParam.EnumCurrentValue = value.ToString();
                OnPropertyChanged("SelectedItem");
            }
        }
        int mSelectedIndex = 0;
        public int SelectedIndex
        {
            get { return mSelectedIndex; }
            set
            {
                mSelectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }

        partial void InitConstruction();
        partial void SetComboItemsSource(System.Collections.IEnumerable items);
        partial void SetEnumFlags(bool flag);
        partial void SetFlagEnumObject(Object obj);

        bool mFlag = false;
        public EnumValue(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlValueLinkHandle", mCtrlValueLinkHandle, null);

            UpdateEnumType();
        }

        void UpdateEnumType()
        {
            var enumParam = CSParam as EnumConstructParam;
            NodeName = enumParam.EnumType.Name;
            var attrs = enumParam.EnumType.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is EngineNS.Editor.Editor_FlagsEnumSetter)
                {
                    mFlag = true;
                    break;
                }
            }
            SetEnumFlags(mFlag);
            if (mFlag)
            {
                Int64 te = 0;
                foreach (var i in enumParam.FlagEnumValues)
                {
                    te |= i;
                }
                SetFlagEnumObject(System.Enum.ToObject(enumParam.EnumType, te));
            }
            else
            {
                var names = enumParam.EnumType.GetEnumNames();
                if (names.Length > 0)
                {
                    var curValue = enumParam.EnumCurrentValue;
                    SetComboItemsSource(names);
                    SelectedItem = curValue;
                }
            }
        }

        private void Combo_Keys_DropDownOpened(object sender, EventArgs e)
        {
            var param = CSParam as EnumConstructParam;
            var newType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(param.EnumType.FullName);
            if(newType != null && newType != param.EnumType)
            {
                // 类型可能在编辑器运行时更改，所以这里每次打开做一下刷新
                param.EnumType = newType;
                UpdateEnumType();
                Combo_Keys.IsDropDownOpen = true;
            }
        }

        partial void OnChangedValue(Object value, Int64[] selects)
        {
            var enumParam = CSParam as EnumConstructParam;
            enumParam.FlagEnumValues.Clear();
            enumParam.FlagEnumValues.AddRange(selects);
            //enumParam.Value.Clear();
            //enumParam.ValueName.Clear();
            //for (int i = 0; i < selects.Length; i++)
            //{
            //    enumParam.Value.Add(selects[i]);
            //    enumParam.ValueName.Add(System.Enum.GetName(enumParam.EnumType, selects[i]));
            //}
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public static string GetEnumParam(string path, Type paramType)
        {
            if (!paramType.IsEnum)
                return "";

            string retStr = path;
            retStr += "," + EngineNS.Rtti.RttiHelper.GetTypeSaveString(paramType);
            retStr += "," + System.Enum.GetName(paramType, 0);
            return retStr;
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {           
            base.Save(xndNode, newGuid);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as EnumConstructParam;
            if (element == mCtrlValueLinkHandle)
            {
                if (SelectedIndex >= 0)
                {
                    return param.EnumType.ToString();
                }
            }

            return "";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as EnumConstructParam;
            if (element == mCtrlValueLinkHandle)
            {
                if (SelectedIndex >= 0)
                {
                    return param.EnumType;
                }
            }

            return null;
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as EnumConstructParam;
            var newType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(param.EnumType.FullName);

            string str = "";
            if (mFlag)
            {
                var enumParam = CSParam as EnumConstructParam;
                foreach(var val in enumParam.FlagEnumValues)
                {
                    var name = System.Enum.GetName(newType, val);
                    str += name + "|";
                }
                str = str.TrimEnd('|');
            }
            else
            {
                str = SelectedItem.ToString();
            }
            return new System.CodeDom.CodeFieldReferenceExpression(new System.CodeDom.CodeTypeReferenceExpression(param.EnumType), SelectedItem.ToString());
        }
    }
}
