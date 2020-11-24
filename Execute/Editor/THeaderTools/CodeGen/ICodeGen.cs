using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools.CodeGen
{
    public abstract class ICodeGen
    {
        public virtual void GenCode(string targetDir)
        {
            foreach(var i in CodeGenerator.Instance.CBCollector)
            {
                GenCallBack(targetDir, i);
            }
            foreach (var i in CodeGenerator.Instance.EnumCollector)
            {
                GenEnum(targetDir, i);
            }
            foreach (var i in CodeGenerator.Instance.ClassCollector)
            {
                GenClass(targetDir, i);
            }
        }
        public virtual void GenCallBack(string targetDir, CppCallback desc)
        {

        }
        public virtual void GenEnum(string targetDir, CppEnum desc)
        {

        }
        public virtual void GenClass(string targetDir, CppClass desc)
        {

        }
        public static string GenLine(int nTable, string content)
        {
            string codeline = "";
            for (int i = 0; i < nTable; i++)
            {
                codeline += "\t";
            }
            codeline += content;
            codeline += "\n";
            return codeline;
        }

        public abstract string GenEnumDefine(CppEnum klass);
        public abstract string GetParameterString(CppCallParameters callParameters, bool convertStar2Ref);
    }
}
