using Org.BouncyCastle.Tls.Crypto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.PythonRuntime
{
    public class TtPyBinderAttribute : Attribute
    {

    }
    public class TtPyModule : AuxPtrType<PythonWrapper.FPyModule>
    {
        public TtPyModule(TtPyModule parent, string name)
        {
            mCoreObject = PythonWrapper.FPyModule.CreateInstance((parent != null) ? parent.mCoreObject : new PythonWrapper.FPyModule(), name);
        }
        public override void Dispose()
        {
            foreach (var i in SubModules)
            {
                i.Dispose();
            }
            SubModules.Clear();
            foreach (var i in TypeDefines)
            {
                i.Dispose();
            }
            TypeDefines.Clear();
            base.Dispose();
        }
        public string Name
        {
            get
            {
                return mCoreObject.GetName();
            }
        }
        public void AddTypeDefine(TtPyTypeDefine type)
        {
            mCoreObject.AddType(type.mCoreObject);
            TypeDefines.Add(type);
        }
        public List<TtPyModule> SubModules { get; } = new List<TtPyModule>();
        public List<TtPyTypeDefine> TypeDefines { get; } = new List<TtPyTypeDefine>();
        public TtPyModule GetModule(string[] ns, int curIndex)
        {
            if (curIndex >= ns.Length)
                return this;
            foreach (var i in SubModules)
            {
                if (i.Name == ns[curIndex])
                    return i.GetModule(ns, curIndex + 1);
            }
            var result = new TtPyModule(this, ns[curIndex]);
            SubModules.Add(result);
            return result.GetModule(ns, curIndex + 1);
        }
    }

    public class TtPython : UModule<TtEngine>
    {
        public TtPyModule RootModule { get; private set; }
        public bool StartPython { get; set; } = false;
        public override async Task<bool> Initialize(TtEngine host)
        {
            if (StartPython == false)
                return true;
            PythonWrapper.FPyUtility.InitializePython();
            RootModule = new TtPyModule(null, "PyTitan");
            var args = new object[] { this, };
            foreach (var i in Rtti.UTypeDescManager.Instance.Services)
            {
                foreach (var j in i.Value.Types)
                {
                    var mtd = j.Value.SystemType.GetMethod("BuildPython", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (mtd == null)
                        continue;

                    mtd.Invoke(null, args);
                }
            }

            RunString("from PyTitan import *\n" +
                "test = EngineNS.Bricks.PythonRuntime.TtTestPythonBinder()\n" +
                "test.TestFunc1(1)\n" +
                "test = None\n");

            return await base.Initialize(host);
        }
        public override void Cleanup(TtEngine host)
        {
            RootModule?.Dispose();
            RootModule = null;
            base.Cleanup(host);
        }
        public TtPyModule GetModule(string[] ns)
        {
            lock (this)
            {
                return RootModule.GetModule(ns, 0);
            }
        }

        public unsafe FPyValue Call(FPyValue self, TtPyTuple args)
        {
            var ret = PythonWrapper.FPyUtility.CallObject(self.mPyObject, args.mCoreObject);
            return new FPyValue(ret);
        }
        public unsafe FPyValue Call(FPyValue self, FPyValue* args, uint num)
        {
            using (var tuple = PythonWrapper.FPyTuple.CreateInstance(num))
            {
                for (uint i = 0; i < num; i++)
                {
                    tuple.SetItem(i, args[i].mPyObject);
                }
                var ret = PythonWrapper.FPyUtility.CallObject(self.mPyObject, tuple);
                return new FPyValue(ret);
            }
        }
        public int RunString(string code)
        {
            var state = PythonWrapper.FPyUtility.PythonStateEnsure();
            var ret = PythonWrapper.FPyUtility.RunPythonString(code);
            PythonWrapper.FPyUtility.PythonStateRelease(state);
            return ret;
        }
    }
    [TtPyBinder]
    public partial class TtTestPythonBinder
    {
        [TtPyBinder]
        public int TestFunc1(int a)
        {
            return a + 1;
        }
        #region Gen PyBinder
        public static void BuildPython(TtPython python)
        {
            var ns = typeof(TtTestPythonBinder).Namespace;
            var nsa = ns.Split('.');
            var module = python.GetModule(nsa);
            //var module = python.RootModule;
            //var subModule = new TtPyModule(module, "EngineNS");
            //module = subModule;
            //var module = new TtPyModule(ns);

            var typeDefine = new TtPyTypeDefine(module, "TtTestPythonBinder");

            typeDefine.mCoreObject.SetNewFunction(mPyTypeNew);
            typeDefine.mCoreObject.SetDeallocFunction(mPyTypeDealloc);

            var methods = new TtPyMethodDefine();
            methods.mCoreObject.AddMethod("TestFunc1", PyTestFunc1, "");
            typeDefine.PyMethodDefine = methods;

            module.AddTypeDefine(typeDefine);
        }
        private unsafe static PythonWrapper.FPyTypeDefine.FDelegate_FNewFunction mPyTypeNew = PyTypeNew;
        private unsafe static void* PyTypeNew(void* InType, void* InArgs, void* InKwds)
        {
            var wrapper = PythonWrapper.FPyTypeDefine.AllocPyClassWrapper(InType);
            TtPyTypeDefine.Bind<TtTestPythonBinder>(wrapper);
            return wrapper;
        }
        private unsafe static PythonWrapper.FPyTypeDefine.FDelegate_FFreeManagedObjectGCHandle mPyTypeDealloc = PyTypeDealloc;
        private unsafe static void PyTypeDealloc(void* InSelf)
        {
            TtPyTypeDefine.UnBind((PythonWrapper.FPyClassWrapper*)InSelf);
            PythonWrapper.FPyTypeDefine.FreePyClassWrapper(InSelf);
        }
        
        private unsafe static PythonWrapper.FPyMethodDefine.FDelegate_FPyFunction PyTestFunc1 = PyWrapper_TestFunc1;
        private unsafe static void* PyWrapper_TestFunc1(void* InSelf, void* InArgs)
        {
            var Self = PythonWrapper.FPyClassWrapper.GetObject<TtTestPythonBinder>(InSelf);
            using (var args = PythonWrapper.FPyTuple.CreateInstance(InArgs))
            {
                var a0 = new FPyValue(args.GetItem(0));
                return FPyValue.CreateInstance(Self.TestFunc1(a0.I32)).mPyObject;
            }   
        }
        #endregion
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Bricks.PythonRuntime.TtPython PythonModule { get; } = new Bricks.PythonRuntime.TtPython();
    }
}