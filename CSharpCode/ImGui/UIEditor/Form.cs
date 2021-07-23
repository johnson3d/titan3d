using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.UIEditor
{
    public interface IElement
    {
        bool VisibleWhenInit { get; set; }
        string Name { get; set; }
        void GenCode(Form form);
        void GenElementDataCode(Form form);
        string GetElementDataName();
        void GenInitFunction(Form form);
    }
    public class ControlBase : IElement
    {
        public bool VisibleWhenInit { get; set; } = true;
        public string Name { get; set; }
        public int LayoutIndex { get; set; } = 0;
        public int LayoutOffset { get; set; } = 0;
        public string GetElementDataName()
        {
            return $"mElement_{Name}";
        }
        public virtual void GenCode(Form form)
        {
            
        }
        public virtual void GenElementDataCode(Form form)
        {
            string vis = (VisibleWhenInit) ? "true" : "false";
            form.AddLine($"public {typeof(UIElementData).FullName} {GetElementDataName()} {{ get; }}= new {typeof(UIElementData).FullName}({vis}, \"{Name}\", typeof({GetType().FullName}))");
            form.PushBrackets();
            form.AddLine($"LayoutIndex = {LayoutIndex}, LayoutOffset = {LayoutOffset}");
            form.PopBrackets(true);
        }
        public virtual void GenInitFunction(Form form)
        {

        }
    }
    public class IContainer : IElement
    {
        public bool VisibleWhenInit { get; set; } = true;
        public string Name { get; set; }
        public List<IElement> Children = new List<IElement>();
        public string GetElementDataName()
        {
            return $"mElement_{Name}";
        }
        public virtual void GenCode(Form form)
        {
            BeginCode(form);
            foreach(var i in Children)
            {
                i.GenCode(form);
            }
            EndCode(form);
        }
        public virtual void BeginCode(Form form)
        {

        }
        public virtual void EndCode(Form form)
        {

        }
        public virtual void GenElementDataCode(Form form)
        {
            string vis = (VisibleWhenInit) ? "true" : "false";
            form.AddLine($"public {typeof(UIElementData).FullName} mElement_{Name} {{ get; }}= new {typeof(UIElementData).FullName}({vis}, \"{Name}\", typeof({GetType().FullName}));");
            foreach(var i in Children)
            {
                i.GenElementDataCode(form);
            }
        }
        public virtual void GenInitFunction(Form form)
        {
            foreach (var i in Children)
            {
                i.GenInitFunction(form);
            }
        }
        public virtual unsafe void OnDraw(Form form)
        {
            OnDrawChildren(form);
        }
        public unsafe void OnDrawChildren(Form form)
        {
            var structureEditor = EditableFormData.Instance.Editor.StructureEditor;
            var DragingItem = EditableFormData.Instance.Editor.StructureEditor.DragingItem;
            bool LBDown = ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            for (int i = 0; i < Children.Count; i++)
            {
                var cur = Children[i];
                var container = cur as IContainer;
                if (container != null)
                {
                    var bShow = ImGuiAPI.TreeNode(cur.Name, cur.Name);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        structureEditor.mMenuType = FormStructureEditor.EMenuType.Container;
                        structureEditor.PopTarget.SetTarget(container, Children);
                    }
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        structureEditor.PGrid.Target = cur;
                        structureEditor.mMenuType = FormStructureEditor.EMenuType.None;
                    }
                    var bActive = ImGuiAPI.IsItemActive();
                    var hover1 = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
                    if (structureEditor.DragingItem.IsDragging)
                    {
                        ImGuiAPI.SameLine(0, -1);
                        ImGuiAPI.Text("PushTo");
                    }
                    var hover2 = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);

                    bool drawDragging = false;
                    bool drawDraggingInContainer = false;
                    if (hover1)
                    {
                        if (DragingItem.DraggingElement != null && DragingItem.DraggingElement!=this)
                        {
                            if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                DragingItem.ReleaseDragging(Children, i + 1);
                            }
                            else if (DragingItem.IsDragging)
                            {
                                drawDragging = true;
                            }
                        }
                        else
                        {
                            if (LBDown)
                            {
                                DragingItem.PreDragging(cur, Children);
                            }
                        }
                    }
                    else if (hover2)
                    {
                        if (DragingItem.IsDragging)
                        {
                            if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                DragingItem.ReleaseDragging(container.Children, -1);
                            }
                            else
                            {
                                drawDraggingInContainer = true;
                            }
                        }
                    }
                    if (bShow)
                    {
                        container.OnDraw(form);
                        ImGuiAPI.TreePop();
                    }
                    if (drawDragging)
                    {
                        ImGuiAPI.TreeNodeEx(DragingItem.DraggingElement.Name, flags, $"<-{DragingItem.DraggingElement.Name}");
                    }
                    if (drawDraggingInContainer)
                    {
                        ImGuiAPI.TreePush("##Dragging2Container");
                        ImGuiAPI.TreeNodeEx(DragingItem.DraggingElement.Name, flags, $"{DragingItem.DraggingElement.Name}");
                        ImGuiAPI.TreePop();
                    }
                }
                else
                {
                    ImGuiAPI.TreeNodeEx(cur.Name, flags, cur.Name);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        structureEditor.mMenuType = FormStructureEditor.EMenuType.Control;
                        structureEditor.PopTarget.SetTarget(cur, Children);
                    }
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        structureEditor.PGrid.Target = cur;
                        structureEditor.mMenuType = FormStructureEditor.EMenuType.None;
                    }
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        if (DragingItem.DraggingElement != null)
                        {
                            if (ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                DragingItem.ReleaseDragging(Children, i + 1);
                            }
                            else if (DragingItem.IsDragging)
                            {
                                ImGuiAPI.TreeNodeEx(DragingItem.DraggingElement.Name, flags, $"<-{DragingItem.DraggingElement.Name}");
                            }
                        }
                        else
                        {
                            if (LBDown)
                            {
                                DragingItem.PreDragging(cur, Children);
                            }
                        }
                    }
                }
            }
        }
    }
    public class Form : IContainer
    {
        public Form()
        {
            Name = "Winform" + EditableFormData.Instance.Editor.StructureEditor.CurrentElementIndex++;
        }
        public string NameSpace { get; set; } = "EngineNS.EditorGUI";
        public string ParentClass { get; set; } = typeof(UIFormBase).FullName;
        public enum ELineMode
        {
            TabKeep,
            TabPush,
            TabPop,
        }
        public ELineMode TestEnumBinding
        {
            get;
            set;
        }
        private int NumOfTab = 0;
        public int GetTabNum()
        {
            return NumOfTab;
        }
        public void PushTab()
        {
            NumOfTab++;
        }
        public void PopTab()
        {
            NumOfTab--;
        }
        public void PushBrackets()
        {
            AddLine("{");
            NumOfTab++;
        }
        public void PopBrackets(bool semicolon = false)
        {
            NumOfTab--;
            if(semicolon)
                AddLine("};");
            else
                AddLine("}");
        }
        public string FormCode = "";
        public string AddLine(string code, ELineMode mode = ELineMode.TabKeep)
        {
            string result = "";
            for (int i = 0; i < NumOfTab; i++)
            {
                result += '\t';
            }
            result += code + '\n';

            FormCode += result;
            return result;
        }
        public Rtti.UTypeDesc BindType
        {
            get;
            set;
        } = Rtti.UTypeDesc.TypeOf(typeof(Form));
        internal static string BindTargetName = "_mBindTarget";
        public void GenCode()
        {
            FormCode = "";
            NumOfTab = 0;

            AddLine($"using System;");
            AddLine($"using System.Collections.Generic;");
            AddLine("");

            AddLine($"namespace {NameSpace}");
            {
                PushBrackets();

                AddLine($"public partial class {Name} : {ParentClass}");
                {
                    PushBrackets();

                    AddLine($"public {Name}()");
                    {
                        PushBrackets();
                        GenInitFunction(this);
                        PopBrackets();
                    }

                    if (BindType != null)
                    {
                        AddLine($"public {BindType.FullName} {BindTargetName} {{ get; set; }} = null;");
                        AddLine($"public override object BindTarget");
                        {
                            PushBrackets();
                            AddLine($"get{{ return {BindTargetName}; }}");
                            AddLine($"set{{ {BindTargetName} = value as {BindType.FullName}; }}");
                            PopBrackets();
                        }
                    }
                    GenElementDataCode();

                    AddLine($"public override unsafe void OnDraw()");
                    {
                        PushBrackets();
                        foreach (var i in Children)
                        {
                            AddLine($"if (mElement_{ i.Name}.Visible)");
                            {
                                PushBrackets();
                                i.GenCode(this);
                                PopBrackets();
                            }
                        }
                        PopBrackets();
                    }

                    PopBrackets();
                }
                PopBrackets();
            }

            if (EditableFormData.Instance.CSharpCodeFile != null)
            {
                System.IO.File.WriteAllText(EditableFormData.Instance.CSharpCodeFile, FormCode);
            }
            var cr = EngineNS.EGui.CSharpCompiler.CompilerImGuiCode(EditableFormData.Instance.CSharpCodeFile);
            if (cr != null)
            {
                var kls = cr.CompiledAssembly.GetType($"{NameSpace}.{Name}");
                if (kls != null)
                {
                    EditableFormData.Instance.EditorDrawForm = Rtti.UTypeDescManager.CreateInstance(kls) as UIFormBase;
                    EditableFormData.Instance.EditorDrawForm.BindTarget = this;
                }
            }
        }
        private void GenElementDataCode()
        {
            foreach (var i in Children)
            {
                i.GenElementDataCode(this);
            }
        }
    }
}
