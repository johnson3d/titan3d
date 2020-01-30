using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using System.Text.RegularExpressions;

namespace MaterialEditor.Controls
{
    [EngineNS.Rtti.MetaClass]
    public class CommonValueControlConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// FloatControl.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(CommonValueControlConstructionParams))]
    public partial class CommonValueControl : BaseNodeControl_ShaderVar
    {
        /*
        D3DDECLTYPE_FLOAT1 = 0,
        D3DDECLTYPE_FLOAT2 = 1,
        D3DDECLTYPE_FLOAT3 = 2,
        D3DDECLTYPE_FLOAT4 = 3,
        D3DDECLTYPE_D3DCOLOR = 4,
        D3DDECLTYPE_UBYTE4 = 5,
        D3DDECLTYPE_SHORT2 = 6,
        D3DDECLTYPE_SHORT4 = 7,
        D3DDECLTYPE_UBYTE4N = 8,
        D3DDECLTYPE_SHORT2N = 9,
        D3DDECLTYPE_SHORT4N = 10,
        D3DDECLTYPE_USHORT2N = 11,
        D3DDECLTYPE_USHORT4N = 12,
        D3DDECLTYPE_UDEC3 = 13,
        D3DDECLTYPE_DEC3N = 14,
        D3DDECLTYPE_FLOAT16_2 = 15,
        D3DDECLTYPE_FLOAT16_4 = 16,
        D3DDECLTYPE_UNUSED = 17,

        */
        
        string mTitle = "ValueX";
        public string Title
        {
            get { return mTitle; }
            set
            {
                mTitle = value;

                OnPropertyChanged("Title");
            }
        }

        string mStrValueType;
        enLinkType mLinkType;
        public enLinkType LinkType => mLinkType;

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        public override bool IsInConstantBuffer
        {
            get => IsGeneric;
            protected set => IsGeneric = value;
        }

        class ValueLinkData
        {
            public string m_name;
            public string m_type;
            public CodeGenerateSystem.Base.LinkPinControl m_inLink;
            public CodeGenerateSystem.Base.LinkPinControl m_outLink;
        }
        Dictionary<CodeGenerateSystem.Base.LinkPinControl, ValueLinkData> m_linkDataDictionary = new Dictionary<CodeGenerateSystem.Base.LinkPinControl, ValueLinkData>();
        List<CodeGenerateSystem.Base.LinkPinControl> m_inLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();
        List<CodeGenerateSystem.Base.LinkPinControl> m_outLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();
        List<string> mValuePropertyNames = new List<string>();

