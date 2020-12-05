using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.UIEditor
{
    public class FormEditor
    {
        public FormStructureEditor StructureEditor = new FormStructureEditor();
        public unsafe void OnDraw()
        {
            var bOpen = CppBool.FromBoolean(true);
            if (ImGuiAPI.Begin($"FormEditor", &bOpen, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                StructureEditor.OnDraw();
            }
            ImGuiAPI.End();

            EditableFormData.Instance.EditorDrawForm?.DrawForm();
        }
    }
    public class FormStructureEditor
    {
        private CppBool mVisible = CppBool.FromBoolean(true);
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = CppBool.FromBoolean(value);
            }
        }
        public Controls.PropertyGrid.PropertyGrid PGrid = new Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            fixed (CppBool* pVisible = &mVisible)
            {
                if (ImGuiAPI.Begin($"Structures", pVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var clientSize = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                    if (ImGuiAPI.BeginChild("ContentWindow", ref clientSize, CppBool.FromBoolean(false), ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                    {
                        var sz = new Vector2(0, 0);
                        if(ImGuiAPI.Button("GenCode", ref sz))
                        {
                            EditableFormData.Instance.CurrentForm.GenCode();
                        }
                        ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
                        ImGuiAPI.TreeNodeEx("WinForm", flags, "WinForm");
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            PGrid.SingleTarget = EditableFormData.Instance.CurrentForm;
                            mMenuType = EMenuType.None;
                        }
                        if (ImGuiAPI.TreeNode("ElementsLayout", "ElementsLayout"))
                        {
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                            {
                                mMenuType = EMenuType.Elements;
                                MenuPopContainer = EditableFormData.Instance.CurrentForm;
                            }

                            DrawStructureElements();

                            ImGuiAPI.TreePop();
                        }

                        OnDrawMenu();
                    }
                    ImGuiAPI.EndChild();
                }
                ImGuiAPI.End();
            }

            PGrid.OnDraw();
        }
        public IContainer MenuPopContainer = null;
        public class DraggingItemDesc
        {
            private IElement Element = null;
            private List<IElement> Container = null;
            private bool Dragging = false;
            public IElement DraggingElement
            {
                get => Element;
            }
            public bool IsDragging
            {
                get => Dragging;
            }
            public void PreDragging(IElement e, List<IElement> c)
            {
                if (Dragging)
                {
                    return;
                }
                Element = e;
                Container = c;
                Dragging = false;
            }
            public void BeginDragging()
            {
                if (Container != null && Element != null)
                {
                    Container.Remove(Element);
                }
                Dragging = true;
            }
            public void ReleaseDragging(List<IElement> target, int index)
            {
                if(Dragging)
                {
                    if (target.Contains(Element) == false)
                    {
                        if (index < 0)
                            target.Add(Element);
                        else
                            target.Insert(index, Element);
                    }
                }                
                Element = null;
                Container = null;
                Dragging = false;
            }
        }

        public DraggingItemDesc DragingItem = new DraggingItemDesc();
        public unsafe void DrawStructureElements()
        {
            if (DragingItem.DraggingElement != null && DragingItem.IsDragging == false)
            {
                if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, 5))
                {
                    DragingItem.BeginDragging();
                }
            }

            var form = EditableFormData.Instance.CurrentForm;
            form.OnDrawChildren(form);
        }
        public enum EMenuType
        {
            None,
            Elements,
        }
        public EMenuType mMenuType = EMenuType.None;
        private void OnDrawMenu()
        {
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                bool isShow = false;
                switch (mMenuType)
                {
                    case EMenuType.Elements:
                        isShow = DrawMenu_Elements();
                        break;
                    default:
                        break;
                }
                if (isShow == false)
                {
                    mMenuType = EMenuType.None;
                }
                ImGuiAPI.EndPopup();
            }
        }
        internal uint CurrentElementIndex = 0;
        public unsafe bool DrawMenu_Elements()
        {
            if (mMenuType == EMenuType.None)
                return false;
            if (ImGuiAPI.BeginMenu("AddElements", CppBool.FromBoolean(true)))
            {
                if (ImGuiAPI.MenuItem($"Text", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
                {
                    if (MenuPopContainer != null)
                    {
                        var elem = new Elements.Text();
                        elem.Name = "Text" + CurrentElementIndex++;
                        elem.TextValue = elem.Name;
                        MenuPopContainer.Children.Add(elem);
                        mMenuType = EMenuType.None;
                    }
                }
                if (ImGuiAPI.MenuItem($"InputText", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
                {
                    if (MenuPopContainer != null)
                    {
                        var elem = new Elements.InputText();
                        elem.Name = "InputText" + CurrentElementIndex++;
                        elem.TextValue = elem.Name;
                        MenuPopContainer.Children.Add(elem);
                        mMenuType = EMenuType.None;
                    }
                }
                if (ImGuiAPI.MenuItem($"Separator", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
                {
                    if (MenuPopContainer != null)
                    {
                        var elem = new Elements.Separator();
                        elem.Name = "Separator" + CurrentElementIndex++;
                        MenuPopContainer.Children.Add(elem);
                        mMenuType = EMenuType.None;
                    }
                }
                if (ImGuiAPI.BeginMenu($"Container", CppBool.FromBoolean(true)))
                {
                    if (ImGuiAPI.MenuItem($"Columns", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
                    {
                        if (MenuPopContainer != null)
                        {
                            var elem = new Elements.Columns();
                            elem.Name = "Columns" + CurrentElementIndex++;
                            MenuPopContainer.Children.Add(elem);
                            mMenuType = EMenuType.None;
                        }
                    }
                    ImGuiAPI.EndMenu();
                }
                ImGuiAPI.EndMenu();
            }
            return true;
        }
    }
}
