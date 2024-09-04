using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCodeTools.PropertyGen
{
    class UPropertyField
    {
        public string Type;
        public string Name;
        public string Index;
        //public string Flags;
        //public bool DirtyFlags;
        //public string GetPropName()
        //{
        //    if (Name.StartsWith("m"))
        //    {
        //        return Name.Substring(1);
        //    }
        //    return "m" + Name;
        //}
    }
    class UPropertyClassDefine : UClassCodeBase
    {
        public List<UPropertyField> Properties = new List<UPropertyField>();
        // { "", 1 } };
        public void GenCode(string dir, string source)
        {
            if (Properties.Count == 0)
                return;
            //Properties.Sort((lh, rh) =>
            //{
            //    return lh.Name.CompareTo(rh.Name);
            //});
            //AddLine("#pragma warning disable 105");
            //foreach (var i in Usings)
            //{
            //    AddLine(i);
            //}
            //NewLine();

            int numOfProp = -1;
            AddLine($"namespace {this.Namespace}");
            PushBrackets();
            {
                AddLine($"partial class {this.Name}");
                PushBrackets();
                {
                    AddLine($"public void AutoSyncWriteValue(IO.IWriter ar, int index)");
                    PushBrackets();
                    {
                        AddLine($"switch(index)");
                        PushBrackets();
                        {
                            foreach (var i in Properties)
                            {
                                AddLine($"case {i.Index}:");
                                PushBrackets();
                                {
                                    AddLine($"ar.Write({i.Name});");
                                    AddLine($"break;");
                                }
                                PopBrackets();
                            }
                        }
                        PopBrackets();
                    }
                    PopBrackets();

                    numOfProp = -1;

                    AddLine($"public void AutoSyncReadValue(IO.IReader ar, int index, bool bSet)");
                    PushBrackets();
                    {
                        AddLine($"switch(index)");
                        PushBrackets();
                        {
                            foreach (var i in Properties)
                            {
                                var idx = System.Convert.ToInt16(i.Index);
                                if (idx > numOfProp)
                                    numOfProp = idx;
                                AddLine($"case {i.Index}:");
                                PushBrackets();
                                {
                                    AddLine($"{i.Type} tmp;"); 
                                    AddLine($"ar.Read(out tmp);");
                                    AddLine($"if (bSet)");
                                    PushBrackets();
                                    {
                                        AddLine($"{i.Name} = tmp;");
                                    }
                                    PopBrackets();
                                    AddLine($"break;");
                                }
                                PopBrackets();
                            }
                        }
                        PopBrackets();
                    }
                    PopBrackets();

                    AddLine($"EngineNS.Support.TtBitset mFlags = new EngineNS.Support.TtBitset({numOfProp + 1});");
                    AddLine($"public ref EngineNS.Support.TtBitset Flags {{ get => ref mFlags; }}");
                    AddLine($"public bool IsGhostSyncObject {{ get; set; }}");
                }
                PopBrackets();
            }
            PopBrackets();

            

            var file = dir + "/" + FullName + ".prop.cs";
            if (!UPropertyCodeManager.Instance.WritedFiles.Contains(file.Replace("\\", "/").ToLower()))
            {
                UPropertyCodeManager.Instance.WritedFiles.Add(file.Replace("\\", "/").ToLower());
            }

            if (System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode == ClassCode)
                    return;
            }

            System.IO.File.WriteAllText(file, ClassCode);
        }
    }
}
