using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public partial class PropertyGrid
    {
        //List<object> mTargetObjects;
        //public List<object> TargetObjects
        //{
        //    get { return mTargetObjects; }
        //    set
        //    {
        //        mTargetObjects = value;
        //    }
        //}
        public object Tag
        {
            get;
            set;
        }
        public object Target { get; set; }
    }

    public class PGCatogoryAttribute : Attribute
    {
        public string Name;
        public PGCatogoryAttribute(string n)
        {
            Name = n;
        }
    }
    public class PGCustomValueEditorAttribute : Attribute
    {
        public PGProvider Provider = null;
        public bool HideInPG = false;
        public bool ReadOnly = false;
        public bool UserDraw = true;
        protected bool FullRedraw = false;
        public bool Expandable = false;
        public bool Initialized { get; private set; } = false;
        public int RefCount = 0;
        public bool IsFullRedraw
        {
            get => FullRedraw;
        }

        public struct EditorInfo
        {
            public string Name;
            public Rtti.UTypeDesc Type;
            public object Value;
            public object ObjectInstance;
            public float RowHeight;
            public Controls.PropertyGrid.PropertyGrid HostPropertyGrid;
            public bool Readonly;
            public bool Expand;
            public ImGuiTreeNodeFlags_ Flags;
            public CustomPropertyDescriptor HostProperty;
        }
        //public virtual void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        public virtual bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = default;
            return false;
        }
        public async Task<bool> Initialize() 
        {
            RefCount++;
            return await Initialize_Override();
        }
        protected virtual async Task<bool> Initialize_Override()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Initialized = true;
            return true;
        }
        public void Cleanup()
        {
            RefCount--;
            if (RefCount <= 0)
                Cleanup_Override();
        }
        protected virtual void Cleanup_Override()
        {

        }
    }
    public class PGTypeEditorAttribute : PGCustomValueEditorAttribute
    {
        
        public string AssemblyFilter = null;
        public UTypeSelector.EFilterMode FilterMode = UTypeSelector.EFilterMode.IncludeObjectType | UTypeSelector.EFilterMode.IncludeValueType;

        public Rtti.UTypeDesc BaseType;
        public PGTypeEditorAttribute()
        {

        }
        public PGTypeEditorAttribute(System.Type baseType)
        {
            BaseType = Rtti.UTypeDesc.TypeOf(baseType);
        }
        public PGTypeEditorAttribute(Rtti.UTypeDesc[] types)
        {
            TypeSlt.TypeList = types;
        }
        protected static EGui.Controls.UTypeSelector TypeSlt = new EGui.Controls.UTypeSelector();
        public override bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var sz = new Vector2(0, 0);
            //var bindType = EGui.UIEditor.EditableFormData.Instance.CurrentForm.BindType;
            //if (bindType == null)
            //    return false;
            //var props = bindType.SystemType.GetProperties();
            ImGuiAPI.SetNextItemWidth(-1);
            TypeSlt.FilterMode = FilterMode;
            TypeSlt.AssemblyFilter = AssemblyFilter;            
            TypeSlt.BaseType = BaseType;
            var multiValue = info.Value as PropertyMultiValue;
            if(multiValue != null && multiValue.HasDifferentValue())
            {
                ImGuiAPI.Text(multiValue.MultiValueString);
            }
            else
            {
                //var typeStr = info.Value.ToString();// as string;
                //TypeSlt.SelectedType = Rtti.UTypeDesc.TypeOf(typeStr); //Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeStr);
                TypeSlt.SelectedType = info.Value as Rtti.UTypeDesc;
                if (TypeSlt.OnDraw(-1, 12))
                {
                    newValue = TypeSlt.SelectedType;
                    return true;
                    //foreach (var i in props)
                    //{
                    //    var v = TypeSlt.SelectedType;
                    //    prop.SetValue(ref target, v);
                    //    //foreach (var j in pg.TargetObjects)
                    //    //{
                    //    //    EGui.Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, v);
                    //    //}
                    //}
                }
            }
            return false;
        }
    }

    public interface IPropertyCustomization
    {
        void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType);
        object? GetPropertyValue(string propertyName);
        void SetPropertyValue(string propertyName, object? value);
    }

    public class PGProvider
    {
        public virtual string GetDisplayName(object arg)
        {
            return arg.ToString();
        }
        public virtual bool IsReadOnly(object arg)
        {
            return false;
        }
        public virtual Rtti.UTypeDesc GetPropertyType(object arg)
        {
            return null;
        }
        public virtual void SetValue(object arg, object val) { }
        public virtual object GetValue(object arg) { return null; }
    }

    public class PGTypeEditorManager : UModule<UEngine>
    {
        public PGTypeEditorManager()
        {
        }

        public override void Cleanup(UEngine host)
        {
            Cleanup();
            base.Cleanup(host);
        }
        public void Cleanup()
        {
            ObjectWithCreateEditor.Cleanup();
            EnumEditor.Cleanup();
            ArrayEditor.Cleanup();
            ListEditor.Cleanup();
            DictionaryEditor.Cleanup();
            foreach(var typeEditor in mTypeEditors.Values)
            {
                typeEditor.Cleanup();
            }
        }

        public override async Task<bool> Initialize(UEngine host)
        {
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(bool)), new BoolEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(int)), new Int32Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(sbyte)), new SByteEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(short)), new Int16Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(long)), new Int64Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(uint)), new UInt32Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(byte)), new ByteEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(ushort)), new UInt16Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(ulong)), new UInt64Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(float)), new FloatEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(double)), new DoubleEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(string)), new StringEditor());

            ObjectWithCreateEditor = new ObjectWithCreateEditor();
            await ObjectWithCreateEditor.Initialize();
            EnumEditor = new EnumEditor();
            await EnumEditor.Initialize();
            ArrayEditor = new ArrayEditor();
            await ArrayEditor.Initialize();
            ListEditor = new ListEditor();
            await ListEditor.Initialize();
            DictionaryEditor = new DictionaryEditor();
            await DictionaryEditor.Initialize();
            return await base.Initialize(host);
        }

        public ObjectWithCreateEditor ObjectWithCreateEditor;
        public EnumEditor EnumEditor;
        public ArrayEditor ArrayEditor;
        public ListEditor ListEditor;
        public DictionaryEditor DictionaryEditor;

        Dictionary<Rtti.UTypeDesc, PGCustomValueEditorAttribute> mTypeEditors = new Dictionary<Rtti.UTypeDesc, PGCustomValueEditorAttribute>();
        public PGCustomValueEditorAttribute GetEditorType(Rtti.UTypeDesc type)
        {
            PGCustomValueEditorAttribute result;
            if (mTypeEditors.TryGetValue(type, out result))
                return result;
            return null;
        }
        public void RegTypeEditor(Rtti.UTypeDesc type, PGCustomValueEditorAttribute editorType)
        {
            mTypeEditors[type] = editorType;
        }

        public bool DrawTypeEditor(in PGCustomValueEditorAttribute.EditorInfo info, out object newValue, out bool valueChanged)
        {
            valueChanged = false;
            newValue = info.Value;
            PGCustomValueEditorAttribute editor;
            if(mTypeEditors.TryGetValue(info.Type, out editor))
            {
                valueChanged = editor.OnDraw(in info, out newValue);
                return true;
            }

            return false;
        }
    }
    public class ColorEditorBaseAttribute : PGCustomValueEditorAttribute
    {
        public bool mHDR = false;
        public bool mDragAndDrop = true;
        public bool mOptionMenu = true;
        public bool mAlphaHalfPreview = true;
        public bool mAlphaPreview = true;
    }
    public class Color3PickerEditorAttribute : ColorEditorBaseAttribute
    {
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#Color3Picker");
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var multiValue = info.Value as PropertyMultiValue;
            if (multiValue != null && multiValue.HasDifferentValue())
                drawList.AddRectFilledMultiColor(in startPos, in endPos, 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF);
            else
            {
                var drawCol = Color3.ToAbgr((Vector3)info.Value);
                drawList.AddRectFilled(in startPos, in endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            }
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Vector3 v;
                if(multiValue != null)
                {
                    if (multiValue.HasDifferentValue())
                        v = Vector3.Zero;
                    else
                        v = (Vector3)multiValue.Values[0];
                }
                else
                    v = (Vector3)info.Value;
                var saved = v;
                ImGuiAPI.ColorPicker3(TName.FromString2("##colorpicker_", info.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview);
                if (v != saved)
                {
                    newValue = v;
                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            return valueChanged;
        }
    }
    public class Color4PickerEditorAttribute : ColorEditorBaseAttribute
    {
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#Color4Picker");
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var multiValue = info.Value as PropertyMultiValue;
            if (multiValue != null && multiValue.HasDifferentValue())
                drawList.AddRectFilledMultiColor(in startPos, in endPos, 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF);
            else
            {
                var drawCol = Color4.ToAbgr((Vector4)info.Value);
                drawList.AddRectFilled(in startPos, in endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            }
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Vector4 v;
                if (multiValue != null)
                {
                    if (multiValue.HasDifferentValue())
                        v = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                    else
                        v = (Vector4)multiValue.Values[0];
                }
                else
                    v = (Vector4)info.Value;
                var saved = v;
                ImGuiAPI.ColorPicker4(TName.FromString2("##colorpicker_", info.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    newValue = v;
                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            return valueChanged;
        }
    }
    public class UByte4ToColor4PickerEditorAttribute : ColorEditorBaseAttribute
    {
        public bool IsABGR = false;
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#UByte4ToColor4Picker");
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var boxSize = EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - boxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + boxSize;
            UInt32 drawCol;
            var multiValue = info.Value as PropertyMultiValue;
            if(multiValue != null && multiValue.HasDifferentValue())
                drawList.AddRectFilledMultiColor(in startPos, in endPos, 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF);
            else
            {
                if (IsABGR)
                    drawCol = (UInt32)info.Value;
                else
                    drawCol = Color4.Argb2Abgr((UInt32)info.Value);
                drawList.AddRectFilled(in startPos, in endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            }
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if(mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                mPopupOn = true;
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Color4 v;
                UInt32 srcValue = 0xFF000000;
                if(multiValue != null)
                {
                    if (!multiValue.HasDifferentValue())
                        srcValue = (UInt32)multiValue.Values[0];
                }
                else
                {
                    srcValue = (UInt32)info.Value;
                }
                if (IsABGR)
                    v = Color4.FromABGR(srcValue);
                else
                    v = new Color4(srcValue);
                var saved = v;
                ImGuiAPI.ColorPicker4(TName.FromString2("##colorpicker_", info.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    if (IsABGR)
                        newValue = v.ToAbgr();
                    else
                        newValue = v.ToArgb();
                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            return valueChanged;
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        public EngineNS.EGui.Controls.PropertyGrid.PGTypeEditorManager PGTypeEditorManagerInstance { get; } = new EGui.Controls.PropertyGrid.PGTypeEditorManager();
    }
}