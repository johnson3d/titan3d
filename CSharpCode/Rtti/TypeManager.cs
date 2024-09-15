using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using static EngineNS.EGui.UIProxy.SingleInputDialog;

namespace EngineNS.Rtti
{
    public class TtAssemblyDesc
    {
        public virtual string Name { get; }
        public virtual string Service { get; }
        public virtual bool IsGameModule { get; }
        public virtual string Platform { get; }
        public WeakReference<Assembly> ModuleAssembly { get; set; }
        public Assembly UnsafeGetAssembly()
        {
            Assembly result;
            if (ModuleAssembly.TryGetTarget(out result))
                return result;
            return null;
        }
        public TtTypeDescManager.ServiceManager Manager { get; set; }
        public override string ToString()
        {
            return $"{Service}:{Name}";
        }
        public virtual void SetAssembly(System.Reflection.Assembly assembly, TtTypeDescManager.ServiceManager manager)
        {
            ModuleAssembly = new WeakReference<Assembly>(assembly);
            Manager = manager;
        }
        public virtual object CreateInstance(RName name)
        {
            return null;
        }

        #region Rtti Reload
        public static void UpdateRtti(string moduleName, Assembly newAssembly, Assembly oldAssembly)
        {
            Rtti.TtTypeDescManager.ServiceManager manager;
            Rtti.TtAssemblyDesc desc;
            if (Rtti.TtTypeDescManager.Instance.RegAssembly(newAssembly, out manager, out desc))
            {
                List<Type> removed = new List<Type>();
                List<Type> changed = new List<Type>();
                List<Type> added = new List<Type>();

                if (oldAssembly != null)
                {
                    GetChangedLists(removed, changed, added, newAssembly, oldAssembly);
                }

                UpdateTypeManager(manager, desc, removed, changed, added);
                desc.ModuleAssembly = new WeakReference<Assembly>(newAssembly);

                for (int i = 0; i < 10; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            else
            {
                manager.AddAssemblyDesc(desc);
            }

            Rtti.TtTypeDescManager.Instance.OnTypeChangedInvoke();

            EngineNS.Rtti.TtClassMetaManager.Instance.LoadMetas(moduleName);
        }
        public static void GetChangedLists(List<Type> removed, List<Type> changed, List<Type> added, System.Reflection.Assembly newAssembly, System.Reflection.Assembly oldAssembly)
        {
            var newTypes = newAssembly.GetTypes();
            if (oldAssembly == null)
            {
                foreach (var i in newTypes)
                {
                    added.Add(i);
                }
                return;
            }
            var oldTypes = oldAssembly.GetTypes();

            foreach (var i in newTypes)
            {
                Type c = null;
                foreach (var j in oldTypes)
                {
                    if (i.FullName == j.FullName)
                    {
                        c = i;
                        break;
                    }
                }
                if (c != null)
                    changed.Add(c);
                else
                    added.Add(i);
            }
            foreach (var i in oldTypes)
            {
                Type c = null;
                foreach (var j in newTypes)
                {
                    if (i.FullName == j.FullName)
                    {
                        c = i;
                        break;
                    }
                }
                if (c == null)
                {
                    removed.Add(i);
                }
            }
        }
        public static void UpdateTypeManager(Rtti.TtTypeDescManager.ServiceManager manager, Rtti.TtAssemblyDesc desc, List<Type> removed, List<Type> changed, List<Type> added)
        {
            List<string> removedNames = new List<string>();
            foreach (var j in manager.Types)
            {
                if (removed.Contains(j.Value.SystemType))
                {
                    j.Value.IsRemoved = true;
                    j.Value.SystemType = null;
                    removedNames.Add(j.Key);
                }
                foreach (var k in changed)
                {
                    if (k.FullName == j.Value.FullName)
                    {
                        j.Value.SystemType = k;
                        j.Value.Assembly = desc;
                        changed.Remove(k);
                        break;
                    }
                }
            }
            foreach (var j in removedNames)
            {
                manager.Types.Remove(j);

                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(j, false);
                if (meta != null)
                {
                    meta.CheckMetaField();
                }
            }
            foreach (var j in added)
            {
                var tmp = new Rtti.TtTypeDesc();
                tmp.SystemType = j;
                tmp.Assembly = desc;
                manager.Types[Rtti.TtTypeDesc.TypeStr(j)] = tmp;
            }
        }
        #endregion
    }
    public class TtGlobalAssemblyDesc : TtAssemblyDesc
    {
        private string mName;
        public override string Name { get => mName; }
        public override string Service { get { return "Global"; } }
        public override bool IsGameModule { get { return false; } }
        public override string Platform { get { return "Global"; } }
        public override void SetAssembly(System.Reflection.Assembly assm, TtTypeDescManager.ServiceManager manager)
        {
            base.SetAssembly(assm, manager);
            mName = "Unknown";
        }
    }
    public class TtTypeDesc
    {
        public bool IsRemoved = false;
        public Type SystemType;
        public TtAssemblyDesc Assembly;
        public bool IsRefType
        {
            get
            {
                if (SystemType == null)
                    return false;
                return SystemType.IsByRef;
            }
        }
        public bool IsPrimitive
        {
            get
            {
                if (SystemType == null)
                    return false;
                return SystemType.IsPrimitive;
            }
        }

        public string FullName
        {
            get
            {
                return SystemType?.FullName;
            }
        }
        public string Name
        {
            get
            {
                return SystemType.Name;
            }
        }
        public string NickName
        {
            get
            {
                if (SystemType.IsGenericType == false)
                    return SystemType.Name;
                var args = SystemType.GetGenericArguments();
                string name = SystemType.Name;
                foreach(var i in args)
                {
                    name += "." + i.Name;
                }
                return name;
            }
        }
        public static string GetCSharpTypeNameString(System.Type type)
        {
            if (type == null)
                return "";

            if (type.IsGenericType)
            {
                string retValue = type.Namespace + "." + type.Name;
                var agTypes = type.GetGenericArguments();
                if (agTypes.Length == 0)
                    return retValue;

                retValue = retValue.Replace("`" + type.GetGenericArguments().Length, "");
                var agStr = "";
                for (int i = 0; i < agTypes.Length; i++)
                {
                    if (i == 0)
                        agStr = GetCSharpTypeNameString(agTypes[i]);
                    else
                        agStr += "," + GetCSharpTypeNameString(agTypes[i]);
                }
                retValue += "<" + agStr + ">";
                return retValue;
            }
            else if (type.IsGenericParameter)
                return type.Name;
            else
                return type.FullName.Replace("+", ".");
        }
        public string CSharpTypeName
        {
            get { return GetCSharpTypeNameString(SystemType); }
        }
        public string Namespace
        {
            get
            {
                return SystemType.Namespace;
            }
        }
        public bool IsPublic => (SystemType != null) ? SystemType.IsPublic : false;
        public bool IsValueType => (SystemType != null) ? SystemType.IsValueType : false;
        public bool IsEnum => (SystemType != null) ? SystemType.IsEnum : false;
        public bool IsArray => (SystemType != null) ? SystemType.IsArray : false;
        public bool IsSealed => (SystemType != null) ? SystemType.IsSealed : false;
        public bool IsDelegate => typeof(Delegate).IsAssignableFrom(SystemType);
        public bool IsPointer => (SystemType != null) ? SystemType.IsPointer : false;
        public TtTypeDesc BaseType
        {
            get
            {
                if (SystemType != null)
                    return TypeOf(SystemType.BaseType);
                return null;
            }
        }

        string mTypeString;
        public static System.Reflection.FieldInfo GetField(System.Type type, string name)
        {
            var fld = type.GetField(name);
            if (fld != null)
                return fld;

            type = type.BaseType;
            if (type == null)
                return null;
            return GetField(type, name);
        }
        public static System.Reflection.PropertyInfo GetProperty(System.Type type, string name)
        {
            var fld = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (fld != null)
                return fld;

            type = type.BaseType;
            if (type == null)
                return null;
            return GetProperty(type, name);
        }
        public static bool CanCast(System.Type from, System.Type to)
        {
            if (from == to)
                return true;

            //return from.IsSubclassOf(to);
            return from.IsAssignableTo(to);
        }
        public string TypeString
        {
            get
            {
                if (mTypeString == null)
                    mTypeString = TtTypeDescManager.Instance.GetTypeStringFromType(SystemType);
                return mTypeString;
            }
        }
        public static TtTypeDesc TypeOf<T>()
        {
            return TypeOf(typeof(T));
        }
        public static TtTypeDesc TypeOf(string typeStr)
        {
            var result = TtTypeDescManager.Instance.GetTypeDescFromString(typeStr);
            if (result == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Typeof failed:{typeStr}");
            }
            return result;
        }
        public static TtTypeDesc TypeOf(System.Type type)
        {
            if (type == null)
                return null;

            var typeStr = TtTypeDescManager.Instance.GetTypeStringFromType(type);
            if (typeStr == null)
                return null;
            return TypeOf(typeStr);
        }
        public static TtTypeDesc TypeOfFullName(string fullName)
        {
            var result = TtTypeDescManager.Instance.GetTypeDescFromFullName(fullName);
            if (result == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"TypeOfFullName failed:{fullName}");
            }
            return result;
        }
        public static string TypeStr(TtTypeDesc type)
        {
            return TtTypeDescManager.Instance.GetTypeStringFromType(type.SystemType);
        }
        public static string TypeStr(System.Type type)
        {
            return TtTypeDescManager.Instance.GetTypeStringFromType(type);
        }

        public override string ToString()
        {
            return TypeStr(SystemType);//SystemType.ToString();
        }
        public bool IsEqual(Type type)
        {
            return SystemType == type;
        }
        public bool IsSubclassOf(TtTypeDesc type)
        {
            return SystemType.IsSubclassOf(type.SystemType);
        }
        public bool IsSubclassOf(Type type)
        {
            return SystemType.IsSubclassOf(type);
        }
        public bool IsParentClass(Type type)
        {
            if (type == null)
                return false;
            return type.IsSubclassOf(SystemType);
        }
        public bool HasInterface(string name)
        {
            return (SystemType.GetInterface(name) != null);
        }
        public Type[] GetGenericArguments()
        {
            return SystemType.GetGenericArguments();
        }
        public object[] GetCustomAttributes(Type type, bool inherit)
        {
            return SystemType.GetCustomAttributes(type, inherit);
        } 
        public Type GetInterface(string name)
        {
            return SystemType.GetInterface(name);
        }
#nullable enable
        public System.Reflection.MethodInfo? GetMethod(string name)
        {
            return SystemType.GetMethod(name);
        }
        public System.Reflection.MethodInfo? GetMethod(string name, System.Type[] types)
        {
            if (types == null)
                return SystemType.GetMethod(name);
            return SystemType.GetMethod(name, types);
        }
        public System.Reflection.MethodInfo[] GetMethods()
        {
            return SystemType.GetMethods();
        }
        public System.Reflection.PropertyInfo[] GetProperties()
        {
            return SystemType.GetProperties();
        }
        public System.Reflection.PropertyInfo? GetProperty(string name)
        {
            return SystemType.GetProperty(name);
        }
        public Attribute? GetCustomAttribute(Type type, bool inherit)
        {
            return SystemType.GetCustomAttribute(type, inherit);
        }
        public T? GetCustomAttribute<T>(bool inherit) where T : Attribute
        {
            return SystemType.GetCustomAttribute<T>(inherit);
        }
        public bool IsInstanceOfType(object? obj)
        {
            if (SystemType != null)
                return SystemType.IsInstanceOfType(obj);
            return false;
        }
#nullable disable
        public void RunClassConstructor()
        {
            if(SystemType != null)
                RuntimeHelpers.RunClassConstructor(SystemType.TypeHandle);
        }
    }
    public struct TtTypeDescGetter<T>
    {
        public static T DefaultObject;
        static TtTypeDesc mTypeDesc;
        public static TtTypeDesc TypeDesc
        { 
            get
            {
                if (mTypeDesc == null)
                    mTypeDesc = TtTypeDesc.TypeOf<T>();
                return mTypeDesc;
            }
        }
    }
    public class TtTypeDescManager
    {
        static TtTypeDescManager()
        {
            CoreSDK.SetCreateManagedObjectFunction(CreateObject);
            CoreSDK.SetFreeManagedObjectGCHandle(FreeManagedObjectGCHandle);
            CoreSDK.SetGetManagedObjectFromGCHandle(GetManagedObjectFromGCHandle);
        }
        public delegate void Delegate_OnTypeChanged();
        public event Delegate_OnTypeChanged OnTypeChanged;
        public void OnTypeChangedInvoke()
        {
            OnTypeChanged?.Invoke();
        }
        unsafe static CoreSDK.FDelegate_FCreateManagedObject CreateObject = CreateObjectImpl;
        unsafe static void* CreateObjectImpl(sbyte* arg0, EngineNS.Support.TtAnyValue* arg1, int NumOfArg, int WeakType)
        {
            var className = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)arg0);

