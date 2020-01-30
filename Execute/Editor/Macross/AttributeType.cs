using EngineNS.IO;
using EngineNS.IO.Serializer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Macross
{
    [EngineNS.Rtti.MetaClass]
    public class AttributeConstructorParamer : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public class Element
        {
            public string ShowName;
            public AttributeConstructorParamer VarObject;
        }
        public class VarPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
        {
            public override string GetName(object arg)
            {
                var elem = arg as Element;
                return elem.ShowName;
            }
            public override Type GetUIType(object arg)
            {
                var elem = arg as Element;
                if (elem.VarObject == null || elem.VarObject.Value == null)
                    return typeof(object);

                Type type = elem.VarObject.Value.GetType();
                return elem.VarObject.Value.GetType();
            }
            public override object GetValue(object arg)
            {
                var elem = arg as Element;
                return elem.VarObject.Value;
            }
            public override void SetValue(object arg, object val)
            {
                var elem = arg as Element;

                elem.VarObject.Value = val;
            }
        }
        [Browsable(false)]
        public Type Type
        {
            get;
            set;
        }
        [EngineNS.Rtti.MetaData]
        [Browsable(false)]
        public string VariableName
        {
            get;
            set;
        }

        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public int Index
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(VarPropertyUIProvider))]
        public Object Name
        {
            get
            {
                var elem = new Element();
                elem.VarObject = this;
                elem.ShowName = VariableName;
                return elem;
            }
        }
        //[EngineNS.Rtti.MetaData]
        [Browsable(false)]
        public Object Value
        {
            get;
            set;
        }

        public string ToValueString()
        {
            if (Value == null)
                return "null";

            if (Value.GetType().IsSubclassOf(typeof(Enum)))
            {
                string typename = Value.GetType().FullName;
                typename = typename.Replace("+", ".");
                return typename + "." + Value.ToString();
            }

            return Value.ToString();
        }

        public bool LoadXnd(EngineNS.IO.XndNode node)
        {
            var attr = node.FindAttrib("AttributeConstructorParamer" + Index);
            if (attr == null)
                return false;
 
            attr.BeginRead();
            attr.ReadMetaObject(this);
            attr.EndRead();

            attr = node.FindAttrib("AttributeConstructorParamer_Value" + Index);
            if (attr == null)
                return false;
            attr.BeginRead();
            string assembly;
            attr.Read(out assembly);

            string typename;
            attr.Read(out typename);

            try
            {
                Type type = Type.GetType(typename + ", " + assembly);

                if (type.Equals(typeof(string)))
                {
                    string value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(byte)) || type.Equals(typeof(char)))
                {
                    byte value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(bool)))
                {
                    bool value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(int)))
                {
                    Int32 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(Int16)))
                {
                    Int16 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(Int32)))
                {
                    Int32 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(Int64)))
                {
                    Int64 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(uint)))
                {
                    UInt32 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(UInt16)))
                {
                    UInt16 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(UInt32)))
                {
                    UInt32 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(UInt64)))
                {
                    UInt64 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(short)))
                {
                    UInt32 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(long)))
                {
                    Int64 value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.Equals(typeof(double)))
                {
                    double value;
                    attr.Read(out value);
                    Value = value;
                }
                else if (type.IsSubclassOf(typeof(Enum)))
                {
                    UInt64 value;
                    attr.Read(out value);
                   
                    Value = Enum.Parse(type, value.ToString());
                }

                else
                {
                    //TODO. Type..
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("System.Exception： " + e);
            }
            attr.EndRead();
            return true;
        }

        public void Save2Xnd(EngineNS.IO.XndNode node)
        {
            //var 
            var attr = node.AddAttrib("AttributeConstructorParamer" + Index);
            attr.BeginWrite();
            attr.WriteMetaObject(this);
            attr.EndWrite();

            attr = node.AddAttrib("AttributeConstructorParamer_Value" + Index);
            attr.BeginWrite();
            if (Value == null)
            {
                attr.Write("");
                attr.Write("System.String");
                attr.Write("null");
                attr.EndWrite();
                return;
            }

            try
            {
                Type type = Value.GetType();

                attr.Write(type.Assembly.FullName);
                attr.Write(type.FullName);
                if (type.Equals(typeof(string)))
                {
                    attr.Write(Value as string);
                }
                else if (type.Equals(typeof(char)) || type.Equals(typeof(byte)))
                {
                    attr.Write(Convert.ToByte(Value));
                }
                else if (type.Equals(typeof(bool)))
                {
                    attr.Write(Convert.ToBoolean(Value));
                }
                else if (type.Equals(typeof(int)))
                {
                    attr.Write(Convert.ToInt32(Value));
                }
                else if (type.Equals(typeof(Int16)))
                {
                    attr.Write(Convert.ToInt16(Value));
                }
                else if (type.Equals(typeof(Int32)))
                {
                    attr.Write(Convert.ToInt32(Value));
                }
                else if (type.Equals(typeof(Int64)))
                {
                    attr.Write(Convert.ToInt64(Value));
                }
                else if (type.Equals(typeof(uint)))
                {
                    attr.Write(Convert.ToUInt32(Value));
                }
                else if (type.Equals(typeof(UInt16)))
                {
                    attr.Write(Convert.ToUInt16(Value));
                }
                else if (type.Equals(typeof(UInt32)))
                {
                    attr.Write(Convert.ToUInt32(Value));
                }
                else if (type.Equals(typeof(UInt64)))
                {
                    attr.Write(Convert.ToUInt64(Value));
                }
                else if (type.Equals(typeof(short)))
                {
                    attr.Write(Convert.ToUInt32(Value));
                }
                else if (type.Equals(typeof(long)))
                {
                    attr.Write(Convert.ToInt64(Value));
                }
                else if (type.Equals(typeof(double)))
                {
                    attr.Write(Convert.ToDouble(Value));
                }
                else if (type.IsSubclassOf(typeof(Enum)))
                {
                    attr.Write(Convert.ToUInt64(Value));
                }
                else
                {
                    //TODO. Type..
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("System.Exception： " + e);
            }


            attr.EndWrite();

        }
    }

    [EngineNS.Rtti.MetaClass]
    public class Constructors : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [Browsable(false)]
        public AttributeType Host
        {
            get;
            set;
        }

        [Browsable(false)]
        public int ParamsCount
        {
            get;
            set;
        } = 0;

        public Constructors(AttributeType at)
        {
            Host = at;
        }

        public Constructors()
        {
        }

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public List<AttributeConstructorParamer> Paramers
        {
            get;
            set;
        } = new List<AttributeConstructorParamer>();

        bool mEnable = false;
        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [DisplayName("启用")]
        public bool Enable
        {
            get
            {
                return mEnable;
            }
            set
            {
                if (value && Host != null)
                {
                    foreach (var i in Host.ConstructorParamers)
                    {
                        i.Enable = false;
                    }
                }

                mEnable = value;
                OnPropertyChanged("Enable");

            }

        }
    }

    [EngineNS.Rtti.MetaClass]
    public class AttributeType : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public string AttributeName
        {
            get;
            set;
        } = "";
        [EngineNS.Rtti.MetaData]
        public string Description
        {
            get;
            set;
        } = "";

        //[EngineNS.Rtti.MetaData]
        //public string TypeName
        //{
        //    get;
        //    set;
        //} = "";

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        //[EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public List<Constructors> ConstructorParamers
        {
            get;
            set;
        } = new List<Constructors>();

        public Attribute GetAttribute(Object[] args)
        {
            if (args == null)
            {
                return Activator.CreateInstance(Type.GetType(AttributeName)) as Attribute;
            }
            else
            {
                return Activator.CreateInstance(Type.GetType(AttributeName), args) as Attribute;
            }

        }
    }

    public class AttributeManager
    {
        public Dictionary<string, AttributeType> ClassAttribute = new Dictionary<string, AttributeType>();
        public Dictionary<string, AttributeType> PropertyAttribute = new Dictionary<string, AttributeType>();
        public AttributeManager()
        {
            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblys.Length; i++)
            {
                Type[] types = assemblys[i].GetTypes();

                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j].IsSubclassOf(typeof(Attribute)) && types[j].FullName.IndexOf("EngineNS") != -1)
                    {
                        bool need = false;
                        AttributeType at = null;
                        AttributeUsageAttribute att = types[j].GetCustomAttribute<AttributeUsageAttribute>();
                        if (att != null)
                        {
                            if ((att.ValidOn & AttributeTargets.Class) != 0)
                            {
                                if (ClassAttribute.ContainsKey(types[j].FullName) == false)
                                {
                                    at = new AttributeType();
                                    at.AttributeName = types[j].FullName;
                                    DescriptionAttribute descriptionattr = types[j].GetCustomAttribute<DescriptionAttribute>();
                                    if (descriptionattr != null)
                                    {
                                        at.Description = descriptionattr.Description;
                                    }
                                    ClassAttribute.Add(types[j].FullName, at);
                                    need = true;
                                }
                            }

                            if ((att.ValidOn & AttributeTargets.Property) != 0 || (att.ValidOn & AttributeTargets.Field) != 0)
                            {
                                if (PropertyAttribute.ContainsKey(types[j].FullName) == false)
                                {
                                    if (at == null)
                                    {
                                        at = new AttributeType();
                                        at.AttributeName = types[j].FullName;

                                        DescriptionAttribute descriptionattr = types[j].GetCustomAttribute<DescriptionAttribute>();
                                        if (descriptionattr != null)
                                        {
                                            at.Description = descriptionattr.Description;
                                        }
                                        need = true;
                                    }

                                    PropertyAttribute.Add(types[j].FullName, at);
                                }
                            }
                        }
                        else
                        {
                            if (ClassAttribute.ContainsKey(types[j].FullName) == false)
                            {
                                at = new AttributeType();
                                at.AttributeName = types[j].FullName;
                                ClassAttribute.Add(types[j].FullName, at);
                                PropertyAttribute.Add(types[j].FullName, at);
                                need = true;
                            }
                        }

                        if (need)
                        {

                            ConstructorInfo[] cis = types[j].GetConstructors();
                            foreach(var ci in cis)
                            {
                                Constructors cs = new Constructors(at);
                                if (at != null && ci != null)
                                {
                                    ParameterInfo[] _params = ci.GetParameters();

                                    foreach (var _p in _params)
                                    {
                                        //Type 类型的参数暂时不加
                                        if (_p.ParameterType.Equals(typeof(Type)) || _p.ParameterType.Equals(typeof(Type[])) || _p.ParameterType.Equals(typeof(List<Type>)))
                                            continue;

                                        AttributeConstructorParamer acp = new AttributeConstructorParamer();
                                        acp.VariableName = _p.Name;
                                        
                                        try
                                        {
                                            if (_p.ParameterType.Equals(typeof(string)))
                                            {
                                                acp.Value = " ";
                                            }
                                            else if (_p.ParameterType.Equals(typeof(char)))
                                            {
                                                acp.Value = ' ';
                                            }
                                            else if (_p.ParameterType.Equals(typeof(bool)))
                                            {
                                                acp.Value = false;
                                            }
                                            else if (_p.ParameterType.Equals(typeof(int)) || _p.ParameterType.Equals(typeof(Int16)) || _p.ParameterType.Equals(typeof(Int32)) || _p.ParameterType.Equals(typeof(Int64))
                                                || _p.ParameterType.Equals(typeof(uint)) || _p.ParameterType.Equals(typeof(UInt16)) || _p.ParameterType.Equals(typeof(UInt32)) || _p.ParameterType.Equals(typeof(UInt64))
                                                || _p.ParameterType.Equals(typeof(short)) || _p.ParameterType.Equals(typeof(long)) || _p.ParameterType.Equals(typeof(double)))
                                            {
                                                acp.Value = 0;
                                            }
                                            else if (_p.ParameterType.IsSubclassOf(typeof(Enum)))
                                            {
                                                acp.Value = Enum.Parse(_p.ParameterType, "0");
                                            }
                                            else if(_p.ParameterType.IsArray)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                acp.Value = Activator.CreateInstance(_p.ParameterType);
                                            }
                                        }
                                        catch (System.Exception e)
                                        {
                                            Console.WriteLine("System.Exception： " + e);
                                            continue;
                                        }

                                        //acp._Value = Activator.CreateInstance(_p.ParameterType);
                                        acp.Index = cs.ParamsCount;
                                        cs.ParamsCount++;
                                        cs.Paramers.Add(acp);
                                    }
                                }
                                at.ConstructorParamers.Add(cs);
                            }

                            //默认选中一个使用的构造函数
                            if (at.ConstructorParamers.Count > 0)
                            {
                                at.ConstructorParamers[0].Enable = true;
                            }
                        }

                    }
                }
            }
        }
    }
}
