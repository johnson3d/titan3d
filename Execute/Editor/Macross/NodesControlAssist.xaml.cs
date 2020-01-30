using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CodeDomNode.CustomMethodInfo;

namespace Macross
{
    /// <summary>
    /// Interaction logic for NodesControlAssist.xaml
    /// </summary>
    public partial class NodesControlAssist : UserControl, INotifyPropertyChanged, CodeGenerateSystem.Base.INodesContainerHost, CodeGenerateSystem.Controls.NodesListOperator
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region INodesContainerHost
        public string UndoRedoKey
        {
            get
            {
                if (HostControl != null)
                {
                    return HostControl.UndoRedoKey;
                }
                return "";
            }
        }
        public void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
        }
        #endregion

        INodesContainerDicKey mLinkedKey;
        public INodesContainerDicKey LinkedKey
        {
            get => mLinkedKey;
            set
            {
                mLinkedKey = value;
                NodesControl.GUID = mLinkedKey.Id;
            }
        }

        public IMacrossOperationContainer HostControl { get; set; }
        public string GetGraphFileName(string graphName)
        {
            return HostControl.GetGraphFileName(graphName);
        }
        public bool IsDirty
        {
            get
            {
                if (NodesControl.IsDirty)
                    return true;

                foreach (var container in mSubNodesContainers)
                {
                    if (container.Value.IsDirty)
                        return true;
                }
                return false;
            }
        }

        EngineNS.ECSType mCSType = EngineNS.ECSType.Common;
        public EngineNS.ECSType CSType
        {
            get => mCSType;
            set
            {
                mCSType = value;
                NodesControl.CSType = mCSType;
                foreach (var container in mSubNodesContainers)
                {
                    container.Value.CSType = mCSType;
                }
            }
        }
        public Guid LinkedCategoryItemID { get; set; }
        public string LinkedCategoryItemName
        {
            get { return (string)GetValue(LinkedCategoryItemNameProperty); }
            set { SetValue(LinkedCategoryItemNameProperty, value); }
        }
        public static readonly DependencyProperty LinkedCategoryItemNameProperty = DependencyProperty.Register("LinkedCategoryItemName", typeof(string), typeof(NodesControlAssist), new UIPropertyMetadata("", OnLinkedCategoryItemNamePropertyChanged));
        private static void OnLinkedCategoryItemNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as NodesControlAssist;
            ctrl.OnPropertyChanged("LinkedCategoryItemDisplayName");
        }
        public string LinkedCategoryItemDisplayName
        {
            get { return (string)GetValue(LinkedCategoryItemDisplayNameProperty); }
            set { SetValue(LinkedCategoryItemDisplayNameProperty, value); }
        }
        public static readonly DependencyProperty LinkedCategoryItemDisplayNameProperty = DependencyProperty.Register("LinkedCategoryItemDisplayName", typeof(string), typeof(NodesControlAssist), new UIPropertyMetadata("", OnLinkedCategoryItemDisplayNamePropertyChanged, OnLinkedCategoryItemDisplayNamePropertyCVCallback));
        private static void OnLinkedCategoryItemDisplayNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }
        private static object OnLinkedCategoryItemDisplayNamePropertyCVCallback(DependencyObject d, object value)
        {
            var ctrl = d as NodesControlAssist;
            string val = (string)value;
            if (string.IsNullOrEmpty(val))
                return ctrl.LinkedCategoryItemName;
            return val;
        }

        public bool IsInPIEMode
        {
            get { return (bool)GetValue(IsInPIEModeProperty); }
            set { SetValue(IsInPIEModeProperty, value); }
        }
        public static readonly DependencyProperty IsInPIEModeProperty = DependencyProperty.Register("IsInPIEMode", typeof(bool), typeof(NodesControlAssist), new UIPropertyMetadata(false));

        public CodeGenerateSystem.Controls.NodesContainerControl NodesControl
        {
            get;
            private set;
        }

        public NodesControlAssist()
        {
            InitializeComponent();

            NodesControl = new CodeGenerateSystem.Controls.NodesContainerControl();
            NodesControl.TypeString = "MACROSS";

            NodesControl.TitleShow = Visibility.Hidden;
            ShowNodesContainer(NodesControl);

            BindingOperations.SetBinding(this, IsInPIEModeProperty, new Binding("IsInPIEMode") { Source = EditorCommon.GamePlay.Instance, Mode = BindingMode.TwoWay });
        }

        public async Task Initialize()
        {
            await InitializeNodesContainer(NodesControl);
        }
        public async Task InitializeNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl nodesControl)
        {
            nodesControl.CSType = CSType;
            nodesControl.HostControl = this;
            var extData = new CodeGenerateSystem.Controls.MacrossNodesContainsExtendData();
            nodesControl.ExtendData = extData;
            if (HostControl != null && HostControl.CurrentResourceInfo != null)
            {
                extData.ClassName = Program.GetClassName(HostControl.CurrentResourceInfo, CSType);
                extData.Namespace = Program.GetClassNamespace(HostControl.CurrentResourceInfo, CSType);
            }

            nodesControl.OnDirtyChanged -= NodesControl_OnDirtyChanged;
            nodesControl.OnDirtyChanged += NodesControl_OnDirtyChanged;
            nodesControl.OnSelectNodeControl -= NodesControl_OnSelectNodeControl;
            nodesControl.OnSelectNodeControl += NodesControl_OnSelectNodeControl;
            nodesControl.OnSelectNull -= NodesControl_OnSelectNull;
            nodesControl.OnSelectNull += NodesControl_OnSelectNull;
            nodesControl.OnUnSelectNodes -= NodesControl_OnUnSelectNodes;
            nodesControl.OnUnSelectNodes += NodesControl_OnUnSelectNodes;
            nodesControl.OnAddedNodeControl -= NodesControl_OnAddedNodeControl;
            nodesControl.OnAddedNodeControl += NodesControl_OnAddedNodeControl;
            nodesControl.OnDeletedNodeControl -= NodesControl_OnDeletedNodeControl;
            nodesControl.OnDeletedNodeControl += NodesControl_OnDeletedNodeControl;
            nodesControl.OnSelectLinkInfo -= NodesControl_OnSelectLinkInfo;
            nodesControl.OnSelectLinkInfo += NodesControl_OnSelectLinkInfo;
            nodesControl.OnLinkInfoDoubleClick -= NodesControl_OnLinkInfoDoubleClick;
            nodesControl.OnLinkInfoDoubleClick += NodesControl_OnLinkInfoDoubleClick;
            nodesControl.OnFilterContextMenu = NodesControl_FilterContextMenu;
            nodesControl.OnCheckDropAvailable = NodesControl_CheckDropAvailable;
            nodesControl._OnDrop = NodesControl_OnDrop;

            // 刷新特殊节点数据
            // 自定义Function
            Category funcCategory;
            if (this.HostControl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.FunctionCategoryName, out funcCategory))
            {
                foreach (var funcItem in funcCategory.Items)
                {
                    var methodInfo = funcItem.PropertyShowItem as CodeDomNode.CustomMethodInfo;
                    if (methodInfo == null)
                        continue;
                    for (int i = 0; i < nodesControl.CtrlNodeList.Count; i++)
                    {
                        var node = nodesControl.CtrlNodeList[i];
                        if (node is CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode)
                        {
                            var funcNode = node as CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode;
                            var nodeMethodInfo = funcNode.GetMethodInfo();

                            // 自定义函数名称不能重复，所以这里用名子判断
                            if (nodeMethodInfo.MethodName == methodInfo.MethodName)
                            {
                                await funcNode.ResetMethodInfo(methodInfo);
                            }
                        }
                    }
                }
            }
            Category propCategory;
            if (this.HostControl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.PropertyCategoryName, out propCategory))
            {
                foreach (var propItem in propCategory.Items)
                {
                    var ciPro = propItem.PropertyShowItem as Macross.CategoryItemProperty_Property;
                    if (ciPro == null)
                        continue;
                    if (ciPro.GetMethodInfo == null || ciPro.SetMethodInfo == null)
                        continue;
                    for (int i = 0; i < nodesControl.CtrlNodeList.Count; i++)
                    {
                        var node = nodesControl.CtrlNodeList[i];
                        if (node is CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode)
                        {
                            var funcNode = node as CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode;
                            var nodeMethodInfo = funcNode.GetMethodInfo();

                            // 函数名称不能重复，所以这里用名子判断
                            if (nodeMethodInfo.MethodName == ciPro.GetMethodInfo.MethodName)
                            {
                                await funcNode.ResetMethodInfo(ciPro.GetMethodInfo);
                            }
                            else if (nodeMethodInfo.MethodName == ciPro.SetMethodInfo.MethodName)
                            {
                                await funcNode.ResetMethodInfo(ciPro.SetMethodInfo);
                            }
                        }
                    }
                }
            }

            if (LinkedKey != null)
            {
                if (!mProcessDataDic.TryGetValue(LinkedKey.Id, out mCurrentProcessData))
                {
                    mCurrentProcessData = new ProcessData(LinkedKey.Id, this);
                    mProcessDataDic[LinkedKey.Id] = mCurrentProcessData;
                }
            }
        }
        public void InitializeSubLinkedNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl nodesControl)
        {
            foreach (var ctrl in nodesControl.CtrlNodeList)
            {
                CodeGenerateSystem.Controls.NodesContainerControl subLinkedNodeContainerCtrl;
                if (mSubNodesContainers.TryGetValue(ctrl.Id, out subLinkedNodeContainerCtrl))
                {
                    ctrl.LinkedNodesContainer = subLinkedNodeContainerCtrl;
                    InitializeSubLinkedNodesContainer(subLinkedNodeContainerCtrl);
                }

            }
        }
        //public event CodeGenerateSystem.Base.NodesContainer.Delegate_SelectedLinkInfo OnSelectedLinkInfo;
        //public event CodeGenerateSystem.Base.NodesContainer.Delegate_SelectedLinkInfo OnDoubleCliclLinkInfo;
        private void NodesControl_OnSelectLinkInfo(LinkInfo linkInfo)
        {
            HostControl.OnSelectedLinkInfo(linkInfo);
        }
        private void NodesControl_OnDirtyChanged(bool dirty)
        {
            if (dirty && HostControl.CurrentResourceInfo != null)
                HostControl.CurrentResourceInfo.IsDirty = true;
        }
        private void NodesControl_OnSelectNull(BaseNodeControl node)
        {
            HostControl.OnSelectNull(node);
        }
        private void NodesControl_OnLinkInfoDoubleClick(LinkInfo linkInfo)
        {
            HostControl.OnDoubleCliclLinkInfo(linkInfo);
        }
        private void NodesControl_OnSelectNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            HostControl.OnSelectNodeControl(node);
        }
        private void NodesControl_OnUnSelectNodes(List<CodeGenerateSystem.Base.BaseNodeControl> nodes)
        {
            HostControl.OnUnSelectNodes(nodes);
        }
        private void NodesControl_OnAddedNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
        }
        public event CodeGenerateSystem.Base.NodesContainer.Delegate_OnOperateNodeControl OnDeletedNodeControl;
        private void NodesControl_OnDeletedNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if (node is CodeDomNode.MethodOverride)
            {
                var methodOVNode = node as CodeDomNode.MethodOverride;
                HostControl.RemoveOverrideMethod(methodOVNode.GetMethodKeyName());
            }

            OnDeletedNodeControl?.Invoke(node);
        }

        public void NodesListOperation(CodeGenerateSystem.Controls.NodeListControl nodesList, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            nodesList.NodesListOperatorCtrl = this;
            nodesList.mCurrentFilterData = filterData;

            nodesList.ClearNodes();
            nodesList.AddNodesFromAssembly(filterData, this.GetType().Assembly);

            // 取得当前地图的对象
            if (mSelectedActors != null)
            {
                foreach (var actor in mSelectedActors)
                {
                    var actorCtrlParam = new CodeDomNode.ActorControl.ActorControlConstructionParams()
                    {
                        CSType = CSType,
                        ActorId = actor.ActorId,
                        ActorName = actor.SpecialName,
                    };
                    nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ActorControl), $"场景对象({actorCtrlParam.ActorName})", actorCtrlParam, $"添加对场景对象{actorCtrlParam.ActorName}的引用");
                }
            }

            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值.数值", "Common,0", "标识所有类型的数值");
            var cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.SByte,
                ValueType = typeof(System.SByte),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/SByte", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Int16,
                ValueType = typeof(System.Int16),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/Int16", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Int32,
                ValueType = typeof(System.Int32),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/Int32", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Int64,
                ValueType = typeof(System.Int64),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/Int64", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Byte,
                ValueType = typeof(System.Byte),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/Byte", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.UInt16,
                ValueType = typeof(System.UInt16),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/UInt16", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.UInt32,
                ValueType = typeof(System.UInt32),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/UInt32", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.UInt64,
                ValueType = typeof(System.UInt64),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/UInt64", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Single,
                ValueType = typeof(System.Single),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/Single", cv, "", "float");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Double,
                ValueType = typeof(System.Double),
                Value = "0",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/Double", cv, "");
            cv = new CodeDomNode.CommonValue.CommonValueConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.String,
                ValueType = typeof(System.String),
                Value = "",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CommonValue), "数值/String", cv, "");
            var vec2Param = new CodeDomNode.Vector.VectorConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Vector2,
                Value = EngineNS.Vector4.Zero,
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Vector), "数值/Vector2", vec2Param, "");
            var vec3Param = new CodeDomNode.Vector.VectorConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Vector3,
                Value = EngineNS.Vector4.Zero,
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Vector), "数值/Vector3", vec3Param, "");
            var vec4Param = new CodeDomNode.Vector.VectorConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Vector4,
                Value = EngineNS.Vector4.Zero,
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Vector), "数值/Vector4", vec4Param, "");

            var aixParam = new CodeDomNode.Aix.AixConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                LinkType = CodeGenerateSystem.Base.enLinkType.Vector4,
                Value = EngineNS.Vector4.UnitZ,

                DisplayName = "旋转轴",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Aix), "数值/Aix", aixParam, "");

            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "运算/+(加)", "＋", "加法运算节点", "AddPlus");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "运算/-(减)", "－", "减法运算节点", "MinusSubtraction");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "运算/*(乘)", "×", "乘法运算节点", "Multiply");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "运算/÷(除)", "÷", "除法运算节点", "Divide/");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "位操作/按位与(&)", "&", "按位与操作节点", "And");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "位操作/按位或(|)", "|", "按位或操作节点", "Or");
            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "位操作/取反(~)", "~", "位操作取反运算节点");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "布尔操作/与(&&)", "&&", "布尔操作与运算节点", "And");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Arithmetic), "布尔操作/或(||)", "||", "布尔操作或运算节点", "Or");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.InverterControl), "布尔操作/非(!)", "!", "布尔操作或运算节点", "!NotNegation");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Compare), "比较/>(大于)", "＞", "比较运算节点（大于）", "Great");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Compare), "比较/==(等于)", "==", "比较运算节点（等于）", "Equal");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Compare), "比较/<(小于)", "＜", "比较运算节点（小于）", "Less");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Compare), "比较/≥(大于等于)", "≥", "比较运算节点（大于等于）", ">=GreatEqual");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Compare), "比较/≤(小于等于)", "≤", "比较运算节点（小于等于）", "<=LesseEual");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Compare), "比较/≠(不等于)", "≠", "比较运算节点（不等于）", "!=Inequal");

            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ClassCastControl), "逻辑/继承类类型转换", CSType.ToString(), "将类型转换为子类型");
            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.TypeCastControl), "逻辑/强制类型转换", CSType.ToString(), "将类型转换为输入的目标类型");

            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.BreakContinueNode), "逻辑/跳出循环", "break", "跳出本层循环节点");
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.BreakContinueNode), "逻辑/执行下一次循环", "continue", "执行下一次循环节点");

            var memberCollectData = new CodeDomNode.Program.MacrossMemberCollectData();
            // Assemblys分析
            var types = EngineNS.Rtti.RttiHelper.GetTypes(CSType);
            foreach (var type in types)
            {
                if (!type.IsGenericTypeDefinition)
                {
                    if (nodesList.Sensitive_ThisMacross)
                    {
                        memberCollectData.NodesListCtrl = nodesList;
                        memberCollectData.ClassType = type;
                        memberCollectData.AttributeName = typeof(EngineNS.Editor.MacrossMemberAttribute).FullName;
                        memberCollectData.CSType = CSType;
                        memberCollectData.HostType = CodeDomNode.MethodInfoAssist.enHostType.Static;
                    }
                    else
                    {
                        memberCollectData.NodesListCtrl = nodesList;
                        memberCollectData.ClassType = type;
                        memberCollectData.AttributeName = typeof(EngineNS.Editor.MacrossMemberAttribute).FullName;
                        memberCollectData.CSType = CSType;
                        var baseType = HostControl.CurrentResourceInfo.BaseType;
                        if (type == baseType || baseType.IsSubclassOf(type))
                            memberCollectData.HostType = CodeDomNode.MethodInfoAssist.enHostType.This;
                        else
                            memberCollectData.HostType = CodeDomNode.MethodInfoAssist.enHostType.Target;
                    }

                    CodeDomNode.Program.InitializeMacrossMembers(filterData, memberCollectData);
                }

                var macrossTypeAtts = type.GetCustomAttributes(typeof(EngineNS.Macross.MacrossTypeClassAttribute), false);
                if (macrossTypeAtts.Length > 0)
                {
                    // Macross Class
                    // todo: MacrossScript分析，不能直接LoadAssembly，用Macross.MacrossGetter<XXX>
                }
                else
                {
                    var typeStr = EngineNS.Rtti.RttiHelper.GetAppTypeString(type);
                    var atts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossClassAttribute), false);
                    if (atts.Length > 0)
                    {
                        var att = atts[0] as EngineNS.Editor.Editor_MacrossClassAttribute;
                        if (att.HasType(EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Createable))
                        {
                            if (type.IsEnum)
                            {
                                var enumAtt = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossEnumAttribute), false);

                                var nodeType = typeof(CodeDomNode.EnumValue);
                                var param = new CodeDomNode.EnumConstructParam()
                                {
                                    EnumType = type,
                                    CSType = this.CSType,
                                };
                                var path = EngineNS.Rtti.RttiHelper.GetAppTypeString(type).Replace(".", "/");
                                nodesList.AddNodesFromType(filterData, nodeType, path, param, "");
                            }
                            else
                            {
                                // Create
                                var nodeType = typeof(CodeDomNode.CreateObject);
                                var param = new CodeDomNode.CreateObject.CreateObjectConstructionParams()
                                {
                                    CreateType = type,
                                    CSType = CSType,
                                    ConstructParam = "",
                                };
                                var idx = typeStr.LastIndexOf('.');
                                var tempTypeStr = typeStr.Insert(idx + 1, "Create ").Replace('.', '/');
                                var mpattr = type.GetCustomAttributes(typeof(EngineNS.Editor.MacrossPanelPathAttribute), false);
                                if (mpattr.Length > 0)
                                {
                                    tempTypeStr = ((EngineNS.Editor.MacrossPanelPathAttribute)mpattr[0]).Path;
                                }
                                nodesList.AddNodesFromType(filterData, nodeType, tempTypeStr, param, "", "", TryFindResource("Icon_Node") as ImageSource);
                            }
                        }
                    }
                }
            }

            if (nodesList.Sensitive_ThisMacross)
            {
                memberCollectData.ClassType = HostControl.CurrentResourceInfo.BaseType;
                memberCollectData.HostType = CodeDomNode.MethodInfoAssist.enHostType.Base;
                memberCollectData.IgnoreStatic = true;
                // 父类成员
                CodeDomNode.Program.InitializeMacrossMembers(filterData, memberCollectData);
            }
            // 本类成员
            //XXX
            // 筛选项成员
            if (filterData.StartLinkObj != null && !string.IsNullOrEmpty(filterData.StartLinkObj.ClassType))
            {
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(filterData.StartLinkObj.ClassType, CSType);
                if (type == null)
                {
                    type = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(filterData.StartLinkObj.ClassType);
                }
                if (type != null)
                {
                    if (nodesList.Sensitive_ThisMacross)
                    {
                        if (filterData.StartLinkObj.LinkOpType == CodeGenerateSystem.Base.enLinkOpType.Start)
                        {
                            memberCollectData.ClassType = type;
                            memberCollectData.HostType = CodeDomNode.MethodInfoAssist.enHostType.Target;
                            CodeDomNode.Program.InitializeMacrossMembers(filterData, memberCollectData);
                        }
                    }

                    // Cast
                    if (type.IsClass || type.IsInterface)
                    {
                        if (filterData.StartLinkObj.LinkOpType == enLinkOpType.Start)
                        {
                            foreach (var tempType in types)
                            {
                                if ((type.IsInterface && (tempType.GetInterface(type.FullName) != null)) || tempType.IsSubclassOf(type))
                                {
                                    var castParam = new CodeDomNode.ClassCastControl.ClassCastControlConstructParam()
                                    {
                                        CSType = CSType,
                                        TargetType = type,
                                        ResultType = tempType,
                                    };
                                    nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ClassCastControl), $"类型转换/类型转换到(Cast) {EngineNS.Rtti.RttiHelper.GetAppTypeString(tempType)}", castParam, "", "", TryFindResource("Icon_Cast") as ImageSource);
                                }
                            }
                        }
                        else if (filterData.StartLinkObj.LinkOpType == enLinkOpType.End)
                        {
                            foreach (var tempType in types)
                            {
                                if ((tempType.IsInterface && (type.GetInterface(tempType.FullName) != null)) || type.IsSubclassOf(tempType))
                                {
                                    var castParam = new CodeDomNode.ClassCastControl.ClassCastControlConstructParam()
                                    {
                                        CSType = CSType,
                                        TargetType = tempType,
                                        ResultType = type,
                                    };
                                    nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ClassCastControl), $"类型转换/从 {EngineNS.Rtti.RttiHelper.GetAppTypeString(tempType)} 类型转换(Cast)", castParam, "", "", TryFindResource("Icon_Cast") as ImageSource);
                                }
                            }
                        }
                    }
                    else if (type.IsEnum)
                    {
                        if (filterData.StartLinkObj.LinkOpType == enLinkOpType.End)
                        {
                            Array values = type.GetEnumValues();
                            foreach (var value in values)
                            {
                                var strVal = value.ToString();
                                var castParam = new CodeDomNode.EnumConstructParam()
                                {
                                    CSType = CSType,
                                    EnumType = type,
                                    EnumCurrentValue = strVal,
                                };
                                //castParam.Value.Add((int)value);
                                //castParam.ValueName.Add(type.GetEnumName(value));
                                var path = EngineNS.Rtti.RttiHelper.GetAppTypeString(type).Replace(".", "/") + "." + strVal;
                                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.EnumValue), path, castParam, "", "", TryFindResource("Icon_Cast") as ImageSource);
                            }
                        }
                        //else if (filterData.StartLinkObj.LinkOpType == enLinkOpType.End)
                        //{
                        //foreach (var tempType in types)
                        //{
                        //    if ((tempType.IsInterface && (type.GetInterface(tempType.FullName) != null)) || type.IsSubclassOf(tempType))
                        //    {
                        //        var castParam = new CodeDomNode.ClassCastControl.ClassCastControlConstructParam()
                        //        {
                        //            CSType = CSType,
                        //            TargetType = tempType,
                        //            ResultType = type,
                        //        };
                        //        nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ClassCastControl), $"类型转换/从 {EngineNS.Rtti.RttiHelper.GetAppTypeString(tempType)} 类型转换(Cast)", castParam, "", TryFindResource("Icon_Cast") as ImageSource);
                        //    }
                        //}
                        //}

                    }

                    //var atts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossClassAttribute), true);
                    //if(atts != null && atts.Length > 0)
                    //{
                    //}

                    bool hasExpand = false;
                    foreach (var pro in type.GetProperties())
                    {
                        var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                        if (atts.Length > 0)
                        {
                            var expParam = new CodeDomNode.ExpandNode.ExpandNodeConstructParam()
                            {
                                CSType = CSType,
                                ExpandType = type,
                            };
                            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ExpandNode), "展开(Expand)", expParam, "", "", TryFindResource("Icon_Expand") as ImageSource);
                            hasExpand = true;
                            break;
                        }
                    }
                    if (!hasExpand)
                    {
                        foreach (var field in type.GetFields())
                        {
                            var atts = field.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                            if (atts.Length > 0)
                            {
                                var expParam = new CodeDomNode.ExpandNode.ExpandNodeConstructParam()
                                {
                                    CSType = CSType,
                                    ExpandType = type,
                                };
                                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ExpandNode), "展开(Expand)", expParam, "", "", TryFindResource("Icon_Expand") as ImageSource);
                                hasExpand = true;
                                break;
                            }
                        }
                    }
                }
            }

            // 针对当前图的特殊节点
            foreach (var ctrl in filterData.HostContainerControl.CtrlNodeList)
            {
                if (ctrl is CodeDomNode.MethodOverride)
                {
                    var param = ctrl.CSParam as CodeDomNode.MethodOverride.MethodOverrideConstructParam;
                    //if (param.MethodInfo.IsEvent())
                    //    continue;
                    var retParam = new CodeDomNode.Return.ReturnConstructParam()
                    {
                        CSType = CSType,
                        MethodInfo = param.MethodInfo,
                    };
                    nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Return), "Add Return", retParam, "", "", TryFindResource("Icon_Node") as ImageSource);
                    break;
                }
                else if (ctrl is CodeDomNode.MethodCustom)
                {
                    var param = ctrl.CSParam as CodeDomNode.MethodCustom.MethodCustomConstructParam;
                    if (param.MethodInfo.OutParams.Count == 0)
                        continue;
                    var retParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                    {
                        CSType = CSType,
                        MethodInfo = param.MethodInfo,
                        ShowPropertyType = CodeDomNode.ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                    };
                    nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ReturnCustom), "Add Return", retParam, "", "", TryFindResource("Icon_Node") as ImageSource);
                    break;
                }
            }

            //foreach (var actionMapping in EngineNS.CEngine.Instance.InputServerInstance.InputConfiguration.InputBindings)
            //{
            //    var validName = actionMapping.BindingName;
            //    var methodInfo = new CodeDomNode.CustomMethodInfo();
            //    methodInfo.MethodName = "InputAction_" + validName;
            //    //var fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
            //    //fucParam.ParamType = new CodeDomNode.VariableType(typeof(EngineNS.Graphics.Mesh.Notify.CGfxNotify), CSType);
            //    //fucParam.ParamName = "sender";
            //    //methodInfo.InParams.Add(fucParam);
            //    var actionMappingCP = new CodeDomNode.InputActionMethodCustom.InputActionMethodCustomConstructParam()
            //    {
            //        CSType = CSType,
            //        ConstructParam = "",
            //        MethodInfo = methodInfo,
            //        IsShowProperty = false,
            //        MethodName = validName,
            //        InputActionType = CodeDomNode.InputActionType.Action,
            //    };
            //    nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.InputActionMethodCustom), "Input/InputAction_" + validName, actionMappingCP, "");
            //}
            foreach (var binding in EngineNS.CEngine.Instance.InputServerInstance.InputConfiguration.InputBindings)
            {
                string typeName = "";
                CodeDomNode.InputActionType inputActionType = CodeDomNode.InputActionType.Axis;
                string pahtName = "";
                var validName = binding.BindingName;
                var methodInfo = new CodeDomNode.CustomMethodInfo();
                if (binding.MappingType == EngineNS.Input.Device.InputMappingType.Axis)
                {
                    typeName = "InputAxis_";
                    inputActionType = CodeDomNode.InputActionType.Axis;
                    pahtName = "Input/InputAxis_";
                }
                else
                {
                    typeName = "InputAction_";
                    inputActionType = CodeDomNode.InputActionType.Action;
                    pahtName = "Input/InputAction_";
                }
                methodInfo.MethodName = typeName + validName;
                if (binding is EngineNS.Input.Device.InputBinding)
                {
                    var fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    fucParam.ParamType = new CodeDomNode.VariableType(typeof(float), CSType);
                    fucParam.ParamName = "Scale";
                    methodInfo.InParams.Add(fucParam);
                }
                if (binding is EngineNS.Input.Device.InputMoveBinding)
                {
                    methodInfo.InParams.Clear();
                    var fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    fucParam.ParamType = new CodeDomNode.VariableType(typeof(float), CSType);
                    fucParam.ParamName = "x";
                    methodInfo.InParams.Add(fucParam);
                    fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    fucParam.ParamType = new CodeDomNode.VariableType(typeof(float), CSType);
                    fucParam.ParamName = "y";
                    methodInfo.InParams.Add(fucParam);
                    fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    fucParam.ParamType = new CodeDomNode.VariableType(typeof(float), CSType);
                    fucParam.ParamName = "deltaX";
                    methodInfo.InParams.Add(fucParam);
                    fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    fucParam.ParamType = new CodeDomNode.VariableType(typeof(float), CSType);
                    fucParam.ParamName = "deltaY";
                    methodInfo.InParams.Add(fucParam);
                }
                var axisMappingCP = new CodeDomNode.InputActionMethodCustom.InputActionMethodCustomConstructParam()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    MethodInfo = methodInfo,
                    IsShowProperty = false,
                    MethodName = validName,
                    InputActionType = inputActionType,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.InputActionMethodCustom), pahtName + validName, axisMappingCP, "");
            }

            // base
            var baseParam = new CodeDomNode.BaseControl.BaseControlConstructParam()
            {
                CSType = CSType,
                ClassType = HostControl.CurrentResourceInfo.BaseType,
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.BaseControl), "Base", baseParam, "", "", TryFindResource("Icon_Node") as ImageSource);
            // this
            var thisClassFullName = Program.GetClassNamespace(HostControl.CurrentResourceInfo, CSType) + "." + Program.GetClassName(HostControl.CurrentResourceInfo, CSType);
            var thisClassType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(thisClassFullName);
            if (thisClassType != null)
            {
                var thisParam = new CodeDomNode.ThisControl.ThisControlConstructParam()
                {
                    CSType = CSType,
                    ClassType = thisClassType,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.ThisControl), "this", thisParam, "", "", TryFindResource("Icon_Node") as ImageSource);
            }
        }
        public void NodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var nodesList = contextMenu.GetNodesList();
            NodesListOperation(nodesList, filterData);
        }
        private bool NodesControl_CheckDropAvailable(DragEventArgs e)
        {
            if (Program.MacrossCategoryItemDragDropTypeName.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
                return true;
            return false;
        }
        public Visibility VariableDropShowVisibile
        {
            get { return (Visibility)GetValue(VariableDropShowVisibileProperty); }
            set { SetValue(VariableDropShowVisibileProperty, value); }
        }
        public static readonly DependencyProperty VariableDropShowVisibileProperty = DependencyProperty.Register("VariableDropShowVisibile", typeof(Visibility), typeof(NodesControlAssist), new UIPropertyMetadata(Visibility.Collapsed));
        public string VariableDropShowName
        {
            get { return (string)GetValue(VariableDropShowNameProperty); }
            set { SetValue(VariableDropShowNameProperty, value); }
        }
        public static readonly DependencyProperty VariableDropShowNameProperty = DependencyProperty.Register("VariableDropShowName", typeof(string), typeof(NodesControlAssist), new UIPropertyMetadata(""));

        public bool ShowGetButton
        {
            get { return (bool)GetValue(ShowGetButtonProperty); }
            set { SetValue(ShowGetButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowGetButtonProperty = DependencyProperty.Register("ShowGetButton", typeof(bool), typeof(NodesControlAssist), new UIPropertyMetadata(true));
        public bool ShowSetButton
        {
            get { return (bool)GetValue(ShowSetButtonProperty); }
            set { SetValue(ShowSetButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowSetButtonProperty = DependencyProperty.Register("ShowSetButton", typeof(bool), typeof(NodesControlAssist), new UIPropertyMetadata(true));

        public CategoryItem CurrentDropItem
        {
            get;
            set;
        } = null;
        Canvas mNodesCanvas = null;
        private void NodesControl_OnDrop(object sender, DragEventArgs e)
        {
            if (NodesControl_CheckDropAvailable(e) == false)
                return;

            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
                return;

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null || datas.Length == 0)
                return;

            mNodesCanvas = sender as Canvas;
            var pos = e.GetPosition(mNodesCanvas);

            var data = datas[0];
            CurrentDropItem = data as CategoryItem;
            //switch (CurrentDropItem.CategoryItemType)
            //{
            //    case CategoryItem.enCategoryItemType.Variable:
            //    case CategoryItem.enCategoryItemType.FunctionVariable:
            //        {
            //            ShowGetButton = true;
            //            ShowSetButton = true;
            //            InitVariableDropShow(pos);
            //        }
            //        break;
            //    case CategoryItem.enCategoryItemType.CustomFunction:
            //        {
            //            var nodeCtrl = EditorCommon.Program.GetParent(mNodesCanvas, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;
            //            var nodeType = typeof(CodeDomNode.MethodCustomInvoke);
            //            var csParam = new CodeDomNode.MethodCustomInvoke.MethodCustomInvokeConstructParam();
            //            csParam.CSType = CSType;
            //            csParam.HostNodesContainer = nodeCtrl;
            //            csParam.ConstructParam = "";
            //            csParam.MethodInfo = CurrentDropItem.PropertyShowItem as CodeDomNode.CustomMethodInfo;
            //            pos = nodeCtrl._RectCanvas.TranslatePoint(pos, nodeCtrl._MainDrawCanvas);
            //            var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
            //            CurrentDropItem.AddInstanceNode(nodeCtrl.GUID, node);
            //        }
            //        break;
            //    case CategoryItem.enCategoryItemType.Property:
            //        {
            //            ShowGetButton = true;
            //            ShowSetButton = true;
            //            InitVariableDropShow(pos);
            //        }
            //        break;
            //}

            var dropData = new CategoryItem.stDropToNodesControlActionData
            {
                NodesContainerHost = this,
                DropCanvas = mNodesCanvas,
                DropPos = pos,
            };
            CurrentDropItem.OnDropToNodesControlAction?.Invoke(dropData);
        }

        public void InitVariableDropShow(Point pos)
        {
            if (mNodesCanvas == null)
                return;

            var panel = VariableDropShow.Parent as Panel;
            panel.Children.Remove(VariableDropShow);
            panel.Children.Remove(VariableDropShowBG);

            mNodesCanvas.Children.Add(VariableDropShowBG);
            mNodesCanvas.Children.Add(VariableDropShow);

            VariableDropShowName = CurrentDropItem.Name;
            VariableDropShowVisibile = Visibility;
            Canvas.SetLeft(VariableDropShow, pos.X);
            Canvas.SetTop(VariableDropShow, pos.Y);

            var nodeCtrl = EditorCommon.Program.GetParent(VariableDropShow, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;
            var bgPos = nodeCtrl.TranslatePoint(new Point(0, 0), mNodesCanvas);
            Canvas.SetLeft(VariableDropShowBG, bgPos.X);
            Canvas.SetTop(VariableDropShowBG, bgPos.Y);
            VariableDropShowBG.Width = nodeCtrl.ActualWidth;
            VariableDropShowBG.Height = nodeCtrl.ActualHeight;
            //VariableDropShow.Margin = new Thickness(pos.X, pos.Y, 0, 0);
        }

        public CodeGenerateSystem.Base.BaseNodeControl FindControl(Guid id)
        {
            var retVal = NodesControl.FindControl(id);
            if (retVal != null)
                return retVal;
            foreach (var container in mSubNodesContainers)
            {
                retVal = container.Value.FindControl(id);
                if (retVal != null)
                    return retVal;
            }
            return null;
        }
        public void DeleteNode(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            NodesControl.DeleteNode(node);
            foreach (var container in mSubNodesContainers)
            {
                container.Value.DeleteNode(node);
            }
        }

        Dictionary<Guid, CodeGenerateSystem.Controls.NodesContainerControl> mSubNodesContainers = new Dictionary<Guid, CodeGenerateSystem.Controls.NodesContainerControl>();
        public Dictionary<Guid, CodeGenerateSystem.Controls.NodesContainerControl> SubNodesContainers
        {
            get => mSubNodesContainers;
        }
        public async Task<CodeGenerateSystem.Controls.NodesContainerControl> GetSubNodesContainer(SubNodesContainerData data)
        {
            data.IsCreated = false;
            CodeGenerateSystem.Controls.NodesContainerControl ctrl;
            if (mSubNodesContainers.TryGetValue(data.ID, out ctrl))
            {
                ctrl.TitleString = data.Title;
                return ctrl;
            }
            else
            {
                if (string.IsNullOrEmpty(LinkedCategoryItemName))
                    throw new InvalidOperationException();
                var tempFile = HostControl.GetGraphFileName(LinkedCategoryItemName);
                await LoadSubLinks(tempFile);
            }
            if (mSubNodesContainers.TryGetValue(data.ID, out ctrl))
            {
                ctrl.TitleString = data.Title;
                return ctrl;
            }
            data.IsCreated = true;
            ctrl = new CodeGenerateSystem.Controls.NodesContainerControl();
            ctrl.GUID = data.ID;
            mSubNodesContainers.Add(data.ID, ctrl);
            await InitializeNodesContainer(ctrl);
            ctrl.TitleString = data.Title;
            ctrl.TypeString = "MACROSS";
            ctrl.TitleShow = Visibility.Hidden;

            return ctrl;
        }
        public async Task<CodeGenerateSystem.Controls.NodesContainerControl> ShowSubNodesContainer(SubNodesContainerData data)
        {
            var ctrl = await GetSubNodesContainer(data);
            ShowNodesContainer(ctrl);
            return ctrl;
        }
        public void RemoveSubNodesContainer(Guid id)
        {
            mSubNodesContainers.Remove(id);
        }

        public delegate void ShowNodesContainerDelegate(CodeGenerateSystem.Controls.NodesContainerControl ctrl);
        public ShowNodesContainerDelegate ShowNodesContainerEvent;

        public void ShowNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl ctrl)
        {
            if (Border_NodesControls.Child == ctrl)
                return;

            ShowNodesContainerEvent?.Invoke(ctrl);
            Border_NodesControls.Child = ctrl;

            if (LinkedKey != null)
            {
                if (mCurrentProcessData != null)
                    LinkedKey.ProcessOnNodesContainerHide(mCurrentProcessData);
                if (!mProcessDataDic.TryGetValue(ctrl.GUID, out mCurrentProcessData))
                {
                    mCurrentProcessData = new ProcessData(ctrl.GUID, this);
                    mProcessDataDic[ctrl.GUID] = mCurrentProcessData;
                }
                LinkedKey.ProcessOnNodesContainerShow(mCurrentProcessData);
            }

            // 刷新Titles
            StackPanel_Titles.Children.Clear();
            var titles = ctrl.TitleString.Split('/');
            for (int i = 0; i < titles.Length; i++)
            {
                Button btn = null;
                if (i == 0)
                {
                    btn = new Button()
                    {
                        FontSize = 17,
                        Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as Style,
                    };
                    //btn.SetBinding(Button.ContentProperty, new Binding("LinkedCategoryItemName") { Source = this });
                    btn.SetBinding(Button.ContentProperty, new Binding("LinkedCategoryItemDisplayName") { Source = this });
                    StackPanel_Titles.Children.Add(btn);

                    btn.Click += (object sender, RoutedEventArgs e) =>
                    {
                        ShowNodesContainer(NodesControl);
                    };
                }
                else
                {
                    var img = new Image()
                    {
                        Source = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Common/SmallArrowRight.png", UriKind.Absolute)),
                        Width = 16,
                        Height = 16,
                        Margin = new Thickness(3, 0, 3, 0),
                    };
                    StackPanel_Titles.Children.Add(img);

                    var splits = titles[i].Split(':');
                    btn = new Button()
                    {
                        FontSize = 17,
                        Content = splits[0],
                        Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as Style,
                    };
                    StackPanel_Titles.Children.Add(btn);

                    btn.Click += (object sender, RoutedEventArgs e) =>
                    {
                        var id = EngineNS.Rtti.RttiHelper.GuidTryParse(splits[1]);
                        CodeGenerateSystem.Controls.NodesContainerControl subCtrl;
                        if (mSubNodesContainers.TryGetValue(id, out subCtrl))
                            ShowNodesContainer(subCtrl);
                        else
                            throw new InvalidOperationException();
                    };
                }
            }
        }

        public bool ContainBreakedNode(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            if (NodesControl.GUID == context.DebuggerId)
                return true;
            foreach (var container in mSubNodesContainers)
            {
                if (container.Value.GUID == context.DebuggerId)
                    return true;
            }
            return false;
        }
        public bool SetNodeBreaked(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            if (NodesControl.GUID == context.DebuggerId)
            {
                NodesControl.SetNodeBreaked(context);
                return true;
            }
            foreach (var container in mSubNodesContainers)
            {
                if (container.Value.GUID == context.DebuggerId)
                {
                    container.Value.SetNodeBreaked(context);
                    ShowNodesContainer(container.Value);
                    return true;
                }
            }
            return false;
        }
        public bool DebugResume(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            if (NodesControl.GUID == context.DebuggerId)
            {
                NodesControl.DebugResume(context);
                return true;
            }
            foreach (var container in mSubNodesContainers)
            {
                if (container.Value.GUID == context.DebuggerId)
                {
                    container.Value.DebugResume(context);
                    return true;
                }
            }
            return false;
        }
        public bool CheckError()
        {
            var hasError = NodesControl.CheckError();
            if (hasError)
                return true;

            foreach (var container in mSubNodesContainers)
            {
                hasError = container.Value.CheckError();
                if (hasError)
                    return true;
            }
            return false;
        }
        public async System.Threading.Tasks.Task Load(bool loadAll = false)
        {
            if (string.IsNullOrEmpty(LinkedCategoryItemName))
                throw new InvalidOperationException();
            var tempFile = HostControl.GetGraphFileName(LinkedCategoryItemName);
            LinkedCategoryItemDisplayName = LinkedCategoryItemName;
            await Load(tempFile, loadAll);
        }

        public async System.Threading.Tasks.Task LoadParticleShapeTemplate(Type template, string name, bool loadAll = false)
        {
            if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeBox)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particle_client.link");
                await Load(rname.Address, name, "particle", loadAll);
            }
            else if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeCone)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particle_client.link");
                await Load(rname.Address, name, "particle", loadAll);
            }
            else if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeMesh)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particle_client.link");
                await Load(rname.Address, name, "particle", loadAll);
            }
            else if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeSphere)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particle_client.link");
                await Load(rname.Address, name, "particle", loadAll);
            }
        }

        public async System.Threading.Tasks.Task LoadParticleTemplate(Type template, string name, bool loadAll = false)
        {
            if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeBox)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particlesystem_1_client.link");
                await Load(rname.Address, name, "ParticleSystem_1", loadAll);
            }
            else if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeCone)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particlesystem_1_client.link");
                await Load(rname.Address, name, "ParticleSystem_1", loadAll);
            }
            else if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeMesh)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particlesystem_1_client.link");
                await Load(rname.Address, name, "ParticleSystem_1", loadAll);
            }
            else if (template.Equals(typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeSphere)))
            {
                var rname = EngineNS.RName.GetRName("ParticleResource/particle2.macross/link_particlesystem_1_client.link");
                await Load(rname.Address, name, "ParticleSystem_1", loadAll);
            }
        }

        public async System.Threading.Tasks.Task Load(string absFile, bool loadAll = false)
        {
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            if (linkXndHolder != null)
            {
                var linkNode = linkXndHolder.Node.FindNode("Link");
                await NodesControl.Load(linkNode);
                await InitializeNodesContainer(NodesControl);

                if (loadAll)
                {
                    var subLinks = linkXndHolder.Node.FindNode("SubLinks");
                    var nodes = subLinks.GetNodes();
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var nodeName = nodes[i].GetName();
                        var id = EngineNS.Rtti.RttiHelper.GuidTryParse(nodeName);
                        var ctrl = new CodeGenerateSystem.Controls.NodesContainerControl();
                        await ctrl.Load(nodes[i]);
                        mSubNodesContainers.Add(id, ctrl);
                        ctrl.TitleShow = Visibility.Hidden;
                        await InitializeNodesContainer(ctrl);
                    }
                    InitializeSubLinkedNodesContainer(NodesControl);
                }

                var spDataNode = linkXndHolder.Node.FindNode("ProcessDataDic");
                if (spDataNode != null)
                {
                    var nodes = spDataNode.GetNodes();
                    foreach (var node in nodes)
                    {
                        var processData = new ProcessData(Guid.Empty, this);
                        processData.Load(node);
                        mProcessDataDic[processData.Id] = processData;
                    }
                }

                linkXndHolder.Node.TryReleaseHolder();
            }

            if (LinkedKey != null)
            {
                if (!mProcessDataDic.TryGetValue(LinkedKey.Id, out mCurrentProcessData))
                {
                    mCurrentProcessData = new ProcessData(LinkedKey.Id, this);
                    mProcessDataDic[LinkedKey.Id] = mCurrentProcessData;
                }
            }
        }
        public async System.Threading.Tasks.Task LoadSubLinks(string absFile)
        {
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            if (linkXndHolder != null)
            {
                var subLinks = linkXndHolder.Node.FindNode("SubLinks");
                var nodes = subLinks.GetNodes();
                for (int i = 0; i < nodes.Count; i++)
                {
                    var nodeName = nodes[i].GetName();
                    var id = EngineNS.Rtti.RttiHelper.GuidTryParse(nodeName);
                    if (mSubNodesContainers.ContainsKey(id))
                        continue;
                    var ctrl = new CodeGenerateSystem.Controls.NodesContainerControl();
                    await ctrl.Load(nodes[i]);
                    mSubNodesContainers.Add(id, ctrl);
                    ctrl.TitleShow = Visibility.Hidden;
                    await InitializeNodesContainer(ctrl);
                }
                InitializeSubLinkedNodesContainer(NodesControl);
                linkXndHolder.Node.TryReleaseHolder();
            }
        }
        public async System.Threading.Tasks.Task LoadSubLinks(EngineNS.IO.XndHolder linkXndHolder)
        {
            var subLinks = linkXndHolder.Node.FindNode("SubLinks");
            var nodes = subLinks.GetNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                var nodeName = nodes[i].GetName();
                var id = EngineNS.Rtti.RttiHelper.GuidTryParse(nodeName);
                var ctrl = new CodeGenerateSystem.Controls.NodesContainerControl();
                await ctrl.Load(nodes[i]);
                mSubNodesContainers.Add(id, ctrl);
                ctrl.TitleShow = Visibility.Hidden;
                await InitializeNodesContainer(ctrl);
            }
            InitializeSubLinkedNodesContainer(NodesControl);
        }
        string[] ParticleSystemTemplate =
        {
            "CreateParticleSystem",
            "DoParticleCompose",
            "Create CGfxParticleSystem",
        };

        string[] ParticleShapeTemplate =
        {
            "DoParticleSubStateBorn",
            "DoParticleSubStateTick",
            "DoParticleStateBorn",
            "DoParticleStateTick",
            "CreateParticleShape",
            "Create CGfxParticleEmitter",
        };
        public void ChangeBaseNodeTemplateName(List<CodeGenerateSystem.Base.BaseNodeControl> CtrlNodeList, string name, string template)
        {
            for (int i = 0; i < CtrlNodeList.Count; i++)
            {
                var node = NodesControl.CtrlNodeList[i];
                ChangeParticleNodeName(node, template, name, true);

            }
        }

        public void OnChangeParticleCategoryName(CodeGenerateSystem.Base.BaseNodeControl node, string template, string name, bool needid)
        {
            bool need_go_on = true;
            for (int j = 0; j < ParticleSystemTemplate.Length; j++)
            {
                if (node.NodeName.IndexOf(ParticleSystemTemplate[j]) != -1)
                {
                    ChangeParticleNodeName(node, template, name, needid);
                    need_go_on = false;
                    break;
                }
            }

            if (need_go_on)
            {
                for (int j = 0; j < ParticleShapeTemplate.Length; j++)
                {
                    if (node.NodeName.IndexOf(ParticleShapeTemplate[j]) != -1)
                    {
                        ChangeParticleNodeName(node, template, name, needid);
                        break;
                    }
                }
            }
        }

        public void ChangeParticleNodeName(CodeGenerateSystem.Base.BaseNodeControl node, string oldname, string name, bool needid = false)
        {
            if (string.IsNullOrEmpty(oldname) == false || string.IsNullOrEmpty(name) == false)
            {
                node.NodeName = node.NodeName.Replace("_" + oldname, "_" + name);
            }

            var createobject = node as CodeDomNode.CreateObject;
            if (createobject != null)
            {
                createobject.SetPropertyValue("Name", name);
            }

            var custommethon = node as CodeDomNode.MethodCustom;
            if (custommethon != null)
            {
                custommethon.IsDeleteable = false; // TODO..
            }
            var childnodes = node.GetChildNodes();
            //if (string.IsNullOrEmpty(node.NodeName) == false && node.NodeName.ToLower().Equals("null") == false)
            //{
            if (childnodes.Count > 0)
            {
                for (int j = 0; j < childnodes.Count; j++)
                {
                    childnodes[j].Id = Guid.NewGuid();
                }
            }
            //}

            node.Id = Guid.NewGuid();
        }

        public async System.Threading.Tasks.Task Load(string absFile, string name, string template, bool loadAll = false)
        {
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            if (linkXndHolder != null)
            {
                var linkNode = linkXndHolder.Node.FindNode("Link");
                await NodesControl.Load(linkNode);
                await InitializeNodesContainer(NodesControl);

                ChangeBaseNodeTemplateName(NodesControl.CtrlNodeList, name, template);

                if (loadAll)
                {
                    var subLinks = linkXndHolder.Node.FindNode("SubLinks");
                    var nodes = subLinks.GetNodes();
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        //  var nodeName = nodes[i].GetName();
                        var id = Guid.NewGuid();//EngineNS.Rtti.RttiHelper.GuidTryParse(nodeName + "_" + name);
                        var ctrl = new CodeGenerateSystem.Controls.NodesContainerControl();
                        //nodes[i].SetName(nodes[i].GetName()+"_" + name);
                        await ctrl.Load(nodes[i]);
                        // ctrl.Name = nodeName + "_" + name;
                        mSubNodesContainers.Add(id, ctrl);
                        ctrl.TitleShow = Visibility.Hidden;
                        await InitializeNodesContainer(ctrl);
                        ChangeBaseNodeTemplateName(ctrl.CtrlNodeList, "", "");
                    }
                    InitializeSubLinkedNodesContainer(NodesControl);

                }
            }
        }

        public void Save()
        {
            var tempFile = HostControl.GetGraphFileName(LinkedCategoryItemName);
            Save(tempFile);
        }
        public void Save(string absFile)
        {
            if (string.IsNullOrEmpty(LinkedCategoryItemName))
                throw new InvalidOperationException();

            var holder = EngineNS.IO.XndHolder.NewXNDHolder();
            var linkNode = holder.Node.AddNode("Link", 0, 0);
            NodesControl.Save(linkNode);

            var subLinkNode = holder.Node.AddNode("SubLinks", 0, 0);
            foreach (var container in mSubNodesContainers.Values)
            {
                var subNode = subLinkNode.AddNode(container.GUID.ToString(), 0, 0);
                container.Save(subNode);
            }

            var spDataNode = holder.Node.AddNode("ProcessDataDic", 0, 0);
            foreach (var data in mProcessDataDic.Values)
            {
                var dataNode = spDataNode.AddNode("ProcessData", 0, 0);
                data.Save(dataNode);
            }

            EngineNS.IO.XndHolder.SaveXND(absFile, holder);
        }

        private void Button_VariablePopupGet_Click(object sender, RoutedEventArgs e)
        {
            var pos = new Point(Canvas.GetLeft(VariableDropShow), Canvas.GetTop(VariableDropShow));
            VariableDropShowVisibile = Visibility.Collapsed;
            var nodeCtrl = EditorCommon.Program.GetParent(VariableDropShow, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;

            switch (CurrentDropItem.CategoryItemType)
            {
                case CategoryItem.enCategoryItemType.Variable:
                case CategoryItem.enCategoryItemType.FunctionVariable:
                    {
                        //处理macross getter
                        VariableCategoryItemPropertys vitem = CurrentDropItem.PropertyShowItem as VariableCategoryItemPropertys;
                        if (vitem != null)
                        {
                            if (vitem.VariableType.IsMacrossGetter)
                            {
                                Category category;

                                if (this.HostControl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.VariableCategoryName, out category))
                                {
                                    var item = new CategoryItem(null, category);
                                    item.CategoryItemType = CurrentDropItem.CategoryItemType;
                                    item.Name = "Object_" + vitem.VariableName;
                                    var data = new CategoryItem.InitializeData();
                                    data.Reset();
                                    item.Initialize(HostControl, data);
                                    VariableCategoryItemPropertys varitem = item.PropertyShowItem as VariableCategoryItemPropertys;
                                    varitem.VariableType.Type = vitem.VariableType.MacrossClassType;
                                    CurrentDropItem = item;
                                }
                            }
                        }

                        var varProperties = CurrentDropItem.PropertyShowItem as VariableCategoryItemPropertys;
                        var nodeType = typeof(CodeDomNode.PropertyNode);
                        var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams();
                        csParam.CSType = CSType;
                        csParam.HostNodesContainer = nodeCtrl;
                        csParam.ConstructParam = "";
                        csParam.PropertyInfo = new CodeDomNode.PropertyInfoAssist()
                        {
                            PropertyName = CurrentDropItem.Name,
                            PropertyType = varProperties.VariableType.GetActualType(),
                            MacrossClassType = HostControl.CurrentResourceInfo.ResourceName.PureName(),
                            Direction = CodeDomNode.PropertyInfoAssist.enDirection.Get,
                        };
                        switch (CurrentDropItem.CategoryItemType)
                        {
                            case CategoryItem.enCategoryItemType.Variable:
                                csParam.PropertyInfo.HostType = CodeDomNode.MethodInfoAssist.enHostType.This;
                                break;
                            case CategoryItem.enCategoryItemType.FunctionVariable:
                                csParam.PropertyInfo.HostType = CodeDomNode.MethodInfoAssist.enHostType.Local;
                                break;
                        }

                        //varProperties.OnVariableNameChanged -= csParam.PropertyInfo.OnVariableNameChanged;
                        //varProperties.OnVariableNameChanged += csParam.PropertyInfo.OnVariableNameChanged;

                        //varProperties.VariableType.OnVariableTypeChanged -= csParam.PropertyInfo.OnVariableTypeChanged;
                        //varProperties.VariableType.OnVariableTypeChanged += csParam.PropertyInfo.OnVariableTypeChanged;
                        pos = nodeCtrl._RectCanvas.TranslatePoint(pos, nodeCtrl._MainDrawCanvas);
                        var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                        CurrentDropItem.AddInstanceNode(nodeCtrl.GUID, node);
                    }
                    break;
                case CategoryItem.enCategoryItemType.Property:
                    {
                        var itemPro = CurrentDropItem.PropertyShowItem as CategoryItemProperty_Property;
                        if (itemPro != null)
                        {
                            var nodeType = typeof(CodeDomNode.PropertyNode);
                            var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams();
                            csParam.CSType = CSType;
                            csParam.HostNodesContainer = nodeCtrl;
                            csParam.ConstructParam = "";
                            csParam.PropertyInfo = new CodeDomNode.PropertyInfoAssist()
                            {
                                PropertyName = CurrentDropItem.Name,
                                PropertyType = itemPro.PropertyType.Type,
                                HostType = CodeDomNode.MethodInfoAssist.enHostType.This,
                                MacrossClassType = HostControl.CurrentResourceInfo.ResourceName.PureName(),
                                Direction = CodeDomNode.PropertyInfoAssist.enDirection.Get,
                            };
                            pos = nodeCtrl._RectCanvas.TranslatePoint(pos, nodeCtrl._MainDrawCanvas);
                            var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                            CurrentDropItem.AddInstanceNode(nodeCtrl.GUID, node);
                        }
                    }
                    break;
            }

            var actionData = new CategoryItem.stDropVariableActionData()
            {
                ContainerHost = this,
                NodesContainer = nodeCtrl,
                Pos = pos
            };
            CurrentDropItem.OnDropVariableGetNodeControlAction?.Invoke(actionData);
        }
        private void Button_VariablePopupSet_Click(object sender, RoutedEventArgs e)
        {
            var pos = new Point(Canvas.GetLeft(VariableDropShow), Canvas.GetTop(VariableDropShow));
            VariableDropShowVisibile = Visibility.Collapsed;
            var nodeCtrl = EditorCommon.Program.GetParent(VariableDropShow, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;
            switch (CurrentDropItem.CategoryItemType)
            {
                case CategoryItem.enCategoryItemType.Variable:
                case CategoryItem.enCategoryItemType.FunctionVariable:
                    {
                        //处理macross getter
                        VariableCategoryItemPropertys vitem = CurrentDropItem.PropertyShowItem as VariableCategoryItemPropertys;
                        //pos = nodeCtrl._RectCanvas.TranslatePoint(pos, nodeCtrl._MainDrawCanvas);
                        if (vitem != null)
                        {
                            if (vitem.VariableType.IsMacrossGetter)
                            {
                                Category category;

                                if (this.HostControl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.FunctionCategoryName, out category))
                                {
                                    CurrentDropItem = new CategoryItem(null, category);
                                    CurrentDropItem.CategoryItemType = CategoryItem.enCategoryItemType.CustomFunction;
                                    var cvs = sender as Canvas;
                                    var funcnodeCtrl = EditorCommon.Program.GetParent(cvs, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;
                                    var funcnodeType = typeof(CodeDomNode.MethodCustomInvoke);
                                    var funccsParam = new CodeDomNode.MethodCustomInvoke.MethodCustomInvokeConstructParam();
                                    funccsParam.CSType = CSType;
                                    funccsParam.HostNodesContainer = nodeCtrl;
                                    funccsParam.ConstructParam = "";
                                    CodeDomNode.CustomMethodInfo CustomMethodInfo = new CodeDomNode.CustomMethodInfo();
                                    CustomMethodInfo.MethodName = "SetMacrossGetterRName_" + vitem.VariableName;
                                    FunctionParam fparam = new FunctionParam();
                                    fparam.ParamName = "gettervalue";
                                    fparam.Attributes.Add(new EngineNS.Editor.Editor_RNameMacrossType(vitem.VariableType.MacrossClassType));
                                    fparam.Attributes.Add(new System.ComponentModel.DescriptionAttribute(vitem.VariableType.MacrossClassType.FullName));
                                    //fparam.Attributes.Add(new EngineNS.Editor.Editor_RNameMExcelType(vitem.VariableType.MacrossClassType));

                                    //RNmaeType.CustomAttributes.a
                                    fparam.ParamType = new CodeDomNode.VariableType(typeof(EngineNS.RName), EngineNS.ECSType.Client);
                                    CustomMethodInfo.InParams.Add(fparam);
                                    CurrentDropItem.PropertyShowItem = CustomMethodInfo;
                                    funccsParam.MethodInfo = CustomMethodInfo;//CurrentDropItem.PropertyShowItem as CodeDomNode.CustomMethodInfo;
                                    //pos = nodeCtrl._RectCanvas.TranslatePoint(pos, nodeCtrl._MainDrawCanvas);
                                    var funcnode = nodeCtrl.AddNodeControl(funcnodeType, funccsParam, pos.X, pos.Y);
                                    CurrentDropItem.AddInstanceNode(nodeCtrl.GUID, funcnode);
                                    return;

                                }

                            }
                        }

                        var varProperties = CurrentDropItem.PropertyShowItem as VariableCategoryItemPropertys;
                        var nodeType = typeof(CodeDomNode.PropertyNode);
                        var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams();
                        csParam.CSType = CSType;
                        csParam.HostNodesContainer = nodeCtrl;
                        csParam.ConstructParam = "";
                        csParam.PropertyInfo = new CodeDomNode.PropertyInfoAssist()
                        {
                            PropertyName = CurrentDropItem.Name,
                            PropertyType = varProperties.VariableType.GetActualType(),
                            MacrossClassType = HostControl.CurrentResourceInfo.ResourceName.PureName(),
                            Direction = CodeDomNode.PropertyInfoAssist.enDirection.Set,
                        };
                        switch (CurrentDropItem.CategoryItemType)
                        {
                            case CategoryItem.enCategoryItemType.Variable:
                                csParam.PropertyInfo.HostType = CodeDomNode.MethodInfoAssist.enHostType.This;
                                break;
                            case CategoryItem.enCategoryItemType.FunctionVariable:
                                csParam.PropertyInfo.HostType = CodeDomNode.MethodInfoAssist.enHostType.Local;
                                break;
                        }

                        //varProperties.OnVariableNameChanged -= csParam.PropertyInfo.OnVariableNameChanged;
                        //varProperties.OnVariableNameChanged += csParam.PropertyInfo.OnVariableNameChanged;

                        //varProperties.VariableType.OnVariableTypeChanged -= csParam.PropertyInfo.OnVariableTypeChanged;
                        //varProperties.VariableType.OnVariableTypeChanged += csParam.PropertyInfo.OnVariableTypeChanged;
                        var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                        CurrentDropItem.AddInstanceNode(nodeCtrl.GUID, node);
                    }
                    break;
                case CategoryItem.enCategoryItemType.Property:
                    {
                        var itemPro = CurrentDropItem.PropertyShowItem as CategoryItemProperty_Property;
                        if (itemPro != null)
                        {
                            var nodeType = typeof(CodeDomNode.PropertyNode);
                            var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams();
                            csParam.CSType = CSType;
                            csParam.HostNodesContainer = nodeCtrl;
                            csParam.ConstructParam = "";
                            csParam.PropertyInfo = new CodeDomNode.PropertyInfoAssist()
                            {
                                PropertyName = CurrentDropItem.Name,
                                PropertyType = itemPro.PropertyType.GetActualType(),
                                HostType = CodeDomNode.MethodInfoAssist.enHostType.This,
                                MacrossClassType = HostControl.CurrentResourceInfo.ResourceName.PureName(),
                                Direction = CodeDomNode.PropertyInfoAssist.enDirection.Set,
                            };
                            var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                            CurrentDropItem.AddInstanceNode(nodeCtrl.GUID, node);
                        }
                    }
                    break;
            }

            var actionData = new CategoryItem.stDropVariableActionData()
            {
                ContainerHost = this,
                NodesContainer = nodeCtrl,
                Pos = pos
            };
            CurrentDropItem.OnDropVariableSetNodeControlAction?.Invoke(actionData);
        }

        private void Rectangle_VarableOpShowBG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VariableDropShowVisibile = Visibility.Collapsed;
            e.Handled = true;
        }

        public async System.Threading.Tasks.Task GenerateCode(CodeTypeDeclaration codeClass, CodeGenerateSystem.Base.GenerateCodeContext_Class context)
        {
            foreach (var ctrl in NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            foreach (var ctrl in NodesControl.CtrlNodeList)
            {
                string sss = ctrl.GetNodeDescriptionString();
                string m_nodeName = ctrl.m_nodeName;
                string NodeName = ctrl.NodeName;
                Type[] types = ctrl.GetType().GetInterfaces();
                var methodGenerator = ctrl as CodeGenerateSystem.Base.IMethodGenerator;
                if (methodGenerator != null)
                //if ((ctrl is CodeDomNode.MethodOverride) ||
                //    (ctrl is CodeDomNode.MethodCustom) ||
                //    (ctrl is CodeDomNode.InputActionMethodCustom))
                {
                    var methodGenerateData = new MethodGenerateData();
                    ProcessData data;
                    if (mProcessDataDic.TryGetValue(NodesControl.GUID, out data))
                    {
                        foreach (var item in data.FunctionVariableCategoryItems)
                        {
                            var paramData = new MethodLocalParamData();
                            var pro = item.PropertyShowItem as VariableCategoryItemPropertys;
                            if (pro == null)
                                continue;

                            paramData.ParamType = pro.VariableType.GetActualType();
                            paramData.ParamName = pro.VariableName;
                            methodGenerateData.LocalParams.Add(paramData);
                        }
                    }

                    await methodGenerator.GCode_CodeDom_GenerateMethodCode(codeClass, null, context, methodGenerateData);
                }
            }
        }

        public class ProcessData
        {
            public Guid Id;
            public List<CategoryItem> FunctionVariableCategoryItems
            {
                get;
                set;
            } = new List<CategoryItem>();

            public MenuItem MenuItem_Add;
            public NodesControlAssist HostControl;

            public ProcessData(Guid id, NodesControlAssist ctrl)
            {
                Id = id;
                HostControl = ctrl;
            }

            public void Save(EngineNS.IO.XndNode node)
            {
                if (node == null)
                    return;
                var functionVarNode = node.AddNode("FunctionVariable", 0, 0);
                var att = functionVarNode.AddAttrib("Data");
                att.BeginWrite();
                att.Write(Id);
                att.EndWrite();
                foreach (var item in FunctionVariableCategoryItems)
                {
                    var itemNode = functionVarNode.AddNode("ItemNode", 0, 0);
                    item.Save(itemNode);
                }
            }
            public void Load(EngineNS.IO.XndNode node)
            {
                if (node == null)
                    return;
                var functionVarNode = node.FindNode("FunctionVariable");
                if (functionVarNode != null)
                {
                    var att = functionVarNode.FindAttrib("Data");
                    att.BeginRead();
                    att.Read(out Id);
                    att.EndRead();

                    var itemNodes = functionVarNode.GetNodes();
                    foreach (var itemNode in itemNodes)
                    {
                        var item = new CategoryItem(null, null);
                        item.Load(itemNode, HostControl.HostControl);
                        item.Initialize(HostControl.HostControl, item.InitData);
                        item.OnRemove += (CategoryItem obj) =>
                        {
                            FunctionVariableCategoryItems.Remove(obj);
                        };
                        FunctionVariableCategoryItems.Add(item);
                    }
                }
            }
        }
        ProcessData mCurrentProcessData;
        public ProcessData CurrentProcessData { get => mCurrentProcessData; }
        Dictionary<Guid, ProcessData> mProcessDataDic = new Dictionary<Guid, ProcessData>();
        public Dictionary<Guid, ProcessData> ProcessDataDic => mProcessDataDic;


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var op = EngineNS.CEngine.Instance.GameEditorInstance.EditOperator;
            if (op != null)
            {
                ((EngineNS.Editor.GEditOperationProcessor)op).OnSelectedActorsChanged -= OnSelectedActorsChanged;
                ((EngineNS.Editor.GEditOperationProcessor)op).OnSelectedActorsChanged += OnSelectedActorsChanged;
            }

            if (mCurrentProcessData != null)
                LinkedKey?.ProcessOnNodesContainerShow(mCurrentProcessData);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //var op = EngineNS.CEngine.Instance.GameEditorInstance.EditOperator;
            //if (op != null)
            //{
            //    ((EngineNS.Editor.GEditOperationProcessor)op).OnSelectedActorsChanged -= OnSelectedActorsChanged;
            //}
            if (mCurrentProcessData != null)
                LinkedKey?.ProcessOnNodesContainerHide(mCurrentProcessData);
        }
        List<EngineNS.GamePlay.Actor.GActor> mSelectedActors = new List<EngineNS.GamePlay.Actor.GActor>();
        public void OnSelectedActorsChanged(object sender, List<EngineNS.GamePlay.Actor.GActor> actors)
        {
            mSelectedActors = actors;
        }
    }
}