            var typeDesc = TtTypeDesc.TypeOf(className);
            if (typeDesc == null)
                return (void*)0;

            object[] createArg = null;
            if (NumOfArg>0)
            {
                createArg = new object[NumOfArg];
                for (int i = 0; i < NumOfArg; i++)
                {
                    createArg[i] = Rtti.TtNativeCoreProvider.BoxValue(ref arg1[i]);
                }
            }

            var obj = CreateInstance(typeDesc, createArg);
            var gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(obj, (System.Runtime.InteropServices.GCHandleType)WeakType);
            return System.Runtime.InteropServices.GCHandle.ToIntPtr(gcHandle).ToPointer();
        }
        unsafe static CoreSDK.FDelegate_FFreeManagedObjectGCHandle FreeManagedObjectGCHandle = FreeManagedObjectGCHandleImpl;
        unsafe static void FreeManagedObjectGCHandleImpl(void* handle)
        {
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)handle);
            gcHandle.Free();
        }
        unsafe static CoreSDK.FDelegate_FGetManagedObjectFromGCHandle GetManagedObjectFromGCHandle = GetManagedObjectFromGCHandleImpl;
        unsafe static void* GetManagedObjectFromGCHandleImpl(void* handle)
        {
            //var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)handle);
            //var tmp = GetObjectPointer(gcHandle.Target);
            //var obj = GetObjectFromPointer(tmp);
            //System.Diagnostics.Debug.Assert(obj == gcHandle.Target);
            //return tmp;
            return handle;
        }
        public unsafe static void* GetObjectPointer(object obj)
        {
            return SDK_Core_GetObjectPointer(obj);
        }
        public unsafe static object GetObjectFromPointer(void* ptr)
        {
            return SDK_Core_GetObjectFromPointer(ptr);
        }
        #region SDK
        const string ModuleNC = EngineNS.CoreSDK.CoreModule;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        extern unsafe static void* SDK_Core_GetObjectPointer([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] object obj);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)]
        extern unsafe static object SDK_Core_GetObjectFromPointer(void* ptr);
        #endregion
        public static object CreateInstance(Rtti.TtTypeDesc t, params object[] args)
        {
            if (t == null)
                return null;
            return CreateInstance(t.SystemType, args);
        }
        public static object CreateInstance(System.Type t, object[] args = null)
        {
            if (t == typeof(string))
                return "";
            
            try
            {
                if (t.IsPrimitive == false && args == null && t.GetConstructor(new Type[] { }) == null)
                {
                    return RuntimeHelpers.GetUninitializedObject(t);
                }
                var result = System.Activator.CreateInstance(t, args);

                if (result != null)
                    return result;
            }
            catch
            {
                
            }
            return RuntimeHelpers.GetUninitializedObject(t);
        }
        public static TtTypeDescManager Instance { get; } = new TtTypeDescManager();
        public Dictionary<string, ServiceManager> Services { get; } = new Dictionary<string, ServiceManager>();
        public Dictionary<string, TtTypeDesc> NameAliasTypes { get; } = new Dictionary<string, TtTypeDesc>();
        public TtTypeDesc FindNameAlias(string name)
        {
            TtTypeDesc result;
            if (NameAliasTypes.TryGetValue(name, out result))
                return result;
            return null;
        }
        public bool GetInheritTypes(TtTypeDesc baseType, List<TtTypeDesc> types)
        {
            bool finded = false;
            foreach(var service in Services.Values)
            {
                foreach(var type in service.Types.Values)
                {
                    if(type.IsSubclassOf(baseType))
                    {
                        types.Add(type);
                        finded = true;
                    }
                }
            }
            return finded;
        }
        public void InitTypes()
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            string[] TypeAssembies = {
                "System.Private.CoreLib",
                "Engine.Window",
                "Engine.Console",
            };
            foreach(var i in ass)
            {
                bool find = false;
                foreach(var j in TypeAssembies)
                {
                    if (i.GetName().Name == j)
                    {
                        find = true;
                        break;
                    }
                }
                if (find == false)
                    continue;
                TtAssemblyDesc desc;
                ServiceManager manager;
                RegAssembly(i, out manager, out desc);
            }

            foreach (var i in Services)
            {
                foreach (var j in i.Value.Assemblies)
                {
                    i.Value.AddAssemblyDesc(j.Value);
                }
            }

            foreach (var i in Services)
            {
                foreach (var j in i.Value.Types)
                {
                    var attr = j.Value.SystemType.GetCustomAttribute(typeof(Rtti.MetaAttribute), false) as Rtti.MetaAttribute;
                    if (attr == null || attr.NameAlias == null)
                        continue;
                    foreach(var k in attr.NameAlias)
                    {
                        NameAliasTypes[k] = j.Value;
                    }
                }
            }
        }
        public void InterateTypes(Action<TtTypeDesc> cb)
        {
            foreach (var i in Services)
            {
                foreach (var j in i.Value.Types)
                {
                    cb(j.Value);
                }
            }
        }
        public class ServiceManager
        {
            public Dictionary<string, TtTypeDesc> Types = new Dictionary<string, TtTypeDesc>();
            public Dictionary<string, TtAssemblyDesc> Assemblies { get; } = new Dictionary<string, TtAssemblyDesc>();
            public void AddAssemblyDesc(TtAssemblyDesc desc)
            {
                var tps = desc.UnsafeGetAssembly().GetTypes();
                foreach (var i in tps)
                {
                    if (i.IsGenericType)
                        continue;
                    RegType(i, desc);
                    foreach (var j in i.GetProperties())
                    {
                        if (j.PropertyType.IsGenericType == false)
                        {
                            continue;
                        }
            
                        var propAssmDesc = FindAssemblyDesc(j.PropertyType.Assembly);
                        if (propAssmDesc != null)
                        {
                            RegType(j.PropertyType, propAssmDesc);
                        }
                    }
                    foreach (var j in i.GetFields())
                    {
                        if (j.FieldType.IsGenericType == false)
                        {
                            continue;
                        }

                        var propAssmDesc = FindAssemblyDesc(j.FieldType.Assembly);
                        if (propAssmDesc != null)
                        {
                            RegType(j.FieldType, propAssmDesc);
                        }
                    }
                }
            }
            public TtAssemblyDesc FindAssemblyDesc(System.Reflection.Assembly assembly)
            {
                foreach(var i in Assemblies)
                {
                    if (i.Value.UnsafeGetAssembly() == assembly)
                        return i.Value;
                }
                return null;
            }
            private void RegType(Type t, TtAssemblyDesc desc)
            {
                var str = TtTypeDescManager.Instance.GetTypeStringFromType(t, false);
                if (str == null)
                    return;
                TtTypeDesc tdesc;
                if (Types.TryGetValue(str, out tdesc) == false)
                {
                    tdesc = new TtTypeDesc()
                    {
                        //SystemType = t,
                        Assembly = desc
                    };
                    tdesc.SystemType = t;
                    Types.Add(str, tdesc);
                }
            }
        }
        internal TtAssemblyDesc FindAssemblyDesc(System.Reflection.Assembly assm)
        {
            foreach(var i in Services)
            {
                foreach(var j in i.Value.Assemblies)
                {
                    if(j.Value.UnsafeGetAssembly() == assm)
                    {
                        return j.Value;
                    }
                }
            }
            return null;
        }
        public Dictionary<string, string> StringMap = new Dictionary<string, string>();
        public string GetTypeStringFromType(TtTypeDesc type, bool tryAdd2Manager = true)
        {
            return GetTypeStringFromType(type.SystemType, tryAdd2Manager);
        }
        public string GetTypeStringFromType(Type type, bool tryAdd2Manager = true)
        {
            var originName = type.ToString();// type.FullName;
            if (originName == null)
            {
                return null;
            }
            else if (type.IsGenericParameter)
            {
                return "#" + type.Name;
            }
            if (originName.StartsWith("<>"))
            {
                return null;
            }
            string result;
            if (StringMap.TryGetValue(originName, out result))
                return result;
            
            TtAssemblyDesc assm = FindAssemblyDesc(type.Assembly);
            if (assm == null)
            {
                return null;
            }
            var fullName = originName.Replace('+', '.');
            var templatePos = fullName.IndexOf('`');
            if (templatePos >=0 )
            {
                fullName = fullName.Substring(0, templatePos);
            }
            var agTypeStr = "";            
            if (type.IsGenericType)
            {
                agTypeStr += "<";
                var agTypes = type.GetGenericArguments();           
                for (int i = 0; i < agTypes.Length; i++)
                {
                    agTypeStr += GetTypeStringFromType(agTypes[i], tryAdd2Manager) + ",";
                }
                agTypeStr += ">";
            }

            result = $"{fullName}{agTypeStr}@{assm.Name}";
            StringMap[originName] = result;
            if (tryAdd2Manager)
            {
                if (GetTypeDescFromString(result) == null)
                {
                    var typeDesc = new TtTypeDesc();
                    typeDesc.Assembly = assm;
                    typeDesc.SystemType = type;
                    assm.Manager.Types[result] = typeDesc;
                }
            }
            return result;
        }
        public TtTypeDesc GetTypeDescFromString(string typeStr)
        {
            foreach (var i in Services)
            {
                TtTypeDesc t;
                if (i.Value.Types.TryGetValue(typeStr, out t))
                {
                    return t;
                }
            }
            return Rtti.TtTypeDescManager.Instance.FindNameAlias(typeStr);
        }
        public TtTypeDesc GetTypeDescFromFullName(string fullName)
        {
            foreach (var i in Services)
            {
                foreach(var j in i.Value.Types)
                {
                    if(j.Value.FullName == fullName)
                    {
                        return j.Value;
                    }
                }
            }
            return Rtti.TtTypeDescManager.Instance.FindNameAlias(fullName);
        }
        public System.Type GetTypeFromString(string typeStr)
        {
            var typeDesc = GetTypeDescFromString(typeStr);
            return (typeDesc != null) ? typeDesc.SystemType : null;
        }
        public bool RegAssembly(System.Reflection.Assembly asm, out ServiceManager manager, out TtAssemblyDesc outDesc)
        {
            ServiceManager mgr = null;
            var type = asm.GetType("EngineNS.Rtti.AssemblyEntry");
            if (type != null)
            {
                var mtd = type.GetMethod("GetAssemblyDesc", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (mtd != null)
                {
                    var retDesc = mtd.Invoke(null, null);
                    var desc = retDesc as TtAssemblyDesc;
                    if (desc != null)
                    {
                        if (Services.TryGetValue(desc.Service, out mgr) == false)
                        {
                            mgr = new ServiceManager();
                            Services.Add(desc.Service, mgr);
                        }
                        manager = mgr;
                        desc.SetAssembly(asm, mgr);
                        outDesc = desc;
                        if (mgr.Assemblies.ContainsKey(desc.Name))
                        {
                            mgr.Assemblies[desc.Name] = desc;
                            return true;
                        }
                        else
                        {
                            mgr.Assemblies.Add(desc.Name, desc);
                            return false;
                        }
                    }
                }
            }
            var assmDesc = new TtGlobalAssemblyDesc();
            if (Services.TryGetValue(assmDesc.Service, out mgr) == false)
            {
                mgr = new ServiceManager();
                Services.Add(assmDesc.Service, mgr);
            }
            manager = mgr;
            assmDesc.SetAssembly(asm, mgr);
            
            mgr.Assemblies.Add(asm.FullName, assmDesc);//assmDesc.Name, assmDesc);
            outDesc = assmDesc;
            return false;
        }
        public bool UnregAssembly(System.Reflection.Assembly asm)
        {
            foreach (var s in Services.Values)
            {
                foreach (var a in s.Assemblies)
                {
                    if (a.Value.UnsafeGetAssembly() == asm)
                    {
                        List<string> removedNames = new List<string>();
                        foreach (var j in s.Types)
                        {
                            if (j.Value.Assembly.UnsafeGetAssembly() == asm)
                            {
                                j.Value.IsRemoved = true;
                                j.Value.SystemType = null;
                                removedNames.Add(j.Key);
                            }
                        }
                        foreach (var j in removedNames)
                        {
                            s.Types.Remove(j);

                            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(j, false);
                            if (meta != null)
                            {
                                meta.CheckMetaField();
                            }
                        }
                        s.Assemblies.Remove(a.Key);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool UnregAssembly(string service, string name)
        {
            ServiceManager mgr;
            if (Services.TryGetValue(service, out mgr))
            {
                TtAssemblyDesc result;
                if (mgr.Assemblies.TryGetValue(name, out result))
                {
                    List<string> removedNames = new List<string>();
                    foreach (var j in mgr.Types)
                    {
                        if (j.Value.Assembly == result)
                        {
                            j.Value.IsRemoved = true;
                            j.Value.SystemType = null;
                            removedNames.Add(j.Key);
                        }
                    }
                    foreach (var j in removedNames)
                    {
                        mgr.Types.Remove(j);

                        var meta = Rtti.TtClassMetaManager.Instance.GetMeta(j, false);
                        if (meta != null)
                        {
                            meta.CheckMetaField();
                        }
                    }
                    mgr.Assemblies.Remove(name);
                    return true;
                }
            }
            return false;
        }
        public TtAssemblyDesc GetAssembly(string service, string name)
        {
            ServiceManager mgr;
            if (Services.TryGetValue(service, out mgr))
            {
                TtAssemblyDesc result;
                if (mgr.Assemblies.TryGetValue(name, out result))
                {
                    return result;
                }
            }
            return null;
        }
    }
}
