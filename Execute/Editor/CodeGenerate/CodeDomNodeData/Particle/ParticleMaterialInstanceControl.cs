using CodeGenerateSystem.Base;
using EngineNS;
using EngineNS.IO;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;

namespace CodeDomNode.Particle
{

    public class MaterialInstanceEditProperty : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public Dictionary<string, Object> SRVRNameValues = new Dictionary<string, object>();
        public Dictionary<int, Object> VarRNameValues = new Dictionary<int, object>();

        public void SyncValues(MaterialInstanceEditProperty showvalue)
        {
            foreach (var varvalue in showvalue.VarRNameValues)
            {
                var varkey = varvalue.Key;
                var objvalue = varvalue.Value;

                var ShowName = SceneMesh.MtlMeshArray[MtlIndex].MtlInst.GetVarName((uint)varkey, true);
                var index = SceneMesh.McFindVar((uint)MtlIndex, ShowName);

                if (objvalue.GetType().Equals(typeof(float)))
                {
                    SceneMesh.McSetVarFloat((uint)MtlIndex, index, (float)objvalue, 0);
                    //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);

                }
                else if (objvalue.GetType().Equals(typeof(Vector2)))
                {
                    SceneMesh.McSetVarVector2((uint)MtlIndex, index, (Vector2)objvalue, 0);
                }
                else if (objvalue.GetType().Equals(typeof(Vector3)))
                {
                    SceneMesh.McSetVarVector3((uint)MtlIndex, index, (Vector3)objvalue, 0);
                }
                else if (objvalue.GetType().Equals(typeof(Vector4)))
                {
                    SceneMesh.McSetVarColor4((uint)MtlIndex, index, (Vector4)objvalue, 0);
                }
                else if (objvalue.GetType().Equals(typeof(EngineNS.Color)))
                {
                    SceneMesh.McSetVarColor4((uint)MtlIndex, index, (EngineNS.Color)objvalue, 0);
                }

                AddVarValue(varkey, objvalue);
            }

            foreach (var srvvalue in showvalue.SRVRNameValues)
            {
                var varkey = srvvalue.Key;
                var objvalue = srvvalue.Value;
                SceneMesh.McSetTexture((uint)MtlIndex, varkey, objvalue as RName);

                AddSRVValue(varkey, objvalue);
            }

        }

        public void AddSRVValue(string index, Object value)
        {
            if (SRVRNameValues.ContainsKey(index))
            {
                SRVRNameValues[index] = value; 
            }
            else
            {
                SRVRNameValues.Add(index, value);
            }
        }

        public void AddVarValue(int index, Object value)
        {
            if (VarRNameValues.ContainsKey(index))
            {
                VarRNameValues[index] = value;
            }
            else
            {
                VarRNameValues.Add(index, value);
            }
        }

        public class SRVRName
        {
            public class ShaderVarPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
            {
                public override string GetName(object arg)
                {
                    var elem = arg as Element;
                    return elem.ShowName;
                }
                public override Type GetUIType(object arg)
                {
                    return typeof(EngineNS.RName);
                }
                public override object GetValue(object arg)
                {
                    var elem = arg as Element;
                    object obj;
                    if (elem.VarObject.Host.SRVRNameValues.TryGetValue(elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetSRVShaderName((uint)elem.VarObject.Index, true), out obj))
                    {
                        return obj as RName;
                    }
                    return elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetSRVName((UInt32)elem.VarObject.Index);
                }
                public override void SetValue(object arg, object val)
                {
                    var elem = arg as Element;

                    var value = val as EngineNS.RName;
                    var texture = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, value);
                    //elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetSRVName((uint)elem.VarObject.Index, true);
                    elem.VarObject.SceneMesh.McSetTexture(elem.VarObject.MtlIndex, elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetSRVShaderName((uint)elem.VarObject.Index, true), value);

                    elem.VarObject.Host.AddSRVValue(elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetSRVShaderName((uint)elem.VarObject.Index, true), value);
                }
            }


