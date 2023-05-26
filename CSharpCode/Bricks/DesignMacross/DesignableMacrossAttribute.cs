using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross
{
    //本代码页用来描述可Designa的变量或者函数相关的Attribute
    
    /// <summary>
    /// 用在变量或函数上，2
    /// </summary>
    public class DesignableAttribute : Attribute
    {
        public Type TypeBeDesigned;
        public string ShowName;
        public DesignableAttribute(Type type)
        {
            TypeBeDesigned = type;
            ShowName = type.Name;
        }
        public DesignableAttribute(Type type, string showName)
        {
            TypeBeDesigned = type;
            ShowName = showName;
        }
    }
}