        public CommonValueControl(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();
           
            m_linkDataDictionary.Clear();

            ShaderVarInfo.EditorType = "Vector";
            //ShaderVarInfo.VarName = GCode_GetValueName(null);

            mLinkType = LinkPinControl.GetLinkTypeFromTypeString(smParam.ConstructParam);
            ValueIn.BackBrush = Program.GetBrushFromValueType(smParam.ConstructParam, this);
            AddLinkPinInfo("ValueIn", ValueIn, null);

            ValueOut.BackBrush = Program.GetBrushFromValueType(smParam.ConstructParam, this);
            AddLinkPinInfo("ValueOut", ValueOut, ValueOut.BackBrush);

            if (!string.IsNullOrEmpty(smParam.ConstructParam))
            {
                mStrValueType = smParam.ConstructParam;
                Title = smParam.ConstructParam;

                var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
                var valueTextBoxs = new List<TextBlock>();
                switch (mStrValueType)
                {
                    case "int":
                        AddCommonValue("值", "value", "int", valueTextBoxs, cpInfos);
                        ValueIn.Visibility = Visibility.Collapsed;
                        ValueOut.Visibility = Visibility.Collapsed;
                        break;

                    case "float":
                    case "float1":
                        AddCommonValue("x(r)", "x", "float1", valueTextBoxs, cpInfos);
                        ValueIn.Visibility = Visibility.Collapsed;
                        ValueOut.Visibility = Visibility.Collapsed;
                        break;

                    case "float2":
                        AddCommonValue("x(r)", "x", "float1", valueTextBoxs, cpInfos);
                        AddCommonValue("y(g)", "y", "float1", valueTextBoxs, cpInfos);
                        break;

                    case "float3":
                        AddCommonValue("x(r)", "x", "float1", valueTextBoxs, cpInfos);
                        AddCommonValue("y(g)", "y", "float1", valueTextBoxs, cpInfos);
                        AddCommonValue("z(b)", "z", "float1", valueTextBoxs, cpInfos);
                        break;

                    case "float4":
                        AddCommonValue("x(r)", "x", "float1", valueTextBoxs, cpInfos);
                        AddCommonValue("y(g)", "y", "float1", valueTextBoxs, cpInfos);
                        AddCommonValue("z(b)", "z", "float1", valueTextBoxs, cpInfos);
                        AddCommonValue("w(a)", "w", "float1", valueTextBoxs, cpInfos);
                        break;

                    case "uint":
                    case "uint1":
                        AddCommonValue("x(r)", "x", "uint1", valueTextBoxs, cpInfos);
                        ValueIn.Visibility = Visibility.Collapsed;
                        ValueOut.Visibility = Visibility.Collapsed;
                        break;

                    case "uint2":
                        AddCommonValue("x(r)", "x", "uint1", valueTextBoxs, cpInfos);
                        AddCommonValue("y(g)", "y", "uint1", valueTextBoxs, cpInfos);
                        break;

                    case "uint3":
                        AddCommonValue("x(r)", "x", "uint1", valueTextBoxs, cpInfos);
                        AddCommonValue("y(g)", "y", "uint1", valueTextBoxs, cpInfos);
                        AddCommonValue("z(b)", "z", "uint1", valueTextBoxs, cpInfos);
                        break;

                    case "uint4":
                        AddCommonValue("x(r)", "x", "uint1", valueTextBoxs, cpInfos);
                        AddCommonValue("y(g)", "y", "uint1", valueTextBoxs, cpInfos);
                        AddCommonValue("z(b)", "z", "uint1", valueTextBoxs, cpInfos);
                        AddCommonValue("w(a)", "w", "uint1", valueTextBoxs, cpInfos);
                        break;
                }

                foreach (var cp in cpInfos)
                {
                    mValuePropertyNames.Add(cp.PropertyName);
                }
                var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "IsParam", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() });
                cpInfos.Add(cpInfo);
                mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, $"{this.GetType().FullName}.PropertyClass_{mStrValueType}");
                var templateClassType = mTemplateClassInstance.GetType();
                for (int i = 0; i < valueTextBoxs.Count; i++)
                {
                    cpInfo = cpInfos[i];
                    var textBox = valueTextBoxs[i];
                    var property = templateClassType.GetProperty(cpInfo.PropertyName);
                    BindingOperations.SetBinding(textBox, TextBlock.TextProperty, new Binding(cpInfo.PropertyName) { Source = mTemplateClassInstance });
                }
                BindingOperations.SetBinding(this, BaseNodeControl_ShaderVar.IsGenericBindProperty, new Binding("IsParam") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
                mTemplateClassInstance.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                {
                    ShaderVarInfo?.SetValueStr(GetValueString());
                };
                if(HostNodesContainer != null && HostNodesContainer.HostControl != null)
                    mTemplateClassInstance.EnableUndoRedo(HostNodesContainer.HostControl.UndoRedoKey, mStrValueType);
            }

