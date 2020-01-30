using CodeGenerateSystem.Base;
using EngineNS.IO;
using Macross;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CodeDomNode.Particle
{
  
    public class ParticleRandomDirectionControlConstructionParams : StructNodeControlConstructionParams
    {

        [EngineNS.Rtti.MetaData]
        public Type CreateType { get; set; } = typeof(EngineNS.Bricks.Particle.RandomDirectionNode.Data);//测试Box

        [EngineNS.Rtti.MetaData]
        public override string BaseClassName { get; set; } = "EngineNS.Bricks.Particle.RandomDirectionNode";
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ParticleRandomDirectionControlConstructionParams;
            retVal.CreateType = CreateType;
            return retVal;
        }

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ParticleRandomDirectionControlConstructionParams))]
    public partial class ParticleRandomDirectionControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode, IParticleGradient
    {
        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();

        partial void InitConstruction();
        CreateObject createObject;
        public ParticleRandomDirectionControl(ParticleRandomDirectionControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            //var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, true);

            //var clsType = mTemplateClassInstance.GetType();
            //var xNamePro = clsType.GetProperty("Name");
            //xNamePro.SetValue(mTemplateClassInstance, csParam.NodeName);

            NodeName = csParam.NodeName;

            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "ParticleRandomDirection";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ParticleRandomDirectionControlDown", mCtrlValueLinkHandleDown, null);
            AddLinkPinInfo("ParticleRandomDirectionControlUp", mCtrlValueLinkHandleUp, null);

            mCtrlValueLinkHandleDown.ResetDefaultFilter();
            mCtrlValueLinkHandleDown.ResetDefaultFilterByShape("ParticleRandomDirectionControlUp");

            CreateObject.CreateObjectConstructionParams createobjparam = new CreateObject.CreateObjectConstructionParams();
            createobjparam.CreateType = csParam.CreateType;
            createObject = new CreateObject(createobjparam);
            createObject.CreateTemplateClas();

            createObject.SetPropertyChangedEvent(OnPropertyChanged);

        }

        #region IParticleGradient
        public Object GetShowGradient()
        {
            return createObject.GetShowPropertyObject();
        }

        Object ParticleNode;
        public void SyncValues(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            if (ParticleNode == null)
                return;

            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            if (subsys.SubStates[j].RandomDirectionNode != null && subsys.SubStates[j].RandomDirectionNode.GetType().Name.Equals(GetClassName()))
                            {
                                subsys.SubStates[j].RandomDirectionNode._Data = ParticleNode as EngineNS.Bricks.Particle.RandomDirectionNode.Data;
                                if (subsys.SubStates[j].RandomDirectionNode._Data != null && subsys.SubStates[j].RandomDirectionNode._Data.EmitterShape != null)
                                {
                                    subsys.SubStates[j].Shape = subsys.SubStates[j].RandomDirectionNode._Data.EmitterShape;
                                }
                                return;
                            }
                        }
                    }

                }

            }
        }
        public void SetPGradient(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            ParticleNode = null;

            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            if (subsys.SubStates[j].RandomDirectionNode != null && subsys.SubStates[j].RandomDirectionNode.GetType().Name.Equals(GetClassName()))
                            {
                                ParticleNode = subsys.SubStates[j].RandomDirectionNode._Data;
                                if (subsys.SubStates[j].RandomDirectionNode._Data != null)
                                {
                                    subsys.SubStates[j].RandomDirectionNode._Data.EmitterShape = subsys.SubStates[j].Shape;
                                }
                                break;
                            }
                        }

                        if (ParticleNode != null)
                            break;
                    }
                   
                }

            }
        }

        List<string> PropertyNames = new List<string>();
        public void OnPropertyChanged(string propertyName, object newValue, object oldValue)
        {
            var createobj = GetCreateObject();
            if (ParticleNode == null || createobj == null)
            {
                return;
            }

            var srcProInfo = ParticleNode.GetType().GetProperty(propertyName);
            if (srcProInfo != null)
            {
                srcProInfo.SetValue(ParticleNode, newValue);
            }
        }
        
        #endregion

        public string GetClassName()
        {
            return "RandomDirction_" + Id.ToString().Replace("-", "_");
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }

        public CreateObject GetCreateObject()
        {
            return createObject;
        }
                
        //初始化每个结点类中的元素
        public async System.Threading.Tasks.Task InitGraph()
        {
          
        }

      
        public override object GetShowPropertyObject()
        {
            return this;
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

        protected override void EndLink(LinkPinControl linkObj)
        {
            if (linkObj == null)
                return;

            bool alreadyLink = false;
            var pinInfo = GetLinkPinInfo("ParticleRandomDirectionControlDown");
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

            var count = pinInfo.GetLinkInfosCount();
            for (int index = 0; index < count; ++index)
            {
                //AnimStateLinkInfoForUndoRedo undoRedoLinkInfo = new AnimStateLinkInfoForUndoRedo();
                //var linkInfo = pinInfo.GetLinkInfo(index);
                //if (linkInfo.m_linkFromObjectInfo == HostNodesContainer.StartLinkObj && linkInfo.m_linkToObjectInfo == linkObj)
                //{
                //    alreadyLink = true;
                //    undoRedoLinkInfo.linkInfo = linkInfo as AnimStateLinkInfo;
                //    NodesContainer.TransitionStaeBaseNodeForUndoRedo transCtrl = new NodesContainer.TransitionStaeBaseNodeForUndoRedo();
                //    var redoAction = new Action<Object>((obj) =>
                //    {
                //        transCtrl.TransitionStateNode = undoRedoLinkInfo.linkInfo.AddTransition();
                //    });
                //    redoAction.Invoke(null);
                //    EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                //                                (obj) =>
                //                                {
                //                                    undoRedoLinkInfo.linkInfo.RemoveTransition(transCtrl.TransitionStateNode);

                //                                }, "Create StateTransition");
                //}

                //if (HostNodesContainer.PreviewLinkCurve != null)
                //    HostNodesContainer.PreviewLinkCurve.Visibility = System.Windows.Visibility.Hidden;
                //base.EndLink(null,false);

            }
            //if (!alreadyLink)
            //    base.EndLink(linkObj);
        }
      
        private void StateControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            //var ctrlAssist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            //List<string> cachedPosesName = new List<string>();
            //foreach (var ctrl in ctrlAssist.NodesControl.CtrlNodeList)
            //{
            //    if (ctrl is CachedAnimPoseControl)
            //    {
            //        if (!cachedPosesName.Contains(ctrl.NodeName))
            //            cachedPosesName.Add(ctrl.NodeName);
            //    }
            //}
            //foreach (var sub in ctrlAssist.SubNodesContainers)
            //{
            //    foreach (var ctrl in sub.Value.CtrlNodeList)
            //    {
            //        if (ctrl is CachedAnimPoseControl)
            //        {
            //            if (!cachedPosesName.Contains(ctrl.NodeName))
            //                cachedPosesName.Add(ctrl.NodeName);
            //        }
            //    }
            //}
            //var assist = mLinkedNodesContainer.HostControl;
            //assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            //var nodesList = contextMenu.GetNodesList();
            //nodesList.ClearNodes();
            //var stateCP = new AnimStateControlConstructionParams()
            //{
            //    CSType = HostNodesContainer.CSType,
            //    ConstructParam = "",
            //    NodeName = "State",
            //};
            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Particle.StructNodeControl), "State", stateCP, "");
            //foreach (var cachedPoseName in cachedPosesName)
            //{
            //    var getCachedPose = new GetCachedAnimPoseConstructionParams()
            //    {
            //        CSType = HostNodesContainer.CSType,
            //        ConstructParam = "",
            //        NodeName = "CachedPose_" + cachedPoseName,
            //    };
            //    nodesList.AddNodesFromType(filterData, typeof(GetCachedAnimPoseControl), "CachedAnimPose/" + getCachedPose.NodeName, getCachedPose, "");
            //}
        }
   
        #region SaveLoad

        public override void Save(XndNode xndNode, bool newGuid)
        {
            ParticleDataSaveLoad.SaveData("ParticleRandomDirectionNode", xndNode, newGuid, CSParam as ParticleRandomDirectionControlConstructionParams, this, mLinkedNodesContainer);
            base.Save(xndNode, newGuid);
        }

        bool NeedResetLoadValue = false;

        //EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        //async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        //{
        //    return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        //}
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            //await AwaitLoad();
            NeedResetLoadValue = await ParticleDataSaveLoad.LoadData("ParticleRandomDirectionNode", xndNode, CSParam as ParticleRandomDirectionControlConstructionParams, this, mLinkedNodesContainer);
            await base.Load(xndNode);

        }


        #endregion


        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ParticleRandomDirectionControlDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "ParticleRandomDirectionControlUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(ParticleRandomDirectionControl);
        }

    }
}
