using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Macross
{
    public interface IMacrossType
    {
        
    }
    // 标识类型是否为生成的Macross类型
    public class MacrossTypeClassAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class MacrossFieldAttribute : Attribute
    {
        public int Offset = -1;
        public int Size = -1;
        public Type Type;
        public string Name;
    }
}
