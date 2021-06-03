using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.UIEditor.Elements
{
    public class Columns : IContainer
    {
        public int NumOfCols { get; set; } = 2;
        public bool Border { get; set; } = true;
        
        #region Test
        public List<int> TestList
        {
            get;
        } = new List<int>();
        public class TDict_Value
        {
            public int A { get; set; } = 1;
            public float B { get; set; } = 2;
        }
        public Dictionary<int, TDict_Value> TestDict
        {
            get;
        } = new Dictionary<int, TDict_Value>();
        public Dictionary<int, int> ColumnsOffset
        {
            get;
        } = new Dictionary<int, int>();
        [Controls.PropertyGrid.Color4PickerEditorAttribute()]
        public Vector4 TestColor1 { get; set; } = new Vector4(1, 1, 1, 1);
        [Controls.PropertyGrid.Color3PickerEditorAttribute()]
        public Vector3 TestColor2 { get; set; } = new Vector3(1, 1, 1);
        #endregion

        public Columns()
        {
            ColumnsOffset[0] = 0;
            ColumnsOffset[1] = 500;

            TestList.Add(10);
            TestList.Add(11);
            TestList.Add(100);
            TestList.Add(1001);
        }
        public override void GenCode(Form form)
        {
            BeginCode(form);
            //form.AddLine($"foreach(var c in {GetElementDataName()}.ColumnsOffset)");
            //{
            //    form.PushBrackets();
            //    form.AddLine($"ImGuiAPI.SetColumnOffset(c.Key, c.Value);");
            //    form.PopBrackets();
            //}
            foreach (var i in Children)
            {
                form.AddLine($"ImGuiAPI.GotoColumns({i.GetElementDataName()}.LayoutIndex);");
                form.AddLine($"if (mElement_{ i.Name}.Visible)");
                {
                    form.PushBrackets();
                    //form.AddLine($"ImGuiAPI.SetColumnOffset(-1, {i.GetElementDataName()}.LayoutOffset);");
                    i.GenCode(form);
                    form.PopBrackets(); 
                }
            }
            EndCode(form);
        }
        public override void BeginCode(Form form)
        {
            ImGuiAPI.Columns(NumOfCols, Name, Border);
            string b = Border ? "true" : "false";
            form.AddLine($"ImGuiAPI.Columns({NumOfCols}, \"{Name}\", {b});");
            form.PushBrackets();
        }
        public override void EndCode(Form form)
        {
            form.PopBrackets();
            form.AddLine($"ImGuiAPI.Columns(1, null, true);");
            //form.AddLine($"ImGuiAPI.SetColumnOffset(-1, 0);");
        }
        public override void GenElementDataCode(Form form)
        {
            string vis = (VisibleWhenInit) ? "true" : "false";
            form.AddLine($"public {typeof(ColumnsElementData).FullName} {GetElementDataName()} {{ get; }}= new {typeof(ColumnsElementData).FullName}({vis}, \"{Name}\", typeof({GetType().FullName}));");
            
            foreach (var i in Children)
            {
                i.GenElementDataCode(form);
            }
        }
        public override void GenInitFunction(Form form)
        {
            foreach (var i in ColumnsOffset)
            {
                form.AddLine($"{GetElementDataName()}.ColumnsOffset[{i.Key}] = {i.Value};");
            }
            foreach (var i in Children)
            {
                i.GenInitFunction(form);
            }
        }
    }
}
