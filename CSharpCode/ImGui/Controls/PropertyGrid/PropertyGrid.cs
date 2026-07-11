using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public partial class TtPropertyGrid
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
        //public object Tag
        //{
        //    get;
        //    set;
        //}
        public object Target
        {
            get;
            set;
        }
    }

    public struct ExternalInfo
    {
        public CustomPropertyDescriptor PropertyDescriptor;
        public object Target;
        public object HostEditor;
    }
    public interface IExternalPropertyData
    {
        void OnDraw(in ExternalInfo info);
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class PGNoCategoryAttribute : Attribute
    {

    }
    public class TtPGCustomValueEditorAttribute : Attribute
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
            public Rtti.TtTypeDesc Type;
            public object Value;
            public object ObjectInstance;
            public object FirstObjectInstance
            {
                get
                {
                    if (ObjectInstance is IList lst)
                    {
                        return lst[0];
                    }
                    return ObjectInstance;
                }
            }
            public float RowHeight;
            public Controls.PropertyGrid.TtPropertyGrid HostPropertyGrid;
            public bool Readonly;
            public bool Expand;
            public ImGuiTreeNodeFlags_ Flags;
            public CustomPropertyDescriptor HostProperty;

            public void CopyTo(ref EditorInfo info)
            {
                Name = info.Name;
                Type = info.Type;
                Value = info.Value;
                ObjectInstance = info.ObjectInstance;
                HostPropertyGrid = info.HostPropertyGrid;
                Readonly = info.Readonly;
                Expand = info.Expand;
                Flags = info.Flags;
                HostProperty = info.HostProperty;
            }
        }
        //public virtual void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        // return value is changed, if the value changed in OnDraw process, then return true, else return false
        public virtual bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = default;
            return false;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            RefCount++;
            return await Initialize_Override();
        }
        protected virtual async Thread.Async.TtTask<bool> Initialize_Override()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
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
        public virtual string GetErrorString<T>(in EditorInfo info, T newValue) { return null; }
    }
    public class TtPGTypeEditorAttribute : TtPGCustomValueEditorAttribute
    {

        public string AssemblyFilter = null;
        public UTypeSelector.EFilterMode FilterMode = UTypeSelector.EFilterMode.IncludeObjectType | UTypeSelector.EFilterMode.IncludeValueType;

        public Rtti.TtTypeDesc BaseType;
        public TtPGTypeEditorAttribute()
        {
            BaseType = null;
        }
        public TtPGTypeEditorAttribute(System.Type baseType)
        {
            BaseType = Rtti.TtTypeDesc.TypeOf(baseType);
        }
        public TtPGTypeEditorAttribute(Rtti.TtTypeDesc[] types)
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
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                ImGuiAPI.Text(multiValue.MultiValueString);
            }
            else
            {
                //var typeStr = info.Value.ToString();// as string;
                //TypeSlt.SelectedType = Rtti.TtTypeDesc.TypeOf(typeStr); //Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(typeStr);
                TypeSlt.SelectedType = info.Value as Rtti.TtTypeDesc;
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

    public class TtButtonAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
    {
        public TtButtonAttribute()
        {
            //FullRedraw = true;
        }
        public string ButtonText;
        Vector3 HalfExtent = new Vector3(1);
        public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var sz = new Vector2(0);
            if (ImGuiAPI.Button(ButtonText, in sz))
            {
                OnButtonClick(in info);
            }
            return false;
        }
        protected virtual void OnButtonClick(in EditorInfo info)
        {

        }
    }

    public interface IPropertySetPreChecker
    {
        bool CanSetPropertyValue(string propertyName, object? value);
    }
    public interface IPropertyCustomization
    {
        bool IsPropertyVisibleDirty { get; set; }
        void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType);
#nullable enable
        object? GetPropertyValue(string propertyName);
        void SetPropertyValue(string propertyName, object? value);
