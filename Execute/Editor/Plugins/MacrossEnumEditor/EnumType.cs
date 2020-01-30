using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace MacrossEnumEditor
{
    public class EnumType
    { 
        public UInt64 EnumTypePropertyIndex = 0;
        public class EnumTypeProperty: INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            public WeakReference<EnumType> Holder;
            public string EnumName
            {
                get;
                set;
            } = "EnumName";

            public string EnumNote
            {
                get;
                set;
            } = "EnumNote";

            public int EnumIndex
            {
                get;
                set;
            } = 0;

            UInt64 mEnumValue;
            public UInt64 EnumValue
            {
                get => mEnumValue;
                set
                {
                    mEnumValue = value;
                    OnPropertyChanged("EnumValue");
                }
            }

            public EnumTypeProperty()
            {
            }

            public EnumTypeProperty(EnumType enumtype)
            {
                EnumName = EnumName + enumtype.EnumTypePropertyIndex;
                EnumIndex = enumtype.EnumTypePropertys.Count;
                if (enumtype.Bitmask)
                {
                    EnumValue = (UInt64)Math.Pow(2, enumtype.EnumTypePropertyIndex);
                }
                else
                {
                    EnumValue = enumtype.EnumTypePropertyIndex;
                }
                enumtype.EnumTypePropertyIndex++;

                Holder = new WeakReference<EnumType>(enumtype);
            }

            public EnumTypeProperty(EnumTypeProperty etp)
            {
                EnumName = etp.EnumName;
                EnumIndex = etp.EnumIndex;
                EnumNote = etp.EnumNote;
                EnumValue = etp.EnumValue;
            }

            public void Event_Up()
            {
                if (Holder == null)
                    return;

                EnumType enumtype;
                if (Holder.TryGetTarget(out enumtype) == false)
                    return;

                int cur = enumtype.EnumTypePropertys.IndexOf(this);
                if (cur < 0 || cur >= enumtype.EnumTypePropertys.Count)
                    return;

                int index = Math.Max(0, cur-1);
                enumtype.EnumTypePropertys.Remove(this);
                enumtype.EnumTypePropertys.Insert(index, this);

            }

            public void Event_Down()
            {
                if (Holder == null)
                    return;

                EnumType enumtype;
                if (Holder.TryGetTarget(out enumtype) == false)
                    return;

                int cur = enumtype.EnumTypePropertys.IndexOf(this);
                if (cur < 0 || cur >= enumtype.EnumTypePropertys.Count)
                    return;

                int index = Math.Min(enumtype.EnumTypePropertys.Count - 1, cur + 1);
                enumtype.EnumTypePropertys.Remove(this);
                enumtype.EnumTypePropertys.Insert(index, this);

            }

            public void Event_Delete()
            {
                if (Holder == null)
                    return;

                EnumType enumtype;
                if (Holder.TryGetTarget(out enumtype) == false)
                    return;

                enumtype.EnumTypePropertys.Remove(this);


            }
        }

        public List<EnumTypeProperty> EnumTypePropertys = new List<EnumTypeProperty>();

        public bool EnumEditor
        {
            get;
            set;
        } = false;

        public string EnumNote
        {
            get;
            set;
        } = "EnumNote";

        bool mBitmask = false;
        public bool Bitmask
        {
            get => mBitmask;
            set
            {
                if (mBitmask == value)
                    return;

                mBitmask = value;
                foreach (var i in EnumTypePropertys)
                {
                    if (mBitmask)
                    {
                        i.EnumValue = (UInt64)Math.Pow(2, i.EnumValue);
                    }
                    else
                    {
                        double xx = Math.Log(2, i.EnumValue);
                        double xxs = Math.Log(i.EnumValue, 2);
                        i.EnumValue = (UInt64)Math.Log(i.EnumValue, 2);
                    }
                }
            }
        }

        public void AddEnumTypeProperty(EnumTypeProperty etp)
        {
            EnumTypeProperty newetp = new EnumTypeProperty(etp);
            newetp.Holder = new WeakReference<EnumType>(this);

            EnumTypePropertys.Add(newetp);
            EnumTypePropertyIndex++;
        }

        public void LoadEnum(string xmlfile)
        {
            EngineNS.IO.XmlHolder xml = EngineNS.IO.XmlHolder.LoadXML(xmlfile);

            EngineNS.IO.XmlNode node = xml.RootNode.FindNode("Enum");
            if (node == null)
                return;

            EngineNS.IO.XmlAttrib attr = node.FindAttrib("Note");
            if (attr == null)
            {
                EnumNote = "枚举的描述";
            }
            else
            {
                EnumNote = attr.Value;
            }

            attr = node.FindAttrib("Bitmask");
            if (attr == null)
            {
                mBitmask = false;
            }
            else
            {
                mBitmask = attr.Value == "True";
            }

            attr = node.FindAttrib("Editor");
            if (attr == null)
            {
                EnumEditor = false;
            }
            else
            {
                EnumEditor = attr.Value == "True";
            }

            List<EngineNS.IO.XmlNode> nodes = xml.RootNode.FindNodes("EnumProperty");
            EnumTypeProperty[] ets = new EnumTypeProperty[nodes.Count];
            for(int i = 0; i < nodes.Count; i ++)
            {
                var inode = nodes[i];
                EngineNS.IO.XmlAttrib nameattr = inode.FindAttrib("Name");
                if (nameattr == null)
                    continue;

                EngineNS.IO.XmlAttrib indexattr = inode.FindAttrib("Index");
                if (indexattr == null)
                    continue;

                EngineNS.IO.XmlAttrib noteattr = inode.FindAttrib("Note");
                if (noteattr == null)
                    continue;

                int index = int.Parse(indexattr.Value);
                ets[index] = new EnumTypeProperty();
                ets[index].EnumNote = noteattr.Value;
                ets[index].EnumName = nameattr.Value;

                EngineNS.IO.XmlAttrib valueattr = inode.FindAttrib("EnumValue");
                if (valueattr == null)
                {
                    if (mBitmask)
                    {
                        ets[index].EnumValue = (UInt64)Math.Pow(2, i);
                    }
                    else
                    {
                        ets[index].EnumValue = (UInt64)i;
                    }
                }
                else
                {
                    ets[index].EnumValue = Convert.ToUInt64(valueattr.Value);
                }
            }

            for (int i = 0; i < ets.Length; i++)
            {
                AddEnumTypeProperty(ets[i]);
            }
        }

        public void SaveEnum(string file)
        {
            RName rname = RName.GetRName(file);
            string name = rname.GetFileName().Replace("." + rname.GetExtension(), "");
            string str = "/*\n";
            str += EnumNote;
            str += "\n*/\n";

            string directory = file.ToLower().Replace(EngineNS.CEngine.Instance.FileManager.ProjectContent.ToLower(), "");
            directory = directory.Replace(name + ".macross_enum", "");
            directory = directory.Replace("/", ".").TrimEnd('.');
            if (string.IsNullOrEmpty(directory) == false)
            {
                str += "namespace " + directory + "{\n";
            }
            
            str += "[EngineNS.Editor.MacrossMemberAttribute(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]\n";
            str += "[EngineNS.Editor.Editor_MacrossClassAttribute(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]\n";
            str += "[EngineNS.Editor.Editor_MacrossEnumAttribute()]\n";
            if (mBitmask)
            {
                str += "[System.Flags]\n";
                str += "[ EngineNS.Editor.Editor_FlagsEnumSetter()]\n";
            }
            
            str += "public enum " + name + "{\n";
            //XML主要作为描述文件
            EngineNS.IO.XmlHolder xml = EngineNS.IO.XmlHolder.NewXMLHolder("Root", "");
            EngineNS.IO.XmlNode node = xml.RootNode.AddNode("Enum", "", xml);
            node.AddAttrib("Name", name);
            node.AddAttrib("Note", EnumNote);
            node.AddAttrib("Bitmask", Bitmask ? "True" : "False");
            node.AddAttrib("Editor", EnumEditor ? "True" : "False");
            for (int i = 0; i < EnumTypePropertys.Count; i++)
            {
                EngineNS.IO.XmlNode snode = xml.RootNode.AddNode("EnumProperty", "", xml);
                snode.AddAttrib("Name", EnumTypePropertys[i].EnumName);
                snode.AddAttrib("Index", i.ToString());
                snode.AddAttrib("Note", EnumTypePropertys[i].EnumNote);
                snode.AddAttrib("EnumValue", EnumTypePropertys[i].EnumValue.ToString());
                str += EnumTypePropertys[i].EnumName;
                //if (Bitmask)
                //{
                    str += "= " + EnumTypePropertys[i].EnumValue.ToString() + ", //" + EnumTypePropertys[i].EnumNote + "\n";
                //}
                //str += " \\\\" + EnumTypePropertys[i].EnumNote + "\n";
            }
            if (string.IsNullOrEmpty(directory) == false)
            {
                str += "}\n";
            }
            
            str += "}";

            System.IO.FileStream fsWrite = new System.IO.FileStream(file + ".cs", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            fsWrite.Write(buffer, 0, buffer.Length);
            fsWrite.Close();

            EngineNS.IO.XmlHolder.SaveXML(file + ".xml", xml);

            Macross.CodeGenerator codeGenerator = new Macross.CodeGenerator();
            var test = codeGenerator.GenerateAndSaveMacrossCollector(EngineNS.ECSType.Client);
            // 收集所有Macross文件，放入游戏共享工程中
            List<string> macrossfiles = codeGenerator.CollectionMacrossProjectFiles(ECSType.Client); ;
            codeGenerator.GenerateMacrossProject(macrossfiles.ToArray(), EngineNS.ECSType.Client);

            EditorCommon.Program.BuildGameDll(true);
        }
    }
}
