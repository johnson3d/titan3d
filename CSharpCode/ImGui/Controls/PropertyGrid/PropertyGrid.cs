using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public partial class PropertyGrid
    {
        List<object> mTargetObjects;
        public List<object> TargetObjects
        {
            get { return mTargetObjects; }
            set
            {
                mTargetObjects = value;
            }
        }
        public object Tag
        {
            get;
            set;
        }
        public object SingleTarget
        {
            get
            {
                if (mTargetObjects == null || mTargetObjects.Count == 0)
                    return null;
                return mTargetObjects[0];
            }
            set
            {
                var targets = new List<object>();
                targets.Add(value);
                TargetObjects = targets;
            }
        }
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
        public bool HideInPG = false;
        public bool ReadOnly = false;
        public bool UserDraw = true;
        protected bool FullRedraw = false;
        public bool IsFullRedraw
        {
            get => FullRedraw;
        }
        public virtual void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {

        }
    }
    public class PGTypeEditorAttribute : PGCustomValueEditorAttribute
    {
        public string AssemblyFilter = null;
        public bool ExcludeSealed = false;
        public bool ExcludeValueType = false;
        public Rtti.UTypeDesc BaseType;
        public PGTypeEditorAttribute()
        {
            BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(void).FullName);
        }
        protected static EGui.Controls.TypeSelector TypeSlt = new EGui.Controls.TypeSelector();
        public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, EGui.Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            var sz = new Vector2(0, 0);
            var bindType = EGui.UIEditor.EditableFormData.Instance.CurrentForm.BindType;
            if (bindType == null)
                return;
            var props = bindType.SystemType.GetProperties();
            ImGuiAPI.SetNextItemWidth(-1);
            TypeSlt.AssemblyFilter = AssemblyFilter;
            TypeSlt.ExcludeSealed = ExcludeSealed;
            TypeSlt.ExcludeValueType = ExcludeValueType;
            TypeSlt.BaseType = BaseType;
            var typeStr = value as string;
            TypeSlt.SelectedType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeStr);
            if (TypeSlt.OnDraw(-1, 12))
            {
                foreach (var i in props)
                {
                    var v = TypeSlt.SelectedType;
                    foreach (var j in pg.TargetObjects)
                    {
                        EGui.Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, v);
                    }
                }
            }
        }
    }
    
    public class PGTypeEditorManager
    {
        public static PGTypeEditorManager Instance { get; } = new PGTypeEditorManager();

        Dictionary<System.Type, PGCustomValueEditorAttribute> mTypeEditors = new Dictionary<Type, PGCustomValueEditorAttribute>();
        public PGCustomValueEditorAttribute GetEditorType(Type type)
        {
            PGCustomValueEditorAttribute result;
            if (mTypeEditors.TryGetValue(type, out result))
                return result;
            return null;
        }
        public void RegTypeEditor(Type type, PGCustomValueEditorAttribute editorType)
        {
            mTypeEditors[type] = editorType;
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
        public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            if (ImGuiAPI.TreeNode(TName.FromString2("##colorpicker_switch_", prop.Name).ToString()))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                var v = (Vector3)value;
                var saved = v;
                ImGuiAPI.ColorPicker3(TName.FromString2("##colorpicker_", prop.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview);
                if (v != saved)
                {
                    foreach (var j in pg.TargetObjects)
                    {
                        Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, v);
                    }
                }
                ImGuiAPI.TreePop();
            }
        }
    }
    public class Color4PickerEditorAttribute : ColorEditorBaseAttribute
    {
        public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            if (ImGuiAPI.TreeNode(TName.FromString2("##colorpicker_switch_", prop.Name).ToString()))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                var v = (Vector4)value;
                var saved = v;
                ImGuiAPI.ColorPicker4(TName.FromString2("##colorpicker_", prop.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    foreach (var j in pg.TargetObjects)
                    {
                        Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, v);
                    }
                }
                ImGuiAPI.TreePop();
            }   
        }
    }
    public class UByte4ToColor4PickerEditorAttribute : ColorEditorBaseAttribute
    {
        public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            if (ImGuiAPI.TreeNode(TName.FromString2("##colorpicker_switch_", prop.Name).ToString()))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                var v = new Color4((UInt32)value);
                var saved = v;
                ImGuiAPI.ColorPicker4(TName.FromString2("##colorpicker_", prop.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    var rgba = v.ToArgb();
                    foreach (var j in pg.TargetObjects)
                    {
                        Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, rgba);
                    }
                }
                ImGuiAPI.TreePop();
            }
        }
    }
}