#nullable disable
    }
    public class PropertyNotFindValueClass
    {
        private static readonly Object s_sync = new Object();
        static PropertyNotFindValueClass mPropertyNotFindValue;
        public static PropertyNotFindValueClass PropertyNotFindValue
        {
            get
            {
                lock (s_sync)
                {
                    if (mPropertyNotFindValue == null)
                        mPropertyNotFindValue = new PropertyNotFindValueClass();
                    return mPropertyNotFindValue;
                }
            }
        }
    }

    public class PropertyCustomizationHelper<T>
    {
        public static void GetProperties(in T obj, ref CustomPropertyDescriptorCollection collection, bool parentIsValueType, bool useDefinitionOrder = false)
        {
            var pros = TypeDescriptor.GetProperties(obj);
            var objType = Rtti.TtTypeDesc.TypeOf(obj.GetType());
            int definitionOrder = 0;
            foreach (PropertyDescriptor prop in pros)
            {
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(obj, objType, prop, parentIsValueType, useDefinitionOrder ? definitionOrder : -1);
                if (useDefinitionOrder)
                    definitionOrder++;
                if (!proDesc.IsBrowsable)
                    continue;
                collection.Add(proDesc);
            }
        }
#nullable enable
        public static object? GetPropertyValue(in T obj, string propertyName)
        {
            var proInfo = obj.GetType().GetProperty(propertyName);
            return proInfo?.GetValue(obj);
        }
        public static void SetPropertyValue(in T obj, string propertyName, object? value)
        {
            var proInfo = obj.GetType().GetProperty(propertyName);
            if (proInfo != null)
                proInfo.SetValue(obj, value);
        }
#nullable disable
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
        public virtual Rtti.TtTypeDesc GetPropertyType(object arg)
        {
            return null;
        }
        public virtual void SetValue(object arg, object val) { }
        public virtual object GetValue(object arg) { return null; }
    }

    public class TtPGTypeEditorManager : TtModule<TtEngine>
    {
        public TtPGTypeEditorManager()
        {
        }

        public override void Cleanup(TtEngine host)
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
            foreach (var typeEditor in mTypeEditors.Values)
            {
                typeEditor.Cleanup();
            }
        }

        public override async Thread.Async.TtTask<bool> Initialize(TtEngine host)
        {
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(bool)), new BoolEditor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(int)), new Int32Editor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(sbyte)), new SByteEditor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(short)), new Int16Editor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(long)), new Int64Editor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(uint)), new UInt32Editor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(byte)), new ByteEditor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(ushort)), new UInt16Editor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(ulong)), new UInt64Editor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(float)), new FloatEditor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(double)), new DoubleEditor());
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(string)), new StringEditor());
            var rNameEditor = new RName.PGRNameAttribute();
            await rNameEditor.Initialize();
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(RName)), rNameEditor);
            var color4fEditor = new TtColor4PickerEditorAttribute();
            await color4fEditor.Initialize();
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(Color4f)), color4fEditor);
            var color3fEditor = new TtColor3PickerEditorAttribute();
            await color3fEditor.Initialize();
            RegTypeEditor(Rtti.TtTypeDesc.TypeOf(typeof(Color3f)), color3fEditor);

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

        Dictionary<Rtti.TtTypeDesc, TtPGCustomValueEditorAttribute> mTypeEditors = new Dictionary<Rtti.TtTypeDesc, TtPGCustomValueEditorAttribute>();
        public TtPGCustomValueEditorAttribute GetEditorType(Rtti.TtTypeDesc type)
        {
            TtPGCustomValueEditorAttribute result;
            if (mTypeEditors.TryGetValue(type, out result))
                return result;
            return null;
        }
        public void RegTypeEditor(Rtti.TtTypeDesc type, TtPGCustomValueEditorAttribute editorType)
        {
            mTypeEditors[type] = editorType;
        }

        public bool DrawTypeEditor(in TtPGCustomValueEditorAttribute.EditorInfo info, out object newValue, out bool valueChanged)
        {
            valueChanged = false;
            newValue = info.Value;
            TtPGCustomValueEditorAttribute editor;
            if (mTypeEditors.TryGetValue(info.Type, out editor))
            {
                valueChanged = editor.OnDraw(in info, out newValue);
                return true;
            }

            return false;
        }
    }
    public class TtColorEditorBaseAttribute : TtPGCustomValueEditorAttribute
    {
        public bool mHDR = false;
        public bool mDragAndDrop = true;
        public bool mOptionMenu = true;
        public bool mAlphaHalfPreview = true;
        public bool mAlphaPreview = true;
    }
    public class TtColor3PickerEditorAttribute : TtColorEditorBaseAttribute
    {
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#Color3Picker" + info.Name);
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var multiValue = info.Value as PropertyMultiValue;
            if (multiValue != null && multiValue.HasDifferentValue())
            {
                drawList.AddRectFilledMultiColor(in startPos, in endPos, 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF);
            }
            else
            {
                var drawCol = Color3f.ToAbgr(Vector3.FromObject(info.Value));
                drawList.AddRectFilled(in startPos, in endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            }
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, true, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Vector3 v;
                if (multiValue != null)
                {
                    if (multiValue.HasDifferentValue())
                        v = Vector3.Zero;
                    else
                        v = Vector3.FromObject(multiValue.Values[0]);
                }
                else
                    v = Vector3.FromObject(info.Value);
                var saved = v;
                ImGuiAPI.ColorPicker3("##colorpicker_" + info.Name, (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview);
                if (v != saved)
                {
                    if (info.Type.IsEqual(typeof(Color4b)))
                        newValue = Color4b.FromArgb((int)(255), (int)(v.r * 255), (int)(v.g * 255), (int)(v.b * 255));
                    else if (info.Type.IsEqual(typeof(Color3f)))
                        newValue = (Color3f)v;
                    else
                        newValue = v;

                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
            return valueChanged;
        }
    }
    public class TtColor4PickerEditorAttribute : TtColorEditorBaseAttribute
    {
        bool mPopupOn = false;
        static TtColor4PickerEditorAttribute GlobalPicker = new TtColor4PickerEditorAttribute();
        public static unsafe bool OnDrawStatic(in EditorInfo info, out object newValue)
        {
            return GlobalPicker.OnDraw(in info, out newValue);
        }
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
            {
                // todo: multi color
                drawList.AddRectFilledMultiColor(in startPos, in endPos, 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF);
            }
            else
            {
                var drawCol = Color4f.ToAbgr(Vector4.FromObject(info.Value));
                drawList.AddRectFilled(in startPos, in endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            }
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, true, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Vector4 v;
                if (multiValue != null)
                {
                    if (multiValue.HasDifferentValue())
                        v = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                    else
                        v = Vector4.FromObject(multiValue.Values[0]);
                }
                else
                {
                    v = Vector4.FromObject(info.Value);
                }
                var saved = v;
                ImGuiAPI.ColorPicker4("##colorpicker_" + info.Name, (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    if (info.Type.IsEqual(typeof(Color4b)))
                        newValue = Color4b.FromArgb((int)(v.A * 255), (int)(v.R * 255), (int)(v.G * 255), (int)(v.B * 255));
                    else if (info.Type.IsEqual(typeof(Color4f)))
                        newValue = (Color4f)v;
                    else
                        newValue = v;

                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
            return valueChanged;
        }
    }
    /// <summary>
    /// UE-style Color Grading Wheel editor for Vector4(R, G, B, Y/Intensity).
    /// Features: Hue/Saturation color wheel + R/G/B/Y HDR sliders (values can exceed 1.0).
    /// Y maps to Vector4.W (overall intensity multiplier).
    /// </summary>
    public class TtColorGradingWheelEditorAttribute : TtPGCustomValueEditorAttribute
    {
        bool mPopupOn = false;

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var v = Vector4.FromObject(info.Value);

            var id = ImGuiAPI.GetID("#CGWheel_" + info.Name);
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var height = ImGuiAPI.GetFrameHeight();
            var boxSize = EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            startPos.Y += (height - boxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + boxSize;

            // Preview: show saturated color (RGB normalized by Y)
            var previewR = Math.Clamp(v.X, 0f, 1f);
            var previewG = Math.Clamp(v.Y, 0f, 1f);
            var previewB = Math.Clamp(v.Z, 0f, 1f);
            uint previewCol = (uint)(255) << 24 | (uint)(previewB * 255) << 16 | (uint)(previewG * 255) << 8 | (uint)(previewR * 255);
            drawList.AddRectFilled(in startPos, in endPos, previewCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);

            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, true, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, boxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("cgWheelPopup_" + info.Name, ImGuiPopupFlags_.ImGuiPopupFlags_None);
                mPopupOn = true;
            }

            EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopup("cgWheelPopup_" + info.Name, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                float popupWidth = 320f;
                float wheelSize = 140f;
                float sliderWidth = popupWidth - wheelSize - 20f;

                // ── Left side: Color Wheel (Hue/Sat only, using ImGui picker in wheel mode) ──
                ImGuiAPI.BeginGroup();
                {
                    // Convert RGB to HSV for the wheel (clamp to [0,1] for wheel display)
                    float maxComp = Math.Max(Math.Max(Math.Abs(v.X), Math.Abs(v.Y)), Math.Max(Math.Abs(v.Z), 0.001f));
                    Vector3 normalizedRgb = new Vector3(
                        Math.Clamp(v.X / maxComp, 0f, 1f),
                        Math.Clamp(v.Y / maxComp, 0f, 1f),
                        Math.Clamp(v.Z / maxComp, 0f, 1f));

                    Vector3 hsv = RgbToHsv(normalizedRgb);
                    var savedHsv = hsv;

                    ImGuiAPI.SetNextItemWidth(wheelSize);
                    ImGuiColorEditFlags_ wheelFlags =
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_NoInputs |
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_NoAlpha |
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview |
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview |
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_PickerHueWheel |
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_InputRGB |
                        ImGuiColorEditFlags_.ImGuiColorEditFlags_DisplayHSV;

                    // Use HSV input for the wheel
                    Vector3 wheelCol = normalizedRgb;
                    if (ImGuiAPI.ColorPicker3("##cgwheel_" + info.Name, (float*)&wheelCol, wheelFlags))
                    {
                        // Guard: if wheel returns near-black (user dragged to center/outside), skip update
                        float newMax = Math.Max(Math.Max(wheelCol.X, wheelCol.Y), Math.Max(wheelCol.Z, 0f));
                        if (newMax > 0.001f)
                        {
                            v.X = (wheelCol.X / newMax) * maxComp;
                            v.Y = (wheelCol.Y / newMax) * maxComp;
                            v.Z = (wheelCol.Z / newMax) * maxComp;
                            valueChanged = true;
                        }
                    }
                }
                ImGuiAPI.EndGroup();

                ImGuiAPI.SameLine(0, 10);

                // ── Right side: R/G/B/Y sliders (HDR, no clamp) ──
                ImGuiAPI.BeginGroup();
                {
                    ImGuiAPI.SetNextItemWidth(sliderWidth);
                    ImGuiAPI.Text("R");
                    ImGuiAPI.SameLine(20, 0);
                    ImGuiAPI.SetNextItemWidth(sliderWidth - 20);
                    float r = v.X;
                    if (ImGuiAPI.DragFloat("##cg_R_" + info.Name, ref r, 0.01f, 0f, 0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None))
                    {
                        v.X = r;
                        valueChanged = true;
                    }

                    ImGuiAPI.Text("G");
                    ImGuiAPI.SameLine(20, 0);
                    ImGuiAPI.SetNextItemWidth(sliderWidth - 20);
                    float g = v.Y;
                    if (ImGuiAPI.DragFloat("##cg_G_" + info.Name, ref g, 0.01f, 0f, 0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None))
                    {
                        v.Y = g;
                        valueChanged = true;
                    }

                    ImGuiAPI.Text("B");
                    ImGuiAPI.SameLine(20, 0);
                    ImGuiAPI.SetNextItemWidth(sliderWidth - 20);
                    float b = v.Z;
                    if (ImGuiAPI.DragFloat("##cg_B_" + info.Name, ref b, 0.01f, 0f, 0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None))
                    {
                        v.Z = b;
                        valueChanged = true;
                    }

                    ImGuiAPI.Text("Y");
                    ImGuiAPI.SameLine(20, 0);
                    ImGuiAPI.SetNextItemWidth(sliderWidth - 20);
                    float y = v.W;
                    if (ImGuiAPI.DragFloat("##cg_Y_" + info.Name, ref y, 0.01f, 0f, 0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None))
                    {
                        v.W = y;
                        valueChanged = true;
                    }
                }
                ImGuiAPI.EndGroup();

                if (valueChanged)
                    newValue = v;

                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
            return valueChanged;
        }

        static Vector3 RgbToHsv(Vector3 rgb)
        {
            float r = rgb.X, g = rgb.Y, b = rgb.Z;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0f, s = 0f, val = max;
            if (max > 0f) s = delta / max;
            if (delta > 0.0001f)
            {
                if (max == r) h = (g - b) / delta + (g < b ? 6f : 0f);
                else if (max == g) h = (b - r) / delta + 2f;
                else h = (r - g) / delta + 4f;
                h /= 6f;
            }
            return new Vector3(h, s, val);
        }
    }

    public class TtByte4ToColor4PickerEditorAttribute : TtColorEditorBaseAttribute
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
            if (multiValue != null && multiValue.HasDifferentValue())
                drawList.AddRectFilledMultiColor(in startPos, in endPos, 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF);
            else
            {
                if (IsABGR)
                    drawCol = System.Convert.ToUInt32(info.Value);
                else
                    drawCol = Color4f.Argb2Abgr((UInt32)info.Value);
                drawList.AddRectFilled(in startPos, in endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            }
            drawList.AddRect(in startPos, in endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(in startPos, in endPos, id, ref hovered, ref held, true, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click && !info.Readonly)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                mPopupOn = true;
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Color4f v;
                Color4b srcValue = Color4b.FromRgb(0, 0, 0);
                if (multiValue != null)
                {
                    if (!multiValue.HasDifferentValue())
                        srcValue = (Color4b)multiValue.Values[0];
                }
                else
                {
                    srcValue = (Color4b)info.Value;
                }
                if (IsABGR)
                    v = Color4f.FromColor4b(srcValue);
                else
                    v = new Color4f(srcValue);
                var saved = v;
                ImGuiAPI.ColorPicker4("##colorpicker_" + info.Name, (float*)&v,
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
            EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
            return valueChanged;
        }
    }

    public class TtSkeletonBoneIndexPickerEditorAttribute : TtPGCustomValueEditorAttribute
    {
        string mCachedPreviewString = "";
        int mSelectedEncoded = -1;
        List<Animation.Asset.TtSkeletonAsset> mAllSkeletonAssets = null;
        UIProxy.ComboBox mComboBox = new UIProxy.ComboBox();

        // Encode: ((skeletonListIndex + 1) << 16) | boneIndex
        // <= 0 means invalid/none. The +1 offset distinguishes from legacy raw bone indices.
        static int EncodeBoneRef(int skelListIdx, int boneIdx)
        {
            if (skelListIdx < 0 || boneIdx < 0) return -1;
            return ((skelListIdx + 1) << 16) | (boneIdx & 0xFFFF);
        }
        static bool DecodeBoneRef(int encoded, out int skelListIdx, out int boneIdx)
        {
            if (encoded <= 0) { skelListIdx = -1; boneIdx = -1; return false; }
            skelListIdx = (encoded >> 16) - 1;
            boneIdx = encoded & 0xFFFF;
            return true;
        }

        protected override async Thread.Async.TtTask<bool> Initialize_Override()
        {
            var rNames = new List<RName>();
            TtEngine.Instance.AssetMetaManager.TourAssetMetas<Animation.Asset.TtSkeletonAssetAMeta, List<RName>>(
                (rName, meta, list) => { list.Add(rName); return false; }, rNames);

            mAllSkeletonAssets = new List<Animation.Asset.TtSkeletonAsset>();
            foreach (var rName in rNames)
            {
                var asset = await TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(rName);
                if (asset != null)
                {
                    bool contains = false;
                    foreach (var skeletonAsset in mAllSkeletonAssets)
                    {
                        if (skeletonAsset.AssetName == rName)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        mAllSkeletonAssets.Add(asset);
                    }
                }

            }

            mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_HeightLarge;
            mComboBox.WinFlags = ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
            mComboBox.Width = -1;
            await mComboBox.Initialize();

            return await base.Initialize_Override();
        }

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            
            var encoded = System.Convert.ToInt32(info.Value);
            DecodeBoneRef(encoded, out int currentSkelIdx, out int currentBoneIdx);
            newValue = currentBoneIdx;
            if (encoded != mSelectedEncoded)
            {
                mSelectedEncoded = encoded;
                mCachedPreviewString = BuildPreviewString(encoded);
            }

            var preview = mCachedPreviewString;
            if (string.IsNullOrEmpty(preview))
                preview = "None";

            mComboBox.Name = "##SkeletonBonePicker_" + info.Name;
            mComboBox.PreviewValue = preview;

            bool comboChanged = false;
            int comboNewEncoded = encoded;
            var capSkelIdx = currentSkelIdx;
            var capBoneIdx = currentBoneIdx;

            mComboBox.ComboOpenAction = (in Support.TtAnyPointer data) =>
            {
                var comboDrawList = ImGuiAPI.GetWindowDrawList();
                var searchBar = TtEngine.Instance.UIProxyManager["SkeletonBonePickerSearchBar"] as EGui.UIProxy.SearchBarProxy;
                if (searchBar == null)
                {
                    searchBar = new EGui.UIProxy.SearchBarProxy()
                    {
                        InfoText = "Search bone...",
                        Width = -1,
                    };
                    TtEngine.Instance.UIProxyManager["SkeletonBonePickerSearchBar"] = searchBar;
                }
                if (!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                    ImGuiAPI.SetKeyboardFocusHere(0);
                searchBar.OnDraw(in comboDrawList, in Support.TtAnyPointer.Default);

                bool hasSearch = !string.IsNullOrEmpty(searchBar.SearchText);

                if (mAllSkeletonAssets != null)
                {
                    for (int si = 0; si < mAllSkeletonAssets.Count; si++)
                    {
                        var skeletonAsset = mAllSkeletonAssets[si];
                        if (skeletonAsset?.Skeleton == null) continue;

                        var rawName = skeletonAsset.AssetName?.Name ?? "Unknown";
                        var skelDisplayName = rawName.EndsWith(".skt") ? rawName.Substring(0, rawName.Length - 4) : rawName;

                        if (hasSearch)
                        {
                            // Search: show matching bones as flat Selectable items
                            if (skeletonAsset.Skeleton.Limbs == null) continue;
                            var searchLower = searchBar.SearchText.ToLower();
                            foreach (var limb in skeletonAsset.Skeleton.Limbs)
                            {
                                var boneName = limb.Desc?.Name;
                                if (string.IsNullOrEmpty(boneName)) continue;
                                if (!boneName.ToLower().Contains(searchLower)) continue;

                                var bi = limb.Index.Value;
                                var label = $"[{bi}] {boneName} ({skelDisplayName})";
                                bool sel = (si == capSkelIdx && bi == capBoneIdx);
                                if (ImGuiAPI.Selectable(label, ref sel, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                                {
                                    comboNewEncoded = EncodeBoneRef(si, bi);
                                    comboChanged = true;
                                }
                            }
                        }
                        else
                        {
                            // Tree display
                            ImGuiTreeNodeFlags_ skFlags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
                            if (si == capSkelIdx)
                                skFlags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen;
                            bool skOpen = ImGuiAPI.TreeNodeEx(skelDisplayName, skFlags, skelDisplayName);

                            if (skOpen)
                            {
                                foreach (var child in skeletonAsset.Skeleton.Children)
                                {
                                    if (DrawBoneTreeItem(child, si, capSkelIdx, capBoneIdx, skelDisplayName, out int selectedEncoded))
                                    {
                                        comboNewEncoded = selectedEncoded;
                                        comboChanged = true;
                                    }
                                }
                                ImGuiAPI.TreePop();
                            }
                        }
                    }
                }
            };

            ImGuiAPI.SetNextWindowSize(Vector2.Zero, ImGuiCond_.ImGuiCond_Appearing);
            var drawList = ImGuiAPI.GetWindowDrawList();
            mComboBox.OnDraw(in drawList, in Support.TtAnyPointer.Default);

            if (comboChanged)
            {
                mSelectedEncoded = comboNewEncoded;
                mCachedPreviewString = BuildPreviewString(comboNewEncoded);
                DecodeBoneRef(comboNewEncoded, out int changedSkelIdx, out int changedBoneIdx);
                newValue = changedBoneIdx;
                return true;
            }

            return false;
        }

        private unsafe bool DrawBoneTreeItem(Animation.SkeletonAnimation.Skeleton.Limb.ILimb limb, int skeletonListIdx, int currentSkelIdx, int currentBoneIdx, string skeletonName, out int selectedEncoded)
        {
            selectedEncoded = -1;
            if (limb == null) return false;

            var boneName = limb.Desc?.Name ?? "Unnamed";
            var boneIndex = limb.Index.Value;
            var label = $"[{boneIndex}] {boneName} ({skeletonName})";

            bool hasChildren = limb.Children != null && limb.Children.Count > 0;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
            if (!hasChildren)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
            if (skeletonListIdx == currentSkelIdx && boneIndex == currentBoneIdx)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;

            bool open = ImGuiAPI.TreeNodeEx(label, flags, label);

            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                selectedEncoded = EncodeBoneRef(skeletonListIdx, boneIndex);
            }

            if (open)
            {
                if (hasChildren)
                {
                    foreach (var child in limb.Children)
                    {
                        if (DrawBoneTreeItem(child, skeletonListIdx, currentSkelIdx, currentBoneIdx, skeletonName, out int childEncoded))
                        {
                            selectedEncoded = childEncoded;
                        }
                    }
                }
                ImGuiAPI.TreePop();
            }

            return selectedEncoded >= 0;
        }

        private string BuildPreviewString(int encoded)
        {
            if (!DecodeBoneRef(encoded, out int skelIdx, out int boneIdx))
                return "None";

            if (mAllSkeletonAssets != null && skelIdx >= 0 && skelIdx < mAllSkeletonAssets.Count)
            {
                var skelAsset = mAllSkeletonAssets[skelIdx];
                if (skelAsset?.Skeleton?.Limbs != null && boneIdx >= 0 && boneIdx < skelAsset.Skeleton.Limbs.Count)
                {
                    var limb = skelAsset.Skeleton.Limbs[boneIdx];
                    var name = limb.Desc?.Name ?? "Unnamed";
                    var skelName = skelAsset.AssetName?.Name ?? "Unknown";
                    if (skelName.EndsWith(".skt"))
                        skelName = skelName.Substring(0, skelName.Length - 4);
                    return $"[{boneIdx}] {name} ({skelName})";
                }
            }

            return $"[{boneIdx}] Unknown";
        }
    }
}
      
namespace EngineNS
{
    public partial class TtEngine
    {
        public EngineNS.EGui.Controls.PropertyGrid.TtPGTypeEditorManager PGTypeEditorManagerInstance { get; } = new EGui.Controls.PropertyGrid.TtPGTypeEditorManager();
    }
}
