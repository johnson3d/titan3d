using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.Controls.PropertyGrid
{
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
        public virtual void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {

        }
    }
    //public class PGCustomValueEditorBase
    //{
    //    public static void OnDraw(string propName, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
    //    {

    //    }
    //}
    public struct Value2
    {
        public int V1 { get; set; }
        public float V2 { get; set; }
        public string V3 { get; set; }
    }

    public class TestTarget
    {
        public string Name { get; set; }
        public float A { get; set; } = 0;
        public int B { get; set; } = 0;
        public class SubClass
        {
            public float A { get; set; } = 1;
            public int B { get; set; } = 1;
            public class SubClass2
            {
                public float A { get; set; } = 2;
                public int B { get; set; } = 2;
                public Value2 C { get; set; }
            }
            public SubClass2 C { get; set; } = new SubClass2();
            public SubClass2 D { get; set; } = new SubClass2();
            public Value2 E { get; set; }
        }
        public SubClass C { get; set; } = new SubClass();
        public SubClass D { get; set; } = new SubClass();
        public TestTarget E { get; set; } = null;
        public Value2 F { get; set; }
    }
    public partial class PropertyGrid
    {
        public static PropertyGrid Test()
        {
            var pg = new PropertyGrid();
            var targets = new List<object>();
            targets.Add(new TestTarget());
            pg.TargetObjects = targets;
            return pg;
        }
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
                if (mTargetObjects == null || mTargetObjects.Count==0)
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
                        Controls.PropertyGrid.PropertyGrid.SetValue(j, callstack, prop, target, v);
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
                        Controls.PropertyGrid.PropertyGrid.SetValue(j, callstack, prop, target, v);
                    }
                }
                ImGuiAPI.TreePop();
            }   
        }
    }
}