            InitializeShaderVarInfo();
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var linkType = LinkPinControl.GetLinkTypeFromTypeString(smParam.ConstructParam);
            switch(linkType)
            {
                case enLinkType.Int32:
                case enLinkType.UInt1:
                    CollectLinkPinInfo(smParam, "ValueIn", enLinkType.Int32 | enLinkType.UInt32 | enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
                    break;
                case enLinkType.Float1:
                    CollectLinkPinInfo(smParam, "ValueIn", enLinkType.Int32 | enLinkType.UInt32 | enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
                    break;
                case enLinkType.Float2:
                case enLinkType.UInt2:
                    CollectLinkPinInfo(smParam, "ValueIn", enLinkType.UInt2 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
                    break;
                case enLinkType.Float3:
                case enLinkType.UInt3:
                    CollectLinkPinInfo(smParam, "ValueIn", enLinkType.UInt3 | enLinkType.Float3 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
                    break;
                case enLinkType.Float4:
                case enLinkType.UInt4:
                    CollectLinkPinInfo(smParam, "ValueIn", enLinkType.UInt4 | enLinkType.Float4, enBezierType.Left, enLinkOpType.End, false);
                    break;
            }

            CollectLinkPinInfo(smParam, "ValueOut", linkType, enBezierType.Right, enLinkOpType.Start, true);

            switch(linkType)
            {
                case enLinkType.Int32:
                    {
                        CollectLinkPinInfo(smParam, "value_in", enLinkType.Int32, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "value_out", enLinkType.Int32, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.Float1:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.Float2:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "y_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "y_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.Float3:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "y_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "y_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "z_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "z_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.Float4:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "y_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "y_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "z_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "z_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "w_in", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "w_out", enLinkType.Float1, enBezierType.Right, enLinkOpType.Start, true);
                   }
                   break;
                case enLinkType.UInt1:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.UInt2:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "y_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "y_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.UInt3:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "y_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "y_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "z_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "z_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
                case enLinkType.UInt4:
                    {
                        CollectLinkPinInfo(smParam, "x_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "x_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "y_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "y_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "z_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "z_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                        CollectLinkPinInfo(smParam, "w_in", enLinkType.UInt1, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "w_out", enLinkType.UInt1, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    break;
            }
        }
        protected override void _OnIsGenericChanging(bool isGeneric)
        {
            base._OnIsGenericChanging(isGeneric);
            if (isGeneric)
            {
                ValueIn.Visibility = Visibility.Collapsed;
                if (ValueIn.HasLink)
                    ValueIn.Clear();
            }
            else
                ValueIn.Visibility = Visibility.Visible;

            foreach (var lk in m_inLinks)
            {
                if (isGeneric)
                {
                    lk.Visibility = Visibility.Collapsed;
                    if (lk.HasLink)
                        lk.Clear();
                }
                else
                    lk.Visibility = Visibility.Visible;
            }
        }

        protected override void InitializeShaderVarInfo()
        {
            ShaderVarInfo.Initialize(GCode_GetValueName(null, null), 
                                     Program.GetShaderVarTypeFromValueType(GCode_GetTypeString(null, null)),
                                     GetValueString());
        }

        protected void AddCommonValue(string strShowName, string strName, string strType, List<TextBlock> valueTextBoxs, List<CodeGenerateSystem.Base.CustomPropertyInfo> cpInfos)
        {
            /*
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="1*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <Ellipse Width="10" Height="10" Fill="LightGreen" Stroke="Black" HorizontalAlignment="Left" Margin="3" />
                <TextBox Grid.Column="2" MinWidth="50" />
                <Rectangle Grid.Column="3" Width="10" Height="10" Fill="LightGreen" Stroke="Black" HorizontalAlignment="Right" Margin="3" />
            </Grid>
             */

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width=new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            FloatValuesStack.Children.Add(grid);

            var textBox = new TextBlock()
            {
                MinWidth = 50,
                Text = "0",
                Margin = new Thickness(3),
                Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as System.Windows.Style,
                Foreground = Brushes.LightGray,
            };
            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);
            switch(strType)
            {
                case "int":
                    {
                        var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(int), strName, new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() });
                        cpInfos.Add(cpInfo);
                    }
                    break;
                case "float":
                case "float1":
                    {
                        var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), strName, new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() });
                        cpInfos.Add(cpInfo);
                    }
                    break;
                case "uint":
                case "uint1":
                    {
                        var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(UInt32), strName, new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() });
                        cpInfos.Add(cpInfo);
                    }
                    break;
            }
            valueTextBoxs.Add(textBox);

            var ellipse = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Width = 15,
                Height = 15,
                BackBrush = TryFindResource("LinkDefault") as Brush, //Program.GetBrushFromValueType(strType, this),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(8,3,3,3),
                Direction = enBezierType.Left,
                NameString = strShowName,
            };
            grid.Children.Add(ellipse);
            AddLinkPinInfo($"{strName}_in", ellipse, null);
            m_inLinks.Add(ellipse);

            var rect = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                Width = 15,
                Height = 15,
                BackBrush = TryFindResource("LinkDefault") as Brush,//Program.GetBrushFromValueType(strType, this),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                Direction = enBezierType.Right,
            };
            Grid.SetColumn(rect, 2);
            grid.Children.Add(rect);
            AddLinkPinInfo($"{strName}_out", rect, rect.BackBrush);
            m_outLinks.Add(rect);

            ValueLinkData linkData = new ValueLinkData();
            linkData.m_name = strName;
            linkData.m_type = strType;
            linkData.m_inLink = ellipse;
            linkData.m_outLink = rect;
            m_linkDataDictionary[ellipse] = linkData;
            m_linkDataDictionary[rect] = linkData;
        }

        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("commonValueData");
            att.Version = 1;
            att.BeginWrite();
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as CommonValueControl;
            var nodeName = copyedNode.NodeName;
            copyedNode.mTemplateClassInstance.DisableUndoRedo();
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            copyedNode.IsGenericBind = IsGenericBind;
            var namePro = copyedNode.mTemplateClassInstance.GetType().GetProperty("Name");
            if (namePro != null)
                namePro.SetValue(copyedNode.mTemplateClassInstance, nodeName);
            if (param.TargetNodesContainer != null)
                copyedNode.mTemplateClassInstance.EnableUndoRedo(param.TargetNodesContainer.HostControl.UndoRedoKey, copyedNode.mStrValueType);
            return copyedNode;
        }
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("commonValueData");
            if(att != null)
            {
                switch(att.Version)
                {
                    case 0:
                        {
                            att.BeginRead();
                            att.ReadMetaObject(mTemplateClassInstance);

                            var type = mTemplateClassInstance.GetType();
                            var pro = type.GetProperty("IsParam");
                            IsGenericBind = (bool)pro.GetValue(mTemplateClassInstance);

                            att.EndRead();
                        }
                        break;
                    case 1:
                        {
                            att.BeginRead();
                            CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);

                            var type = mTemplateClassInstance.GetType();
                            var pro = type.GetProperty("IsParam");
                            IsGenericBind = (bool)pro.GetValue(mTemplateClassInstance);

                            att.EndRead();
                        }
                        break;
                }
            }

            await base.Load(xndNode);

            InitializeShaderVarInfo();
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strValueName = "";

            if (element == null || element == ValueOut)
            {
                strValueName = Program.GetValidNodeName(this);
            }
            else
            {
                string strBackword = "";
                switch(mStrValueType)
                {
                    case "int":
                    case "float":
                    case "float1":
                    case "uint":
                    case "uint1":
                        strBackword = "";
                        break;

                    default:
                        strBackword = "." + m_linkDataDictionary[element].m_name;
                        break;
                }
                strValueName = GCode_GetValueName(null, context) + strBackword;
            }

            return strValueName;
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == ValueOut)
                return CSParam.ConstructParam;
            else
                return m_linkDataDictionary[element].m_type;
        }

        public string GetValueString()
        {
            string retStr = "";
            var clsType = mTemplateClassInstance.GetType();
            foreach(var proName in mValuePropertyNames)
            {
                var property = clsType.GetProperty(proName);
                retStr += property.GetValue(mTemplateClassInstance) + ",";
            }
            //foreach (var textBox in mValueTextboxs)
            //{
            //    float value = 0;
            //    try
            //    {

            //        value = System.Convert.ToSingle(textBox.Text);
            //        if (float.IsNaN(value))
            //            value = 0.0f;
            //    }
            //    catch (System.Exception)
            //    {
            //        value = 0;
            //    }

            //    retStr += value + ",";
            //}

            retStr = retStr.Remove(retStr.Length - 1);
            return retStr;
        }

        public override string GetValueDefine()
        {
            if (IsGeneric)
                return CSParam.ConstructParam + " " + GCode_GetValueName(null, null) + ";\r\n";
            else
                return "";
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strTab = GCode_GetTabString(nLayer);

            // 变量声明
            if (!IsGeneric)
            {
                var strIdentity = mStrValueType + " " + GCode_GetValueName(null, context);
                var strDefinition = "";
                switch (mStrValueType)
                {
                    case "int":
                        {
                            int value = 0;
                            try
                            {
                                var pro = mTemplateClassInstance.GetType().GetProperty(mValuePropertyNames[0]);
                                var val = pro.GetValue(mTemplateClassInstance);
                                value = System.Convert.ToInt32(val);                                
                            }
                            catch (System.Exception)
                            {
                                value = 0;
                            }
                            strDefinition = "    " + strIdentity + " = " + value + ";\r\n";
                        }
                        break;

                    case "float":
                    case "float1":
                        {
                            float value = 0.0f;
                            try
                            {
                                var pro = mTemplateClassInstance.GetType().GetProperty(mValuePropertyNames[0]);
                                var val = pro.GetValue(mTemplateClassInstance);
                                value = System.Convert.ToSingle(val);
                                if (float.IsNaN(value))
                                    value = 0.0f;
                            }
                            catch (System.Exception)
                            {
                                value = 0.0f;
                            }
                            strDefinition = "    " + strIdentity + " = " + value + ";\r\n";
                        }
                        break;

                    case "uint":
                    case "uint1":
                        {
                            float value = 0.0f;
                            try
                            {
                                var pro = mTemplateClassInstance.GetType().GetProperty(mValuePropertyNames[0]);
                                var val = pro.GetValue(mTemplateClassInstance);
                                value = System.Convert.ToUInt32(val);
                                if (float.IsNaN(value))
                                    value = 0.0f;
                            }
                            catch (System.Exception)
                            {
                                value = 0.0f;
                            }
                            strDefinition = "    " + strIdentity + " = " + value + ";\r\n";
                        }
                        break;

                    case "float2":
                    case "float3":
                    case "float4":
                    case "uint2":
                    case "uint3":
                    case "uint4":
                        {
                            strDefinition = "    " + strIdentity + " = " + mStrValueType + "(" + GetValueString() + ");\r\n";
                        }
                        break;
                }

                if (!strDefinitionSegment.Contains(strDefinition))
                {
                    strDefinitionSegment += strDefinition;
                }
            }


            if (ValueIn.HasLink)
            {
                ValueIn.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, ValueIn.GetLinkedPinControl(0, true), context);

                var inType = ValueIn.GetLinkedObject(0, true).GCode_GetTypeString(ValueIn.GetLinkedPinControl(0, true), context);
                var rightStr = ValueIn.GetLinkedObject(0, true).GCode_GetValueName(ValueIn.GetLinkedPinControl(0, true), context);
                switch (mLinkType)
                {
                    case enLinkType.Int32:
                    case enLinkType.Float1:
                    case enLinkType.UInt1:
                        {
                            switch (inType)
                            {
                                case "float2":
                                case "float3":
                                case "float4":
                                case "uint2":
                                case "uint3":
                                case "uint4":
                                    rightStr += ".x";
                                    break;
                            }
                        }
                        break;

                    case enLinkType.Float2:
                    case enLinkType.UInt2:
                        {
                            switch (inType)
                            {
                                case "float3":
                                case "float4":
                                case "uint3":
                                case "uint4":
                                    rightStr += ".xy";
                                    break;
                            }
                        }
                        break;

                    case enLinkType.Float3:
                    case enLinkType.UInt3:
                        {
                            switch (inType)
                            {
                                case "float4":
                                case "uint4":
                                    rightStr += ".xyz";
                                    break;
                            }
                        }
                        break;
                }

                var assignStr = strTab + GCode_GetValueName(null, context) + " = " + rightStr + ";\r\n";
                // 这里先不做判断，连线中有if的情况下会导致问题
                // 判断赋值语句是否重复
                //if (!strSegment.Contains(assignStr))
                if (!Program.IsSegmentContainString(strSegment.Length - 1, strSegment, assignStr))
                    strSegment += assignStr;
            }

            //处理xyzw等有连接的情况
            foreach (var link in m_inLinks)
            {
                if (link.HasLink)
                {
                    link.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, link.GetLinkedPinControl(0, true), context);

                    var rightStr = link.GetLinkedObject(0, true).GCode_GetValueName(link.GetLinkedPinControl(0, true), context);
                    var inType = link.GetLinkedObject(0, true).GCode_GetTypeString(link.GetLinkedPinControl(0, true), context);
                    if (inType == "float2" || inType == "float3" || inType == "float4" ||
                        inType == "uint2" || inType == "uint3" || inType == "uint4")
                        rightStr += ".x";

                    var assignStr = strTab + GCode_GetValueName(link, context) + " = " + rightStr + ";\r\n";
                    // 这里先不做判断，连线中有if的情况下会导致问题
                    //if (!strSegment.Contains(assignStr))
                    if(!Program.IsSegmentContainString(strSegment.Length - 1, strSegment, assignStr))
                        strSegment += assignStr;
                }
            }
        }
    }
}
