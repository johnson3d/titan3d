using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCodeTools.PropertyGen
{
    class UPropertyField
    {
        public string Type;
        public string Name;
        public string Flags;
        public bool DirtyFlags;
        public string GetPropName()
        {
            if (Name.StartsWith("m"))
            {
                return Name.Substring(1);
            }
            return "m" + Name;
        }
    }
    class UPropertyClassDefine : UClassCodeBase
    {
        public bool IsOverrideBitset = true;
        public List<UPropertyField> Properties = new List<UPropertyField>();
        // { "", 1 } };
        public override void GenCode(string dir)
        {
            if (Properties.Count == 0)
                return;
            Properties.Sort((lh, rh) =>
            {
                return lh.Name.CompareTo(rh.Name);
            });
            AddLine("#pragma warning disable 105");
            foreach (var i in Usings)
            {
                AddLine(i);
            }
            NewLine();

            AddLine($"namespace {this.Namespace}");
            PushBrackets();
            {
                AddLine($"partial class {this.Name}");
                PushBrackets();
                {
                    if (IsOverrideBitset)
                    {
                        AddLine($"public new EngineNS.Support.UBitset Bitset = new EngineNS.Support.UBitset({Properties.Count});");
                    }
                    else
                    {
                        AddLine($"public EngineNS.Support.UBitset Bitset = new EngineNS.Support.UBitset({Properties.Count});");
                    }

                    AddLine($"partial void OnPropertyPreChanged(string name, int index, ref EngineNS.Support.UAnyPointer info);");
                    AddLine($"partial void OnPropertyChanged(string name, int index, ref EngineNS.Support.UAnyPointer info);");
                    int index = 0;
                    foreach (var i in Properties)
                    {
                        if (i.Flags != null)
                            AddLine($"[Rtti.Meta(Flags = {i.Flags})]");
                        else
                            AddLine($"[Rtti.Meta]");
                        AddLine($"public {i.Type} {i.GetPropName()}");
                        PushBrackets();
                        {
                            AddLine($"get");
                            PushBrackets();
                            {
                                AddLine($"return {i.Name};");
                            }
                            PopBrackets();

                            AddLine($"set");
                            PushBrackets();
                            {
                                AddLine($"var preInfo = new EngineNS.Support.UAnyPointer();");
                                AddLine($"try");
                                PushBrackets();
                                {
                                    AddLine($"OnPropertyPreChanged(\"{i.GetPropName()}\", {index}, ref preInfo);");
                                    AddLine($"{i.Name} = value;");
                                    AddLine($"Bitset.SetBit({index});");
                                    AddLine($"OnPropertyChanged(\"{i.GetPropName()}\", {index}, ref preInfo);");
                                }
                                PopBrackets();
                                AddLine($"finally");
                                PushBrackets();
                                {
                                    AddLine($"preInfo.Dispose();");
                                }
                                PopBrackets();
                            }
                            PopBrackets();
                        }
                        PopBrackets();

                        index++;
                    }

                    if (IsOverrideBitset)
                        AddLine($"public new readonly static string[] Index2Name = ");
                    else
                        AddLine($"public readonly static string[] Index2Name = ");
                    PushBrackets();
                    {
                        foreach (var i in Properties)
                        {
                            AddLine($"\"{i.GetPropName()}\","); 
                        }
                    }
                    PopBrackets(true);

                    string hashString = "";
                    if (IsOverrideBitset)
                        AddLine($"public new readonly static Dictionary<string, int> Name2Index = new Dictionary<string, int>");
                    else
                        AddLine($"public readonly static Dictionary<string, int> Name2Index = new Dictionary<string, int>");
                    PushBrackets();
                    {
                        index = 0;
                        foreach (var i in Properties)
                        {
                            AddLine($"{{\"{i.GetPropName()}\", {index}}},");
                            index++;

                            hashString += $"{i.Type} {i.GetPropName()};";
                        }
                    }
                    PopBrackets(true);

                    if (IsOverrideBitset)
                        AddLine($"public new readonly static uint PropertyHash = {APHash(hashString)};");
                    else
                        AddLine($"public readonly static uint PropertyHash = {APHash(hashString)};");
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
