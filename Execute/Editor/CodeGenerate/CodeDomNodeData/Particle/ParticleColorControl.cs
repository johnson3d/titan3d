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
    public class ParticleColorControlConstructionParams : StructNodeControlConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public override string BaseClassName { get; set; } = "EngineNS.Bricks.Particle.ColorBaseNode";

        [EngineNS.Rtti.MetaData]
        public int Colume { get; set; } = 0;

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ParticleColorControlConstructionParams;
            retVal.Colume = Colume;
            return retVal;
        }

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ParticleColorControlConstructionParams))]
    public partial class ParticleColorControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode, IParticleGradient
    {
        public class SaveData
        {
            public int Colume = 0;
        }

        SaveData _SaveData = new SaveData();

        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();

        partial void InitConstruction();
        ParticleColorGradient ColorGradient;
        public ParticleColorControl(ParticleColorControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            NodeName = csParam.NodeName;
            
            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "ParticleColor";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ParticleColorControlDown", mCtrlValueLinkHandleDown, null);
            AddLinkPinInfo("ParticleColorControlUp", mCtrlValueLinkHandleUp, null);

            mCtrlValueLinkHandleDown.ResetDefaultFilter("ParticleColorControlUp");

            ColorGradient = new ParticleColorGradient();
            _SaveData.Colume = csParam.Colume;
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }

        #region IParticleGradient
        public Object GetShowGradient()
        {
            return ColorGradient;
        }
        public void SetPGradient(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            //if (DataGradient.PDataGradient != null)
            //{
            //    return;
            //}
            ColorGradient.ColorBaseNode = null;
            if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
            {
                for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                {
                    var subsys = sys.SubParticleSystems[i];
                    if (subsys.SubStates != null)
                    {
                        for (int j = 0; j < subsys.SubStates.Length; j++)
                        {
                            var colornode = subsys.SubStates[j].ColorNode;
                            if (colornode != null)
                            {
                                if (colornode.GetType().Name.Equals(GetClassName()))
                                {
                                    ColorGradient.ColorBaseNode = colornode;
                                    break;
                                }
                            }
                        }
                        if (ColorGradient.ColorBaseNode != null)
                            break;
                    }
                   
                }

            }
        }

        public void SyncValues(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            if (ColorGradient.ColorBaseNode != null)
            {
                if (sys.SubParticleSystems != null && sys.SubParticleSystems.Count != 0)
                {
                    for (int i = 0; i < sys.SubParticleSystems.Count; i++)
                    {
                        var subsys = sys.SubParticleSystems[i];
                        if (subsys.SubStates != null)
                        {
                            for (int j = 0; j < subsys.SubStates.Length; j++)
                            {
                                var colornode = subsys.SubStates[j].ColorNode;
                                if (colornode != null)
                                {
                                    if (colornode.GetType().Name.Equals(GetClassName()))
                                    {
                                        subsys.SubStates[j].ColorNode = ColorGradient.ColorBaseNode;
                                        return;
                                    }
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
            return "ColorNode_"+Id.ToString().Replace("-", "_");
        }
        
        public CreateObject GetCreateObject()
        {
            return null;
        }
                
      
        bool NeedInitGrapth = true;
        //初始化每个结点类中的元素
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
            var pinInfo = GetLinkPinInfo("ParticleColorControlDown");
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
            var csParam = CSParam as ParticleColorControlConstructionParams;
            csParam.Colume = _SaveData.Colume;
            base.Save(xndNode, newGuid);

            ColorGradient.Save(xndNode, newGuid);

        }

      
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            //if (NeedInitGrapth)
            //{
            //    var test = InitGraph();
            //}
            
            await base.Load(xndNode);

            await ColorGradient.Load(xndNode);
            var csParam = CSParam as ParticleColorControlConstructionParams;
            _SaveData.Colume = csParam.Colume;
        }
        
        #endregion
        
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ParticleColorControlDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "ParticleColorControlUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(ParticleColorControl);
        }


        CodeVariableReferenceExpression stateRef = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            ColorGradient.SetDataCollect();
            
            var InitArray = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            InitArray.Name = "InitArray";
            InitArray.ReturnType = new CodeTypeReference(typeof(void));
            InitArray.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(InitArray);

            CodeStatementCollection initcollection = new CodeStatementCollection();

            ColorGradient.GCode_CodeDom_GenerateCode_For(codeClass, initcollection, "DataArray");

            ColorGradient.GCode_CodeDom_GenerateCode(codeClass);

            InitArray.Statements.AddRange(initcollection);


        }
    }
}
