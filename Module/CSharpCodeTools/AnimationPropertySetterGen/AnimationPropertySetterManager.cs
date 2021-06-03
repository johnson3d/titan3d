using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCodeTools.AnimationPropertySetterGen
{
    class UAnimationPropertySetterManager : UCodeManagerBase
    {
        public static UAnimationPropertySetterManager Instance = new UAnimationPropertySetterManager();
        public Dictionary<string, AnimationPropertySetterClassDefine> ClassDefines = new Dictionary<string, AnimationPropertySetterClassDefine>();

    }
}
