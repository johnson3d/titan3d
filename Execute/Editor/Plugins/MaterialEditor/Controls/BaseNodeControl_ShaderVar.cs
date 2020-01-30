using CodeGenerateSystem.Base;
using EngineNS.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MaterialEditor.Controls
{
    public class BaseNodeControl_ShaderVar : BaseNodeControl
    {
        public delegate void Delegate_OnShaderVarChanged(BaseNodeControl_ShaderVar control, string oldValue, string newValue);
        public Delegate_OnShaderVarChanged OnShaderVarChanged;
        public delegate bool Delegate_OnShaderVarRenamed(BaseNodeControl_ShaderVar control, string oldName, string newName);
        public Delegate_OnShaderVarRenamed OnShaderVarRenamed;

        public delegate void Delegate_OnIsGenericChanging(BaseNodeControl_ShaderVar ctrl, bool oldValue, bool newValue);
        public Delegate_OnIsGenericChanging OnIsGenericChanging;


        EngineNS.Graphics.CGfxMaterialParam mShaderVarInfo = new EngineNS.Graphics.CGfxMaterialParam();
        protected EngineNS.Graphics.CGfxMaterialParam ShaderVarInfo
        {
            get { return mShaderVarInfo; }
        }

        public virtual bool IsInConstantBuffer
        {
            get;
            protected set;
        } = true;

        protected bool mIsGeneric = false;
        /// <summary>
        /// 是否为外部可用参数
        /// </summary>
        public bool IsGeneric
        {
            get { return mIsGeneric; }
            set
            {
                var oldVal = mIsGeneric;
                mIsGeneric = value;
                OnIsGenericChanging?.Invoke(this, oldVal, value);
                _OnIsGenericChanging(value);
                IsDirty = true;
            }
        }

        // isGeneric要在其他线程操作，所以这里使用IsGenericBind来和控件中的对象绑定
        public bool IsGenericBind
        {
            get { return (bool)GetValue(IsGenericBindProperty); }
            set { SetValue(IsGenericBindProperty, value); }
        }
        public static readonly DependencyProperty IsGenericBindProperty = DependencyProperty.Register("IsGenericBind", typeof(bool), typeof(BaseNodeControl_ShaderVar), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsGenericBindChanged)));
        public static void OnIsGenericBindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as BaseNodeControl_ShaderVar;
            ctrl.IsGeneric = (bool)(e.NewValue);
        }

        protected virtual void _OnIsGenericChanging(bool isGeneric)
        {
            //if(HostNodesContainer != null)
            //    HostNodesContainer.IsDirty = true;
        }

        public BaseNodeControl_ShaderVar()
        {
            ShaderVarInfo.OnReName = new EngineNS.Graphics.CGfxMaterialParam.Delegate_OnReName(_OnShaderVarRenamed);
            ShaderVarInfo.OnSetValue = new EngineNS.Graphics.CGfxMaterialParam.Delegate_OnSetValue(_OnShaderVarSetValue);
        }

        public BaseNodeControl_ShaderVar(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            ShaderVarInfo.OnReName = new EngineNS.Graphics.CGfxMaterialParam.Delegate_OnReName(_OnShaderVarRenamed);
            ShaderVarInfo.OnSetValue = new EngineNS.Graphics.CGfxMaterialParam.Delegate_OnSetValue(_OnShaderVarSetValue);
        }

        protected virtual void InitializeShaderVarInfo()
        {

        }

        protected override string NodeNameCoerceValueCallbackOverride(BaseNodeControl d, string baseValue)
        {
            if(ShaderVarInfo != null)
            {
                if (!IsShaderVarNameValid(baseValue))
                    return NodeName;
                //else if (ShaderVarInfo.Rename(GetTextureName()) == false)
                //{
                //    EditorCommon.MessageBox.Show("名称 " + newVal + " 已经被使用，请换其他名称");
                //    return NodeName;
                //}
            }
            return baseValue;
        }
        protected override void NodeNameChangedOverride(BaseNodeControl d, string oldVal, string newVal)
        {
            if (ShaderVarInfo == null)
                return;
            var newNodeName = Program.GetValidNodeName(this);
            ShaderVarInfo.Rename(newNodeName);
        }

        protected bool _OnShaderVarRenamed(EngineNS.Graphics.CGfxMaterialParam info, string oldName, string newName)
        {
            if (OnShaderVarRenamed?.Invoke(this, oldName, newName) == true)
            {
                if (HostNodesContainer != null)
                    HostNodesContainer.IsDirty = true;
                return true;
            }

            return false;
        }
        protected void _OnShaderVarSetValue(EngineNS.Graphics.CGfxMaterialParam info, string oldValue, string newValue)
        {
            OnShaderVarChanged?.Invoke(this, oldValue, newValue);
            if (HostNodesContainer != null)
                HostNodesContainer.IsDirty = true;
        }

        public virtual string GetValueDefine() { return ""; }

        public virtual EngineNS.Graphics.CGfxMaterialParam GetShaderVarInfo(bool force = false)
        {
            if (IsGeneric || force)
                return ShaderVarInfo;

            return null;
        }

        protected virtual bool IsShaderVarNameValid(string name)
        {
            if (!Regex.IsMatch(name, "^[a-zA-Z0-9_]*$"))
            {
                EditorCommon.MessageBox.Show($"名称 {name} 非法，只能使用数字字母和下划线");
                return false;
            }
            else if (!string.IsNullOrEmpty(name) && char.IsNumber(name, 0))
            {
                EditorCommon.MessageBox.Show($"名称 {name} 非法，第一个字符必须是字母");
                return false;
            }
            else if(name.IndexOf(EngineNS.Graphics.CGfxMaterialManager.ValueNamePreString) == 0)
            {
                EditorCommon.MessageBox.Show($"名称 {name} 不能包含前缀{EngineNS.Graphics.CGfxMaterialManager.ValueNamePreString}");
                return false;
            }
            else if(Program.IsShaderKeyWorld(name))
            {
                EditorCommon.MessageBox.Show($"名称{name}为材质关键字，请换用其他名称");
                return false;
            }

            return true;
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as BaseNodeControl_ShaderVar;
            string newVarName = "";
            string postStr = "";
            if (param.TargetNodesContainer != null)
            {
                var matCtrl = param.TargetNodesContainer.HostControl as MaterialEditorControl;
                if (matCtrl != null)
                {
                    // matCtrl.CurrentMaterial 不应为空，所以这里不判断
                    var hash64 = matCtrl.CurrentMaterial.GetHash64();
                    postStr = hash64.ToString();
                }
            }
            if (string.IsNullOrEmpty(NodeName) || NodeName == CodeGenerateSystem.Program.NodeDefaultName)
                newVarName = EngineNS.Graphics.CGfxMaterialManager.GetValidShaderVarName(EngineNS.Editor.Assist.GetValuedGUIDString(copyedNode.Id), postStr);
            else
            {
                // 名称重复处理
                var tempNodeName = NodeName;
                var _idx = NodeName.LastIndexOf('_');
                var tempLen = NodeName.Length - _idx;
                if(_idx >= 0 && tempLen >= 5)
                {
                    if (NodeName.Substring(_idx, 5) == "_copy")
                        tempNodeName = NodeName.Substring(0, _idx);
                }
                string newNodeName = tempNodeName + "_copy";
                int idx = 0;
                bool find = false;
                bool useOrigionName = true;
                do
                {
                    find = false;
                    if (useOrigionName)
                        newNodeName = tempNodeName;
                    else
                        newNodeName = tempNodeName + "_copy" + idx;
                    if (param.TargetNodesContainer != null)
                    {
                        foreach (var node in param.TargetNodesContainer.CtrlNodeList)
                        {
                            var svNode = node as BaseNodeControl_ShaderVar;
                            if (svNode == null)
                                continue;

                            if(svNode.NodeName == newNodeName)
                            {
                                find = true;
                                useOrigionName = false;
                                break;
                            }
                        }
                    }
                    idx++;
                }
                while (find);
                copyedNode.NodeName = newNodeName;
                newVarName = EngineNS.Graphics.CGfxMaterialManager.GetValidShaderVarName(copyedNode.NodeName, postStr);
            }
            copyedNode.ShaderVarInfo.CopyWithNewName(ShaderVarInfo, newVarName);
            return copyedNode;
        }
        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("_shaderVar");
            att.Version = 0;
            att.BeginWrite();
            att.WriteMetaObject(ShaderVarInfo);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("_shaderVar");
            if(att != null)
            {
                switch (att.Version)
                {
                    case 0:
                        {
                            att.BeginRead();
                            att.ReadMetaObject(ShaderVarInfo);
                            att.EndRead();
                        }
                        break;
                }
            }
            await base.Load(xndNode);
        }
    }
}
