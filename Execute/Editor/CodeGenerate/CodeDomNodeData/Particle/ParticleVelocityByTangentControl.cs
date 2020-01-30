using CodeGenerateSystem.Base;
using EngineNS.IO;
using Macross;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CodeDomNode.Particle
{
    public class ParticleVelocityByTangentControlConstructionParams : StructNodeControlConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public override string BaseClassName { get; set; } = "EngineNS.Bricks.Particle.ParticleVelocityByTangentNode";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ParticleVelocityByTangentControlConstructionParams;
            return retVal;
        }

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ParticleVelocityByTangentControlConstructionParams))]
    public partial class ParticleVelocityByTangentControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode, IParticleGradient
    {
        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();

        partial void InitConstruction();
        public ParticleDataGradient2 DataGradient;
        public ParticleVelocityByTangentControl(ParticleVelocityByTangentControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            NodeName = csParam.NodeName;

            //csParam.SetHostNodesContainerEvent -= InitGraphEvent;
            //csParam.SetHostNodesContainerEvent += InitGraphEvent;

            //InitGraphEvent();

            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "Particle切线运动";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ParticleVelocityByTangentControlDown", mCtrlValueLinkHandleDown, null);
            AddLinkPinInfo("ParticleVelocityByTangentControlUp", mCtrlValueLinkHandleUp, null);

            mCtrlValueLinkHandleDown.ResetDefaultFilter("ParticleVelocityByTangentControlUp");

            DataGradient = new ParticleDataGradient2("Float3", "Float");

            //需要解绑
            //DependencyObject parent = UIColorGradient.Parent;
            //if (parent != null)
            //{
            //    var grid = parent as System.Windows.Controls.Grid;
            //    if (grid != null)
            //    {
            //        grid.Children.Clear();
            //    }
            //    else
            //    {
            //        parent.SetValue(System.Windows.Controls.ContentPresenter.ContentProperty, null);
            //    }

            //    //ColorGradient.UIColorGradient = UIColorGradient;
            //}
        }

        //public override BaseNodeControl Duplicate(DuplicateParam param)
        //{
        //    var node = base.Duplicate(param) as ParticleAcceleratedControl;
        //    node.DataGradient = DataGradient.Duplicate();
        //    return node;
        //}

        #region IParticleGradient
        public Object GetShowGradient()
        {
            return DataGradient;
        }
        public void SetPGradient(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            DataGradient.DataGradient1.PDataGradient = null;
            DataGradient.DataGradient2.PDataGradient = null;
            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            var node = subsys.SubStates[j].VelocityByTangentNode;
                            if (node != null && node.GetType().Name.Equals(GetClassName()))
                            {
                                DataGradient.DataGradient1.PDataGradient = node;
                                DataGradient.DataGradient2.PDataGradient = node.Power;
                                break;
                            }

                        }
                        if (DataGradient.DataGradient1.PDataGradient != null)
                            break;
                    }

                }

            }
        }

        public void SyncValues(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            //if (DataGradient.DataGradient1.PDataGradient != null)
            //    return;

            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            if (subsys.SubStates[j].VelocityByTangentNode != null)
                            {
                                if (subsys.SubStates[j].VelocityByTangentNode.GetType().Name.Equals(GetClassName()))
                                {
                                    subsys.SubStates[j].VelocityByTangentNode = DataGradient.DataGradient1.PDataGradient as EngineNS.Bricks.Particle.ParticleVelocityByTangentNode;
                                    subsys.SubStates[j].VelocityByTangentNode.Power = DataGradient.DataGradient2.PDataGradient as EngineNS.Bricks.Particle.ParticleVelocityByTangentNode.TangentPower;
                                    return;
                                }
                            }

                        }
                    }

                }

            }
        }
        #endregion

        public string GetClassName()
        {
            return "VelocityByTangentNode" + Id.ToString().Replace("-", "_");
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }
        
        public CreateObject GetCreateObject()
        {
            return null;
        }


        public async System.Threading.Tasks.Task InitGraphEvent()
        {
            //await InitGraph();
            //await ParticleDataSaveLoad.LoadData(CSParam as StructNodeControlConstructionParams, mLinkedNodesContainer, this);
        }

        bool NeedInitGrapth = true;
        //初始化每个结点类中的元素
        public async System.Threading.Tasks.Task InitGraph()
        {
            if (this.HostNodesContainer == null || mLinkedNodesContainer != null)
                return;

            if (NeedInitGrapth == false)
                return;

            NeedInitGrapth = false;

            var assist = this.HostNodesContainer.HostControl;
            if (string.IsNullOrEmpty(HostNodesContainer.TitleString))
            {
                HostNodesContainer.TitleString = "MainGraph";
            }

            var TitleString = HostNodesContainer.TitleString;

            var title = TitleString + "/" + NodeName + ":" + this.Id.ToString();

            var data = new SubNodesContainerData()
            {
                ID = Id,
                Title = title,
            };
            mLinkedNodesContainer = await assist.GetSubNodesContainer(data);

            //TODO..
            Macross.NodesControlAssist NodesControlAssist = mLinkedNodesContainer.HostControl as Macross.NodesControlAssist;
            var MacrossOpPanel = NodesControlAssist.HostControl.MacrossOpPanel;

            var names = new string[] { MacrossPanelBase.GraphCategoryName, MacrossPanelBase.FunctionCategoryName, MacrossPanelBase.VariableCategoryName, MacrossPanelBase.AttributeCategoryName };

            var csparam = CSParam as StructNodeControlConstructionParams;
            csparam.CategoryDic = new Dictionary<string, Category>();
            foreach (var name in names)
            {
                var category1 = new Category(MacrossOpPanel);
                category1.CategoryName = name;
                csparam.CategoryDic.Add(name, category1);
                //categoryPanel.Children.Add(category);
            }
            foreach (var category1 in csparam.CategoryDic)
            {
                category1.Value.OnSelectedItemChanged = (categoryName) =>
                {
                    foreach (var cName in names)
                    {
                        if (cName == categoryName)
                            continue;

                        Category ctg;
                        if (csparam.CategoryDic.TryGetValue(cName, out ctg))
                        {
                            ctg.UnSelectAllItems();
                        }
                    }
                };
            }

            if (data.IsCreated)
            {
                await InitializeLinkedNodesContainer();
            }
            mLinkedNodesContainer.HostNode = this;

            //ParticleDataSaveLoad.ResetNodeConrol(NeedResetLoadValue, mLinkedNodesContainer, csparam);
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);
        }

        async System.Threading.Tasks.Task InitializeLinkedNodesContainer()
        {
            var param = CSParam as ParticleVelocityByTangentControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;

            if (mLinkedNodesContainer == null)
            {
                var TitleString = HostNodesContainer.TitleString;
                if (string.IsNullOrEmpty(HostNodesContainer.TitleString))
                {
                    TitleString = "MainGraph";
                }
                var title = TitleString + "/" + param.NodeName + ":" + this.Id.ToString();

                var data = new SubNodesContainerData()
                {
                    ID = Id,
                    Title = title,
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (!data.IsCreated)
                    return;
            }
            // 读取graph
            var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(tempFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            bool bLoaded = false;
            if (linkXndHolder != null)
            {
                var linkNode = linkXndHolder.Node.FindNode("SubLinks");
                var idStr = Id.ToString();
                foreach (var node in linkNode.GetNodes())
                {
                    if (node.GetName() == idStr)
                    {
                        await mLinkedNodesContainer.Load(node);
                        bLoaded = true;
                        break;
                    }
                }
            }
            if (bLoaded)
            {

            }
            else
            {
                for (int i = 0; i < ParticleTemplate.Length; i++)
                {
                    CreateParticleMethodCategory(ParticleTemplate[i], 50, 100 * i + 50);
                }
            }
            mLinkedNodesContainer.HostNode = this;
        }

        string[] ParticleTemplate =
        {
            "SetAix2",
        };
        public override void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
           //EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
           // {
           //     await InitGraphEvent();
           //     await OpenGraph();
           //     return true;
           // }, EngineNS.Thread.Async.EAsyncTarget.Editor);

        }

        async System.Threading.Tasks.Task OpenGraph()
        {
            var param = CSParam as ParticleVelocityByTangentControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl;
            var TitleString = HostNodesContainer.TitleString;
            if (string.IsNullOrEmpty(TitleString))
            {
                TitleString = "MainGraph";
            }
            var title = TitleString + "/" + NodeName + ":" + this.Id.ToString();
            bool isCreated;
            var data = new SubNodesContainerData()
            {
                ID = Id,
                Title = title,
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                //await InitializeLinkedNodesContainer();
            }
            mLinkedNodesContainer.HostNode = this;
            //mLinkedNodesContainer.OnFilterContextMenu = StateControl_FilterContextMenu;
        }

        private void CreateParticleMethodCategory(string methodname, float x, float y)
        {
            Macross.NodesControlAssist NodesControlAssist = mLinkedNodesContainer.HostControl as Macross.NodesControlAssist;

            Type type = typeof(EngineNS.Bricks.Particle.ParticleVelocityByTangentNode);
            System.Reflection.MethodInfo methodInfo = type.GetMethod(methodname);
            var methodinfo = CodeDomNode.Program.GetMethodInfoAssistFromMethodInfo(methodInfo, type, CodeDomNode.MethodInfoAssist.enHostType.Base, "");

            //加入列表信息
            Macross.Category category;
            var csparam = CSParam as StructNodeControlConstructionParams;
            if (methodInfo.ReturnType.Equals(typeof(void)))
            {
                if (!csparam.CategoryDic.TryGetValue(Macross.MacrossPanelBase.GraphCategoryName, out category))
                    return;
            }
            else
            {
                if (!csparam.CategoryDic.TryGetValue(Macross.MacrossPanelBase.FunctionCategoryName, out category))
                    return;
            }

            var HostControl = this.HostNodesContainer.HostControl;

            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.OverrideFunction;
            var data = new Macross.CategoryItem.InitializeData();
            data.Reset();

            var MacrossOpPanel = NodesControlAssist.HostControl.MacrossOpPanel;
            item.Initialize(MacrossOpPanel.HostControl, data);
            //HostControl.CreateNodesContainer(item);

            //MainGridItem.Children.Add(item);
            item.Name = methodname;
            category.Items.Add(item);

            //if (methodInfo.ReturnType.Equals(typeof(void)) == false)
            {
                var pnodeType = typeof(CodeDomNode.PropertyNode);
                var pncp = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams();
                pncp.CSType = mLinkedNodesContainer.CSType;
                pncp.HostNodesContainer = mLinkedNodesContainer;
                pncp.ConstructParam = "";
                pncp.PropertyInfo = new CodeDomNode.PropertyInfoAssist()
                {
                    PropertyName = "Aix",
                    PropertyType = typeof(EngineNS.Vector3),
                    HostType = CodeDomNode.MethodInfoAssist.enHostType.This,
                    MacrossClassType = csparam.BaseClassName,
                    Direction = CodeDomNode.PropertyInfoAssist.enDirection.Set,
                };
                var pnode = mLinkedNodesContainer.AddNodeControl(pnodeType, pncp, x, y);
            }


            //加入结点信息
            System.Reflection.ParameterInfo[] paramstype = methodInfo.GetParameters();

            //拷貝方法的attribute.
            var attrts = methodInfo.GetCustomAttributes(true);
            string displayname = "";
            for (int i = 0; i < attrts.Length; i++)
            {
                var displayattr = attrts[i] as System.ComponentModel.DisplayNameAttribute;
                if (displayattr != null)
                {
                    displayname = displayattr.DisplayName;
                    break;
                }
            }

            //var CustomFunctionData = new Macross.ResourceInfos.MacrossResourceInfo.CustomFunctionData();
            var nodeType = typeof(CodeDomNode.MethodOverride);
            var csParam = new CodeDomNode.MethodOverride.MethodOverrideConstructParam()
            {
                CSType = mLinkedNodesContainer.CSType,
                HostNodesContainer = mLinkedNodesContainer,
                ConstructParam = "",
                MethodInfo = methodinfo,
                DisplayName = displayname,
            };

            //var center = nodesContainer.NodesControl.GetViewCenter();
            var node = mLinkedNodesContainer.AddOrigionNode(nodeType, csParam, x, y);
            node.IsDeleteable = false;

            //重写双击事件 不需要进入二级编辑
            //item.OnDoubleClick -= item.OnDoubleClick;
            Type ItemType = item.GetType();
            FieldInfo _Field = item.GetType().GetField("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (_Field != null)
            {
                object _FieldValue = _Field.GetValue(item);
                if (_FieldValue != null && _FieldValue is Delegate)
                {
                    Delegate _ObjectDelegate = (Delegate)_FieldValue;
                    Delegate[] invokeList = _ObjectDelegate.GetInvocationList();

                    foreach (Delegate del in invokeList)
                    {
                        ItemType.GetEvent("OnDoubleClick").RemoveEventHandler(item, del);
                    }
                }
            }
            item.OnDoubleClick += (categoryItem) =>
            {
                mLinkedNodesContainer.FocusNode(node);
            };

            //NodesControlAssist.Save();
        }

        public override object GetShowPropertyObject()
        {
            return this;
        }

        protected override void EndLink(LinkPinControl linkObj)
        {
            if (linkObj == null)
                return;

            bool alreadyLink = false;
            var pinInfo = GetLinkPinInfo("ParticleVelocityByTangentControlDown");
            if (HostNodesContainer.StartLinkObj == linkObj)
            {
                base.EndLink(null);
                if (HostNodesContainer.PreviewLinkCurve != null)
                    HostNodesContainer.PreviewLinkCurve.Visibility = System.Windows.Visibility.Hidden;
                return;
            }
            if (pinInfo == null)
                return;

            if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(HostNodesContainer.StartLinkObj, linkObj) && 
                ModuleLinkInfo.CanLinkWith2(HostNodesContainer.StartLinkObj, linkObj))
            {
                var container = new NodesContainer.LinkInfoContainer();
                //链接之前删除linkObj其他链接
                if (linkObj.LinkOpType == enLinkOpType.End)
                {
                    if (linkObj.LinkInfos.Count > 0)
                    {
                        //linkObj.LinkInfos[0].m_linkFromObjectInfo.RemoveLink(linkObj.LinkInfos[0]);
                        //linkObj.LinkInfos[0].m_linkToObjectInfo.RemoveLink(linkObj.LinkInfos[0]);
                        linkObj.LinkInfos[0].Clear();
                        linkObj.LinkInfos.Clear();
                    }
                }
                //if (mStartLinkObj.LinkOpType == enLinkOpType.Start)
                //{
                container.Start = HostNodesContainer.StartLinkObj;
                    container.End = linkObj;
                //}
                //else
                //{
                //    container.Start = objInfo;
                //    container.End = mStartLinkObj;
                //}

                HostNodesContainer.IsOpenContextMenu = false;
                var redoAction = new Action<Object>((obj) =>
                {
                    var linkInfo = new ModuleLinkInfo(HostNodesContainer.ContainerDrawCanvas, container.Start, container.End);
                });
                redoAction.Invoke(null);
                //EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                //                            (obj) =>
                //                            {
                //                                for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                //                                {
                //                                    var info = container.End.GetLinkInfo(i);
                //                                    if (info.m_linkFromObjectInfo == container.Start)
                //                                    {
                //                                        info.Clear();
                //                                        break;
                //                                    }
                //                                }
                //                            }, "Create Link");
                IsDirty = true;
            }

          
        }

        public IParticleShape FindParticleShapeNode()
        {
            if (mCtrlValueLinkHandleUp.HasLink == false)
            {
                return null;
            }

            LinkInfo linkinfo = mCtrlValueLinkHandleUp.GetLinkInfo(0);
            if (linkinfo.m_linkFromObjectInfo == null || linkinfo.m_linkFromObjectInfo.HostNodeControl == null)
                return null;

            var basenodecontrol = linkinfo.m_linkFromObjectInfo.HostNodeControl;
            while (basenodecontrol != null)
            {
                if ((basenodecontrol as IParticleShape) != null)
                {
                    return basenodecontrol as IParticleShape;
                }

                IParticleNode particlenode = basenodecontrol as IParticleNode;
                if (particlenode == null)
                {
                    return null;
                }

                var linkcontrol = particlenode.GetLinkControlUp();
                if (linkcontrol == null)
                {
                    return null;
                }

                if (linkcontrol.HasLink == false)
                {
                    return null;
                }

                linkinfo = linkcontrol.GetLinkInfo(0);
                if (linkinfo.m_linkFromObjectInfo == null)
                    return null;


                basenodecontrol = linkinfo.m_linkFromObjectInfo.HostNodeControl;
            }
            return null;
        }

        #region SaveLoad

        public override void Save(XndNode xndNode, bool newGuid)
        {
            ParticleDataSaveLoad.SaveData("ParticleVelocityByTangent", xndNode, newGuid, CSParam as ParticleVelocityByTangentControlConstructionParams, this, mLinkedNodesContainer);

            var csParam = CSParam as ParticleVelocityByTangentControlConstructionParams;
            base.Save(xndNode, newGuid);
            DataGradient.Save(xndNode, newGuid);
        }

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }

        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            //if (NeedInitGrapth)
            //{
            //    var test = InitGraph();
            //}
            //await AwaitLoad();
            await ParticleDataSaveLoad.LoadData("ParticleVelocityByTangent", xndNode, CSParam as ParticleVelocityByTangentControlConstructionParams, this, mLinkedNodesContainer);
            //await ParticleDataSaveLoad.LoadData2("ParticleVelocityByTangent", xndNode, CSParam as ParticleScaleControlConstructionParams, this, mLinkedNodesContainer);
            await base.Load(xndNode);

            await DataGradient.Load(xndNode);
        }
        
        #endregion
        
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ParticleVelocityByTangentControlDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "ParticleVelocityByTangentControlUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(ParticleVelocityByTangentControl); 
        }


        CodeVariableReferenceExpression stateRef = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            DataGradient.SetDataCollect();
            

            var InitArray = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            InitArray.Name = "InitArray";
            InitArray.ReturnType = new CodeTypeReference(typeof(void));
            InitArray.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(InitArray);

            var GetStartValue = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            GetStartValue.Name = "GetStartValue";
            GetStartValue.ReturnType = new CodeTypeReference(typeof(void));
            GetStartValue.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(GetStartValue);

            CodeStatementCollection initcollection = new CodeStatementCollection();

            DataGradient.DataGradient1.GCode_CodeDom_GenerateCode_For(codeClass, initcollection,"DataArray");
            DataGradient.DataGradient1.GCode_CodeDom_GenerateCode(codeClass, GetClassName(), GetStartValue, "", "StartValue");
 
            //设置power
            {
                //InitArray = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                //InitArray.Name = "InitArray";
                //InitArray.ReturnType = new CodeTypeReference(typeof(void));
                //InitArray.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                //codeClass.Members.Add(InitArray);

                //initcollection = new CodeStatementCollection();

                DataGradient.DataGradient2.GCode_CodeDom_GenerateCode_For(codeClass, initcollection, "Power.DataArray");
                DataGradient.DataGradient2.GCode_CodeDom_GenerateCode(codeClass, GetClassName(), GetStartValue, "Power", "Power.StartValue");

                //InitArray.Statements.AddRange(initcollection);
            }

            InitArray.Statements.AddRange(initcollection);

        }
    }
}
