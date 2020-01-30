using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Resources
{
    public class ResourceEditorContext
    {
        public string EditorKeyName
        {
            get;
            protected set;
        }

        public ResourceInfo ResInfo
        {
            get;
            protected set;
        }

        public object PropertyShowValue;
        public Action SaveAction;
        public ResourceEditorContext(string editorKeyName, ResourceInfo resInfo)
        {
            EditorKeyName = editorKeyName;
            ResInfo = resInfo;
        }
    }
}
