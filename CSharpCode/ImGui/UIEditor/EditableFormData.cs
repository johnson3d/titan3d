using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIEditor
{
    public class EditableFormData
    {
        public static EditableFormData Instance = new EditableFormData();
        public FormEditor Editor = new FormEditor(); 
        public Form CurrentForm { get; set; }
        public string CSharpCodeFile = null;
        public UIFormBase EditorDrawForm = null;
        public void NewForm()
        {
            CurrentForm = new Form();
        }
    }
}