            public int Index;
            public UInt32 MtlIndex;
            public EngineNS.Graphics.Mesh.CGfxMesh SceneMesh;
            public MaterialInstanceEditProperty Host;
            public class Element
            {
                public string ShowName;
                public SRVRName VarObject;
            }
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Texture)]
            [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(ShaderVarPropertyUIProvider))]
            public object Name
            {
                get
                {
                    var elem = new Element();
                    elem.VarObject = this;
                    elem.ShowName = SceneMesh?.MtlMeshArray[MtlIndex].MtlInst.GetSRVShaderName((UInt32)Index, true);
                    return elem;
                }
            }
        }

        public class VarRName
        {
            public class ShaderVarPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
            {
                public override string GetName(object arg)
                {
                    var elem = arg as Element;
                    return elem.ShowName;
                }
                public override Type GetUIType(object arg)
                {
                    var elem = arg as Element;
                    return elem.Type;
                }
                public override object GetValue(object arg)
                {
                    var elem = arg as Element;
                    var mtlins = elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst;
                    object obj;
                    if (elem.VarObject.Host.VarRNameValues.TryGetValue(elem.VarObject.Index, out obj))
                    {
                        return obj;
                    }

                    if (elem.Type == typeof(float))
                    {
                       
                        float value = 0.0f;
                        
                        mtlins.GetVarValue((uint)elem.VarObject.Index, 0, ref value);
                        return value;
                    }
                    else if (elem.Type == typeof(Vector2))
                    {
                        var value = new Vector2();
                        mtlins.GetVarValue((uint)elem.VarObject.Index, 0, ref value);
                        return value;
                    }
                    else if (elem.Type == typeof(Vector3))
                    {
                        var value = new Vector3();
                        mtlins.GetVarValue((uint)elem.VarObject.Index, 0, ref value);
                        return value;
                    }
                    else if (elem.Type == typeof(Vector4))
                    {
                        var value = new Vector4();
                        mtlins.GetVarValue((uint)elem.VarObject.Index, 0, ref value);
                        return value;
                    }
                    else if (elem.Type == typeof(EngineNS.Color))
                    {
                        var value = new Vector4();
                        mtlins.GetVarValue((uint)elem.VarObject.Index, 0, ref value);
                        var retVal = new EngineNS.Color();
                        retVal.R = (byte)(value.X * 255);
                        retVal.G = (byte)(value.Y * 255);
                        retVal.B = (byte)(value.Z * 255);
                        retVal.A = (byte)(value.W * 255);
                        return retVal;
                    }
                    return null;
                }
                public override void SetValue(object arg, object val)
                {
                    var elem = arg as Element;

                    var ShowName = elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetVarName((uint)elem.VarObject.Index, true);
                    var index = elem.VarObject.SceneMesh.McFindVar(elem.VarObject.MtlIndex, ShowName);

                    if (elem.Type == typeof(float))
                    {
                        float value = (float)val;
                        elem.VarObject.SceneMesh.McSetVarFloat(elem.VarObject.MtlIndex, index, value, 0);
                        //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);

                    }
                    else if (elem.Type == typeof(Vector2))
                    {
                        var value = (Vector2)val;
                        elem.VarObject.SceneMesh.McSetVarVector2(elem.VarObject.MtlIndex, index, value, 0);
                        //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                    }
                    else if (elem.Type == typeof(Vector3))
                    {
                        var value = (Vector3)val;
                        //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        elem.VarObject.SceneMesh.McSetVarVector3(elem.VarObject.MtlIndex, index, value, 0);
                    }
                    else if (elem.Type == typeof(Vector4))
                    {
                        var value = (Vector4)val;
                        //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        elem.VarObject.SceneMesh.McSetVarColor4(elem.VarObject.MtlIndex, index, value, 0);
                    }
                    else if (elem.Type == typeof(EngineNS.Color))
                    {
                        var cl = (EngineNS.Color)val;
                        var value = new Vector4(cl.R / 255.0f, cl.G / 255.0f, cl.B / 255.0f, cl.A / 255.0f);
                        //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        elem.VarObject.SceneMesh.McSetVarColor4(elem.VarObject.MtlIndex, index, value, 0);
                    }

                    elem.VarObject.Host.AddVarValue(elem.VarObject.Index, val);
                }
            }

            public int Index;
            public UInt32 MtlIndex;
            public EngineNS.Graphics.Mesh.CGfxMesh SceneMesh;
            public MaterialInstanceEditProperty Host;
            public class Element
            {
                public System.Type Type;
                public string ShowName;
                public VarRName VarObject;
            }
            [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(ShaderVarPropertyUIProvider))]
            public object Name
            {
                get
                {
                    var elem = new Element();
                    elem.Type = typeof(Vector4);
                    elem.VarObject = this;
                    elem.ShowName = SceneMesh.MtlMeshArray[MtlIndex].MtlInst.GetVarName((uint)Index, true);
                    EngineNS.Graphics.CGfxVar varDesc = new EngineNS.Graphics.CGfxVar();
                    SceneMesh.MtlMeshArray[MtlIndex].MtlInst.GetVarDesc((uint)Index, ref varDesc);
                    var param = SceneMesh.MtlMeshArray[MtlIndex].MtlInst.Material.GetParam(SceneMesh.MtlMeshArray[MtlIndex].MtlInst.GetVarName((uint)Index, false));
                    switch (varDesc.Type)
                    {
                        case EShaderVarType.SVT_Float1:
                            elem.Type = typeof(float);
                            break;
                        case EShaderVarType.SVT_Float2:
                            elem.Type = typeof(Vector2);
                            break;
                        case EShaderVarType.SVT_Float3:
                            elem.Type = typeof(Vector3);
                            break;
                        case EShaderVarType.SVT_Float4:
                            {
                                if (param != null && param.EditorType == "Color")
                                    elem.Type = typeof(EngineNS.Color);
                                else
                                    elem.Type = typeof(Vector4);
                            }
                            break;
                        //case EShaderVarType.SVT_Matrix4x4:
                        //    elem.Type = typeof(EngineNS.Matrix);
                        //    break;
                        default:
                            throw new InvalidOperationException();
                    }
                    return elem;
                }
            }
        }

        protected EngineNS.Graphics.CGfxMaterialInstance mMaterialInstance;
        public int MtlIndex = 0;
        public EngineNS.Graphics.Mesh.CGfxMesh SceneMesh;
        public void SetMaterialInstance(EngineNS.Graphics.CGfxMaterialInstance mtl, EngineNS.Graphics.Mesh.CGfxMesh mesh, int mtlindex)
        {
            mMaterialInstance = mtl;
            MaterialInstanceName = mtl.Name;
            MtlIndex = mtlindex;
            SceneMesh = mesh;

            mSRVPrimitives = new List<SRVRName>();
            for (UInt32 i = 0; i < mMaterialInstance.SRVNumber; i++)
            {
                var item = new SRVRName();
                item.Index = (int)i;
                item.SceneMesh = mesh;
                item.MtlIndex = (UInt32)mtlindex;
                item.Host = this;
                mSRVPrimitives.Add(item);
            }

            mShaderVars = new List<VarRName>();
            for (UInt32 i = 0; i < mMaterialInstance.VarNumber; i++)
            {
                var item = new VarRName();
                item.Index = (int)i;
                item.SceneMesh = mesh;
                item.MtlIndex = (UInt32)mtlindex;
                item.Host = this;
                mShaderVars.Add(item);
            }
        }
        [Browsable(false)]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Material)]
        [DisplayName("材质")]
        public RName MaterialSourceName
        {
            get { return mMaterialInstance?.MaterialName; }
            set
            {
                //var noUse = mHostCtrl.OnResetPreviewMaterialParentMaterial(this, value);
                if (mMaterialInstance != null)
                {
                    //mMaterialInstance.MaterialName = value;
                }
            }
        }
        List<SRVRName> mSRVPrimitives;
        public List<SRVRName> Textures
        {
            get { return mSRVPrimitives; }
        }
        List<VarRName> mShaderVars;
        public List<VarRName> ShaderVars
        {
            get { return mShaderVars; }
        }
        [Browsable(false)]
        public EngineNS.Graphics.CGfxMaterialInstance MaterialInstance
        {
            get
            {
                return mMaterialInstance;
            }
        }

        [Browsable(false)]
        public RName MaterialInstanceName
        {
            get;
            set;
        }
        [Browsable(false)]
        public EngineNS.Graphics.View.ERenderLayer RenderLayer
        {
            get
            {
                return mMaterialInstance.mRenderLayer;
            }
            set
            {
                mMaterialInstance.mRenderLayer = value;
            }
        }

        //MaterialInstanceEditorControl mHostCtrl;
        //public MaterialInstanceEditProperty(MaterialInstanceEditorControl ctrl)
        //{
        //    mHostCtrl = ctrl;
        //}
        public MaterialInstanceEditProperty()
        {

        }
    }

    public class ParticleMaterialInstanceControlConstructionParams : StructNodeControlConstructionParams
    {

        [EngineNS.Rtti.MetaData]
        public Type CreateType { get; set; } = typeof(EngineNS.Bricks.Particle.MaterialInstanceNode.Data);//测试Box

        [EngineNS.Rtti.MetaData]
        public override string BaseClassName { get; set; } = "EngineNS.Bricks.Particle.MaterialInstanceNode";
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ParticleMaterialInstanceControlConstructionParams;
            retVal.CreateType = CreateType;
            return retVal;
        }

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ParticleMaterialInstanceControlConstructionParams))]
    public partial class ParticleMaterialInstanceControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode, IParticleGradient
    {
        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();

        partial void InitConstruction();
        List<MaterialInstanceEditProperty> ShowValues = new List<MaterialInstanceEditProperty>();
        bool mNeedInitMaterialDatas = false;
        public ParticleMaterialInstanceControl(ParticleMaterialInstanceControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            NodeName = csParam.NodeName;

            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "ParticleMaterialInstance";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("ParticleMaterialInstanceControlDown", mCtrlValueLinkHandleDown, null);
            AddLinkPinInfo("ParticleMaterialInstanceControlUp", mCtrlValueLinkHandleUp, null);

            mCtrlValueLinkHandleDown.ResetDefaultFilterBySystem("ParticleMaterialInstanceControlUp"); 
        }

        #region IParticleGradient
        public Object GetShowGradient()
        {
            return ShowValues;//createObject.GetShowPropertyObject();
        }

        Object ParticleNode;
        public void SyncValues(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            SetPGradient(sys);
        }
        public void SetPGradient(EngineNS.Bricks.Particle.CGfxParticleSystem sys)
        {
            ParticleNode = null;

            //ShowValues.Clear();
            var TempShowValues = ShowValues;
            ShowValues = new List<MaterialInstanceEditProperty>();
            if (sys.SubParticleSystems != null)
            {
                for (int s = 0; s < sys.SubParticleSystems.Count; s++)
                {
                    var subsys = sys.SubParticleSystems[s];
                    if (subsys.MaterialInstanceNode != null && subsys.MaterialInstanceNode.GetType().Name.Equals(GetClassName()))
                    {
                        if (subsys.HostActorMesh != null && subsys.HostActorMesh.SceneMesh != null && subsys.HostActorMesh.SceneMesh.MtlMeshArray != null)
                        {
                            bool needcreate = subsys.HostActorMesh.SceneMesh.MtlMeshArray.Length != TempShowValues.Count;
                            if (needcreate == false)
                            {
                                for (int i = 0; i < subsys.HostActorMesh.SceneMesh.MtlMeshArray.Length; i++)
                                {
                                    var mtl = subsys.HostActorMesh.SceneMesh.MtlMeshArray[i];
                                    if (mtl.MtlInst == null)
                                    {
                                        needcreate = true;
                                        break;
                                    }

                                    if (mtl.MtlInst.Name.Equals(TempShowValues[i].MaterialInstanceName) == false)
                                    {
                                        needcreate = true;
                                        break;
                                    }
                                }
                            }

                            for (int i = 0; i < subsys.HostActorMesh.SceneMesh.MtlMeshArray.Length; i++)
                            {
                                var mtl = subsys.HostActorMesh.SceneMesh.MtlMeshArray[i];
                                //if (mtl.MtlInst != null)
                                {
                                    var ShowValue = new MaterialInstanceEditProperty();
                                    ShowValue.SetMaterialInstance(mtl.MtlInst, subsys.HostActorMesh.SceneMesh, i);
                                    if (needcreate == false)
                                    {
                                        ShowValue.SyncValues(TempShowValues[i]);
                                    }
                                    ShowValues.Add(ShowValue);
                                   
                                }
                            }
                        }
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
            return "Material_" + Id.ToString().Replace("-", "_");
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }

        public CreateObject GetCreateObject()
        {
            return null;
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
            var pinInfo = GetLinkPinInfo("ParticleMaterialInstanceControlDown");
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
        List<MaterialInstanceEditProperty> InitValueDatas = new List<MaterialInstanceEditProperty>();

        public override void Save(XndNode xndNode, bool newGuid)
        {
            ParticleDataSaveLoad.SaveData("ParticleMaterialInstanceNode", xndNode, newGuid, CSParam as ParticleMaterialInstanceControlConstructionParams, this, mLinkedNodesContainer);
            base.Save(xndNode, newGuid);

            if (ShowValues.Count > 0)
            {
                InitValueDatas = ShowValues;
            }

            var attr = xndNode.AddAttrib("MaterialInstance_control");
            attr.Version = 0;
            attr.BeginWrite();
            attr.Write(InitValueDatas.Count);

            for (int i = 0; i < InitValueDatas.Count; i++)
            {
                var showvalue = InitValueDatas[i];
                attr.Write(showvalue.MaterialInstanceName);
                attr.Write(showvalue.VarRNameValues.Count);
                foreach (var varvalue in showvalue.VarRNameValues)
                {
                    attr.Write(varvalue.Key);
                    attr.Write(varvalue.Value.GetType().FullName);
                    {
                        if (varvalue.Value.GetType().Equals(typeof(float)))
                        {
                            attr.Write((float)varvalue.Value);
                            //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);

                        }
                        else if (varvalue.Value.GetType().Equals(typeof(Vector2)))
                        {
                            attr.Write((Vector2)varvalue.Value);
                        }
                        else if (varvalue.Value.GetType().Equals(typeof(Vector3)))
                        {
                            attr.Write((Vector3)varvalue.Value);
                        }
                        else if (varvalue.Value.GetType().Equals(typeof(Vector4)))
                        {
                            attr.Write((Vector4)varvalue.Value);
                        }
                        else if (varvalue.Value.GetType().Equals(typeof(EngineNS.Color)))
                        {
                            attr.Write((Color4)varvalue.Value);
                        }
                    }
                }

                attr.Write(showvalue.SRVRNameValues.Count);
                foreach (var srvvalue in showvalue.SRVRNameValues)
                {
                    attr.Write(srvvalue.Key);
                    attr.Write(srvvalue.Value as RName);
                }
            }
            attr.EndWrite();
        }

        //EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        //async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        //{
        //    return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        //}
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            //await AwaitLoad();
            await ParticleDataSaveLoad.LoadData("ParticleMaterialInstanceNode", xndNode, CSParam as ParticleMaterialInstanceControlConstructionParams, this, mLinkedNodesContainer);
            await base.Load(xndNode);

            InitValueDatas = new List<MaterialInstanceEditProperty>();
            var attr = xndNode.FindAttrib("MaterialInstance_control");
            attr.BeginRead();
            if (attr.Version == 0)
            {
                int count;
                attr.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    var showvalue = new MaterialInstanceEditProperty();
                    RName materialname;
                    attr.Read(out materialname);
                    showvalue.MaterialInstanceName = materialname;
                    int varcount;
                    attr.Read(out varcount);
                    //foreach (var varvalue in showvalue.VarRNameValues)
                    for(int varn = 0; varn < varcount; varn ++)
                    {
                        int key;
                        string typefullname;
                        attr.Read(out key);
                        attr.Read(out typefullname);
                        {
                            if (typefullname.Equals(typeof(float).FullName))
                            {
                                float value;
                                attr.Read(out value);
                                //elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                                showvalue.VarRNameValues.Add(key, value);

                            }
                            else if (typefullname.Equals(typeof(Vector2).FullName))
                            {
                                Vector2 value;
                                attr.Read(out value);
                                showvalue.VarRNameValues.Add(key, value);
                            }
                            else if (typefullname.Equals(typeof(Vector3).FullName))
                            {
                                Vector3 value;
                                attr.Read(out value);
                                showvalue.VarRNameValues.Add(key, value);
                            }
                            else if (typefullname.Equals(typeof(Vector4).FullName))
                            {
                                Vector4 value;
                                attr.Read(out value);
                                showvalue.VarRNameValues.Add(key, value);
                            }
                            else if (typefullname.Equals(typeof(EngineNS.Color).FullName))
                            {
                                Vector4 value;
                                attr.Read(out value);
                                showvalue.VarRNameValues.Add(key, value);
                            }
                        }
                    }

                    int srvcount;
                    attr.Read(out srvcount);
                    //foreach (var srvvalue in showvalue.SRVRNameValues)
                    for(int srvn = 0; srvn < srvcount; srvn ++)
                    {
                        string key;
                        RName rname;
                        attr.Read(out key);
                        attr.Read(out rname);
                        showvalue.SRVRNameValues.Add(key, rname);
                    }

                    InitValueDatas.Add(showvalue);
                }
                attr.EndRead();
            }

            if (InitValueDatas.Count > 0)
            {
                ShowValues = InitValueDatas;
            }
        }
        
        #endregion


        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ParticleMaterialInstanceControlDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "ParticleMaterialInstanceControlUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(ParticleMaterialInstanceControl);
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {

            var SetMaterialInstanceValue = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            SetMaterialInstanceValue.Name = "SetMaterialInstanceValue";
            SetMaterialInstanceValue.ReturnType = new CodeTypeReference(typeof(void));
            SetMaterialInstanceValue.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(SetMaterialInstanceValue);

            var param = new CodeParameterDeclarationExpression(typeof(EngineNS.Graphics.Mesh.CGfxMesh), "mesh");
            SetMaterialInstanceValue.Parameters.Add(param);

            SetMaterialInstanceValue.Statements.Add(new CodeVariableDeclarationStatement(typeof(int), "keyindex"));
            SetMaterialInstanceValue.Statements.Add(new CodeVariableDeclarationStatement(typeof(string), "varname"));
            var varmesh = new CodeVariableReferenceExpression("mesh");
            for (int i = 0; i < ShowValues.Count; i++)
            {
                var showvalue = ShowValues[i];

                foreach (var varvalue in showvalue.VarRNameValues)
                {
                    var varmeshMtlInst = new CodeVariableReferenceExpression("mesh.MtlMeshArray["+i+"].MtlInst");
                    SetMaterialInstanceValue.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("varname"), new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmeshMtlInst, "GetVarName", new CodeExpression[] { new CodePrimitiveExpression(varvalue.Key), new CodePrimitiveExpression(true) })));

                    SetMaterialInstanceValue.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("keyindex"), new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McFindVar", new CodeExpression[] { new CodePrimitiveExpression(i), new CodeVariableReferenceExpression("varname") })));
                    if (varvalue.Value.GetType().Equals(typeof(float)))
                    {
                        SetMaterialInstanceValue.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McSetVarFloat", new CodeExpression[] {new CodePrimitiveExpression(i), new CodeVariableReferenceExpression("keyindex"), new CodePrimitiveExpression((float)varvalue.Value), new CodePrimitiveExpression(0) }));

                    }
                    else if (varvalue.Value.GetType().Equals(typeof(Vector2)))
                    {
                        Vector2 value = (Vector2)varvalue.Value;
                        SetMaterialInstanceValue.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McSetVarVector2", new CodeExpression[] { new CodePrimitiveExpression(i), new CodeVariableReferenceExpression("keyindex"), new CodeObjectCreateExpression(typeof(Vector2), new CodeExpression[] { new CodePrimitiveExpression(value.X), new CodePrimitiveExpression(value.Y) }), new CodePrimitiveExpression(0) }));
                    }
                    else if (varvalue.Value.GetType().Equals(typeof(Vector3)))
                    {
                        Vector3 value = (Vector3)varvalue.Value;
                        SetMaterialInstanceValue.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McSetVarVector3", new CodeExpression[] { new CodePrimitiveExpression(i), new CodeVariableReferenceExpression("keyindex"), new CodeObjectCreateExpression(typeof(Vector3), new CodeExpression[] { new CodePrimitiveExpression(value.X), new CodePrimitiveExpression(value.Y), new CodePrimitiveExpression(value.Z) }), new CodePrimitiveExpression(0) }));
                    }
                    else if (varvalue.Value.GetType().Equals(typeof(Vector4)))
                    {
                        Vector4 value = (Vector4)varvalue.Value;
                        SetMaterialInstanceValue.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McSetVarColor4", new CodeExpression[] { new CodePrimitiveExpression(i), new CodeVariableReferenceExpression("keyindex"), new CodeObjectCreateExpression(typeof(Vector4), new CodeExpression[] { new CodePrimitiveExpression(value.X), new CodePrimitiveExpression(value.Y), new CodePrimitiveExpression(value.Z),new CodePrimitiveExpression(value.W) }), new CodePrimitiveExpression(0) }));
                    }
                    else if (varvalue.Value.GetType().Equals(typeof(EngineNS.Color)))
                    {
                        EngineNS.Color value = (EngineNS.Color)varvalue.Value;
                        SetMaterialInstanceValue.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McSetVarColor4", new CodeExpression[] { new CodePrimitiveExpression(i), new CodeVariableReferenceExpression("keyindex"), new CodeObjectCreateExpression(typeof(Vector4), new CodeExpression[] { new CodePrimitiveExpression(value.R), new CodePrimitiveExpression(value.G), new CodePrimitiveExpression(value.B), new CodePrimitiveExpression(value.A) }), new CodePrimitiveExpression(0) }));
                    }
                }

                foreach (var srvvalue in showvalue.SRVRNameValues)
                {
                    //var texture = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, value);
                    ////elem.VarObject.SceneMesh.MtlMeshArray[elem.VarObject.MtlIndex].MtlInst.GetSRVName((uint)elem.VarObject.Index, true);
                    //elem.VarObject.SceneMesh.McSetTexture(elem.VarObject.MtlIndex, key, value);

                    //elem.VarObject.Host.AddSRVValue(elem.VarObject.Index, value);
                    SetMaterialInstanceValue.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(varmesh, "McSetTexture", new CodeExpression[] { new CodePrimitiveExpression(i), new CodePrimitiveExpression(srvvalue.Key), new CodeSnippetExpression("EngineNS.CEngine.Instance.FileManager.GetRName(\""+ srvvalue.Value + "\")")}));
                } 
            }
        }
    }
}
