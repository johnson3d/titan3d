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
    public class ParticleVelocityByCenterControlConstructionParams : StructNodeControlConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public override string BaseClassName { get; set; } = "EngineNS.Bricks.Particle.ParticleVelocityByCenterNode";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ParticleVelocityByCenterControlConstructionParams;
            return retVal;
        }

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ParticleVelocityByCenterControlConstructionParams))]
    public partial class ParticleVelocityByCenterControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode, IParticleGradient
    {
        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();

        partial void InitConstruction();
        public ParticleDataGradient DataGradient;

        public ParticleVelocityByCenterControl(ParticleVelocityByCenterControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            NodeName = csParam.NodeName;

            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "Particle向心运动";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ParticleVelocityByCenterControlDown", mCtrlValueLinkHandleDown, null);
            AddLinkPinInfo("ParticleVelocityByCenterControlUp", mCtrlValueLinkHandleUp, null);

            mCtrlValueLinkHandleDown.ResetDefaultFilter("ParticleVelocityByCenterControlUp");

            DataGradient = new ParticleDataGradient("Float");

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
                            var node = subsys.SubStates[j].VelocityByCenterNode;
                            if (node != null)
                            {
                                if (node.GetType().Name.Equals(GetClassName()))
                                {
                                    subsys.SubStates[j].VelocityByCenterNode = DataGradient.PDataGradient as EngineNS.Bricks.Particle.ParticleVelocityByCenterNode;
                                    return;
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
                            var node = subsys.SubStates[j].VelocityByCenterNode;
                            if (node != null)
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
                   
                }

            }
        }

        #endregion

        public string GetClassName()
        {
            return "VelocityByCenterNode" + Id.ToString().Replace("-", "_");
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }
        
        public CreateObject GetCreateObject()
        {
            return null;
        }

        public async System.Threading.Tasks.Task InitGraph()
        {
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
            var pinInfo = GetLinkPinInfo("ParticleVelocityByCenterControlDown");
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
            ParticleDataSaveLoad.SaveData("ParticleVelocityByCenter", xndNode, newGuid, CSParam as ParticleVelocityByCenterControlConstructionParams, this, mLinkedNodesContainer);

            var csParam = CSParam as ParticleVelocityByCenterControlConstructionParams;
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
            await ParticleDataSaveLoad.LoadData("ParticleVelocityByCenter", xndNode, CSParam as ParticleVelocityByCenterControlConstructionParams, this, mLinkedNodesContainer);

            await base.Load(xndNode);

            await DataGradient.Load(xndNode);
        }
        
        #endregion
        
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ParticleVelocityByCenterControlDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "ParticleVelocityByCenterControlUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(ParticleVelocityByCenterControl); 
        }

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

            InitArray.Statements.AddRange(initcollection);


        }

    }
}
