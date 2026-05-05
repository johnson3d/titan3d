using System;
using System.Collections.Generic;

namespace CSharpCodeTools.PropertyGen
{
    class UPropertyField
    {
        public string Type;
        public string Name;
        public string Index;
    }

    class UPropertyClassDefine : UClassCodeBase
    {
        public List<UPropertyField> Properties = new List<UPropertyField>();

        public void GenCode(string dir, string source)
        {
            if (Properties.Count == 0)
                return;

            int numOfProp = -1;
            AddLine($"namespace {Namespace}");
            PushBrackets();
            {
                AddLine($"partial class {Name}");
                PushBrackets();
                {
                    AddLine("public void AutoSyncWriteValue(IO.IWriter ar, int index)");
                    PushBrackets();
                    {
                        AddLine("switch(index)");
                        PushBrackets();
                        {
                            foreach (var i in Properties)
                            {
                                AddLine($"case {i.Index}:");
                                PushBrackets();
                                {
                                    AddLine($"ar.Write({i.Name});");
                                    AddLine("break;");
                                }
                                PopBrackets();
                            }
                        }
                        PopBrackets();
                    }
                    PopBrackets();

                    AddLine("public void AutoSyncReadValue(IO.IReader ar, int index, bool bSet)");
                    PushBrackets();
                    {
                        AddLine("switch(index)");
                        PushBrackets();
                        {
                            foreach (var i in Properties)
                            {
                                var idx = Convert.ToInt16(i.Index);
                                if (idx > numOfProp)
                                    numOfProp = idx;
                                AddLine($"case {i.Index}:");
                                PushBrackets();
                                {
                                    AddLine($"{i.Type} tmp;");
                                    AddLine("ar.Read(out tmp);");
                                    AddLine("if (bSet)");
                                    PushBrackets();
                                    {
                                        AddLine($"{i.Name} = tmp;");
                                    }
                                    PopBrackets();
                                    AddLine("break;");
                                }
                                PopBrackets();
                            }
                        }
                        PopBrackets();
                    }
                    PopBrackets();

                    AddLine($"EngineNS.Support.TtBitset mFlags = new EngineNS.Support.TtBitset({numOfProp + 1});");
                    AddLine("public ref EngineNS.Support.TtBitset Flags { get => ref mFlags; }");
                    AddLine("public bool IsGhostSyncObject { get; set; }");
                }
                PopBrackets();
            }
            PopBrackets();

            var file = dir + "/" + FullName + ".prop.cs";
            var normalizedFile = file.Replace("\\", "/").ToLower();
            if (!UPropertyCodeManager.Instance.WritedFiles.Contains(normalizedFile))
            {
                UPropertyCodeManager.Instance.WritedFiles.Add(normalizedFile);
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
