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
    public class ParticleVelocityControlConstructionParams : StructNodeControlConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public override string BaseClassName { get; set; } = "EngineNS.Bricks.Particle.ParticleVelocityNode";
        
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ParticleVelocityControlConstructionParams;
            return retVal;
        }

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ParticleVelocityControlConstructionParams))]
    public partial class ParticleVelocityControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode, IParticleSaveData, IParticleGradient
    {
        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();

        partial void InitConstruction();
        public ParticleDataGradient DataGradient;
        public ParticleVelocityControl(ParticleVelocityControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            NodeName = csParam.NodeName;

            //csParam.SetHostNodesContainerEvent -= InitGraphEvent;
            //csParam.SetHostNodesContainerEvent += InitGraphEvent;

            //InitGraphEvent();

            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "ParticleVelocity";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ParticleVelocityControlDown", mCtrlValueLinkHandleDown, null);
            AddLinkPinInfo("ParticleVelocityControlUp", mCtrlValueLinkHandleUp, null);

            mCtrlValueLinkHandleDown.ResetDefaultFilter("ParticleVelocityControlUp");

            DataGradient = new ParticleDataGradient("Float3");

            Vector3Data v3 = DataGradient.VectorData as Vector3Data;
            v3.Max = EngineNS.Vector3.UnitXYZ;
            v3.Min = EngineNS.Vector3.UnitXYZ;

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
        
        #region IParticleGradient
        public Object GetShowGradient()
        {
            return DataGradient;
        }
        public void SyncValues(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            if (DataGradient.PDataGradient == null)
            {
                return;
            }

            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            var nodes = subsys.SubStates[j].ParticleVelocityNodes;
                            if (nodes != null)
                            {
                                for (int n = 0; n < nodes.Count; n ++)
                                {
                                    if (nodes[n].GetType().Name.Equals(GetClassName()))
                                    {
                                        nodes[n] = DataGradient.PDataGradient as EngineNS.Bricks.Particle.ParticleVelocityNode;
                                        return;
                                    }
                                }

                            }
                        }
                    }

                }

            }
        }
        public void SetPGradient(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            DataGradient.PDataGradient = null;
            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            var nodes = subsys.SubStates[j].ParticleVelocityNodes;
                            if (nodes != null)
                            {
                                foreach (var node in nodes)
                                {
                                    if (node.GetType().Name.Equals(GetClassName()))
                                    {
                                        DataGradient.PDataGradient = node;
                                        break;
                                    }
                                }

                            }
                            if (DataGradient.PDataGradient != null)
                                break;
                        }

                        if (DataGradient.PDataGradient != null)
                            break;
                    }
                    
                }

            }
        }

        #endregion


        public string GetClassName()
        {
            return "VelocityNode" + Id.ToString().Replace("-", "_");
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }
        
        public CreateObject GetCreateObject()
        {
            return null;
        }



        #region IParticleSaveData
        bool mIsLoadLink = false;
        public bool IsLoadLink()
        {
            return mIsLoadLink;
        }

        public void SetLoadLink(bool value)
        {
            mIsLoadLink = value;
        }

        XndNode XndNode;
        public void SetXndNode(XndNode data)
        {
            XndNode = data;
        }

        public XndNode GetXndNode()
        {
            return XndNode;
        }

        string XndAttribName;
        public void SetXndAttribName(string name)
        {
            XndAttribName = name;
        }

        public string GetXndAttribName()
        {
            return XndAttribName;
        }

        #endregion

        public async System.Threading.Tasks.Task InitGraphEvent()
        {
            await InitGraph();
            await ParticleDataSaveLoad.LoadData(CSParam as StructNodeControlConstructionParams, mLinkedNodesContainer, this);
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
            var param = CSParam as ParticleVelocityControlConstructionParams;
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
            "SetParticleVelocity2",
        };
        public override void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                await InitGraphEvent();
                await OpenGraph();
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);

        }

        async System.Threading.Tasks.Task OpenGraph()
        {
            var param = CSParam as ParticleVelocityControlConstructionParams;
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

            Type type = typeof(EngineNS.Bricks.Particle.ParticleVelocityNode);
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
                    PropertyName = "Velocity",
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
            var pinInfo = GetLinkPinInfo("ParticleVelocityControlDown");
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
            ParticleDataSaveLoad.SaveData("ParticleVelocityNode", xndNode, newGuid, CSParam as ParticleVelocityControlConstructionParams, this, mLinkedNodesContainer);

            var csParam = CSParam as ParticleVelocityControlConstructionParams;
            base.Save(xndNode, newGuid);
            DataGradient.Save(xndNode, newGuid);
        }

        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
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
            await ParticleDataSaveLoad.LoadData2("ParticleVelocityNode", xndNode, CSParam as ParticleVelocityControlConstructionParams, this, mLinkedNodesContainer);

            await base.Load(xndNode);

            await DataGradient.Load(xndNode);
        }
        
        #endregion
        
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ParticleVelocityControlDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "ParticleVelocityControlUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(ParticleVelocityControl);
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


            CodeStatementCollection initcollection = new CodeStatementCollection();

            DataGradient.GCode_CodeDom_GenerateCode_For(codeClass, initcollection, "DataArray");

            DataGradient.GCode_CodeDom_GenerateCode(codeClass, GetClassName());

            InitArray.Statements.AddRange(initcollection);


        }
    }
}
