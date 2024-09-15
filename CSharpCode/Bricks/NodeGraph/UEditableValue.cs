using Assimp;
using EngineNS.EGui.Controls.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UEditableValue : PGCustomValueEditorAttribute, IO.ISerializer, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        public virtual void OnWriteMember(IO.IWriter ar, IO.ISerializer obj, Rtti.TtMetaVersion metaVersion)
        {
            IO.SerializerHelper.WriteMember(ar, obj, metaVersion);
        }
        public virtual void OnReadMember(IO.IReader ar, IO.ISerializer obj, Rtti.TtMetaVersion metaVersion)
        {
            IO.SerializerHelper.ReadMember(ar, obj, metaVersion);
        }

        public interface IValueEditNotify
        {
            void OnValueChanged(UEditableValue ev);
        }
        public object Tag;
        protected IValueEditNotify mNotify;
        public UEditableValue(IValueEditNotify notify)
        {
            mNotify = notify;
            LabelName = "EVL_" + System.Threading.Interlocked.Add(ref GID_EditableValue, 1).ToString();
        }
        static UEditableValue CreateEditableValue_Internal(IValueEditNotify notify, Rtti.TtTypeDesc type, object tag)
        {
            var result = new UEditableValue(notify);
            result.ValueType = type;
            result.Tag = tag;
            return result;
        }
        public static UEditableValue CreateEditableValue(IValueEditNotify notify, Type type, object tag, object defaultValue = null)
        {
            return CreateEditableValue(notify, Rtti.TtTypeDesc.TypeOf(type), tag, defaultValue);
        }
        public static UEditableValue CreateEditableValue(IValueEditNotify notify, Rtti.TtTypeDesc type, object tag, object defaultValue = null)
        {
            if (type.IsEqual(typeof(bool)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = false;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(SByte)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (SByte)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(Int16)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (Int16)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(Int32)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (Int32)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(Int64)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (Int64)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(byte)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (byte)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(UInt16)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (UInt16)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(UInt32)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (UInt32)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(UInt64)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (UInt64)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(float)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (float)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(double)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = (double)0;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(string)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = "";
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(Vector2)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = Vector2.Zero;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(Vector3)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = Vector3.Zero;
                else
                    result.Value = defaultValue;
                return result;
            }
            if (type.IsEqual(typeof(Vector4)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = Vector4.Zero;
                else
                    result.Value = defaultValue;
                return result;
            }
            else if (type.IsEqual(typeof(System.Type)))
            {
                var result = new UTypeSelectorEValue(notify);
                result.ValueType = type;
                result.Selector.CtrlId = result.LabelName;
                result.Tag = tag;
                result.Value = defaultValue;
                return result;
            }
            else if (type.IsEqual(typeof(RName)))
            {
                var result = new URNameEValue(notify);
                result.ValueType = type;
                result.Tag = tag;
                result.Value = defaultValue;
                return result; 
            }
            else if (type.IsEqual(typeof(Color3f)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = Color3f.FromColor(Color4b.White);
                else
                    result.Value = defaultValue;
                return result;
            }
            else if(type.IsEqual(typeof(Color4f)))
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = Color4f.FromColor4b(Color4b.White);
                else
                    result.Value = defaultValue;
                return result;
            }
            else if(type.IsEnum)
            {
                var result = CreateEditableValue_Internal(notify, type, tag);
                if (defaultValue == null)
                    result.Value = System.Enum.GetValues(type.SystemType).GetValue(0);
                else
                    result.Value = defaultValue;
                result.UserDraw = false;
                return result;
            }
            return null;
        }
        private string LabelName;
        [Rtti.Meta]
        public Rtti.TtTypeDesc ValueType { get; set; }
        [Rtti.Meta]
        public virtual object Value { get; set; } = null;   // todo: change value to TtAnyValue
        public string GetValueString()
        {
            if (Value.GetType() == typeof(bool))
            {
                bool v = (bool)Value;
                if (v)
                    return "true";
                else
                    return "false";
            }
            else
            {
                return Value.ToString();
            }
        }

        public override bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            EngineNS.TtEngine.Instance.PGTypeEditorManagerInstance.DrawTypeEditor(info, out newValue, out valueChanged);
            if (valueChanged)
            {
                Value = newValue;
                mNotify?.OnValueChanged(this);
            }
            return valueChanged;
        }
        public float ControlWidth { get; set; } = 80;
        public float ControlHeight { get; set; } = 30;
        private static int GID_EditableValue = 0;
        public virtual unsafe void OnDraw(UNodeBase node, PinIn pin, UNodeGraphStyles styles, float fScale, bool useDPI)
        {
            //LabelName目前每次构造时确保id唯一，也许以后可以找到类似
            //GetObjectHandleAddress这样的方法替换
            if (ValueType.SystemType == typeof(bool))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToBoolean(Value);
                var saved = v;
                EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox($"##{LabelName}", ref v);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(SByte) ||
                ValueType.SystemType == typeof(Int16) ||
                ValueType.SystemType == typeof(Int32))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToInt32(Value);
                var saved = v;
                ImGuiAPI.InputInt($"##{LabelName}", ref v, -1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(byte) ||
                ValueType.SystemType == typeof(UInt16) ||
                ValueType.SystemType == typeof(UInt32) ||
                ValueType.SystemType == typeof(UInt64) ||
                ValueType.SystemType == typeof(Int64))
            {
                unsafe
                {
                    ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                    var oldStr = Value.ToString();
                    bool nameChanged = ImGuiAPI.InputText($"##{LabelName}", ref oldStr);
                    if (nameChanged)
                    {
                        Value = EngineNS.Support.TConvert.ToObject(ValueType.SystemType, oldStr);
                        mNotify?.OnValueChanged(this);
                    }
                    ImGuiAPI.PopItemWidth();
                }
            }
            else if (ValueType.SystemType == typeof(float))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToSingle(Value);
                var saved = v;
                ImGuiAPI.InputFloat($"##{LabelName}", ref v, 0, 0, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(double))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToDouble(Value);
                var saved = v;
                ImGuiAPI.InputDouble($"##{LabelName}", ref v, 0, 0, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(Vector2))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = (Vector2)(Value);
                var saved = v;
                ImGuiAPI.InputFloat2($"##{LabelName}", (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(Vector3))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = (Vector3)(Value);
                var saved = v;
                ImGuiAPI.InputFloat3($"##{LabelName}", (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(Vector4))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = (Vector4)(Value);
                var saved = v;
                ImGuiAPI.InputFloat4($"##{LabelName}", (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(string))
            {
                unsafe
                {
                    ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                    var oldStr = Value.ToString();
                    bool nameChanged = ImGuiAPI.InputText($"##{LabelName}", ref oldStr);
                    if (nameChanged)
                    {
                        Value = EngineNS.Support.TConvert.ToObject(ValueType.SystemType, oldStr);
                        mNotify?.OnValueChanged(this);
                    }
                    ImGuiAPI.PopItemWidth();
                }
            }
        }

        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;
        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            var pro = pros.Find("Value", false);
            var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
            proDesc.InitValue(this, Rtti.TtTypeDesc.TypeOf(this.GetType()), pro, false);
            proDesc.PropertyType = ValueType;
            proDesc.CustomValueEditor = this;
            collection.Add(proDesc);
        }

        public object GetPropertyValue(string propertyName)
        {
            return Value;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            Value = value;
            mNotify?.OnValueChanged(this);
        }

        public virtual bool IsPopupPinContextMenu()
        {
            return true;
        }
    }
    public class UTypeSelectorEValue : UEditableValue
    {
        public UTypeSelectorEValue(UEditableValue.IValueEditNotify notify)
            : base(notify)
        {
            ControlWidth = 100;
        }
        public EGui.Controls.UTypeSelector Selector
        {
            get;
        } = new EGui.Controls.UTypeSelector();
        public override object Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
            }
        }
        public override void OnDraw(UNodeBase node, PinIn pin, UNodeGraphStyles styles, float fScale, bool useDPI)
        {
            var width = ControlWidth;
            if (width < 60)
                width = 60;
            if (this.Value == null)
            {
                this.Value = Selector.SelectedType;
            }
            Selector.SelectedType = this.Value as Rtti.TtTypeDesc;
            var saved = Selector.SelectedType;
            Selector.OnDraw(width * fScale, 8);
            if (saved != Selector.SelectedType)
            {
                this.Value = Selector.SelectedType;
                mNotify?.OnValueChanged(this);
            }
        }
        public void OnValueChanged(UEditableValue ev)
        {

        }
        public override bool IsPopupPinContextMenu()
        {
            return !Selector.PopupVisible;
        }
    }

    public class URNameEValue : UEditableValue
    {
        public URNameEValue(UEditableValue.IValueEditNotify notify)
           : base(notify)
        {
            ControlWidth = 100;
            ControlHeight = 120;

            //mComboBox = new EGui.UIProxy.ComboBox()
            //{
            //    ComboOpenAction = (in Support.UAnyPointer data) =>
            //    {
            //        TtEngine.Instance.EditorInstance.RNamePopupContentBrowser.OnDraw();
            //    }
            //};
        }
        //EGui.UIProxy.ComboBox mComboBox;
        public string FilterExts;
        public RName AssetName 
        { 
            get
            {
                return Value as RName;
            }
        }
        public Rtti.TtTypeDesc MacrossType { get; set; }
        public EGui.Controls.UContentBrowser ContentBrowser;
        bool BrowserVisible = false;
        public override unsafe void OnDraw(UNodeBase node, PinIn pin, UNodeGraphStyles styles, float fScale, bool useDPI)
        {
            //Support.UAnyPointer anyPt = new Support.UAnyPointer()
            //{
            //    RefObject = mDrawData,
            //};
            //mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_None | ImGuiComboFlags_.ImGuiComboFlags_NoArrowButton | ImGuiComboFlags_.ImGuiComboFlags_HeightLarge;
            //mComboBox.Width = ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X;
            //mComboBox.Name = TName.FromString2("##", info.Name).ToString();
            //mComboBox.PreviewValue = preViewStr;

            if (ContentBrowser == null)
            {
                ContentBrowser = Editor.TtEditor.NewPopupContentBrowser();//TtEngine.Instance.EditorInstance.RNamePopupContentBrowser
            }

            if (AssetName != null)
            {
                var pos = ImGuiAPI.GetCursorScreenPos();
                var iconSize = new Vector2(64 * fScale, 64 * fScale);
                var end = pos + iconSize;
                var cmdList = ImGuiAPI.GetWindowDrawList();
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
                ameta?.OnDrawSnapshot(cmdList, ref pos, ref end);
                var cmdListFont = ImGuiAPI.GetDrawListFont(cmdList);
                var textPos = new Vector2(pos.X, end.Y);
                cmdList.AddText(cmdListFont, cmdListFont.FontSize, &textPos, 0xFFFFFFFF, AssetName.Name, null, 0.0f, null);
                ImGuiAPI.SetCursorScreenPos(new Vector2(pos.X + iconSize.X, pos.Y));
            }
            else
            {
                ImGuiAPI.Text("null");
                ImGuiAPI.SameLine(0, -1);
            }
            //ImGuiAPI.SameLine(0, -1);

            ContentBrowser.ExtNames = FilterExts;
            ContentBrowser.MacrossBase = MacrossType;
            ContentBrowser.SelectedAssets.Clear();
            if (ImGuiAPI.Button("+"))
            {
                ContentBrowser.Visible = true;
                ImGuiAPI.OpenPopup($"RName: {node.NodeId} {pin.Name}", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                BrowserVisible = true;
            }
            ImGuiAPI.SetNextWindowSize(new Vector2(400, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
            if (BrowserVisible)
            {
                if (ImGuiAPI.BeginPopupModal($"RName: {node.NodeId} {pin.Name}", ref BrowserVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    ContentBrowser.OnDraw();
                    ImGuiAPI.EndPopup();
                }
            }
            if (ContentBrowser.SelectedAssets.Count > 0 &&
                    ContentBrowser.SelectedAssets[0].GetAssetName() != AssetName)
            {
                this.Value = ContentBrowser.SelectedAssets[0].GetAssetName();
            }
        }
        public override bool IsPopupPinContextMenu()
        {
            return !BrowserVisible;
        }
    }
}
