using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    public class AssemblyDesc
    {
        public virtual string Name { get; }
        public virtual string Service { get; }
        public virtual bool IsGameModule { get; }
        public virtual string Platform { get; }
        public System.Reflection.Assembly Assembly { get; set; }
        public UTypeDescManager.ServiceManager Manager { get; set; }
        public override string ToString()
        {
            return $"{Service}:{Name}";
        }
        public virtual void SetAssembly(System.Reflection.Assembly assm, UTypeDescManager.ServiceManager manager)
        {
            Assembly = assm;
            Manager = manager;
        }
        public virtual object CreateInstance(RName name)
        {
            return null;
        }
    }
    public class GlobalAssemblyDesc : AssemblyDesc
    {
        private string mName;
        public override string Name { get => mName; }
        public override string Service { get { return "Global"; } }
        public override bool IsGameModule { get { return false; } }
        public override string Platform { get { return "Global"; } }
        public override void SetAssembly(System.Reflection.Assembly assm, UTypeDescManager.ServiceManager manager)
        {
            base.SetAssembly(assm, manager);
            mName = "Unknown";
        }
    }
    public class UTypeDesc
    {
        public bool IsRemoved = false;
        public Type SystemType;
        public AssemblyDesc Assembly;
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
        public string Namespace
        {
            get
            {
                return SystemType.Namespace;
            }
        }
        public bool IsValueType => SystemType.IsValueType;
        public bool IsEnum => SystemType.IsEnum;
        public bool IsArray => SystemType.IsArray;
        public bool IsSealed => SystemType.IsSealed;
        public bool IsDelegate => typeof(Delegate).IsAssignableFrom(SystemType);
        public bool IsPointer => SystemType.IsPointer;
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
                    mTypeString = UTypeDescManager.Instance.GetTypeStringFromType(SystemType);
                return mTypeString;
            }
        }
        public static UTypeDesc TypeOf<T>()
        {
            return TypeOf(typeof(T));
        }
        public static UTypeDesc TypeOf(string typeStr)
        {
            var result = UTypeDescManager.Instance.GetTypeDescFromString(typeStr);
            if (result == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Rtti", $"Typeof failed:{typeStr}");
            }
            return result;
        }
        public static UTypeDesc TypeOf(System.Type type)
        {
            if (type == null)
                return null;

            var typeStr = UTypeDescManager.Instance.GetTypeStringFromType(type);
            return TypeOf(typeStr);
        }
        public static UTypeDesc TypeOfFullName(string fullName)
        {
            var result = UTypeDescManager.Instance.GetTypeDescFromFullName(fullName);
            if (result == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Rtti", $"TypeOfFullName failed:{fullName}");
            }
            return result;
        }
        public static string TypeStr(UTypeDesc type)
        {
            return UTypeDescManager.Instance.GetTypeStringFromType(type.SystemType);
        }
        public static string TypeStr(System.Type type)
        {
            return UTypeDescManager.Instance.GetTypeStringFromType(type);
        }

        public override string ToString()
        {
            return TypeStr(SystemType);//SystemType.ToString();
        }
        public bool IsEqual(Type type)
        {
            return SystemType == type;
        }
        public bool IsSubclassOf(UTypeDesc type)
        {
            return SystemType.IsSubclassOf(type.SystemType);
        }
        public bool IsSubclassOf(Type type)
        {
            return SystemType.IsSubclassOf(type);
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
    }
    public struct UTypeDescGetter<T>
    {
        static UTypeDesc mTypeDesc;
        public static UTypeDesc TypeDesc
        { 
            get
            {
                if (mTypeDesc == null)
                    mTypeDesc = UTypeDesc.TypeOf<T>();
                return mTypeDesc;
            }
        }
    }
    public class UTypeDescManager
    {
        static UTypeDescManager()
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
        unsafe static void* CreateObjectImpl(sbyte* arg0, EngineNS.Support.UAnyValue* arg1, int NumOfArg, int WeakType)
        {
            var className = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)arg0);

            var typeDesc = UTypeDesc.TypeOf(className);
            if (typeDesc == null)
                return (void*)0;

            object[] createArg = null;
            if (NumOfArg>0)
            {
                createArg = new object[NumOfArg];
                for (int i = 0; i < NumOfArg; i++)
                {
                    createArg[i] = Rtti.UNativeCoreProvider.BoxValue(ref arg1[i]);
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
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)handle);
            var tmp = GetObjectPointer(gcHandle.Target);
            var obj = GetObjectFromPointer(tmp);
            System.Diagnostics.Debug.Assert(obj == gcHandle.Target);
            return tmp;
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
        public static object CreateInstance(Rtti.UTypeDesc t, params object[] args)
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
                    return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t);
                }
                var result = System.Activator.CreateInstance(t, args);

                if (result != null)
                    return result;
            }
            catch
            {
                
            }
            return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t);
        }
        public static UTypeDescManager Instance { get; } = new UTypeDescManager();
        public Dictionary<string, ServiceManager> Services { get; } = new Dictionary<string, ServiceManager>();
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
                AssemblyDesc desc;
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
        }
        public class ServiceManager
        {
            public Dictionary<string, UTypeDesc> Types = new Dictionary<string, UTypeDesc>();
            public Dictionary<string, AssemblyDesc> Assemblies { get; } = new Dictionary<string, AssemblyDesc>();
            public void AddAssemblyDesc(AssemblyDesc desc)
            {
                var tps = desc.Assembly.GetTypes();
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
            public void RemoveAssembly(System.Reflection.Assembly assembly)
            {
                AssemblyDesc desc = null;
                foreach (var i in Assemblies)
                {
                    if (i.Value.Assembly == assembly)
                    {
                        desc = i.Value;
                        Assemblies.Remove(i.Key);
                        break;
                    }
                }
                if (desc == null)
                    return;
                var rmv = new List<string>();
                foreach(var i in Types)
                {
                    if(i.Value.Assembly == desc)
                    {
                        rmv.Add(i.Key);
                    }
                }
                foreach(var i in rmv)
                {
                    Types.Remove(i);
                }
            }
            public AssemblyDesc FindAssemblyDesc(System.Reflection.Assembly assembly)
            {
                foreach(var i in Assemblies)
                {
                    if (i.Value.Assembly == assembly)
                        return i.Value;
                }
                return null;
            }
            private void RegType(Type t, AssemblyDesc desc)
            {
                var str = UTypeDescManager.Instance.GetTypeStringFromType(t, false);
                if (str == null)
                    return;
                UTypeDesc tdesc;
                if (Types.TryGetValue(str, out tdesc) == false)
                {
                    tdesc = new UTypeDesc()
                    {
                        //SystemType = t,
                        Assembly = desc
                    };
                    tdesc.SystemType = t;
                    Types.Add(str, tdesc);
                }
            }
        }
        internal AssemblyDesc FindAssemblyDesc(System.Reflection.Assembly assm)
        {
            foreach(var i in Services)
            {
                foreach(var j in i.Value.Assemblies)
                {
                    if(j.Value.Assembly == assm)
                    {
                        return j.Value;
                    }
                }
            }
            return null;
        }
        public Dictionary<string, string> StringMap = new Dictionary<string, string>();
        public string GetTypeStringFromType(Type type, bool tryAdd2Manager = true)
        {
            if (@type.IsGenericParameter || type.FullName == null)
            {
                return null;
            }
            if (type.FullName.StartsWith("<>"))
            {
                return null;
            }
            string result;
            if (StringMap.TryGetValue(type.FullName, out result))
                return result;
            AssemblyDesc assm = FindAssemblyDesc(type.Assembly);
            if (assm == null)
            {
                return null;
            }
            var fullName = type.FullName.Replace('+', '.');
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
            StringMap[type.FullName] = result;
            if (tryAdd2Manager)
            {
                if (GetTypeDescFromString(result) == null)
                {
                    var typeDesc = new UTypeDesc();
                    typeDesc.Assembly = assm;
                    typeDesc.SystemType = type;
                    assm.Manager.Types[result] = typeDesc;
                }
            }
            return result;
        }
        public UTypeDesc GetTypeDescFromString(string typeStr)
        {
            foreach (var i in Services)
            {
                UTypeDesc t;
                if (i.Value.Types.TryGetValue(typeStr, out t))
                {
                    return t;
                }
            }
                
            return null;
        }
        public UTypeDesc GetTypeDescFromFullName(string fullName)
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
            return null;
        }
        public System.Type GetTypeFromString(string typeStr)
        {
            var typeDesc = GetTypeDescFromString(typeStr);
            if (typeDesc != null)
                return typeDesc.SystemType;

            var cvtType = UMissingTypeManager.Instance.GetConvertType(typeStr);
            if (cvtType == null)
                return null;
            return cvtType.SystemType;
        }
        public bool RegAssembly(System.Reflection.Assembly asm, out ServiceManager manager, out AssemblyDesc outDesc)
        {
            ServiceManager mgr = null;
            var type = asm.GetType("EngineNS.Rtti.AssemblyEntry");
            if (type != null)
            {
                var mtd = type.GetMethod("GetAssemblyDesc", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (mtd != null)
                {
                    var retDesc = mtd.Invoke(null, null);
                    var desc = retDesc as AssemblyDesc;
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
            var assmDesc = new GlobalAssemblyDesc();
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
        public AssemblyDesc GetAssembly(string service, string name)
        {
            ServiceManager mgr;
            if (Services.TryGetValue(service, out mgr))
            {
                AssemblyDesc result;
                if (mgr.Assemblies.TryGetValue(name, out result))
                {
                    return result;
                }
            }
            return null;
        }
    }
}
